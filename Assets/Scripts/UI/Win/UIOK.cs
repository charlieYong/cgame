using UnityEngine;
using System.Collections;


namespace Sgame.UI
{
    public class UIOK : UITopBase
    {

        UILabel _contentLabel;

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
                    case "Close":
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, () => { Close(null); });
                        break;
                }
            }
        }


        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            if (null == paramObject) {
                Debug.LogError("The param to open UIOK is null!");
                return;
            }
            WinParam_Text param = (WinParam_Text)paramObject;
            _contentLabel.text = param.text;
        }
    }
}