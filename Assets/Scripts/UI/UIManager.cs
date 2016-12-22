using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

using ZR_MasterTools;


namespace Sgame.UI
{
    public class UIManager : MonoBehaviour
    {
        bool isDuplicated = false;
        GameObject _gameObject;
        public static UIManager Instance { get; private set; }
        public Transform CacheTransform { get; private set; }
        public UIRoot UIRoot { get; private set; }


        /// UI界面相机
        public Camera UICamera { get; private set; }

        /// NGUI Camera组件
        public UICamera NguiCamera { get; private set; }
        int _defaultEventMask = -1;


        #region UIManager本身生命周期
        void Awake() {
            if (null != Instance) {
                isDuplicated = true;
                Debug.LogWarning("Duplicated UIManager Instance!");
                DestroyImmediate(gameObject);
                return;
            }
            Instance = this;
            _gameObject = gameObject;
            CacheTransform = transform;
            InitComponent();
        }

        void InitComponent() {
            UIRoot = NGUITools.FindInParents<UIRoot>(CacheTransform);
            if (null == UIRoot) {
                Debug.LogError("No UIRoot Found");
                return;
            }
            DontDestroyOnLoad(UIRoot.gameObject);
            UICamera = UIRoot.transform.Find("UICamera").GetComponent<Camera>();
            NguiCamera = UICamera.GetComponent<UICamera>();
            _defaultEventMask = NguiCamera.eventReceiverMask;
        }

        void OnEnable() {

        }

        void Start() {
            SetupFingerGersture();
        }

        void OnDisable() {

        }

        void OnDestroy() {
            if (!isDuplicated) {
                Instance = null;
            }
        }

        void SetupFingerGersture() {
            if (null == FingerGestures.Instance) {
                Debug.LogWarning("no finger gestures found");
                return;
            }
            ScreenRaycaster ray = FingerGestures.Instance.gameObject.GetComponent<ScreenRaycaster>();
            // UICamera First
            List<Camera> cameraList = new List<Camera>();
            GameObject[] goList = GameObject.FindGameObjectsWithTag("UICamera");
            for (int i = 0, imax = goList.Length; i < imax; i++) {
                cameraList.Add(goList[i].GetComponent<Camera>());
            }
            cameraList.Add(SceneMgr.Instance.GetMainSceneCamera());
            ray.Cameras = cameraList.ToArray();
        }

        public void Setup() {
            ScaleAnimationClip = Resources.Load<AnimationClip>("Animations/UIScale");
        }

        public void Reset() {
            _openedWinDict.Clear();
            Utils.RemoveAllChildNode(CacheTransform);
        }
        #endregion



        #region 窗口流程
        public AnimationClip ScaleAnimationClip { get; private set; }

        // 保存已打开的窗口
        Dictionary<WinID, UIWin> _openedWinDict = new Dictionary<WinID, UIWin>();
        public WinID curWinID { get; private set; }
        public UIWin OpenWin(WinID winID, WinParamBase openWinParam = null) {
            if (winID == curWinID && _openedWinDict.ContainsKey(winID)) {
                return _openedWinDict[winID];
            }
            UIWin win = LoadWin(winID);
            curWinID = winID;
            if (UILayer.BUTTOM == win.layer) {
                CloseAllOtherWins();
            }
            _openedWinDict.Add(winID, win);
            win.Open(openWinParam);
            return win;
        }

        UIWin LoadWin(WinID ID) {
            if (_openedWinDict.ContainsKey(ID)) {
                return _openedWinDict[ID];
            }
            GameObject prefab = Resources.Load<GameObject>(GlobalConfig.UIWinPath + ID);
            if (null == prefab) {
                Debug.LogError(string.Format("Resource of {0}{1} does not exist!", GlobalConfig.UIWinPath, ID));
                return null;
            }
            GameObject go = NGUITools.AddChild(_gameObject, prefab);
            go.name = prefab.name;
            UIWin win = go.GetComponent<UIWin>();
            if (null == win) {
                Debug.LogError(string.Format("UI:{0} has not UIWin component!", ID));
                return null;
            }
            win.ID = ID;
            ResetLayer(go.transform, win.layer, ID);
            return win;
        }

        // 根据层级设置对应的Z轴值
        void ResetLayer(Transform trans, UILayer layer, WinID ID) {
            float z = 0;
            switch (layer) {
                case UILayer.BUTTOM:
                    z = 800F;
                    break;
                case UILayer.MIDDLE:
                    z = 0F;
                    break;
                case UILayer.TOP:
                    z = -800F;
                    break;
            }
            trans.localPosition = new Vector3(0, 0, z);
            SetupSpecialWinZValue(trans, ID);
        }

        //readonly float _SpecialCoordZValue = -2000F;
        // 设置特殊界面的Z轴值
        void SetupSpecialWinZValue(Transform trans, WinID ID) {
            //switch (ID) {
            //    case WinID.UIUpLevel:
            //    case WinID.UIEventDoneTips:
            //        trans.localPosition = new Vector3(0F, 0F, _SpecialCoordZValue);
            //        break;
            //}
        }

        public void SyncOnClose(WinID ID) {
            _openedWinDict.Remove(ID);
        }

        /// 关闭除了当前窗口之外的其他窗口
        void CloseAllOtherWins() {
            // 不能迭代修改dictionary内容，所以先缓存起来再关闭窗口
            List<WinID> needCloseWinIDs = new List<WinID>();
            foreach (UIWin win in _openedWinDict.Values) {
                if (win.ID != curWinID) {
                    needCloseWinIDs.Add(win.ID);
                }
            }
            for (int i = 0; i < needCloseWinIDs.Count; i++) {
                _openedWinDict[needCloseWinIDs[i]].Close(null);
            }
        }

        public void CloseWin(WinID winID, object msg = null) {
            if (_openedWinDict.ContainsKey(winID)) {
                _openedWinDict[winID].Close(msg);
            }
        }

        public void CloseCurWin() {
            if (WinID.UIMain != curWinID) {
                CloseWin(curWinID);
            }
        }

        public bool IsWinOpen(WinID winID) {
            return _openedWinDict.ContainsKey(winID);
        }

        public void OpenHomeWin() {
            // temp
            OpenWin(WinID.UIMainTemp);
        }
        #endregion



        #region 顶部条
        public TopBar topBar;
        public void ShowTopBar() {
            if (null == topBar) {
                GameObject prefab = Utils.LoadUIItemPrefab("TopBar");
                GameObject go = NGUITools.AddChild(_gameObject, prefab);
                topBar = go.GetComponent<TopBar>();
            }
            topBar.CacheGameObject.SetActive(true);
        }

        public void HideTopBar() {
            if (null != topBar) {
                topBar.CacheGameObject.SetActive(false);
            }
        }
        #endregion

        public int RealScreenHeight {
            get {
                return UIRoot.activeHeight;
            }
        }

        public int RealScreenWidth {
            get {
                float a = (float)UIRoot.activeHeight / Screen.height;
                return Mathf.CeilToInt(Screen.width * a);
            }
        }


        /// 是否能进行UI交互
        public bool InteractiveEnabled { get; private set; }

        public void EnableInteractive(bool enable) {
            InteractiveEnabled = enable;
            if (null != FingerGestures.Instance) {
                FingerGestures.Instance.enabled = enable;
            }
            if (NguiCamera) {
                ChangeUIEventMask(enable ? _defaultEventMask : 0);
            }
        }

        /// 改变NGUI的UICamera点击事件的响应层
        public void ChangeUIEventMask(int newMask) {
            NguiCamera.eventReceiverMask = newMask;
        }

        /// 是否需要处理返回键（android手机）
        public bool CanHandleEscapeKey() {
            if (!InteractiveEnabled) {
                return false;
            }
            return true;
        }
    }
}
