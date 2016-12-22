using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using ProtoBuf;

using ZR_MasterTools;
using SLMS;


namespace Sgame
{
    public class ProtobufDecode : IMessageHandler
    {
        // 需要更新本地数据版本号的协议列表
        HashSet<CmdID> _synDataCmdSet = new HashSet<CmdID>() {

        };

        void IMessageHandler.HandleMessage(object messageKey, MemoryStream data) {
            int key = (int)messageKey;
            CmdID messageID = (CmdID)key;

            // 是否需要更新版本号
            if (_synDataCmdSet.Contains(messageID)) {
                AuthMgr.Instance.UpdateDataVersion();
            }

            switch (messageID) {
                // 99 CMD_CheckLogin 登录校验.client - loginserver
                case CmdID.CMD_CheckLogin: {
                        scCheckLogin attr = Serializer.Deserialize<scCheckLogin>(data);
                        NetworkMgr.Instance.Excute(key, attr);
                        break;
                    }
                // 100 CMD_Login2Game 客户端连接上GameServer需要发送的第一个包
                case CmdID.CMD_Login2Game: {
                        scLogin2Game attr = Serializer.Deserialize<scLogin2Game>(data);
                        NetworkMgr.Instance.Excute(key, attr);
                        break;
                    }
                // 101 CMD_CreateRole 创建角色
                case CmdID.CMD_CreateRole: {
                        scCreateRole attr = Serializer.Deserialize<scCreateRole>(data);
                        NetworkMgr.Instance.Excute(key, attr);
                        break;
                    }
                // 102 CMD_EnterGame 进入游戏
                case CmdID.CMD_EnterGame: {
                        scEnterGame attr = Serializer.Deserialize<scEnterGame>(data);
                        NetworkMgr.Instance.Excute(key, attr);
                        break;
                    }
            }
        }
    }
}