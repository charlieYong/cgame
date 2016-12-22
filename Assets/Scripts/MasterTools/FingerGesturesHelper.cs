using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    public class FingerGesturesHelper : MonoBehaviour
    {
        static FingerGesturesHelper _instance = null;
        /// 手势控件Helper
        public static FingerGesturesHelper Instance { get { return _instance; } }

        // 常用的手势组件 
        public SwipeRecognizer swipeRecognizer { get; private set; }
        public DragRecognizer dragRecognizer { get; private set; }
        public TapRecognizer tapRecognizer { get; private set; }
        public ScreenRaycaster screenRaycaster { get; private set; }
        public LongPressRecognizer longPressRecognizer { get; private set; }
        public PinchRecognizer pinchRecognizer { get; private set; }
        // 手指事件
        public FingerDownDetector fingerDownDetector { get; private set; }
        public FingerUpDetector fingerUpDetector { get; private set; }
        public FingerHoverDetector fingerHoverDetector { get; private set; }

        OneTimeSignal _updateSignal = new OneTimeSignal();

        // 触屏信号
        OneTimeSignal _tapSignal = new OneTimeSignal();

        void Awake() {
            _instance = this;
            Init();
        }

        void Init() {
            GameObject go = gameObject;
            swipeRecognizer = go.GetComponent<SwipeRecognizer>();
            dragRecognizer = go.GetComponent<DragRecognizer>();
            tapRecognizer = go.GetComponent<TapRecognizer>();
            screenRaycaster = go.GetComponent<ScreenRaycaster>();
            longPressRecognizer = go.GetComponent<LongPressRecognizer>();
            pinchRecognizer = go.GetComponent<PinchRecognizer>();
            fingerDownDetector = go.GetComponent<FingerDownDetector>();
            fingerUpDetector = go.GetComponent<FingerUpDetector>();
            fingerHoverDetector = go.GetComponent<FingerHoverDetector>();
        }

        void OnEnable() {
            tapRecognizer.OnGesture += OnTapScreen;
        }

        void OnDisable() {
            tapRecognizer.OnGesture -= OnTapScreen;
        }

        void Update() {
            _updateSignal.Call();
        }

        void OnTapScreen(TapGesture gesture) {
            _tapSignal.Call();
        }

        /// 等待点击屏幕
        public IEnumerator WaitForTap() {
            yield return Continuation.WaitForSignal(_tapSignal);
            for (int i = 0; i < 2; i++) {
                yield return Continuation.WaitForSignal(_updateSignal);
            }
        }
    }

}