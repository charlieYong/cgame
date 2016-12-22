using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	public class SmEffectBehaviour : MonoBehaviour {
		public class _RuntimeIntance {
			public GameObject	m_ParentGameObject;
			public GameObject	m_ChildGameObject;

			public _RuntimeIntance(GameObject	parentGameObject, GameObject childGameObject) {
				m_ParentGameObject	= parentGameObject;
				m_ChildGameObject	= childGameObject;
			}
		}

		private	static	bool				m_bShuttingDown		= false;
		private	static	GameObject			m_RootInstance;
		protected		MeshFilter			m_MeshFilter;
		protected		List<Material>		m_RuntimeMaterials;
		protected		bool				m_bReplayState		= false;
		protected		SmEffectInitBackup	m_NcEffectInitBackup;

		public SmEffectBehaviour()
		{
			m_MeshFilter	= null;
		}

		public static float GetEngineTime () {
			if (Time.time == 0) {
				return 0.000001f;
			}
			return Time.time;
		}

		public static float GetEngineDeltaTime () {
			return Time.deltaTime;
		}

		// 1 = ing, 0 = stop, -1 = none
		public virtual int GetAnimationState () {
			return -1;
		}

		public static GameObject GetRootInstanceEffect() {
			if (IsSafe() == false) {
				return null;
			}
			if (m_RootInstance == null) {
				m_RootInstance = GameObject.Find("_InstanceObject");
				if (m_RootInstance == null) {
					m_RootInstance = new GameObject("_InstanceObject");
				}
			}
			return m_RootInstance;
		}

		protected static void SetActive (GameObject target, bool bActive) {
			target.SetActive (bActive);
		}

		protected static void SetActiveRecursively (GameObject target, bool bActive) {
			for (int n = target.transform.childCount-1; 0 <= n; n--) {
				if (n < target.transform.childCount) {
					SetActiveRecursively(target.transform.GetChild(n).gameObject, bActive);
				}
			}
			target.SetActive(bActive);
		}

		protected static bool IsActive (GameObject target) {
			return (target.activeInHierarchy && target.activeSelf);
		}

		protected static void RemoveAllChildObject (GameObject parent, bool bImmediate) {
			for (int n = parent.transform.childCount-1; 0 <= n; n--) {
				if (n < parent.transform.childCount) {
					Transform	obj = parent.transform.GetChild(n);
					if (bImmediate)
						Object.DestroyImmediate(obj.gameObject);
					else Object.Destroy(obj.gameObject);
				}
			}
		}

		public static void HideNcDelayActive (GameObject tarObj) {
			SetActiveRecursively(tarObj, false);
		}

		// RuntimeMaterials
		protected void AddRuntimeMaterial (Material addMaterial) {
			if (m_RuntimeMaterials == null) {
				m_RuntimeMaterials = new List<Material>();
			}
			if (m_RuntimeMaterials.Contains (addMaterial) == false) {
				m_RuntimeMaterials.Add (addMaterial);
			}
		}

		public static string GetMaterialColorName(Material mat) {
			string[] propertyNames = { "_Color", "_TintColor", "_EmisColor" };

			if (mat != null) {
				foreach (string name in propertyNames) {
					if (mat.HasProperty(name)) {
						return name;
					}
				}
			}
			return null;
		}

		public static bool IsSafe () {
			return (!m_bShuttingDown);
		}

		protected void UpdateMeshColors(Color color) {
			if (m_MeshFilter == null) {
				m_MeshFilter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
			}
			if (m_MeshFilter == null || m_MeshFilter.sharedMesh == null || m_MeshFilter.mesh == null) {
				return;
			}

			Color[]	colors = new Color[m_MeshFilter.mesh.vertexCount];
			for (int n = 0; n < colors.Length; n++) {
				colors[n] = color;
			}
			m_MeshFilter.mesh.colors = colors;
		}

		protected virtual void OnDestroy() {
			if (m_RuntimeMaterials != null) {
				foreach (Material mat in m_RuntimeMaterials) {
					Destroy(mat);
				}
				m_RuntimeMaterials = null;
			}
		}

		public void OnApplicationQuit () {
			m_bShuttingDown = true;
		}

		public virtual void OnUpdateEffectSpeed(float fSpeedRate, bool bRuntime)
		{
		}

		public virtual void OnSetActiveRecursively(bool bActive)
		{
		}

		public virtual void OnUpdateToolData()
		{
		}

		public virtual void OnSetReplayState()
		{
			m_bReplayState	= true;
		}

		public virtual void OnResetReplayStage(bool bClearOldParticle)
		{
		}
	}
}