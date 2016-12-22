using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using SLMS;


namespace Sgame
{
    public class AuthMgr : MonoSingleton<AuthMgr>
    {

        // 连接状态
        enum LoginStatus
        {
            Init,
            CheckLogin,
            FetchPartList,
            LoginToGame,
            EnterGame,
        }
        LoginStatus _status = LoginStatus.Init;

        // 账号列表
        GameAccount _gameAccount;
        // 所有的分区信息列表
        GamePart[] _gamePartList;
        // 用户默认登录的分区（登录界面使用），由服务器下发
        int _defaultPartid = -1;
        // 用户最近登录的分区列表，由服务器下发
        int[] _recentPartidList;
        // 服务器下发的账号信息
        RoleBaseInfo[] _partitionRoleList;

        string _loginServerIP;
        int _loginServerPort;
        // 当前登录的分区
        int _partitionid;
        // 标记是否重连
        int _reConnect = 0;
        // 服务器数据版本号，前后端不一致时，表示客户端数据有问题，需要重启游戏进入
        int _serverDataVersion = 0;

        /// 是否已收到EnterGame包，用于判断是否为登录初始化内容，防止在未初始化玩家信息时调用倒计时触发函数，以及其他需要区分第一个包的情况
        bool _hasReceiveEnterGame = false;
        public bool IsEnterGameDone { get { return _hasReceiveEnterGame; } }


        void OnEnable() {
            NetworkMgr.Instance.Register((int)CmdID.CMD_CheckLogin, OnCheckLoginResult);
            NetworkMgr.Instance.Register((int)CmdID.CMD_Login2Game, OnLoginToPartitionResult);
            NetworkMgr.Instance.Register((int)CmdID.CMD_EnterGame, OnEnterGameResult);
            NetworkMgr.Instance.Register((int)CmdID.CMD_CreateRole, OnCreateRoleResult);
        }

        void OnDisable() {
            NetworkMgr.Instance.UnRegister((int)CmdID.CMD_CheckLogin, OnCheckLoginResult);
            NetworkMgr.Instance.UnRegister((int)CmdID.CMD_Login2Game, OnLoginToPartitionResult);
            NetworkMgr.Instance.UnRegister((int)CmdID.CMD_EnterGame, OnEnterGameResult);
            NetworkMgr.Instance.UnRegister((int)CmdID.CMD_CreateRole, OnCreateRoleResult);
        }

        void Start() {
            _loginServerIP = ClientLocalConfig.Instance.LoginServerIP;
            _loginServerPort = ClientLocalConfig.Instance.LoginServerPort;
            NetworkMgr.Instance.onConnectComplete += OnConnectComplete;
        }

        protected override void OnDestroy() {
            if (null != NetworkMgr.Instance) {
                NetworkMgr.Instance.onConnectComplete -= OnConnectComplete;
            }
            base.OnDestroy();
        }

        void OnConnectComplete() {
            switch (_status) {
                case LoginStatus.CheckLogin:
                    CheckLogin();
                    break;
                case LoginStatus.FetchPartList:
                    // 获取分区列表的连接
                    //SendFetchPartListMsg();
                    break;
                case LoginStatus.LoginToGame:
                case LoginStatus.EnterGame:
                    // ReConnect
                    LoginToPartition();
                    break;
                default:
                    Debug.LogWarning("Unexpected Connection? status = " + _status.ToString());
                    break;
            }
        }


        /// 登录SDK
        public void LoginToSDK() {
            SDKProxy.Instance.Login();
        }

        /// 连接游戏登录服务器，准备校验SDK登录
        public void CheckAuth() {
            _status = LoginStatus.CheckLogin;
            NetworkMgr.Instance.Connect(_loginServerIP, _loginServerPort);
        }

        /// 与游戏服务器校验SDK登录
        void CheckLogin() {
            csCheckLogin packet = new csCheckLogin();
            packet.userId = SDKProxy.Instance.SDK.UserID;
            packet.token = SDKProxy.Instance.SDK.AuthToken;
            packet.sdk = SDKProxy.Instance.SDK.ChannelID;
            packet.app = SDKProxy.Instance.SDK.AppID;

            packet.device_guid = Utils.DeviceID;
            packet.device = SDKProxy.Instance.DeviceModel;
            packet.os = SDKProxy.Instance.DeviceSystem;
            packet.os_version = SDKProxy.Instance.DeviceSysVersion;

            packet.client_version = VersionConfig.Instance.CurVersion;
            packet.current_version = (uint)VersionConfig.Instance.VersionCode;
            // FakeSDK特殊处理
            if (SDKProxy.Instance.SDK is FakeSDK) {
                packet.device_guid = SDKProxy.Instance.SDK.UserID;
            }

            Debug.Log("user id = " + packet.userId);
            Debug.Log("token = " + packet.token);
            Debug.Log("sdk = " + packet.sdk);
            Debug.Log("app = " + packet.app);
            Debug.Log("device_guid = " + packet.device_guid);
            Debug.Log("device = " + packet.device);
            Debug.Log("os = " + packet.os);
            Debug.Log("os_version = " + packet.os_version);
            Debug.Log("client_version = " + packet.client_version);
            Debug.Log("current_version = " + packet.current_version);

            NetworkMgr.Instance.SendMessage<csCheckLogin>((int)CmdID.CMD_CheckLogin, packet);
        }

        void OnCheckLoginResult(object packet) {
            scCheckLogin data = (scCheckLogin)packet;
            if (0 == data.ret) {
                _gameAccount = data.account;
                _gamePartList = data.gameParts.ToArray();
                if (data.useridSpecified && !string.IsNullOrEmpty(data.userid)) {
                    SDKProxy.Instance.SDK.SetUserID(data.userid);
                }
                _defaultPartid = data.part_id;
                _recentPartidList = data.role_part.ToArray();
            }
            else if (data.ret == (int)ResultCode.ERR_TestVersion) {
                // 灰度测试服，需要重新链接服务器下发的地址
                if (!string.IsNullOrEmpty(data.test_ip) && data.test_port > 0) {
                    _status = LoginStatus.CheckLogin;
                    _loginServerIP = data.test_ip;
                    _loginServerPort = (int)data.test_port;
                    NetworkMgr.Instance.Connect(_loginServerIP, _loginServerPort);
                    return;
                }
                else {
                    Debug.LogWarning(string.Format("invalid test_ip({0}) or test_port({1})", data.test_ip, data.test_port));
                }
            }
            TriggerEvent<int>.InvokeListener(TriggerEventType.CheckLogin_Result, data.ret);
            // checklogin之后客户端需要主动断开服务器的连接
            NetworkMgr.Instance.ReleaseSocket();
        }

        /// 连接游戏分区服务器，准备登录游戏
        public void LoginToGame(int partID) {
            _status = LoginStatus.LoginToGame;
            _partitionid = partID;
            GamePart part = GetGamePart(_partitionid);
            NetworkMgr.Instance.Connect(part.ip, part.port);
        }

        /// 登录分区
        void LoginToPartition() {
            csLogin2Game packet = new csLogin2Game();
            packet.uin = _gameAccount.uin;
            packet.reconnect = _reConnect;
            packet.sid = _gameAccount.sid;
            packet.partid = _partitionid;
            packet.client_data_version = (uint)_serverDataVersion;

            packet.device_id = Utils.DeviceIDFA;
            packet.str_uin = SDKProxy.Instance.SDK.UserID;
            packet.sdk = SDKProxy.Instance.SDK.ChannelID;
            packet.device = SDKProxy.Instance.DeviceModel;
            packet.sys = SDKProxy.Instance.DeviceSystem;
            packet.sys_version = SDKProxy.Instance.DeviceSysVersion;

            packet.client_version = (uint)VersionConfig.Instance.VersionCode;
            // -1时表示没有拉取到正确的版本号，不上传版本号
            if (BundleMgr.Instance.PatchVersionNum >= 0) {
                packet.config_version = (uint)BundleMgr.Instance.PatchVersionNum;
            }
            NetworkMgr.Instance.SendMessage<csLogin2Game>((int)CmdID.CMD_Login2Game, packet);
        }

        void OnLoginToPartitionResult(object packet) {
            scLogin2Game data = (scLogin2Game)packet;
            if (ResultCode.Success != data.ret) {
                TriggerEvent<ResultCode>.InvokeListener(TriggerEventType.LoginGame_Result, data.ret);
                return;
            }
            if (data.serverStampSpecified) {
                TimeManager.CacheTimeOffset((int)data.serverStamp);
            }
            //AppManager.Instance.IsGMEnabled = (1 == data.gmFlag);
            if (data.exchangeCodeFlagSpecified) {
                //AppManager.Instance.IsCDKeyEnabled = (1 == data.exchangeCodeFlag);
            }
            if (data.system_enable_flagSpecified) {
                //AppManager.Instance.systemOpenFlag = data.system_enable_flag;
            }
            if (1 == _reConnect) {
                // 检查前后端数据是否一致
                if ((int)data.dataVersion != _serverDataVersion) {
                    Debug.LogWarning(string.Format("数据不一致，需要重新登陆：客户端＝{0}，服务器={1}", _serverDataVersion, data.dataVersion));
                    NetworkHelper.HandleSocketError();
                    return;
                }
                // 重置数据版本号
                _serverDataVersion = 0;
                NetworkMgr.Instance.ReSendLastPacket();
                return;
            }
            // 重置数据版本号
            _serverDataVersion = 0;
            _partitionRoleList = data.roles.ToArray();
            if (!IsNewUser()) {
                EnterGame();
            }
            TriggerEvent<ResultCode>.InvokeListener(TriggerEventType.LoginGame_Result, data.ret);
        }

        /// 进入游戏
        void EnterGame() {
            csEnterGame packet = new csEnterGame();
            packet.device_id = Utils.DeviceIDFA;
            packet.uin = SDKProxy.Instance.SDK.UserID;
            packet.sdk = SDKProxy.Instance.SDK.ChannelID;
            packet.device = SDKProxy.Instance.DeviceModel;
            packet.sys = SDKProxy.Instance.DeviceSystem;
            packet.sys_version = SDKProxy.Instance.DeviceSysVersion;
            NetworkMgr.Instance.SendMessage<csEnterGame>((int)CmdID.CMD_EnterGame, packet);
        }

        void OnEnterGameResult(object packet) {
            _hasReceiveEnterGame = true;
            _reConnect = 1;
            _status = LoginStatus.EnterGame;
            TriggerEvent<ResultCode>.InvokeListener(TriggerEventType.EnterGame_Result, ResultCode.Success);
        }

        /// 创建角色
        public void CreateRole(bool isMale, string name) {
            csCreateRole packet = new csCreateRole();
            packet.sex = isMale;
            packet.name = name;
            packet.device_id = Utils.DeviceIDFA;
            packet.uin = SDKProxy.Instance.SDK.UserID;
            packet.sdk = SDKProxy.Instance.SDK.ChannelID;
            packet.device = SDKProxy.Instance.DeviceModel;
            packet.sys = SDKProxy.Instance.DeviceSystem;
            packet.sys_version = SDKProxy.Instance.DeviceSysVersion;

            Debug.Log("sex = " + packet.sex);
            Debug.Log("name = " + packet.name);
            Debug.Log("device_id = " + packet.device_id);
            Debug.Log("uin = " + packet.uin);
            Debug.Log("sdk = " + packet.sdk);
            Debug.Log("device = " + packet.device);
            Debug.Log("sys = " + packet.sys);
            Debug.Log("sys_version = " + packet.sys_version);
 
            NetworkMgr.Instance.SendMessage<csCreateRole>((int)CmdID.CMD_CreateRole, packet);
        }

        void OnCreateRoleResult(object packet) {
            scCreateRole data = (scCreateRole)packet;
            if (ResultCode.Success == data.ret) {
                _partitionRoleList = new RoleBaseInfo[] { data.newRole };
                SDKProxy.Instance.CacheCreateRoleTimestamp(data.newRole.guid, TimeManager.GetServerTimestamp());
                EnterGame();
            }
            TriggerEvent<ResultCode>.InvokeListener(TriggerEventType.CreateRole_Result, data.ret);
        }



        public GamePart GetGamePart(int partid) {
            for (int i = 0, imax = _gamePartList.Length; i < imax; i++) {
                if (_gamePartList[i].partId == partid) {
                    return _gamePartList[i];
                }
            }
            return null;
        }

        /// 更新本地维护的数据版本号 
        public void UpdateDataVersion() {
            _serverDataVersion++;
        }

        // 判断是否有可用的分区
        public bool HasUsablePartition() {
            // _defaultPartid为0时表示没有可用分区（后台协定）
            if ((null != _gamePartList) && (_gamePartList.Length > 0)) {
                return true;
            }
            return false;
        }

        public bool IsNewUser() {
            return (null == _partitionRoleList) || (0 == _partitionRoleList.Length);
        }

        public ulong GetUID() {
            return (null != _gameAccount) ? _gameAccount.uin : 0;
        }
    }
}