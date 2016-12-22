using UnityEngine;
using System.Collections;

namespace Sgame {
    /// 数据类的单例基类简单封装 
    public class DataSingleton<T> where T : DataSingleton<T>, new () {
        // 单例对象
        protected static T _instance = null;
        public static T Instance {
            get {
                if (null == _instance) {
                    _instance = new T ();
                    _instance.OnInit ();
                }
                return _instance;
            }
        }

        // 销毁对象
        public static void Destroy () {
            if (null != _instance) {
                _instance.OnDestroy ();
                _instance = null;
            }
        }

        // 初始化时调用
        protected virtual void OnInit () {}
        // 对象销毁时被调用
        protected virtual void OnDestroy () {}
    }
}
