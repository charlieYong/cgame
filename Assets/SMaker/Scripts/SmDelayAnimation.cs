using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	public class SmDelayAnimation : SmBaseDelayTimer {
		// 需要播放的动作名
		public string ClipName = string.Empty;
		// 播放模式（循环等）
		public WrapMode PlayMode = WrapMode.Default;

		Animation _animation = null;
		AnimationState _state = null;

		protected override void OnTimerInit () {
			_animation = GetComponent<Animation>();
			if (null == _animation) {
				// 没有动作组件
				return;
			}
			_state = _animation[ClipName];
			if (null == _state) {
				// 找不到动作
				return;
			}
		}

		protected override void OnTimeHandler () {
			if (null == _state) {
				return;
			}
			_state.wrapMode = PlayMode;
			_animation.Play (ClipName);
		}
	}
}
