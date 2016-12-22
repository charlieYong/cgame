using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	// 延迟类型
	public enum SmDelayTimerType {
		Once,  // 单次
		Loop,  // 循环
		Max
	}
	// 延时类操作的基类
	public class SmBaseDelayTimer : MonoBehaviour {
		// 延迟时间
		public float DelayDuration = 0f;
		// 类型
		public SmDelayTimerType DelayType = SmDelayTimerType.Once;
		// 当前时间
		float _duration = 0f;

		// 子类初始化
		protected virtual void OnTimerInit () {}
		// 子类实现的逻辑，延时达到时触发
		protected virtual void OnTimeHandler () {}

		// 标识timer已经结束
		protected void SetTimerFinish () {
			_duration = Mathf.NegativeInfinity;
		}

		// 判断timer是否已经结束
		protected bool IsTimerFinish () {
			if ((DelayType == SmDelayTimerType.Once) && (Mathf.NegativeInfinity == _duration)) {
				return true;
			}
			return false;
		}

		void Awake () {
			_duration = 0f;
			OnTimerInit ();
		}

		void Update () {
			if (IsTimerFinish ()) {
				return;
			}
			_duration += SmEffectBehaviour.GetEngineDeltaTime ();
			if (_duration < DelayDuration) {
				return;
			}
			OnTimeHandler ();
			if (SmDelayTimerType.Once == DelayType) {
				SetTimerFinish ();
			}
			else if (SmDelayTimerType.Loop == DelayType) {
				_duration = 0f;
			}
		}
	}
}
