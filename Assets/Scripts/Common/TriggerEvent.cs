using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using ZR_MasterTools;


namespace Sgame
{
    public enum TriggerEventType
    {

        /* Auth and Login */
        LoginSDK_Result,
        CheckLogin_Result,
        LoginGame_Result,
        EnterGame_Result,
        CreateRole_Result,
    }


    public class TriggerEvent<T>
    {

        static Dictionary<TriggerEventType, Delegate> _eventDict = new Dictionary<TriggerEventType, Delegate>();

        public static void ClearAllEvent() {
            _eventDict.Clear();
        }

        public static void AddListener(TriggerEventType type, Callback<T> e) {
            lock (_eventDict) {
                if (!_eventDict.ContainsKey(type)) {
                    _eventDict.Add(type, e);
                }
                else {
                    Delegate[] list = _eventDict[type].GetInvocationList();
                    for (int i = 0; i < list.Length; i++) {
                        if (e == list[i]) {
                            GameDebug.LogWarning("注册重复的事件：" + type);
                            return;
                        }
                    }
                    _eventDict[type] = (Callback<T>)_eventDict[type] + e;
                }
            }
        }

        public static void SetListener(TriggerEventType type, Callback<T> e) {
            lock (_eventDict) {
                _eventDict[type] = e;
            }
        }

        public static void RemoveListener(TriggerEventType type, Callback<T> e) {
            lock (_eventDict) {
                if (_eventDict.ContainsKey(type)) {
                    _eventDict[type] = (Callback<T>)_eventDict[type] - e;
                    if (null == _eventDict[type]) {
                        _eventDict.Remove(type);
                    }
                }
            }
        }

        public static void InvokeListener(TriggerEventType type, T arg) {
            Delegate del;
            if (_eventDict.TryGetValue(type, out del)) {
                Callback<T> e = (Callback<T>)del;
                if (null != e) {
                    e(arg);
                }
            }
        }
    }
}