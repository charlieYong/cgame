using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using SLMS;
using Sgame.UI;


namespace Sgame
{
    public class NetworkHelper
    {

        /// 获取错误码对应的错误提示信息
        public static string GetErrorInfo(ResultCode code) {
            TErrorCode info = ConfigTableErrCode.Instance.GetInfoByID((int)code);
            if (null != info) {
                return info.info;
            }
            return code.ToString();
        }

        /// 网络异常处理
        public static void HandleSocketError() {
            NetworkMgr.Instance.StopNetworkTimer(false);
            ReStartGameWithConfirmTips(ConfigTableGameText.Instance.GetText("NetworkError"));
        }

        static void ReStartGameWithConfirmTips(string tips) {
            UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(tips, (arg) => { SceneMgr.Instance.ReStartGame(); }));
        }

        /// 网络超时处理
        public static void HandleNetworkTimeout() {
            HandleBadNetwork(ConfigTableGameText.Instance.GetText("NetworkNotGood"));
        }

        static void HandleBadNetwork(string tips) {
            NetworkMgr.Instance.StopNetworkTimer(false);
            UIConfirmOption win = (UIConfirmOption)UIManager.Instance.OpenWin(
                WinID.UIConfirmOption, new WinParam_ConfirmOption(tips, BadNetworkHandler)
            );
            win.DisableMaskClick();
        }

        static void BadNetworkHandler(bool isReConnect) {
            if (isReConnect) {
                // 尝试重连
                NetworkMgr.Instance.TryReConnectAgain();
                return;
            }
            // 重启游戏
            SceneMgr.Instance.ReStartGame();
        }
    }
}