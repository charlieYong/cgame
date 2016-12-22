using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ZR_MasterTools
{
    public class GameDebug
    {
        public enum Level
        {
            Info,
            Warning,
            Error,
            Exception
        }

        static bool _isEnableDebugLog = true;
        public static bool IsEnableDebugLog { get { return (Application.isEditor || _isEnableDebugLog); } }

        static List<Level> _whiteList = new List<Level>() { Level.Error, Level.Exception };
        static bool _mobileEnabled = false;

        /// <summary>
        /// 打开/关闭手机上的Log
        /// </summary>
        public static void EnableMobileLog(bool value) {
            _mobileEnabled = value;
        }

        static bool IsLoggable(Level level) {
            return (_whiteList.Contains(level) || Application.isEditor || _mobileEnabled);
        }

        static void DoLog(Level level, object message) {
            if (!IsLoggable(level)) {
                return;
            }
            //return;
            switch (level) {
                case Level.Info:
                    Debug.Log(message);
                    break;
                case Level.Warning:
                    Debug.LogWarning(message);
                    break;
                case Level.Error:
                    Debug.LogError(message);
                    break;
            }
        }

        public static void Log(object message) {
            DoLog(Level.Info, message);
        }

        public static void LogWarning(object message) {
            DoLog(Level.Warning, message);
        }

        public static void LogError(object message) {
            DoLog(Level.Error, message);
        }
    }
}