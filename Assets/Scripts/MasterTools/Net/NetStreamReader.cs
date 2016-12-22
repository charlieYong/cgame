using UnityEngine;
using System.Collections;
using System;


namespace ZR_MasterTools
{
    public class NetStreamReader : INetMessageReader
    {
        // 网络传输数据（源数据）
        NetStreamBuffer _netBuffer = new NetStreamBuffer();
        // 协议处理对象
        IMessageHandler _messageHandler;
        // 协议数据
        MemoryStreamEx _msgDataBody = new MemoryStreamEx();
        // 协议头
        ProtoHeader _msgHeader = new ProtoHeader();
        // 协议头是否已处理
        bool _isHeaderProcessed = false;

        public NetStreamReader(IMessageHandler messageHandler) {
            _messageHandler = messageHandler;
        }

        void INetMessageReader.ReadData(byte[] data, int size) {
            MemoryStreamEx activedStream = this._netBuffer.GetActivedStream();
            activedStream.Write(data, 0, size);
            byte[] buffer = activedStream.GetBuffer();
            int length = (int)activedStream.Length;
            while (true) {
                if (_isHeaderProcessed) {
                    if (length < _msgHeader.m_nPackLen) {
                        break;
                    }
                    _isHeaderProcessed = false;
                    this._msgDataBody.Clear();
                    int bodyLen = _msgHeader.m_nPackLen - ProtoHeader.HEAD_LEN;
                    if (bodyLen < 0) {
                        this._netBuffer.Reset();
                        throw new System.Exception(
                            string.Format("异常的包体:{0} pkg={1} header={2}", _msgHeader.m_nCmd, _msgHeader.m_nPackLen, ProtoHeader.HEAD_LEN)
                        );
                    }
                    if (bodyLen > 0) {
                        byte[] body = new byte[bodyLen];
                        Array.Copy(buffer, ProtoHeader.HEAD_LEN, body, 0, bodyLen);
                        //string encodeKey = null;
                        //if (NetworkMgr.Instance.GameSpecificLogic.NeedEncodePacket(_msgHeader.m_nCmd, out encodeKey)) {
                        //    NetworkMgr.Instance.GameSpecificLogic.EncodePacketBody(body, bodyLen, encodeKey);
                        //}
                        this._msgDataBody.Write(body, 0, bodyLen);
                    }
                    // 处理消息
                    try {
                        this._msgDataBody.Position = 0;
                        this._messageHandler.HandleMessage((int)_msgHeader.m_nCmd, this._msgDataBody);
                    }
                    catch (Exception ex) {
                        Debug.LogError(ex);
                    }
                    if (length > 0) {
                        activedStream = this._netBuffer.MoveStream((int)(_msgHeader.m_nPackLen));
                        buffer = activedStream.GetBuffer();
                        length = (int)activedStream.Length;
                    }
                    else {
                        activedStream.Clear();
                        break;
                    }
                }
                else {
                    _isHeaderProcessed = false;
                    if (length < ProtoHeader.HEAD_LEN) {
                        break;
                    }
                    _msgHeader.readFromBytes(buffer);
                    if (_msgHeader.m_nCmd <= 0 || _msgHeader.m_nPackLen < ProtoHeader.HEAD_LEN) {
                        this._netBuffer.Reset();
                        // 抛出异常触发信息上报，从而在TestIn里查看具体报错信息
                        throw new System.Exception("异常协议头");
                    }
                    _isHeaderProcessed = true;
                }
            }
        }

        // 重置当前网络数据
        void INetMessageReader.Reset() {
            _isHeaderProcessed = false;
            _msgHeader.reset();
            this._netBuffer.Reset();
            this._msgDataBody.Clear();
        }

        /// 获取协议序号
        public ushort GetHeaderSeq() {
            if (null == _msgHeader) {
                return ushort.MaxValue;
            }
            return _msgHeader.m_nSeq;
        }
    }
}