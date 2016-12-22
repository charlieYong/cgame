using UnityEngine;
using System.Collections;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 飞机操作交互类
    public class FighterOperator : MonoBehaviour {
        Transform _transform;

        void Awake () {
            _transform = transform;
        }

        void OnEnable () {
            FingerGestures.Instance.GetComponent<DragRecognizer> ().OnGesture += OnDragHandler;
        }

        void OnDisable () {
            if (null != FingerGestures.Instance) {
                FingerGestures.Instance.GetComponent<DragRecognizer> ().OnGesture -= OnDragHandler;
            }
        }

        void OnDragHandler(DragGesture gesture) {
            switch (gesture.Phase) {
            case ContinuousGesturePhase.Updated:
                FingerGestures.Finger finger = gesture.Fingers[0];
                Vector3 prevFingerPos, curFingerPos;
                if (Utils.ProjectScreenPointToCamPlane (BattleManager.Instance.BattleCamera, _transform.position, finger.PreviousPosition, out prevFingerPos) &&
                    Utils.ProjectScreenPointToCamPlane (BattleManager.Instance.BattleCamera, _transform.position, gesture.Position, out curFingerPos)) {
                    _transform.localPosition += (curFingerPos - prevFingerPos);
                }
                break;
            default:
                return;
            }
        }
    }
}