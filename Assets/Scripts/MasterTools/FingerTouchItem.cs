using UnityEngine;
using System.Collections;



namespace ZR_MasterTools
{
    public class FingerTouchItem : MonoBehaviour
    {

        protected Transform _transform;
        protected GameObject _gameobject;
        FingerDownDetector _fingerDown;
        FingerUpDetector _fingerUp;

        bool _hasDown = false;
        public bool HasDown { get { return _hasDown; } }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnGameObjectDestroy() { }

        protected virtual void OnFingerDown(FingerDownEvent e) { }
        protected virtual void OnFingerUp(FingerUpEvent e) { }

        GameObject _colliderGO;
        protected void CacheColliderGameObject(GameObject colliderGO) {
            _colliderGO = colliderGO;
        }


        void Awake() {
            _transform = transform;
            _gameobject = gameObject;
            OnAwake();
        }

        void Start() {
            if (null == FingerGestures.Instance) {
                Debug.LogWarning("no finger gestures instance found");
                return;
            }
            _fingerDown = FingerGestures.Instance.GetComponent<FingerDownDetector>();
            _fingerUp = FingerGestures.Instance.GetComponent<FingerUpDetector>();
            if (null == _fingerDown || null == _fingerUp) {
                Debug.LogWarning("no Finger Detector found in the FingerGestures gameobject");
                return;
            }
            _fingerDown.OnFingerDown += FingerDownHandler;
            _fingerUp.OnFingerUp += FingerUpHandler;
            OnStart();
        }

        void OnDestroy() {
            if (null != _fingerUp) {
                _fingerUp.OnFingerUp -= FingerUpHandler;
            }
            if (null != _fingerDown) {
                _fingerDown.OnFingerDown -= FingerDownHandler;
            }
            OnGameObjectDestroy();
        }


        void FingerDownHandler(FingerDownEvent e) {
            //Debug.Log("selecion :" + e.Selection + "  collider : " + _colliderGO);
            if (!IsValidTouch(e)) {
                return;
            }
            _hasDown = true;
            OnFingerDown(e);
        }

        bool IsValidTouch(FingerDownEvent e) {
            return ((null == _colliderGO) ? e.Selection == _gameobject : e.Selection == _colliderGO);
        }

        void FingerUpHandler(FingerUpEvent e) {
            if (!_hasDown) {
                return;
            }
            _hasDown = false;
            //Debug.Log("FingerUpHandler");
            OnFingerUp(e);
        }
    }
}