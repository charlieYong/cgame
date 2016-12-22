using UnityEngine;
using System.Collections;


namespace Sgame.UI
{
    public class Loading : LoadingBase
    {
        UISlider _progressSlider;
        UILabel _progressLabel;
        //UITexture _bgTexture;
        //UILabel _tipsLabel;


        protected override void OnAwake() {
            //_bgTexture = _transform.Find("BG").GetComponent<UITexture>();
            Transform parent = _transform.Find("Content");
            for (int i = 0; i < parent.childCount; i++) {
                Transform child = parent.GetChild(i);
                switch (child.name) {
                    case "Slider":
                        _progressSlider = child.GetComponent<UISlider>();
                        _progressSlider.value = 0;
                        _progressLabel = child.Find("Text").GetComponent<UILabel>();
                        break;
                    //case "Tips":
                    //    _tipsLabel = child.GetComponent<UILabel>();
                    //    break;

                }
            }
        }

        public override void Show() {
            UpdateLoadingProgress(0);
            //_tipsLabel.text = ConfigTableLoading.Instance.GetRandomTips();
            //_bgTexture.mainTexture = Resources.Load(ConfigTableLoading.Instance.GetRandomBG()) as Texture;
        }

        public override void UpdateLoadingProgress(int percent) {
            _progressSlider.value = (float)percent / 100F;
            _progressLabel.text = string.Format("[b]loading...{0}%[-]", percent);
        }
    }
}