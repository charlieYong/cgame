using UnityEngine;
using System.Collections;
using System;


namespace Sgame.UI
{
    public enum UILayer
    {
        TOP = -300,
        MIDDLE = 0,
        BUTTOM = 300,
    }

    public enum WinID
    {
        UIOK,
        UITips,
        UIConfirmOption,
        UIConfirmUseResource,

        UIMainTemp,

        UILogin,
        UICreateRole,
        UIMain,
        UIPilot,

        Max
    }

    /// <summary>
    /// 窗口打开参数空基类，需要参数的窗口自己实现子类
    /// </summary>
    public class WinParamBase {
        public Action<object> onCloseCallBack;
    }

    public class WinParam_Text : WinParamBase
    {
        public string text;
        public WinParam_Text(string showContent, Action<object> onCloseEvent = null) {
            text = showContent;
            onCloseCallBack = onCloseEvent;
        }
    }



    public class UIWin : MonoBehaviour
    {

        [HideInInspector]
        public WinID ID;
        public UILayer layer;
        protected Transform _transform;
        protected GameObject _gameObject;
        protected UIPanel _panel;

        protected virtual void OnInit() { }
        protected virtual void OnMsgInit() { }
        protected virtual void OnStart() { }
        protected virtual void OnMsgRemove() { }
        protected virtual void OnOpen(WinParamBase paramObject) { }
        protected virtual void OnClose() { }


        void Awake() {
            _gameObject = gameObject;
            _transform = transform;
            _panel = _transform.GetComponent<UIPanel>();
            OnInit();
        }

        void OnEnable() { OnMsgInit(); }

        void Start() { OnStart(); }

        void OnDisable() { OnMsgRemove(); }


        Action<object> _onCloseCallback;
        public Action<object> OnCloseCallback { get { return _onCloseCallback; } }

        public void Open(WinParamBase openWinParam = null) {
            if (null != openWinParam) {
                _onCloseCallback = openWinParam.onCloseCallBack;
            }
            OnOpen(openWinParam);
        }

        public void Close(object msg) {
            if (null != _onCloseCallback) {
                _onCloseCallback(msg);
                _onCloseCallback = null;
            }
            OnClose();
            UIManager.Instance.SyncOnClose(ID);
            Destroy(_gameObject);
        }



        protected void TweenTopWindow(GameObject go) {
            AnimationClip clip = UIManager.Instance.ScaleAnimationClip;
            if (null == clip) {
                return;
            }
            Animation anim = NGUITools.AddMissingComponent<Animation>(go);
            if (null == anim[clip.name]) {
                anim.AddClip(clip, clip.name);
                anim.clip = clip;
            }
            anim.Play();
        }


        /// <summary>
        /// 取该窗口中最大的renderQueue值，不同窗口该值大的显示在前
        /// </summary>
        public int MaxRenderQueueSortOrder {
            get {
                int renderQueueSortOrder = 0;
                foreach (UIPanel panel in _transform.GetComponentsInChildren<UIPanel>()) {
                    foreach (UIDrawCall dc in panel.drawCalls) {
                        renderQueueSortOrder = Mathf.Max(renderQueueSortOrder, dc.renderQueue);
                    }
                    renderQueueSortOrder = Mathf.Max(renderQueueSortOrder, panel.sortingOrder);
                }
                return renderQueueSortOrder;
            }
        }

        /// <summary>
        /// 设置RenderQueueSortOrder为指定值
        /// </summary>
        protected void SetBaseRenderQueueSortOrder(int sortOrder) {
            _panel.sortingOrder = sortOrder;
            foreach (UIPanel panel in _transform.GetComponentsInChildren<UIPanel>()) {
                panel.sortingOrder = sortOrder + (panel.depth - _panel.depth);
            }
        }

        /// <summary>
        /// 设置RenderQueueSortOrder在目前打开的窗口中最高值 + 1
        /// </summary>
        protected void SetBaseRenderQueueSortOrder() {
            int maxRenderQueueSortOrderInShow = 0;
            foreach (UIWin win in UIManager.Instance.GetComponentsInChildren<UIWin>()) {
                maxRenderQueueSortOrderInShow = Mathf.Max(maxRenderQueueSortOrderInShow, win.MaxRenderQueueSortOrder);
            }
            SetBaseRenderQueueSortOrder(maxRenderQueueSortOrderInShow + 1);
        }
    }
}