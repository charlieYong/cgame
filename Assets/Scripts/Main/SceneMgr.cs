using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

using Sgame.UI;
using ZR_MasterTools;


namespace Sgame
{
    public class SceneMgr : MonoBehaviour
    {

        /// 启动场景（登录界面）
        public static readonly string StartSceneName = "GameStart";
        /// 主场景
        public static readonly string MainSceneName = "Main";
        /// 清理场景（空场景，负责清理所有常驻对象和静态对象、数据等）
        public static readonly string CleanSceneName = "Clean";

        Transform _transform;

        string _nextSceneName = string.Empty;
        AsyncOperation _asyncOperation = null;
        Action _onLoadedAction;

        LoadingBase _loadingItem;
        Fader _fader;
        Connecting _connectingItem;

        static SceneMgr _instance;
        public static SceneMgr Instance { get { return _instance; } }


        void Awake() {
            if (null != Instance) {
                Debug.LogError("Double SceneManager awake, destroy the new one");
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            _transform = transform;
            Initialize();
        }

        void Initialize() {
            _nextSceneName = string.Empty;
            _fader = _transform.Find("Fader").GetComponent<Fader>();
            _connectingItem = _transform.Find("Connecting").GetComponent<Connecting>();
        }

        void Start() {
            _transform.localPosition = new Vector3(0, 0, -600F);
        }


        public void ChangeScene(string name, Action act = null) {
            if (name == SceneManager.GetActiveScene().name) {
                if (null != act) {
                    act();
                }
                return;
            }
            if (null != _asyncOperation && !_asyncOperation.isDone) {
                Debug.LogError(string.Format("Fail to load new scene({0}): {1} is loading", name, _nextSceneName));
                return;
            }
            _nextSceneName = name;
            _onLoadedAction = act;
            Fade(Fader.FaderType.FadeOut, null, LoadNextScene);
        }

        public void GoToMainScene(Action onFinish = null) {
            ChangeScene(MainSceneName, (null == onFinish ? UIManager.Instance.OpenHomeWin : onFinish));
        }

        public void GoToStartScene() {
            SceneManager.LoadScene(StartSceneName);
        }

        public void ReStartGame() {
            // 屏蔽相机（有的手机在清理场景时会闪屏）
            if (null != Sgame.UI.UIManager.Instance.UICamera) {
                Sgame.UI.UIManager.Instance.UICamera.enabled = false;
            }
            Camera mainCamera = GetMainSceneCamera();
            if (null != mainCamera) {
                mainCamera.enabled = false;
            }
            SceneManager.LoadScene(CleanSceneName);
        }

        // 被Destroy的对象/组件也会被判断为null，因此在切换场景被销毁后，访问时会重新赋值这两个相机
        GameObject _mainCameraGO = null;
        /// 主场景相机对象
        public GameObject MainCameraGO {
            get {
                if (null == _mainCameraGO) {
                    _mainCameraGO = GameObject.Find("/MainScene/MainCamera");
                    if (null == _mainCameraGO) {
                        Debug.LogWarning("找不到对象：/MainScene/MainCamera，当前场景：" + SceneManager.GetActiveScene().name);
                    }
                }
                return _mainCameraGO;
            }
        }

        Camera _mainCamera = null;
        /// 获取主场景相机
        public Camera GetMainSceneCamera() {
            if (null == MainCameraGO) {
                return null;
            }
            if (null == _mainCamera) {
                _mainCamera = MainCameraGO.GetComponent<Camera>();
            }
            return _mainCamera;
        }




        void Fade(Fader.FaderType type, Action onStart, Action onFinish, float duration = 0.7F) {
            _fader.Fade(type, onStart, onFinish, duration);
        }

        void LoadNextScene() {
            if (null != UIManager.Instance) {
                UIManager.Instance.Reset();
            }
            StartCoroutine(LoadNextSceneAsync());
        }

        IEnumerator LoadNextSceneAsync() {
            UIEffectManager.Instance.EnableCamera(false);
            // AppManager.instance.EnableBgAudio (false);
            BundleMgr.Instance.ClearCache();
            ShowLoading();
            AssetBundle ab = BundleMgr.Instance.LoadSceneResources(_nextSceneName);
            _asyncOperation = SceneManager.LoadSceneAsync(_nextSceneName);

            int showProgress = 0;
            int realProgress = 0;
            _asyncOperation.allowSceneActivation = false;
            // NOTE: 
            // when set *allowSceneActivation* to false, unity would just load 90% of the Scene
            // the left 10% will auto loaded when *allowSceneActivation* set to true
            while (_asyncOperation.progress < 0.9F) {
                realProgress = (int)(_asyncOperation.progress * 100);
                float factor = 1F;
                while (showProgress < realProgress) {
                    showProgress = Mathf.Min(realProgress, showProgress + (int)factor);
                    UpdateLoadingProcess(showProgress);
                    factor *= 1.1F;
                    yield return null;
                }
                yield return null;
            }
            // make it 100%
            realProgress = 100;
            while (showProgress < realProgress) {
                showProgress = Math.Min(realProgress, showProgress + 2);
                UpdateLoadingProcess(showProgress);
                yield return null;
            }
            // 这里需要优化下，因为设置_asyncOpp.allowSceneActivation = true后不一定能马上加载新场景，大部分情况下需要等一段时间
            // 这样界面上已经显示100%，会让人觉得卡在100%的加载界面
            _asyncOperation.allowSceneActivation = true;
            yield return _asyncOperation;
            HideLoading();
            _asyncOperation = null;
            UIEffectManager.Instance.EnableCamera(true);
            _nextSceneName = string.Empty;
            if (null != ab) {
                ab.Unload(false);
                ab = null;
            }
            Resources.UnloadUnusedAssets();
            LevelLoaded();
        }

        void ShowLoading() {
            GameObject loadingPrefab = Utils.LoadUIItemPrefab("Loading");
            _loadingItem = NGUITools.AddChild(gameObject, loadingPrefab).GetComponent<LoadingBase>();
            _loadingItem.Show();
        }

        void UpdateLoadingProcess(int percent) {
            _loadingItem.UpdateLoadingProgress(percent);
        }

        void HideLoading() {
            NGUITools.Destroy(_loadingItem.gameObject);
            _loadingItem = null;
        }

        public void ShowConnecting(bool show) {
            _connectingItem.Show(show);
        }

        void LevelLoaded() {
            if (null != _onLoadedAction) {
                _onLoadedAction();
                _onLoadedAction = null;
            }
        }
    }
}