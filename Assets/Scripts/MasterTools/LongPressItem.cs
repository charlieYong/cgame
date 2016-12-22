using UnityEngine;
using System.Collections;



namespace ZR_MasterTools
{
    public class LongPressItem : MonoBehaviour
    {
        protected Transform _transform;
        protected GameObject _gameObject;
        private LongPressRecognizer _longPress;

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        // implement these two functions to finish the job
        protected virtual void OnLongPress(LongPressGesture gesture) { }
        protected virtual void OnLongPressEnd(LongPressGesture gesture) { }
        protected virtual void OnGameObjectDestroy() { }

        private void Awake() {
            _transform = transform;
            _gameObject = gameObject;
            OnAwake();
        }

        // Use this for initialization
        private void Start() {
            if (null == FingerGestures.Instance) {
                Debug.LogWarning("no finger gestures instance found");
                return;
            }
            _longPress = FingerGestures.Instance.GetComponent<LongPressRecognizer>();
            if (null == _longPress) {
                Debug.LogWarning("no LongPressRecognizer found in the FingerGestures gameobject");
                return;
            }
            _longPress.OnGesture += LongPressHandler;
            OnStart();
        }

        private void OnDestroy() {
            if (null != _longPress) {
                _longPress.OnGesture -= LongPressHandler;
            }
            OnGameObjectDestroy();
        }

        private void LongPressHandler(LongPressGesture gesture) {
            if (_gameObject != gesture.StartSelection) {
                return;
            }
            gesture.OnStateChanged += OnGestureStatusChanged;
            OnLongPress(gesture);
        }

        private void OnGestureStatusChanged(Gesture gesture) {
            if (GestureRecognitionState.Ended == gesture.PreviousState &&
                GestureRecognitionState.Ready == gesture.State) {
                // long press gone
                gesture.OnStateChanged -= OnGestureStatusChanged;
                OnLongPressEnd((LongPressGesture)gesture);
            }
        }
    }
}