using UnityEngine;
using System.Collections;
using System;


namespace Sgame
{
    public class Fader : MonoBehaviour
    {

        public enum FaderType
        {
            FadeIn,
            FadeOut,
        }

        Action _onFinishAction;
        UITexture _fader;

        void Awake() {
            _fader = GetComponent<UITexture>();
            gameObject.SetActive(false);
        }

        public void Fade(FaderType type, Action onStart = null, Action OnFinish = null, float duration = 0.7F) {
            _fader.alpha = (FaderType.FadeIn == type) ? 1F : 0F;
            _fader.gameObject.SetActive(true);
            _onFinishAction = OnFinish;
            TweenAlpha tween = TweenAlpha.Begin(_fader.gameObject, duration, 1 - _fader.alpha);
            EventDelegate.Set(tween.onFinished, FadeDone);
            if (null != onStart) {
                onStart();
            }
        }

        void FadeDone() {
            gameObject.SetActive(false);
            if (null != _onFinishAction) {
                _onFinishAction();
            }
            _onFinishAction = null;
        }
    }
}