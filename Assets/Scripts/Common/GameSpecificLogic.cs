using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using SLMS;
using Sgame.UI;


namespace Sgame
{
    public class GameSpecificLogic : GameSpecificLogicBase
    {

        public override bool NeedEncodePacket(int packageKey, out string encodeKey) {
            CmdID cmdID = (CmdID)packageKey;
            switch (cmdID) {
                // 登陆协议
                case CmdID.CMD_CheckLogin:
                case CmdID.CMD_PartList:
                    encodeKey = "e69687e5ad90e5869cb4e69687e58d";
                    return true;
                // 游戏协议
                default:
                    encodeKey = AuthMgr.Instance.GetUID().ToString();
                    return true;
            }
        }

        public override void EncodePacketBody(byte[] data, int len, string key) {
            base.EncodePacketBody(data, len, key);
        }

        public override ulong GetProtoHeaderUin() {
            return AuthMgr.Instance.GetUID();
        }

        public override string GetPacketKeyText(int key) {
            return ((CmdID)key).ToString();
        }

        public override bool IsPacketNeedDoubleTimeout(int packetKey) {
            return false;
        }

        public override bool IsLoginToGamePacket(int packetKey) {
            return (CmdID.CMD_Login2Game == (CmdID)packetKey);
        }


        public override void ShowConnecting(bool show) {
            SceneMgr.Instance.ShowConnecting(show);
        }

        public override void EnableInteractive(bool enable) {
            Sgame.UI.UIManager.Instance.EnableInteractive(enable);
        }
    }
}