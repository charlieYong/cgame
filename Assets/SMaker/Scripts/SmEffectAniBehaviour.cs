using UnityEngine;
using System.Collections;

namespace SMaker {
	/// SMaker 特效脚本的基类
	public class SmEffectAniBehaviour : SmEffectBehaviour {
		protected	SmTimerTool		m_Timer;
		protected	GameObject		m_OnEndAniGameObject	= null;	
		protected	bool			m_bEndAnimation			= false;
		[HideInInspector]
		public		string			m_OnEndAniFunction		= "OnEndAnimation";	

		public void SetCallBackEndAnimation (GameObject callBackGameObj) {
			m_OnEndAniGameObject	= callBackGameObj;
		}

		public void SetCallBackEndAnimation (GameObject callBackGameObj, string nameFunction) {
			m_OnEndAniGameObject	= callBackGameObj;
			m_OnEndAniFunction		= nameFunction;
		}

		public bool IsStartAnimation () {
			return (m_Timer != null && m_Timer.IsEnable());
		}

		public bool IsEndAnimation () {
			return m_bEndAnimation;
		}

		protected void InitAnimationTimer () {
			if (m_Timer == null) {
				m_Timer = new SmTimerTool ();
			}
			m_bEndAnimation = false;
			m_Timer.Start();
		}

		public virtual void ResetAnimation () {
			m_bEndAnimation = false;
			if (m_Timer != null)
				m_Timer.Reset(0);
		}

		public virtual void PauseAnimation () {
			if (m_Timer != null) {
				m_Timer.Pause();
			}
		}

		public virtual void ResumeAnimation () {
			if (m_Timer != null) {
				m_Timer.Resume();
			}
		}

		public virtual void MoveAnimation (float fElapsedTime) {
			if (m_Timer != null) {
				m_Timer.Reset(fElapsedTime);
			}
		}

		protected void OnEndAnimation () {
			m_bEndAnimation = true;

			if (m_OnEndAniGameObject != null) {
				m_OnEndAniGameObject.SendMessage (m_OnEndAniFunction, this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
