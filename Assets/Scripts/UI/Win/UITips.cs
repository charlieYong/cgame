using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using ZR_MasterTools;


namespace Sgame.UI
{
    public class UITips : UIWin
    {

        UILabel _tipsLabel;
        public UILabel Label { get { return _tipsLabel; } }
        TweenAlpha _tweenAlpha;
        TweenPosition _tweenPosition;

        static UITips _instance;
        public static UITips Instance { get { return _instance; } }


        protected override void OnInit() {
            base.OnInit();
            _instance = this;
            InitUI();
        }

        void InitUI() {
            _tweenAlpha = _transform.GetComponent<TweenAlpha>();
            _tweenPosition = _transform.GetComponent<TweenPosition>();
            _tipsLabel = _transform.Find("Text").GetComponent<UILabel>();
            EventDelegate.Set(_tweenAlpha.onFinished, OnTipsFadeOut);
        }

        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            WinParam_Text param = (WinParam_Text)paramObject;
            if (null == param || string.IsNullOrEmpty(param.text)) {
                Show(ConfigTableGameText.Instance.GetText("UnOpen"));
            }
            else {
                Show(param.text);
            }
        }

        public static void ShowTips(string content = null) {
            if (!_instance) {
                _instance = (UITips)UIManager.Instance.OpenWin(WinID.UITips);
            }
            _instance.SetBaseRenderQueueSortOrder();
            _instance.Show(null == content ? ConfigTableGameText.Instance.GetText("UnOpen") : content);
        }

        void Show(string content) {
            _tipsLabel.text = BBcodeHelper.BoldText(content);
            PlayTween();
            TweenTopWindow(_gameObject);
        }

        void PlayTween() {
            _tweenAlpha.ResetToBeginning();
            _tweenAlpha.PlayForward();
            _tweenPosition.ResetToBeginning();
            _tweenPosition.PlayForward();
        }

        void OnTipsFadeOut() {
            Close(null);
        }
    }
}