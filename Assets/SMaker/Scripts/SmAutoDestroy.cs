using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	public class SmAutoDestroy : SmBaseDelayTimer {
		protected override void OnTimeHandler () {
			Destroy (gameObject);
		}
	}
}
