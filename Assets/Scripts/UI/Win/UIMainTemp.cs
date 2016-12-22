using UnityEngine;
using System.Collections;


namespace Sgame.UI
{
    public class UIMainTemp : UIWin
    {

        protected override void OnInit() {
            base.OnInit();
            InitUI();
        }

        void InitUI() {
            EventDelegate.Set(_transform.Find("Restart").GetComponent<UIButton>().onClick, SceneMgr.Instance.ReStartGame);

            EventDelegate.Set(_transform.Find("ResetUUID").GetComponent<UIButton>().onClick, FakeSDK.ResetUUID);

            EventDelegate.Set(_transform.Find("Pilot").GetComponent<UIButton>().onClick, () => {
                UIManager.Instance.OpenWin(WinID.UIPilot);
            });

            UIManager.Instance.ShowTopBar();
        }
    }
}