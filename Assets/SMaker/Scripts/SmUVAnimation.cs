using UnityEngine;
using System.Collections;

namespace SMaker {
	public class SmUVAnimation : SmEffectAniBehaviour {
		public		float		m_fScrollSpeedX			= 1.0f;
		public		float		m_fScrollSpeedY			= 0.0f;

		public		float		m_fTilingX				= 1;
		public		float		m_fTilingY				= 1;

		public		float		m_fOffsetX				= 0;
		public		float		m_fOffsetY				= 0;

		public		bool		m_bUseSmoothDeltaTime	= false;
		public		bool		m_bFixedTileSize		= false;
		public		bool		m_bRepeat				= true;
		public		bool		m_bAutoDestruct			= false;

		protected	Vector3		m_OriginalScale			= new Vector3();
		protected	Vector2		m_OriginalTiling		= new Vector2();
		protected	Vector2		m_EndOffset				= new Vector2();
		protected	Vector2		m_RepeatOffset			= new Vector2();
		protected	Renderer	m_Renderer;

		Transform _transform;

		public void SetFixedTileSize(bool bFixedTileSize)
		{
			m_bFixedTileSize = bFixedTileSize;
		}

		public override int GetAnimationState()
		{
			int re;
			if (m_bRepeat == false)
			{
				if (enabled && IsActive(gameObject) && IsEndAnimation() == false)
					re = 1;
				re = 0;
			}
			re = -1;
			return re;
		}

		public override void ResetAnimation()
		{
			if (enabled == false)
				enabled = true;
			Start();
		}

		void Start()
		{
			m_Renderer = GetComponent<Renderer>();
			_transform = transform;
			if (m_Renderer == null || m_Renderer.sharedMaterial == null || m_Renderer.sharedMaterial.mainTexture == null)
			{
				enabled = false;
			} else {
				GetComponent<Renderer>().material.mainTextureScale	= new Vector2(m_fTilingX, m_fTilingY);
				AddRuntimeMaterial(GetComponent<Renderer>().material);

				// 0~1 value
				float offset;
				offset = m_fOffsetX + m_fTilingX;
				m_RepeatOffset.x	= offset - (int)(offset);
				if (m_RepeatOffset.x < 0)
					m_RepeatOffset.x += 1;
				offset = m_fOffsetY + m_fTilingY;
				m_RepeatOffset.y	= offset - (int)(offset);
				if (m_RepeatOffset.y < 0)
					m_RepeatOffset.y += 1;
				m_EndOffset.x = 1 - (m_fTilingX - (int)(m_fTilingX) + ((m_fTilingX - (int)(m_fTilingX)) < 0 ? 1:0));
				m_EndOffset.y = 1 - (m_fTilingY - (int)(m_fTilingY) + ((m_fTilingY - (int)(m_fTilingY)) < 0 ? 1:0));

				InitAnimationTimer();
			}
		}

		void Update()
		{
			if (m_Renderer == null || m_Renderer.sharedMaterial == null || m_Renderer.sharedMaterial.mainTexture == null)
				return;

			if (m_bFixedTileSize)
			{
				if (m_fScrollSpeedX != 0 && m_OriginalScale.x != 0)
					m_fTilingX = m_OriginalTiling.x * (_transform.lossyScale.x / m_OriginalScale.x);
				if (m_fScrollSpeedY != 0 && m_OriginalScale.y != 0)
					m_fTilingY = m_OriginalTiling.y * (_transform.lossyScale.y / m_OriginalScale.y);
				GetComponent<Renderer>().material.mainTextureScale	= new Vector2(m_fTilingX, m_fTilingY);
			}

			if (m_bUseSmoothDeltaTime)
			{
				m_fOffsetX += m_Timer.GetSmoothDeltaTime() * m_fScrollSpeedX;
				m_fOffsetY += m_Timer.GetSmoothDeltaTime() * m_fScrollSpeedY;
			} else {
				m_fOffsetX += m_Timer.GetDeltaTime() * m_fScrollSpeedX;
				m_fOffsetY += m_Timer.GetDeltaTime() * m_fScrollSpeedY;
			}

			bool bCallEndAni = false;
			if (m_bRepeat == false)
			{
				m_RepeatOffset.x	+= m_Timer.GetDeltaTime() * m_fScrollSpeedX;
				if (m_RepeatOffset.x < 0 || 1 < m_RepeatOffset.x)
				{
					m_fOffsetX	= m_EndOffset.x;
					enabled		= false;
					bCallEndAni	= true;
				}
				m_RepeatOffset.y += m_Timer.GetDeltaTime() * m_fScrollSpeedY;
				if (m_RepeatOffset.y < 0 || 1 < m_RepeatOffset.y)
				{
					m_fOffsetY	= m_EndOffset.y;
					enabled		= false;
					bCallEndAni	= true;
				}
			}
			m_Renderer.material.mainTextureOffset	= new Vector2(m_fOffsetX - ((int)m_fOffsetX), m_fOffsetY - ((int)m_fOffsetY));
			if (bCallEndAni)
			{
				OnEndAnimation();
				if (m_bAutoDestruct)
				{
					if (m_bReplayState)
						SetActiveRecursively(gameObject, false);
					else DestroyObject(gameObject);
				}
			}
		}

		public override void OnUpdateEffectSpeed(float fSpeedRate, bool bRuntime)
		{
			m_fScrollSpeedX		*= fSpeedRate;
			m_fScrollSpeedY		*= fSpeedRate;
		}

		public override void OnUpdateToolData()
		{
			m_OriginalScale		= _transform.lossyScale;
			m_OriginalTiling.x	= m_fTilingX;
			m_OriginalTiling.y	= m_fTilingY;
		}

		public override void OnResetReplayStage(bool bClearOldParticle)
		{
			base.OnResetReplayStage(bClearOldParticle);
			ResetAnimation();
		}
	}
}
