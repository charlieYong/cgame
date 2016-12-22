using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame.UI
{
    public class RotateModel : MonoBehaviour
    {
        GameObject mFingerGO;

        float mRotateAngle;
        float mRotateDirection;
        bool mIsRotate;

        Transform mTransform;
        Animation mAnimation;
        Transform mParent;

        int _pilotlID;
        public int PilotID {
            set { _pilotlID = value; }
            get { return _pilotlID; }
        }

        void Start() {
            mFingerGO = FingerGestures.Instance.gameObject;
            if (null == mFingerGO) {
                Debug.LogWarning("no FingerGestures found in scene");
                Destroy(this);
                return;
            }
            mTransform = transform;
            mAnimation = GetComponent<Animation>();
            mParent = mTransform.parent;
            mFingerGO.GetComponent<DragRecognizer>().OnGesture += OnDragHandler;
            mFingerGO.GetComponent<TapRecognizer>().OnGesture += OnTapHandler;
            mIsRotate = false;
            mRotateAngle = 0;
        }

        void OnDestroy() {
            if (null != mFingerGO) {
                mFingerGO.GetComponent<DragRecognizer>().OnGesture -= OnDragHandler;
                mFingerGO.GetComponent<TapRecognizer>().OnGesture -= OnTapHandler;
            }
        }

        void OnEnable() {
            if (null != mAnimation && !mAnimation.isPlaying && null != mAnimation["idle"]) {
                AnimationState state = mAnimation.PlayQueued("idle");
                state.wrapMode = WrapMode.Loop;
            }
        }

        bool CheckGestrue(Gesture gesture) {
            return (gameObject.activeSelf) && (gesture.Selection == mParent.gameObject);
        }

        void OnTapHandler(TapGesture gesture) {
            if (!CheckGestrue(gesture)) {
                return;
            }
            // tmp code, to be config
            string[] actions = { "attack", "attack1", "attack2", "attack3", "attack4", "win" };
            string tapClip = actions[ThreadSafeRandom.ThisThreadsRandom.Next(0, actions.Length)];
            tapClip = (null == mAnimation[tapClip] ? "attack" : tapClip);
            if (null != mAnimation[tapClip] && !mAnimation.IsPlaying(tapClip)) {
                mAnimation.Play(tapClip, PlayMode.StopAll);
                if (null != mAnimation["idle"]) {
                    AnimationState state = mAnimation.PlayQueued("idle");
                    state.wrapMode = WrapMode.Loop;
                }
            }
        }

        void OnDragHandler(DragGesture gesture) {
            if (!CheckGestrue(gesture)) {
                return;
            }
            switch (gesture.Phase) {
                case ContinuousGesturePhase.Started:
                    mIsRotate = true;
                    mRotateAngle = 0;
                    break;
                case ContinuousGesturePhase.Ended:
                    mIsRotate = false;
                    mRotateAngle = 0;
                    break;
                case ContinuousGesturePhase.Updated:
                    mRotateAngle = gesture.DeltaMove.magnitude;
                    mRotateDirection = (gesture.DeltaMove.x > 0) ? -1 : 1;
                    break;
                default:
                    return;
            }
        }

        void Update() {
            if (!mIsRotate || mRotateAngle <= 0) {
                return;
            }
            mTransform.Rotate(Vector3.up, mRotateAngle * mRotateDirection);
            mRotateAngle *= 0.9f;
            if (mRotateAngle < 0.4f) mRotateAngle = 0;
        }
    }
}