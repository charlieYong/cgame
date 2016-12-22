using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    public class BaseTapHandler : MonoBehaviour
    {
        protected Transform mTransform;
        protected GameObject mGameObject;
        GameObject _mFingerGO;

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnGameObjectDestroy() { }
        protected virtual void OnTapHandler(Gesture gesture) { }

        void Awake() {
            mTransform = transform;
            mGameObject = gameObject;
            OnAwake();
        }

        void Start() {
            if (null == FingerGestures.Instance) {
                Debug.LogWarning("no FingerGestures found in scene");
                return;
            }
            if (null == _mFingerGO) {
                _mFingerGO = FingerGestures.Instance.gameObject;
            }
            _mFingerGO.GetComponent<TapRecognizer>().OnGesture += OnTapHandler;
            OnStart();
        }

        void OnDestroy() {
            if (null != _mFingerGO) {
                _mFingerGO.GetComponent<TapRecognizer>().OnGesture -= OnTapHandler;
            }
            OnGameObjectDestroy();
        }
    }
}