using UnityEngine;
using System.Collections;

using SLMS;
using ZR_MasterTools;


namespace Sgame.UI
{
    public class WinParam_ConfirmUseResource : WinParamBase
    {
        public ResType resType;
        public int useNum;
        public string confirmTips;
        public EventDelegate.Callback onConfirmCallback;

        public WinParam_ConfirmUseResource(ResType type, int num, string confirmText, EventDelegate.Callback confirmCallback) {
            resType = type;
            useNum = num;
            confirmTips = confirmText;
            onConfirmCallback = confirmCallback;
        }
    }


    public class UIConfirmUseResource : UITopBase
    {
        UISprite _resIconSprite;
        UILabel _resNumLabel;
        UILabel _optionTipsLabel;

        WinParam_ConfirmUseResource _useInfo;
        EventDelegate.Callback _onConfirmCallback;


        protected override void OnInit() {
            base.OnInit();
            InitUI();
        }

        void InitUI() {
            for (int i = 0; i < _transform.childCount; i++) {
                Transform child = _transform.GetChild(i);
                switch (child.name) {
                    case "ResIcon":
                        _resIconSprite = child.GetComponent<UISprite>();
                        break;
                    case "ResNum":
                        _resNumLabel = child.GetComponent<UILabel>();
                        break;
                    case "Tips":
                        _optionTipsLabel = child.GetComponent<UILabel>();
                        break;
                    case "Confirm":
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, OnConfirmOption);
                        break;
                    case "Cancel":
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, () => { Close(null); });
                        break;
                }
            }
        }


        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            if (null == paramObject) {
                Debug.LogError("Empty param to open UIConfirmUseResource");
                return;
            }
            _useInfo = (WinParam_ConfirmUseResource)paramObject;
            SetInfo();
        }

        void SetInfo() {
            _resIconSprite.spriteName = UseResourceHelper.GetResIconID(_useInfo.resType);
            _resIconSprite.MakePixelPerfect();
            _resNumLabel.text = _useInfo.useNum.ToString();
            _optionTipsLabel.text = _useInfo.confirmTips;
            _onConfirmCallback = _useInfo.onConfirmCallback;
        }

        void OnConfirmOption() {
            if (null == _onConfirmCallback) {
                Debug.LogError("_onConfirmCallback == null");
                return;
            }
            _onConfirmCallback();
        }
    }
}