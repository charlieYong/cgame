using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	// 延迟激活对象
	public class SmDelayActive : SmBaseDelayTimer {
		// 需要激活的对象
		public GameObject ActiveGo = null;

		protected override void OnTimeHandler () {
			if (null != ActiveGo && !ActiveGo.activeSelf) {
				ActiveGo.SetActive (true);
			}
		}
	}
}
