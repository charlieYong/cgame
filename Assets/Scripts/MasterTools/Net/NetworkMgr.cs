using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.IO;
using System.Text;



namespace ZR_MasterTools
{
    internal class NetRecvData {
    public byte[] buffer = new byte[NetworkMgr.BUFFER_SIZE];
    public readonly int BufferSize = NetworkMgr.BUFFER_SIZE;
    public Socket workSocket;
}


    public class NetworkMgr : MonoSingleton<NetworkMgr>
    {
        // timeout configs
        public static readonly int SOCKET_CONNECT_TIMEOUT = 5;
        public static readonly int SOCKET_RECV_TIMEOUT = 8;
        public static readonly int SOCKET_SEND_TIMEOUT = 7;

        public static readonly int SOCKET_CONNECT_RETRY = 2;
        public static readonly int BUFFER_SIZE = 4096;

        string IP;
        int PORT;

        public event VoidCallback onConnectComplete;
        public event VoidCallback onRetryFail;

        MemoryStreamEx m_ComunicationMem = new MemoryStreamEx();
        Queue<byte[]> m_SendQueue = new Queue<byte[]>();

        GameSpecificLogicBase _gameSpecificLogic;
        public GameSpecificLogicBase GameSpecificLogic { get { return _gameSpecificLogic; } }

        INetMessageReader m_Reader;
        public INetMessageReader Reader { get; set; }

        INetMessageWriter m_Writer = new NetStreamWriter();
        public INetMessageWriter Writer { get; set; }

        IAsyncResult _asyncReader;

        int _retry = 0;
        Socket _clientSocket;
        ushort _seq = 0;
        LastPacket _lastPacket = null;
        bool _raiseConnectionEvent = false;
        NetTaskTimer _networkTimer = null;


        public void Setup(GameSpecificLogicBase specificLogic, IMessageHandler messageHandler) {
            _gameSpecificLogic = specificLogic;
            m_Reader = new NetStreamReader(messageHandler);
        }

        void OnAsyncConnectComplete(IAsyncResult ar) {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            // NOTE: ios运行环境不能设置blocking
            if (RuntimePlatform.IPhonePlayer != Application.platform) {
                socket.Blocking = false;
            }
            NetRecvData state = new NetRecvData { workSocket = _clientSocket };
            _clientSocket.BeginReceive(state.buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            _raiseConnectionEvent = true;
        }

        bool Connect() {
            ReleaseSocket();
            try {
#if UNITY_IOS
            // ios平台需要处理ipv6
            string ip = IP;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                ip = IOSIPv6Helper.SynthesizeIP (ip);
            }
            IPAddress address = IPAddress.Parse (ip);
#else
                IPAddress address = IPAddress.Parse(IP);
#endif
                _clientSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, true);
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                StartNetworkTimer(NetOperateType.Connect, SOCKET_CONNECT_TIMEOUT);
                _clientSocket.BeginConnect(address, PORT, new AsyncCallback(OnAsyncConnectComplete), _clientSocket);
                return true;
            }
            catch (Exception ex) {
                Debug.LogWarning("Connect() Exception:");
                Debug.LogWarning(ex);
                return false;
            }
        }

        public bool Connect(string ip, int port) {
            if (GameDebug.IsEnableDebugLog) {
                GameDebug.Log(string.Format("Connect To Server: {0} {1}", ip, port));
            }
            IP = ip;
            PORT = port;
            return Connect();
        }

        bool ReConnect() {
            while (_retry < SOCKET_CONNECT_RETRY) {
                _retry += 1;
                if (GameDebug.IsEnableDebugLog) {
                    GameDebug.LogWarning("Re Connect, Count=" + _retry);
                }
                return Connect();
            }
            GameSpecificLogic.ShowConnecting(false);
            GameSpecificLogic.EnableInteractive(true);
            if (null != onRetryFail) {
                onRetryFail();
            }
            return false;
        }

        /// 自动重连数次失败后，用户继续尝试重连 
        public void TryReConnectAgain() {
            _retry = 0;
            ReConnect();
        }

        public void ReSendLastPacket() {
            if (null == _lastPacket || null == _lastPacket.packet) {
                return;
            }
            SendPacket(_lastPacket.cmdid, _lastPacket.packet, true);
            if (GameDebug.IsEnableDebugLog) {
                GameDebug.Log("ReSend Last Packet:" + _lastPacket.cmdid.ToString());
            }
        }

        void HandleException(Exception e) {
            if (e is SocketException) {
                Debug.LogWarning(string.Format("Socket Exception, ErrCode={0}, Info={1}", ((SocketException)e).ErrorCode, e.ToString()));
            }
            else {
                Debug.LogWarning(string.Format("NetworkMgr Exception: Type={0}, Info={1}", e.GetType(), e.ToString()));
            }
            ReleaseSocket();
        }

        public void ReleaseSocket() {
            if (null != _asyncReader) {
                _asyncReader = null;
            }
            try {
                if (null != _clientSocket) {
                    _clientSocket.Close();
                    _clientSocket = null;
                }
            }
            catch (Exception e) {
                Debug.LogWarning("Close Socket Exception on ReleaseSocket:" + e.ToString());
            }
            if (m_Reader != null) m_Reader.Reset();
            if (m_Writer != null) m_Writer.Reset();
            if (null != m_ComunicationMem) {
                m_ComunicationMem.Clear();
            }
            m_SendQueue.Clear();
        }

        bool Send(Socket handle, byte[] byteData) {
            try {
                handle.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, handle);
                return true;
            }
            catch (Exception exception) {
                HandleException(exception);
            }
            return false;
        }

        void SendCallback(IAsyncResult ar) {
            try {
                ((Socket)ar.AsyncState).EndSend(ar);
                Queue<byte[]> sendQueue = m_SendQueue;
                lock (sendQueue) {
                    if (m_SendQueue.Count > 0) {
                        Send(_clientSocket, m_SendQueue.Dequeue());
                    }
                }
            }
            catch (Exception e) {
                HandleException(e);
            }
        }

        void ReceiveCallback(IAsyncResult ar) {
            try {
                NetRecvData asyncState = (NetRecvData)ar.AsyncState;
                Socket workSocket = asyncState.workSocket;
                if (!workSocket.Connected) {
                    // 主动调用Shutdown也会触发这个回调
                    return;
                }
                int count = workSocket.EndReceive(ar);
                if (count > 0) {
                    MemoryStreamEx comunicationMem = m_ComunicationMem;
                    lock (comunicationMem) m_ComunicationMem.Write(asyncState.buffer, 0, count);
                }
                else if (0 == count) {
                    // 服务器主动断开链接：这里可能会是出现bug导致，比如重复发了某些包
                    Debug.LogWarning("Server Close the Connection!");
                    _lastPacket = null;
                    ReleaseSocket();
                    return;
                }
                _asyncReader = workSocket.BeginReceive(asyncState.buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), asyncState);
            }
            catch (Exception exception) {
                HandleException(exception);
            }
        }

        void Update() {
            if (_raiseConnectionEvent && IsConnected()) {
                StopNetworkTimer(true);
                _raiseConnectionEvent = false;
                if (null != onConnectComplete) {
                    onConnectComplete();
                }
            }
            MemoryStreamEx comunicationMem = m_ComunicationMem;
            lock (comunicationMem) {
                if (m_ComunicationMem.Length > 0L) {
                    if (m_Reader != null) {
                        m_Reader.ReadData(m_ComunicationMem.GetBuffer(), (int)m_ComunicationMem.Length);
                    }
                    m_ComunicationMem.Clear();
                }
            }
        }

        Dictionary<int, Callback<object>> handles = new Dictionary<int, Callback<object>>();
        public void Register(int key, Callback<object> handle) {
            if (!handles.ContainsKey(key)) {
                handles.Add(key, handle);
            }
            else {
                GameDebug.LogWarning("注册重复的回调函数：" + key.ToString());
                handles[key] = handle;
            }
        }
        public void UnRegister(int key, Callback<object> handle) {
            if (handles.ContainsKey(key)) {
                handles.Remove(key);
            }
        }

        public void Excute(int key, object obj) {
            if (GameDebug.IsEnableDebugLog) {
                GameDebug.Log(
                    string.Format(
                        "Receive Packet [{0}] And Excute Handler, Seq={1}",
                        GameSpecificLogic.GetPacketKeyText(key),
                        ((NetStreamReader)m_Reader).GetHeaderSeq()
                    )
                );
            }
            if (((NetStreamReader)m_Reader).GetHeaderSeq() == _seq) {
                StopNetworkTimer(true);
            }
            if (NetworkMgr.Instance.handles.ContainsKey(key)) {
                NetworkMgr.Instance.handles[key](obj);
            }
        }

        ushort NextPacketSeq() {
            _seq = (ushort.MaxValue == _seq) ? (ushort)1 : (ushort)(_seq + 1);
            return _seq;
        }

        public bool SendAsyncMessage<T>(int id, T data) {
            return SendMessage<T>(id, data, false);
        }

        public bool SendMessage<T>(int id, T data, bool block = true) {
            MemoryStream mem = new MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(mem, data);
            return SendPacket(id, mem, block);
        }

        bool SendPacket(int id, MemoryStream data, bool block) {
            if (!IsConnected()) {
                // 还没连接上服务器，有可能是服务器端主动关闭了
                _lastPacket = new LastPacket(id, data);
                Connect();
                return true;
            }
            byte[] buffer = m_Writer.MakeDataStream(id, data, (block ? NextPacketSeq() : (ushort)0));
            if (GameDebug.IsEnableDebugLog) {
                GameDebug.Log(string.Format("Send Packet [{0}], Block={1}, Seq={2}, at time={3}", GameSpecificLogic.GetPacketKeyText(id), block, _seq, TimeManager.Now()));
            }
            if (block) {
                float timeout = SOCKET_RECV_TIMEOUT;
                if (GameSpecificLogic.IsPacketNeedDoubleTimeout(id)) {
                    // 支付成功校验协议需要延长超时时间（服务器跟第三方服务器校验耗时较长）
                    timeout *= 2;
                }
                StartNetworkTimer(NetOperateType.Receive, timeout, GameSpecificLogic.IsLoginToGamePacket(id) ? null : new LastPacket(id, data));
            }
            else {
                _lastPacket = null;
            }
            lock (m_SendQueue) {
                if (m_SendQueue.Count == 0) {
                    return Send(_clientSocket, buffer);
                }
                else {
                    m_SendQueue.Enqueue(buffer);
                    return true;
                }
            }
        }

        public bool IsConnected() {
            return ((_clientSocket != null) && _clientSocket.Connected);
        }

        IEnumerator NetworkTimer(float seconds) {
            // 0.5秒内完成网络交互的，不展示“连接中”界面
            float quick = 0.5f;
            if (seconds < quick) {
                yield return new WaitForSeconds(seconds);
                yield break;
            }
            yield return new WaitForSeconds(quick);
            GameSpecificLogic.ShowConnecting(true);
            yield return new WaitForSeconds(seconds - quick);
        }

        NetTaskTimer StartNetworkTimer(NetOperateType type, float seconds, LastPacket packet = null) {
            if (null != _networkTimer) {
                Debug.LogWarning("Bug! Timer Already Exists: " + _networkTimer.ToString());
                return _networkTimer;
            }
            GameSpecificLogic.EnableInteractive(false);
            _networkTimer = new NetTaskTimer(type, NetworkTimer(seconds));
            _networkTimer.Finished += OnNetworkTimeOut;
            _networkTimer.packet = packet;
            return _networkTimer;
        }

        void OnNetworkTimeOut(bool isManualStop) {
            if (isManualStop) {
                GameDebug.Log("Network Timer Stoped by Manual");
                return;
            }
            Debug.LogWarning(TimeManager.Now() + " Network Timeout," + _networkTimer.ToString());
            switch (_networkTimer.type) {
                case NetOperateType.Connect:
                case NetOperateType.Login:// csLogin2Game
                    _networkTimer = null;
                    break;
                case NetOperateType.Receive:
                    if (null != _networkTimer.packet) {
                        if (null != _lastPacket) {
                            Debug.LogWarning(string.Format(
                                "Overide Lastpacket? Prev CmdID={0}, Current CmdID={1}",
                                _lastPacket.cmdid.ToString(), _networkTimer.packet.cmdid.ToString()
                           ));
                        }
                        _lastPacket = _networkTimer.packet;
                    }
                    _networkTimer = null;
                    break;
            }
            ReConnect();
        }

        public void StopNetworkTimer(bool isReSendOK = false) {
            if (null == _networkTimer) {
                return;
            }
            //Debug.Log(DataManager.Now() + " Stop Timer, " + _networkTimer.ToString());
            if (isReSendOK && (null != _networkTimer.packet) && (null != _lastPacket) && (_lastPacket.cmdid == _networkTimer.packet.cmdid)) {
                // ReSend Last Packet Success?
                GameDebug.Log("ReSend Last Packet Success, CmdID=" + _lastPacket.cmdid);
                _lastPacket = null;
                _retry = 0;
            }
            _networkTimer.Finished -= OnNetworkTimeOut;
            _networkTimer.Stop();
            _networkTimer = null;
            GameSpecificLogic.ShowConnecting(false);
            GameSpecificLogic.EnableInteractive(true);
        }


        public class LastPacket
        {
            public int cmdid;
            public MemoryStream packet;

            public LastPacket(int cmd, MemoryStream data) {
                cmdid = cmd;
                packet = data;
            }
        }

        public enum NetOperateType
        {
            None,
            Connect,
            Login,
            Receive
        }

        public class NetTaskTimer : Task
        {
            public NetOperateType type;
            public LastPacket packet;

            public NetTaskTimer(NetOperateType inType, IEnumerator timer, bool autoStart = true)
                : base(timer, autoStart) {
                type = inType;
            }

            public override string ToString() {
                string t = "Type=" + type.ToString();
                string cmd = (null == packet) ? string.Empty : "CmdID=" + NetworkMgr.Instance.GameSpecificLogic.GetPacketKeyText(packet.cmdid);
                return string.Format("NetTaskTimer: {0} {1}", t, cmd);
            }
        }
    }
}