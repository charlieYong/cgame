using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame.UI
{
    public delegate void ConfirmOptionHandler(bool isDecide);

    public class WinParam_ConfirmOption : WinParamBase
    {
        public string confirmTips;
        public ConfirmOptionHandler optionHandler;

        public WinParam_ConfirmOption(string tips, ConfirmOptionHandler confirmHandler) {
            confirmTips = tips;
            optionHandler = confirmHandler;
        }
    }


    public class UIConfirmOption : UITopBase
    {

        UILabel _contentLabel;

        ConfirmOptionHandler _optionHandler;


        protected override void OnInit() {
            base.OnInit();
            InitUI();
        }

        void InitUI() {
            for (int i = 0; i < _transform.childCount; i++) {
                Transform child = _transform.GetChild(i);
                switch (child.name) {
                    case "Content":
                        _contentLabel = child.GetComponent<UILabel>();
                        break;
                    case "Confirm":
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, () => {
                            _optionHandler(true);
                            Close(null);
                        });
                        break;
                    case "Cancel":
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, () => {
                            _optionHandler(false);
                            Close(null);
                        });
                        break;
                }
            }
        }

        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            if (null == paramObject) {
                Debug.LogError("Empty winParam to open UIConfirmOption");
                return;
            }
            WinParam_ConfirmOption param = (WinParam_ConfirmOption)paramObject;
            _contentLabel.text = BBcodeHelper.BoldText(param.confirmTips);
            _optionHandler = param.optionHandler;
        }
    }
}