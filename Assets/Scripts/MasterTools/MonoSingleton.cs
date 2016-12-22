using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = null;
        public static T Instance {
            get {
                if (null == _instance) {
                    return null;
                }
                return _instance;
            }
        }

        bool _isDuplicated = false;
        void Awake() {
            if (Instance != null) {
                Debug.LogError("duplicate instance of singleton:" + typeof(T));
                _isDuplicated = true;
                Destroy(this);
                return;
            }
            _instance = this as T;
            OnAwake();
        }

        protected virtual void OnAwake() { }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy() {
            if (_isDuplicated) {
                return;
            }
            _instance = null;
        }
    }
}