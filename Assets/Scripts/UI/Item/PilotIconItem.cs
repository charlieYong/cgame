using UnityEngine;
using System.Collections;
using System;

using ZR_MasterTools;


namespace Sgame.UI
{
    public class PilotIconItem : MonoBehaviour
    {
        Transform _transform;
        UITexture _iconTexture;
        UIToggle _toggle;

        Pilot _pilot;
        Action<Pilot> _onSelect;
        

        void Awake() {
            InitUI();
        }

        void InitUI() {
            _transform = transform;
            _toggle = _transform.GetComponent<UIToggle>();
            EventDelegate.Set(_toggle.onChange, OnToggleChange);
            _iconTexture = _transform.Find("Icon").GetComponent<UITexture>();
        }

        void OnToggleChange() {
            if (!UIToggle.current.value) {
                return;
            }
            if (null != _onSelect) {
                _onSelect(_pilot);
            }
        }


        public void SetItem(Pilot pilot, Action<Pilot> onSelect) {
            _pilot = pilot;
            _onSelect = onSelect;
            _iconTexture.mainTexture = BundleMgr.Instance.LoadResource<Texture>(GlobalConfig.PilotIconTexturePath + _pilot.Config.iconID);
            _iconTexture.color = (PilotMgr.Instance.HasOwnPilot(_pilot.id) ? Color.white : Color.grey);
        }

        public void SetSelected() {
            _toggle.value = true;
        }
    }
}