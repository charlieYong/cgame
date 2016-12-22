using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	public class SmRotation : SmEffectBehaviour {
		public bool m_bWorldSpace = false;
		public Vector3 m_vRotationValue	= new Vector3 (0, 360, 0);

		Transform _transform;

		void Start () {
			_transform = transform;
		}

		void Update () {
			float deltaTime = GetEngineDeltaTime ();
			_transform.Rotate (
				deltaTime * m_vRotationValue.x,
				deltaTime * m_vRotationValue.y,
				deltaTime * m_vRotationValue.z,
				(m_bWorldSpace ? Space.World : Space.Self)
			);
		}

		public override void OnUpdateEffectSpeed (float fSpeedRate, bool bRuntime) {
			m_vRotationValue *= fSpeedRate;
		}
	}
}
