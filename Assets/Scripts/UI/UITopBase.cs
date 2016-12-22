using UnityEngine;
using System.Collections;


namespace Sgame.UI
{
    public class UITopBase : UIWin
    {
        UIMaskPanel _mask;

        protected override void OnInit() {
            base.OnInit();
        }

        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            AddMask();
        }

        protected override void OnStart() {
            base.OnStart();
            // 设置mask的sortingOrder，避免被前面一些设置过sortingOrder的窗口挡到
            _mask.GetComponent<UIPanel>().sortingOrder = Mathf.Max(_panel.sortingOrder - 1, 0);
        }

        protected void ShowTween() {
            TweenTopWindow(gameObject);
        }

        void AddMask() {
            _mask = UIMaskPanel.Create(gameObject.GetComponent<UIPanel>().depth, _transform.localPosition.z + 50);    // 加50处理一些碰撞体层级问题
            _mask.OnMaskClickDelegate += OnMaskClick;
        }

        protected void OnMaskClick() {
            _mask.OnMaskClickDelegate -= OnMaskClick;
            Close(null);
        }

        protected override void OnClose() {
            base.OnClose();
            if (null != _mask) {
                Destroy(_mask.gameObject);
            }
        }

        /// 取消点击框体关闭对象的设定
        public void DisableMaskClick() {
            if (null != _mask) {
                _mask.OnMaskClickDelegate -= OnMaskClick;
            }
        }
    }
}