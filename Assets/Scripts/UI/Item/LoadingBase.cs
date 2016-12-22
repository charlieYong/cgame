using UnityEngine;
using System.Collections;


namespace Sgame.UI
{
    public class LoadingBase : MonoBehaviour
    {

        protected Transform _transform;

        void Awake() {
            _transform = transform;
            OnAwake();
        }

        protected virtual void OnAwake() { }

        public virtual void Show() { }
        public virtual void UpdateLoadingProgress(int percent) { }
    }
}