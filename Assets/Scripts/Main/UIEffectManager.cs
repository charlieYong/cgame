using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame
{
    public class UIEffectManager : MonoBehaviour
    {

        Camera _camera;
        Transform _transform;
        int _layer;
        Transform _uiRootTransform;

        static UIEffectManager _instance;
        public static UIEffectManager Instance { get { return _instance; } }

        /// UI特效相机
        public Camera EffectCamera { get { return _camera; } }
        public Transform CachedTransform { get { return _transform; } }

        void Awake() {
            if (null != _instance) {
                Debug.LogWarning("UIEffectManager already Exists!");
                Destroy(this.gameObject);
                return;
            }
            _transform = transform;
            _camera = GetComponent<Camera>();
            _layer = LayerMaskHelper.UIEffectLayer;
            _uiRootTransform = _transform.parent;
            _instance = this;
        }

        /// 创建UI特效
        public GameObject CreateEffect(GameObject prefab, Transform parent, bool changeLayer = true) {
            if (!_camera.enabled) {
                EnableCamera(true);
            }
            GameObject go = NGUITools.AddChild(parent.gameObject, prefab);
            if (changeLayer && go.layer != _layer) {
                Utils.ChangeGoLayerRecursively(go.transform, _layer);
            }
            go.transform.localScale = new Vector3(1F / _uiRootTransform.localScale.x, 1F / _uiRootTransform.localScale.y, 1F / _uiRootTransform.localScale.z);
            return go;
        }

        public void EnableCamera(bool v) {
            _camera.enabled = v;
        }
    }
}