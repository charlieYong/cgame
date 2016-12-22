using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	public class SmCurveAnimation : SmEffectAniBehaviour {
		class SmComparerCurve : IComparer<SmInfoCurve>
		{
			static protected float	m_fEqualRange	= 0.03f;
			static protected float	m_fHDiv			= 5.0f;

			public int Compare(SmInfoCurve a, SmInfoCurve b)
			{
				float val = a.m_AniCurve.Evaluate(m_fEqualRange/m_fHDiv) - b.m_AniCurve.Evaluate(m_fEqualRange/m_fHDiv);
				// Equal
				if (Mathf.Abs(val) < m_fEqualRange)
				{
					val = b.m_AniCurve.Evaluate(1-m_fEqualRange/m_fHDiv) - a.m_AniCurve.Evaluate(1-m_fEqualRange/m_fHDiv);
					if (Mathf.Abs(val) < m_fEqualRange)
						return 0;
				}
				return (int)(val * 1000);
			}

			static public int GetSortGroup(SmInfoCurve info)
			{
				float val = info.m_AniCurve.Evaluate(m_fEqualRange/m_fHDiv);
				if (val < -m_fEqualRange) return 1;		// low
				if (m_fEqualRange < val) return 3;		// high
				return 2;								// middle
			}
		}

		[System.Serializable]
		public class SmInfoCurve
		{
			protected	const float		m_fOverDraw			= 0.2f;
			// edit
			public		bool			m_bEnabled			= true;
			public		string			m_CurveName			= "";
			public		AnimationCurve	m_AniCurve			= new AnimationCurve();
			public		enum APPLY_TYPE						{ NONE, POSITION, ROTATION, SCALE, MATERIAL_COLOR, TEXTUREUV, MESH_COLOR };
			public		static string[]	m_TypeName			= { "None", "Position", "Rotation", "Scale", "MaterialColor", "TextureUV", "MeshColor" };
			public		APPLY_TYPE		m_ApplyType			= APPLY_TYPE.POSITION;
			public		bool[]			m_bApplyOption		= new bool[4] { false, false, false, true };	// w, true=World, false=local
			public		bool			m_bRecursively		= false;			// olny color
			public		float			m_fValueScale		= 1.0f;
			public		Vector4			m_FromColor			= Color.white;
			public		Vector4			m_ToColor			= Color.white;

			// internal
			public		int				m_nTag				= 0;
			public		int				m_nSortGroup;
			public		Vector4			m_OriginalValue;
			public		Vector4			m_BeforeValue;
			public		Vector4[]		m_ChildOriginalColorValues;
			public		Vector4[]		m_ChildBeforeColorValues;

			public bool IsEnabled()
			{
				return m_bEnabled;
			}

			public void SetEnabled(bool bEnable)
			{
				m_bEnabled = bEnable;
			}

			public string GetCurveName()
			{
				return m_CurveName;
			}

			public SmInfoCurve GetClone()
			{
				SmInfoCurve	newInfo = new SmInfoCurve();
				newInfo.m_AniCurve				= new AnimationCurve(m_AniCurve.keys);
				newInfo.m_AniCurve.postWrapMode	= m_AniCurve.postWrapMode;
				newInfo.m_AniCurve.preWrapMode	= m_AniCurve.preWrapMode;

				newInfo.m_bEnabled		= m_bEnabled;
				newInfo.m_CurveName		= m_CurveName;
				newInfo.m_ApplyType		= m_ApplyType;
				System.Array.Copy(m_bApplyOption, newInfo.m_bApplyOption, m_bApplyOption.Length);
				newInfo.m_fValueScale	= m_fValueScale;
				newInfo.m_bRecursively	= m_bRecursively;
				newInfo.m_FromColor		= m_FromColor;
				newInfo.m_ToColor		= m_ToColor;

				newInfo.m_nTag			= m_nTag;
				newInfo.m_nSortGroup	= m_nSortGroup;

				return newInfo;
			}

			public void CopyTo(SmInfoCurve target)
			{
				target.m_AniCurve				= new AnimationCurve(m_AniCurve.keys);
				target.m_AniCurve.postWrapMode	= m_AniCurve.postWrapMode;
				target.m_AniCurve.preWrapMode	= m_AniCurve.preWrapMode;

				target.m_bEnabled		= m_bEnabled;
				target.m_ApplyType		= m_ApplyType;
				System.Array.Copy(m_bApplyOption, target.m_bApplyOption, m_bApplyOption.Length);
				target.m_fValueScale	= m_fValueScale;
				target.m_bRecursively	= m_bRecursively;
				target.m_FromColor		= m_FromColor;
				target.m_ToColor		= m_ToColor;
				target.m_nTag			= m_nTag;
				target.m_nSortGroup	= m_nSortGroup;
			}

			public int GetValueCount()
			{
				switch (m_ApplyType)
				{
				case SmInfoCurve.APPLY_TYPE.POSITION:		return 4;
				case SmInfoCurve.APPLY_TYPE.ROTATION:		return 4;
				case SmInfoCurve.APPLY_TYPE.SCALE:			return 3;
				case SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR:	return 4;
				case SmInfoCurve.APPLY_TYPE.TEXTUREUV:		return 2;
				case SmInfoCurve.APPLY_TYPE.MESH_COLOR:		return 4;
				case SmInfoCurve.APPLY_TYPE.NONE:
				default: return 0;
				}
			}

			public string GetValueName(int nIndex)
			{
				string[] valueNames;

				switch (m_ApplyType)
				{
				case SmInfoCurve.APPLY_TYPE.POSITION:
				case SmInfoCurve.APPLY_TYPE.ROTATION:		valueNames	= new string[4] { "X", "Y", "Z", "World" };		break;
				case SmInfoCurve.APPLY_TYPE.SCALE:			valueNames	= new string[4] { "X", "Y", "Z", "" };			break;
				case SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR:	valueNames	= new string[4] { "R", "G", "B", "A" };			break;
				case SmInfoCurve.APPLY_TYPE.TEXTUREUV:		valueNames	= new string[4] { "X", "Y", "", "" };			break;
				case SmInfoCurve.APPLY_TYPE.MESH_COLOR:		valueNames	= new string[4] { "R", "G", "B", "A" };			break;
				case SmInfoCurve.APPLY_TYPE.NONE:
				default: valueNames	= new string[4] { "", "", "", "" }; break;
				}
				return valueNames[nIndex];
			}

			public void SetDefaultValueScale()
			{
				switch (m_ApplyType)
				{
				case SmInfoCurve.APPLY_TYPE.POSITION:		m_fValueScale	= 1;	break;
				case SmInfoCurve.APPLY_TYPE.ROTATION:		m_fValueScale	= 360;	break;
				case SmInfoCurve.APPLY_TYPE.SCALE:			m_fValueScale	= 1;	break;
				case SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR:	break;
				case SmInfoCurve.APPLY_TYPE.TEXTUREUV:		m_fValueScale	= 10;	break;
				case SmInfoCurve.APPLY_TYPE.MESH_COLOR:		break;
				case SmInfoCurve.APPLY_TYPE.NONE:			break;
				}
			}

			public Rect GetFixedDrawRange()
			{
				return new Rect(-m_fOverDraw, -1-m_fOverDraw, 1+m_fOverDraw*2, 2+m_fOverDraw*2);
			}

			public Rect GetVariableDrawRange()
			{
				Rect range = new Rect();
				for (int n = 0; n < m_AniCurve.keys.Length; n++)
				{
					range.yMin = Mathf.Min(range.yMin, m_AniCurve[n].value);
					range.yMax = Mathf.Max(range.yMax, m_AniCurve[n].value);
				}
				int unit = 20;
				for (int n = 0; n < unit; n++)
				{
					float value = m_AniCurve.Evaluate(n / (float)unit);
					range.yMin = Mathf.Min(range.yMin, value);
					range.yMax = Mathf.Max(range.yMax, value);
				}
				range.xMin = 0;
				range.xMax = 1;
				range.xMin -= range.width * m_fOverDraw;
				range.xMax += range.width * m_fOverDraw;
				range.yMin -= range.height * m_fOverDraw;
				range.yMax += range.height * m_fOverDraw;

				return range;
			}

			public Rect GetEditRange()
			{
				return new Rect(0, -1, 1, 2);
			}

			public void NormalizeCurveTime()
			{
				int n = 0;
				while (n < m_AniCurve.keys.Length)
				{
					Keyframe	key = m_AniCurve[n];
					float	fMax = Mathf.Max(0, key.time);
					float	fVal = Mathf.Min(1, Mathf.Max(fMax, key.time));
					if (fVal != key.time)
					{
						Keyframe	newKey = new Keyframe(fVal, key.value, key.inTangent, key.outTangent);
						m_AniCurve.RemoveKey(n);
						n = 0;
						m_AniCurve.AddKey(newKey);
						continue;
					}
					n++;
				}
			}
		}
		[SerializeField]

		public		List<SmInfoCurve>	m_CurveInfoList;
		public		float				m_fDelayTime		= 0;
		public		float				m_fDurationTime		= 0.6f;
		public		bool				m_bAutoDestruct		= true;

		protected	float				m_fStartTime;
		public		float				m_fAddElapsedTime	= 0;
		protected	float				m_fElapsedRate		= 0;
		protected	Transform			m_Transform;

		protected	string				m_ColorName;
		protected	Material			m_MainMaterial;
		protected	string[]			m_ChildColorNames;
		protected	Renderer[]			m_ChildRenderers;
		protected	MeshFilter			m_MainMeshFilter;
		protected	MeshFilter[]		m_ChildMeshFilters;
		protected	SmUVAnimation		m_NcUvAnimation;
		protected	SmTransformTool		m_NcTansform;
		protected	bool				m_bSavedOriginalValue	= false;

		public override int GetAnimationState()
		{
			if (enabled == false || IsActive(gameObject) == false)
				return -1;
			if ((0 < m_fDurationTime && (m_fStartTime == 0 || IsEndAnimation() == false)))
				return 1;
			return 0;
		}

		public void ResetPosition()
		{
			m_NcTansform = new SmTransformTool(transform);
		}

		public override void ResetAnimation()
		{
			m_NcTansform.CopyToLocalTransform(transform);

			m_fStartTime		= GetEngineTime() - m_fAddElapsedTime;
			m_Transform			= null;
			m_ChildRenderers	= null;
			m_ChildColorNames	= null;

			base.ResetAnimation();
			if (0 < m_fDelayTime)
				m_Timer = null;
			InitAnimation();
			UpdateAnimation(0);
		}

		public void AdjustfElapsedTime(float fAddStartTime)
		{
			m_fAddElapsedTime = fAddStartTime;
		}

		public float GetRepeatedRate()
		{
			return m_fElapsedRate;
		}

		void Awake()
		{
			ResetPosition();
		}

		void Start()
		{
			m_fStartTime	= GetEngineTime() - m_fAddElapsedTime;
			InitAnimation();

			if (0 < m_fDelayTime)
			{
				if (GetComponent<Renderer>())
					GetComponent<Renderer>().enabled = false;
			} else {
				InitAnimationTimer();
				UpdateAnimation(0);
			}
		}

		void LateUpdate()
		{
			if (m_fStartTime == 0)
				return;

			if (IsStartAnimation() == false && m_fDelayTime != 0)
			{
				if (GetEngineTime() < m_fStartTime + m_fDelayTime)
					return;
				InitAnimationTimer();
				if (GetComponent<Renderer>())
					GetComponent<Renderer>().enabled = true;
			}

			float fElapsedTime	= m_Timer.GetTime() + m_fAddElapsedTime;
			float fElapsedRate	= fElapsedTime;

			if (0 != m_fDurationTime)
				fElapsedRate = fElapsedTime / m_fDurationTime;
			UpdateAnimation(fElapsedRate);
		}

		void InitAnimation()
		{
			if (m_Transform != null)
				return;
			m_fElapsedRate	= 0;
			m_Transform		= transform;
			foreach (SmInfoCurve curveInfo in m_CurveInfoList)
			{
				if (curveInfo.m_bEnabled == false)
					continue;

				switch (curveInfo.m_ApplyType)
				{
				case SmInfoCurve.APPLY_TYPE.NONE: continue;
				case SmInfoCurve.APPLY_TYPE.POSITION:
					{
						curveInfo.m_OriginalValue	= Vector4.zero;
						curveInfo.m_BeforeValue		= curveInfo.m_OriginalValue;
						break;
					}
				case SmInfoCurve.APPLY_TYPE.ROTATION:
					{
						curveInfo.m_OriginalValue	= Vector4.zero;
						curveInfo.m_BeforeValue		= curveInfo.m_OriginalValue;
						break;
					}
				case SmInfoCurve.APPLY_TYPE.SCALE:
					{
						curveInfo.m_OriginalValue	= m_Transform.localScale;
						curveInfo.m_BeforeValue		= curveInfo.m_OriginalValue;
						break;
					}
				case SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR:
					{
						if (curveInfo.m_bRecursively)
						{
							// Recursively
							if (m_ChildRenderers == null)
							{
								m_ChildRenderers	= m_Transform.GetComponentsInChildren<Renderer>(true);
								m_ChildColorNames	= new string[m_ChildRenderers.Length];
							}
							curveInfo.m_ChildOriginalColorValues	= new Vector4[m_ChildRenderers.Length];
							curveInfo.m_ChildBeforeColorValues		= new Vector4[m_ChildRenderers.Length];

							for (int n = 0; n < m_ChildRenderers.Length; n++)
							{
								Renderer ren	= m_ChildRenderers[n];
								m_ChildColorNames[n]	= Ng_GetMaterialColorName(ren.sharedMaterial);

								if (m_ChildColorNames[n] != null)
								{
									if (m_bSavedOriginalValue == false)
									{
										curveInfo.m_ChildOriginalColorValues[n] = ren.material.GetColor(m_ChildColorNames[n]);
									} else {
										ren.material.SetColor(m_ChildColorNames[n], curveInfo.m_ChildOriginalColorValues[n]);
									}
								}
								curveInfo.m_ChildBeforeColorValues[n]		= Vector4.zero;
							}
						} else {
							// this Only
							if (GetComponent<Renderer>() != null)
							{
								m_ColorName		= Ng_GetMaterialColorName(GetComponent<Renderer>().sharedMaterial);
								if (m_ColorName != null)
								{
									if (m_bSavedOriginalValue == false)
										curveInfo.m_OriginalValue = GetComponent<Renderer>().material.GetColor(m_ColorName);
									else GetComponent<Renderer>().material.SetColor(m_ColorName, curveInfo.m_OriginalValue);
								}

								curveInfo.m_BeforeValue		= Vector4.zero;
							}
						}
						break;
					}
				case SmInfoCurve.APPLY_TYPE.TEXTUREUV:
					{
						if (m_NcUvAnimation == null)
							m_NcUvAnimation	= GetComponent<SmUVAnimation>();
						if (m_NcUvAnimation != null)
						{
							if (m_bSavedOriginalValue == false)
								curveInfo.m_OriginalValue = new Vector4(m_NcUvAnimation.m_fScrollSpeedX, m_NcUvAnimation.m_fScrollSpeedY, 0, 0);
							else {
								m_NcUvAnimation.m_fScrollSpeedX	= curveInfo.m_OriginalValue.x;
								m_NcUvAnimation.m_fScrollSpeedY	= curveInfo.m_OriginalValue.y;
							}
						}
						curveInfo.m_BeforeValue		= curveInfo.m_OriginalValue;
						break;
					}
				case SmInfoCurve.APPLY_TYPE.MESH_COLOR:
					{
						float fValue = curveInfo.m_AniCurve.Evaluate(0);
						Color tarColor = Color.Lerp(curveInfo.m_FromColor, curveInfo.m_ToColor, fValue);
						if (curveInfo.m_bRecursively)
						{
							// Recursively
							m_ChildMeshFilters	= m_Transform.GetComponentsInChildren<MeshFilter>(true);
							if (m_ChildMeshFilters == null || m_ChildMeshFilters.Length < 0)
								break;
							for (int arrayIndex = 0; arrayIndex < m_ChildMeshFilters.Length; arrayIndex++)
								ChangeMeshColor(m_ChildMeshFilters[arrayIndex], tarColor);
						} else {
							// this Only
							m_MainMeshFilter	= GetComponent<MeshFilter>();
							ChangeMeshColor(m_MainMeshFilter, tarColor);
						}
						break;
					}
				}
			}
			m_bSavedOriginalValue = true;
		}

		void UpdateAnimation(float fElapsedRate)
		{
			m_fElapsedRate = fElapsedRate;

			foreach (SmInfoCurve curveInfo in m_CurveInfoList)
			{
				if (curveInfo.m_bEnabled == false)
					continue;

				float fValue = curveInfo.m_AniCurve.Evaluate(m_fElapsedRate);

				if (curveInfo.m_ApplyType != SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR && curveInfo.m_ApplyType != SmInfoCurve.APPLY_TYPE.MESH_COLOR)
					fValue *= curveInfo.m_fValueScale;

				switch (curveInfo.m_ApplyType)
				{
				case SmInfoCurve.APPLY_TYPE.NONE:		continue;
				case SmInfoCurve.APPLY_TYPE.POSITION:
					{
						if (curveInfo.m_bApplyOption[3])
							m_Transform.position		+= new Vector3(GetNextValue(curveInfo, 0, fValue), GetNextValue(curveInfo, 1, fValue), GetNextValue(curveInfo, 2, fValue));
						else m_Transform.localPosition	+= new Vector3(GetNextValue(curveInfo, 0, fValue), GetNextValue(curveInfo, 1, fValue), GetNextValue(curveInfo, 2, fValue));
						break;
					}
				case SmInfoCurve.APPLY_TYPE.ROTATION:
					{
						if (curveInfo.m_bApplyOption[3])
							m_Transform.rotation		*= Quaternion.Euler(GetNextValue(curveInfo, 0, fValue), GetNextValue(curveInfo, 1, fValue), GetNextValue(curveInfo, 2, fValue));
						else m_Transform.localRotation	*= Quaternion.Euler(GetNextValue(curveInfo, 0, fValue), GetNextValue(curveInfo, 1, fValue), GetNextValue(curveInfo, 2, fValue));
						break;
					}
				case SmInfoCurve.APPLY_TYPE.SCALE:
					{
						m_Transform.localScale += new Vector3(GetNextScale(curveInfo, 0, fValue), GetNextScale(curveInfo, 1, fValue), GetNextScale(curveInfo, 2, fValue));
						break;
					}
				case SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR:
					{
						if (curveInfo.m_bRecursively)
						{
							// Recursively
							if (m_ChildColorNames == null || m_ChildColorNames.Length < 0)
								break;
							for (int arrayIndex = 0; arrayIndex < m_ChildColorNames.Length; arrayIndex++)
								if (m_ChildColorNames[arrayIndex] != null && m_ChildRenderers[arrayIndex] != null)
									SetChildMaterialColor(curveInfo, fValue, arrayIndex);
						} else {
							if (GetComponent<Renderer>() != null && m_ColorName != null)
							{
								if (m_MainMaterial == null)
								{
									m_MainMaterial = GetComponent<Renderer>().material;
									AddRuntimeMaterial(m_MainMaterial);
								}

								// this Only
								Color color = curveInfo.m_ToColor - curveInfo.m_OriginalValue;
								Color currentColor = m_MainMaterial.GetColor(m_ColorName);

								for (int n = 0; n < 4; n++)
									currentColor[n] += GetNextValue(curveInfo, n, color[n] * fValue);
								m_MainMaterial.SetColor(m_ColorName, currentColor);
							}
						}
						break;
					}
				case SmInfoCurve.APPLY_TYPE.TEXTUREUV:
					{
						if (m_NcUvAnimation)
						{
							m_NcUvAnimation.m_fScrollSpeedX	+= GetNextValue(curveInfo, 0, fValue);
							m_NcUvAnimation.m_fScrollSpeedY	+= GetNextValue(curveInfo, 1, fValue);
						}
						break;
					}
				case SmInfoCurve.APPLY_TYPE.MESH_COLOR:
					{
						Color tarColor = Color.Lerp(curveInfo.m_FromColor, curveInfo.m_ToColor, fValue);
						if (curveInfo.m_bRecursively)
						{
							// Recursively
							if (m_ChildMeshFilters == null || m_ChildMeshFilters.Length < 0)
								break;
							for (int arrayIndex = 0; arrayIndex < m_ChildMeshFilters.Length; arrayIndex++)
								ChangeMeshColor(m_ChildMeshFilters[arrayIndex], tarColor);
						} else {
							// this Only
							ChangeMeshColor(m_MainMeshFilter, tarColor);
						}
						break;
					}
				}
			}

			if (0 != m_fDurationTime)
			{
				if (1 < m_fElapsedRate)
				{
					if (IsEndAnimation() == false)
						OnEndAnimation();
					// AutoDestruct
					if (m_bAutoDestruct)
					{
						if (m_bReplayState)
							SetActiveRecursively(gameObject, false);
						else DestroyObject(gameObject);
					}
				}
			}
		}

		void ChangeMeshColor(MeshFilter mFilter, Color tarColor)
		{
			if (mFilter == null || mFilter.mesh == null)
			{
				Debug.LogWarning("ChangeMeshColor mFilter : " + mFilter);
				Debug.LogWarning("ChangeMeshColor mFilter.mesh : " + mFilter.mesh);
				return;
			}
			Color[]	colors = mFilter.mesh.colors;
			if (colors.Length == 0)
			{
				if (mFilter.mesh.vertices.Length == 0)
					NcSpriteFactory.CreateEmptyMesh(mFilter);
				colors = new Color[mFilter.mesh.vertices.Length];
				for (int c = 0; c < colors.Length; c++)
					colors[c] = Color.white;
			}
			for (int c = 0; c < colors.Length; c++)
				colors[c]	= tarColor;
			mFilter.mesh.colors	= colors;
		}

		void SetChildMaterialColor(SmInfoCurve curveInfo, float fValue, int arrayIndex)
		{
			//m_bRecursively
			Color color			= curveInfo.m_ToColor - curveInfo.m_ChildOriginalColorValues[arrayIndex];
			Color currentColor	= m_ChildRenderers[arrayIndex].material.GetColor(m_ChildColorNames[arrayIndex]);
			for (int n = 0; n < 4; n++)
				currentColor[n] += GetChildNextColorValue(curveInfo, n, color[n] * fValue, arrayIndex);
			m_ChildRenderers[arrayIndex].material.SetColor(m_ChildColorNames[arrayIndex], currentColor);
		}

		float GetChildNextColorValue(SmInfoCurve curveInfo, int nIndex, float fValue, int arrayIndex)
		{
			if (curveInfo.m_bApplyOption[nIndex])
			{
				float incValue = fValue - curveInfo.m_ChildBeforeColorValues[arrayIndex][nIndex];
				curveInfo.m_ChildBeforeColorValues[arrayIndex][nIndex] = fValue;
				return incValue;
			}
			return 0;
		}

		float GetNextValue(SmInfoCurve curveInfo, int nIndex, float fValue)
		{
			if (curveInfo.m_bApplyOption[nIndex])
			{
				float incValue = fValue - curveInfo.m_BeforeValue[nIndex];
				curveInfo.m_BeforeValue[nIndex] = fValue;
				return incValue;
			}
			return 0;
		}

		float GetNextScale(SmInfoCurve curveInfo, int nIndex, float fValue)
		{
			if (curveInfo.m_bApplyOption[nIndex])
			{
				float absValue = curveInfo.m_OriginalValue[nIndex] * (1.0f + fValue);
				float incValue = absValue - curveInfo.m_BeforeValue[nIndex];
				curveInfo.m_BeforeValue[nIndex] = absValue;
				return incValue;
			}
			return 0;
		}

		public float GetElapsedRate()
		{
			return m_fElapsedRate;
		}

		public void CopyTo(SmCurveAnimation target, bool bCurveOnly)
		{
			target.m_CurveInfoList = new List<SmInfoCurve>();

			foreach (SmInfoCurve curveInfo in m_CurveInfoList)
				target.m_CurveInfoList.Add(curveInfo.GetClone());
			if (bCurveOnly == false)
			{
				target.m_fDelayTime		= m_fDelayTime;
				target.m_fDurationTime	= m_fDurationTime;
				// 			target.m_bAutoDestruct	= m_bAutoDestruct;
			}
		}

		public void AppendTo(SmCurveAnimation target, bool bCurveOnly)
		{
			if (target.m_CurveInfoList == null)
				target.m_CurveInfoList = new List<SmInfoCurve>();

			foreach (SmInfoCurve curveInfo in m_CurveInfoList)
				target.m_CurveInfoList.Add(curveInfo.GetClone());
			if (bCurveOnly == false)
			{
				target.m_fDelayTime		= m_fDelayTime;
				target.m_fDurationTime	= m_fDurationTime;
				// 			target.m_bAutoDestruct	= m_bAutoDestruct;
			}
		}

		public SmInfoCurve GetCurveInfo(int nIndex)
		{
			if (m_CurveInfoList == null || nIndex < 0 || m_CurveInfoList.Count <= nIndex)
				return null;
			return m_CurveInfoList[nIndex] as SmInfoCurve;
		}

		public SmInfoCurve GetCurveInfo(string curveName)
		{
			if (m_CurveInfoList == null)
				return null;
			foreach (SmInfoCurve curveInfo in m_CurveInfoList)
				if (curveInfo.m_CurveName == curveName)
					return curveInfo;
			return null;
		}

		public SmInfoCurve SetCurveInfo(int nIndex, SmInfoCurve newInfo)
		{
			if (m_CurveInfoList == null || nIndex < 0 || m_CurveInfoList.Count <= nIndex)
				return null;
			SmInfoCurve	oldCurveInfo = m_CurveInfoList[nIndex] as SmInfoCurve;
			m_CurveInfoList[nIndex] = newInfo;
			return oldCurveInfo;
		}

		public int AddCurveInfo()
		{
			SmInfoCurve	curveInfo	= new SmInfoCurve();
			curveInfo.m_AniCurve	= AnimationCurve.Linear(0, 0, 1, 1);
			// 		curveInfo.m_AniCurve.AddKey(1.0f, 1.0f);
			curveInfo.m_ToColor		= Color.white;
			curveInfo.m_ToColor.w	= 0.0f;

			if (m_CurveInfoList == null)
				m_CurveInfoList = new List<SmInfoCurve>();
			m_CurveInfoList.Add(curveInfo);
			return m_CurveInfoList.Count-1;
		}

		public int AddCurveInfo(SmInfoCurve addCurveInfo)
		{
			if (m_CurveInfoList == null)
				m_CurveInfoList = new List<SmInfoCurve>();
			m_CurveInfoList.Add(addCurveInfo.GetClone());
			return m_CurveInfoList.Count-1;
		}

		public void DeleteCurveInfo(int nIndex)
		{
			if (m_CurveInfoList == null || nIndex < 0 || m_CurveInfoList.Count <= nIndex)
				return;
			m_CurveInfoList.Remove(m_CurveInfoList[nIndex]);
		}

		public void ClearAllCurveInfo()
		{
			if (m_CurveInfoList == null)
				return;
			m_CurveInfoList.Clear();
		}

		public int GetCurveInfoCount()
		{
			if (m_CurveInfoList == null)
				return 0;
			return m_CurveInfoList.Count;
		}

		public void SortCurveInfo()
		{
			if (m_CurveInfoList == null)
				return;
			m_CurveInfoList.Sort(new SmComparerCurve());

			foreach (SmInfoCurve info in m_CurveInfoList)
				info.m_nSortGroup = SmComparerCurve.GetSortGroup(info);
		}

		public bool CheckInvalidOption()
		{
			bool	bDup = false;

			for (int n = 0; n < m_CurveInfoList.Count; n++)
				if (CheckInvalidOption(n))
					bDup = true;
			return bDup;
		}

		public bool CheckInvalidOption(int nSrcIndex)
		{
			SmInfoCurve	srcCurveInfo = GetCurveInfo(nSrcIndex);

			if (srcCurveInfo == null)
				return false;
			if (srcCurveInfo.m_ApplyType != SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR && srcCurveInfo.m_ApplyType != SmInfoCurve.APPLY_TYPE.SCALE && srcCurveInfo.m_ApplyType != SmInfoCurve.APPLY_TYPE.TEXTUREUV)
				return false;
			return false;
		}

		public override void OnUpdateEffectSpeed(float fSpeedRate, bool bRuntime)
		{
			m_fDelayTime		/= fSpeedRate;
			m_fDurationTime		/= fSpeedRate;
		}

		public override void OnSetReplayState()
		{
			base.OnSetReplayState();
		}

		public override void OnResetReplayStage(bool bClearOldParticle)
		{
			base.OnResetReplayStage(bClearOldParticle);
			ResetAnimation();
		}

		public static string Ng_GetMaterialColorName(Material mat)
		{
			string[] propertyNames = { "_Color", "_TintColor", "_EmisColor" };

			if (mat != null)
				foreach (string name in propertyNames)
					if (mat.HasProperty(name))
						return name;
			return null;
		}
	}
}