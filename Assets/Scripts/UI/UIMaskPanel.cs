using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Sgame.UI
{
    public class UIMaskPanel : MonoBehaviour
    {
        static List<UIMaskPanel> _maskList = new List<UIMaskPanel>();

        public EventDelegate.Callback OnMaskClickDelegate;
        static GameObject _prefab = null;
        UIPanel _panel;
        UITexture _texture;

        public static UIMaskPanel Create(int depth, float z = 0f) {
            if (null == _prefab) {
                _prefab = Resources.Load<GameObject>(GlobalConfig.UIWinPath + "UIMaskPanel");
            }
            GameObject go = NGUITools.AddChild(UIManager.Instance.gameObject, _prefab);
            go.transform.localPosition = new Vector3(0f, 0f, z);
            UIMaskPanel mask = go.GetComponent<UIMaskPanel>();
            mask.SetPanelDepth(depth);
            return mask;
        }

        static void EnableLastMaskTextures(bool enable) {
            int count = _maskList.Count;
            if (count > 0) {
                _maskList[count - 1].EnableMaskTexture(enable);
            }
        }

        void Awake() {
            _panel = gameObject.GetComponent<UIPanel>();
            _texture = transform.Find("MaskTexture").GetComponent<UITexture>();
        }

        void OnEnable() {
            EnableLastMaskTextures(false);
            _maskList.Add(this);
        }

        void OnDisable() {
            _maskList.Remove(this);
            EnableLastMaskTextures(true);
        }

        void Start() {
            ReSize();
        }

        void ReSize() {
            _texture.width = UIManager.Instance.RealScreenWidth;
            _texture.height = UIManager.Instance.RealScreenHeight;

            BoxCollider collider = GetComponent<BoxCollider>();
            collider.size = new Vector3(
                UIManager.Instance.RealScreenWidth,
                UIManager.Instance.RealScreenHeight,
                0
            );
        }

        public void EnableMaskTexture(bool enable) {
            _texture.enabled = enable;
        }

        public void SetPanelDepth(int depth) {
            _panel.depth = depth;
        }

        // trigger by uicamera's sendmessage from NGUI
        void OnClick() {
            if (gameObject.activeSelf && null != OnMaskClickDelegate) {
                OnMaskClickDelegate();
            }
        }
    }
}