using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SLMS;
using Sgame.UI;
using ZR_MasterTools;


namespace Sgame
{
    public class UseResourceHelper
    {
        readonly static HashSet<ResType> _canBuyResType = new HashSet<ResType>() { ResType.RT_GOLD, ResType.RT_DIAMOND, ResType.RT_ENERGY };

        public static string GetResIconID(ResType resType) {
            switch (resType) {
                case ResType.RT_GOLD:
                    return "00039";
                case ResType.RT_DIAMOND:
                    return "00040";
                case ResType.RT_ENERGY:
                    return "00041";
            }
            return string.Empty;
        }

        public static int GetResNum(ResType resType) {
            Player player = PlayerMgr.Instance.player;
            switch (resType) {
                case ResType.RT_GOLD:
                    return player.goldNum;
                case ResType.RT_DIAMOND:
                    return player.diamondNum;
            }
            return 0;
        }

        public static string GetResName(ResType resType) {
            switch (resType) {
                case ResType.RT_GOLD:
                    return ConfigTableGameText.Instance.GetText("Gold");
                case ResType.RT_DIAMOND:
                    return ConfigTableGameText.Instance.GetText("Diamond");
                case ResType.RT_ENERGY:
                    return ConfigTableGameText.Instance.GetText("Energy");
            }
            return string.Empty;
        }


        public static void UseResource(WinParam_ConfirmUseResource useInfo) {
            if (useInfo.useNum > GetResNum(useInfo.resType)) {
                if (!_canBuyResType.Contains(useInfo.resType)) {
                    UITips.ShowTips(string.Format(ConfigTableGameText.Instance.GetText("ResourceNotEnough"), GetResName(useInfo.resType)));
                    return;
                }
                // 购买资源
                UIManager.Instance.OpenWin(WinID.UIConfirmOption, new WinParam_ConfirmOption(
                    string.Format(ConfigTableGameText.Instance.GetText("ConfirmBuyResource"), GetResName(useInfo.resType)),
                    (isDecide) => {
                        if (isDecide) {
                            OnConfirmBuyResource(useInfo.resType);
                        }
                    }
                ));
                return;
            }
            UIManager.Instance.OpenWin(WinID.UIConfirmUseResource, useInfo);
        }

        static void OnConfirmBuyResource(ResType resType) {
            UITips.ShowTips();
        }
    }
}