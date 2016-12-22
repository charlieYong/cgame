using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

using SMaker;

[CustomEditor(typeof(SmCurveAnimation))]

public class SmCurveAnimationEditor : Editor {
	SmCurveAnimation m_Sel;

	void OnEnable () {
		m_Sel = target as SmCurveAnimation;
	}

	void AddScriptNameField (SmCurveAnimation com) {
		EditorGUILayout.TextField(new GUIContent("Script", "Script"), SMakerUtils.GetScriptName(com));
	}

	public override void OnInspectorGUI()
	{
		AddScriptNameField(m_Sel);

		Rect			rect;
		int				nLeftWidth		= 115;
		int				nAddHeight		= 22;
		int				nDelHeight		= 17;
		int				nLineHeight		= 19;
		int				nCurveHeight	= 50;
		List<SmCurveAnimation.SmInfoCurve>	curveInfoList = m_Sel.m_CurveInfoList;

		EditorGUI.BeginChangeCheck();
		{
			m_Sel.m_fDelayTime		= EditorGUILayout.FloatField ("DelayTime", m_Sel.m_fDelayTime);
			m_Sel.m_fDurationTime	= EditorGUILayout.FloatField ("DurationTime", m_Sel.m_fDurationTime);
			m_Sel.m_bAutoDestruct	= EditorGUILayout.Toggle ("AutoDestruct", m_Sel.m_bAutoDestruct);

			m_Sel.m_fDelayTime = Mathf.Max (m_Sel.m_fDelayTime, 0);
			m_Sel.m_fDurationTime = Mathf.Max (m_Sel.m_fDurationTime, 0.01f);

			EditorGUILayout.Space();
			rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nAddHeight*3));
			{
				if (SMakerUtils.GUIButton (
						SMakerUtils.GetInnerVerticalRect(rect, 3, 0, 1),
						new GUIContent("Clear All"),
						(0 <m_Sel.GetCurveInfoCount())
					)) {
					m_Sel.ClearAllCurveInfo();
				}
				if (GUI.Button(SMakerUtils.GetInnerVerticalRect(rect, 3, 2, 1), new GUIContent ("Add EmptyCurve"))) {
					m_Sel.AddCurveInfo();
				}
				GUILayout.Label("");
			}
			EditorGUILayout.EndHorizontal();

			for (int n = 0; n < (curveInfoList != null ? curveInfoList.Count : 0); n++)
			{
				EditorGUILayout.Space();

				rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nDelHeight));
				{
					GUI.Box(rect, "");
					curveInfoList[n].m_bEnabled = GUILayout.Toggle(
						curveInfoList[n].m_bEnabled, "CurveInfo " + n.ToString(), GUILayout.Width(nLeftWidth)
					);
				}
				EditorGUILayout.EndHorizontal();

				curveInfoList[n].m_CurveName = EditorGUILayout.TextField("CurveName", curveInfoList[n].m_CurveName);

				EditorGUI.BeginChangeCheck();
				{
					rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nLineHeight));
					{
						GUI.Box(rect, "");
						GUILayout.Label("", GUILayout.Width(nLeftWidth));
						SmCurveAnimation.SmInfoCurve.APPLY_TYPE nApplyType	= (SmCurveAnimation.SmInfoCurve.APPLY_TYPE)EditorGUI.Popup (
							new Rect(rect.x, rect.y, nLeftWidth, rect.height),
							(int)curveInfoList[n].m_ApplyType,
							SmCurveAnimation.SmInfoCurve.m_TypeName
						);
						if (curveInfoList[n].m_ApplyType != nApplyType) {
							curveInfoList[n].m_ApplyType = nApplyType;
							curveInfoList[n].SetDefaultValueScale();
						}

						// Add Component
						bool bShowOption = true;
						if (curveInfoList[n].m_ApplyType == SmCurveAnimation.SmInfoCurve.APPLY_TYPE.TEXTUREUV) {
							if (m_Sel.gameObject.GetComponent<SmUVAnimation>() == null) {
								bShowOption = false;
								if (GUI.Button (
									new Rect(rect.x+nLeftWidth, rect.y, rect.width-nLeftWidth, rect.height),
									new GUIContent ("Add UVAnimation Script"))
								) {
									m_Sel.gameObject.AddComponent<SmUVAnimation>();
								}
							}
						}
						if (bShowOption) {
							for (int nValueIndex = 0; nValueIndex < curveInfoList[n].GetValueCount(); nValueIndex++) {
								curveInfoList[n].m_bApplyOption[nValueIndex] = GUILayout.Toggle(
									curveInfoList[n].m_bApplyOption[nValueIndex], curveInfoList[n].GetValueName(nValueIndex)
								);
							}
						}
						if (curveInfoList[n].m_ApplyType == SmCurveAnimation.SmInfoCurve.APPLY_TYPE.SCALE) {
							GUILayout.Label("LocalSpace");
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				if (EditorGUI.EndChangeCheck()) {
					m_Sel.CheckInvalidOption(n);
				}

				if (curveInfoList[n].m_ApplyType == SmCurveAnimation.SmInfoCurve.APPLY_TYPE.MATERIAL_COLOR)
				{
					// ValueScale --------------------------------------------------------------
					rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nLineHeight * 2));
					{
						GUI.Box(rect, "");
						GUILayout.Label("", GUILayout.Width(nLeftWidth));
						bool bEnableColor	= (m_Sel.GetComponent<Renderer>() != null &&
							m_Sel.GetComponent<Renderer>().sharedMaterial != null &&
							SMakerUtils.IsMaterialColor(m_Sel.GetComponent<Renderer>().sharedMaterial)
						);
						Rect colorRect		= SMakerUtils.GetInnerVerticalRect(rect, 2, 0, 1);
						colorRect.width		= nLeftWidth;
						if (SMakerUtils.GUIButton (
							SMakerUtils.GetInnerHorizontalRect(colorRect, 2, 0, 1), new GUIContent("White"), bEnableColor
						)) {
							curveInfoList[n].m_ToColor = Color.white;
						}
						if (SMakerUtils.GUIButton(
							SMakerUtils.GetInnerHorizontalRect(colorRect, 2, 1, 1), new GUIContent("Current"), bEnableColor)
						) {
							curveInfoList[n].m_ToColor = SMakerUtils.GetMaterialColor(m_Sel.GetComponent<Renderer>().sharedMaterial);
						}
						colorRect.x += colorRect.width;
						GUI.Label(colorRect, new GUIContent("ToColor"));
						colorRect.x += 60;
						colorRect.width = rect.width - colorRect.x;
						curveInfoList[n].m_ToColor	= EditorGUI.ColorField(colorRect, curveInfoList[n].m_ToColor);

						// m_bRecursively
						Rect recRect = SMakerUtils.GetInnerVerticalRect(rect, 2, 1, 1);
						curveInfoList[n].m_bRecursively = GUI.Toggle(
							SMakerUtils.GetRightRect(recRect, rect.width-nLeftWidth),
							curveInfoList[n].m_bRecursively, new GUIContent ("Recursively")
						);
					}
					EditorGUILayout.EndHorizontal();
				} else
					if (curveInfoList[n].m_ApplyType == SmCurveAnimation.SmInfoCurve.APPLY_TYPE.MESH_COLOR)
					{
						// ValueScale --------------------------------------------------------------
						rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nLineHeight * 3));
						{
							GUI.Box(rect, "");
							GUILayout.Label("", GUILayout.Width(nLeftWidth));
							// From Color
							Rect colorRect		= SMakerUtils.GetInnerVerticalRect(rect, 3, 0, 1);
							colorRect.width		= nLeftWidth;
							if (SMakerUtils.GUIButton(SMakerUtils.GetInnerHorizontalRect(colorRect, 2, 0, 1), new GUIContent("White"), true))
								curveInfoList[n].m_FromColor = Color.white;
							if (SMakerUtils.GUIButton(SMakerUtils.GetInnerHorizontalRect(colorRect, 2, 1, 1), new GUIContent("Black"), true))
								curveInfoList[n].m_FromColor = Color.black;
							colorRect.x += colorRect.width;
							GUI.Label(colorRect, new GUIContent("FromColor"));
							colorRect.x += 60;
							colorRect.width = rect.width - colorRect.x;
							curveInfoList[n].m_FromColor	= EditorGUI.ColorField(colorRect, curveInfoList[n].m_FromColor);

							// To Color
							colorRect			= SMakerUtils.GetInnerVerticalRect(rect, 3, 1, 1);
							colorRect.width		= nLeftWidth;
							if (SMakerUtils.GUIButton(SMakerUtils.GetInnerHorizontalRect(colorRect, 2, 0, 1), new GUIContent("White"), true))
								curveInfoList[n].m_ToColor = Color.white;
							if (SMakerUtils.GUIButton(SMakerUtils.GetInnerHorizontalRect(colorRect, 2, 1, 1), new GUIContent("Black"), true))
								curveInfoList[n].m_ToColor = Color.black;
							colorRect.x += colorRect.width;
							GUI.Label(colorRect, new GUIContent("ToColor"));
							colorRect.x += 60;
							colorRect.width = rect.width - colorRect.x;
							curveInfoList[n].m_ToColor	= EditorGUI.ColorField(colorRect, curveInfoList[n].m_ToColor);

							// m_bRecursively
							Rect recRect = SMakerUtils.GetInnerVerticalRect(rect, 3, 2, 1);
							curveInfoList[n].m_bRecursively = GUI.Toggle(SMakerUtils.GetRightRect(recRect, rect.width-nLeftWidth), curveInfoList[n].m_bRecursively, new GUIContent ("Recursively"));
						}
						EditorGUILayout.EndHorizontal();
					} else {
						// ValueScale --------------------------------------------------------------
						rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nLineHeight));
						{
							GUI.Box(rect, "");
							GUILayout.Label("", GUILayout.Width(nLeftWidth));
							if (curveInfoList[n].m_ApplyType == SmCurveAnimation.SmInfoCurve.APPLY_TYPE.SCALE)
								curveInfoList[n].m_fValueScale = EditorGUI.FloatField(rect, new GUIContent("Value Scale"), curveInfoList[n].m_fValueScale+1)-1;
							else curveInfoList[n].m_fValueScale = EditorGUI.FloatField(rect, new GUIContent("Value Scale"), curveInfoList[n].m_fValueScale);
						}
						EditorGUILayout.EndHorizontal();
					}

				// Curve --------------------------------------------------------------
				rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(nCurveHeight));
				{
					GUI.Box(rect, "");
					GUILayout.Label("", GUILayout.Width(nLeftWidth));
					EditorGUI.BeginChangeCheck();
					{
						curveInfoList[n].m_AniCurve	= EditorGUI.CurveField(SMakerUtils.GetOffsetRect(rect, nLeftWidth+4, 0, 0, -4), curveInfoList[n].m_AniCurve, Color.green, curveInfoList[n].GetEditRange());
					}
					if (EditorGUI.EndChangeCheck()) {
						curveInfoList[n].NormalizeCurveTime();
					}

					Rect buttonRect = rect;
					buttonRect.width = nLeftWidth;
					SMakerUtils.GetOffsetRect(rect, 0, 5, 0, -5);
					if (GUI.Button(SMakerUtils.GetInnerHorizontalRect(SMakerUtils.GetInnerVerticalRect(buttonRect, 2, 1, 1), 2, 0, 2), new GUIContent("Delete")))
					{
						m_Sel.DeleteCurveInfo(n);
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}
		}
	}

	Rect GetCurveRect(int line)
	{
		int		nLineWidth	= 100;
		int		nLineHeight	= 100;

		return new Rect(0, line * nLineHeight, nLineWidth, nLineHeight);
	}
}
