using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;


namespace Sgame
{
    public class ClientLocalConfig
    {
        Dictionary<string, string> _configDict = new Dictionary<string, string>();

        static ClientLocalConfig _instance = null;
        public static ClientLocalConfig Instance {
            get {
                if (null == _instance) {
                    _instance = new ClientLocalConfig();
                }
                return _instance;
            }
        }

        public static void Clear() {
            if (null != _instance) {
                _instance = null;
            }
        }

        private ClientLocalConfig() {
            string text = Utils.ReadConfigFile("client_local_config");
            string[] rowList = text.Split('\n');
            for (int i = 0; i < rowList.Length; i++) {
                if (string.IsNullOrEmpty(rowList[i])) {
                    continue;
                }
                string[] items = rowList[i].Split('\t');
                Add(items[0], items[1]);
            }
        }

        public void Load() {
            // 空函数，触发单例生成
        }

        void Add(string key, string value) {
            if (_configDict.ContainsKey(key)) {
                return;
            }
            _configDict.Add(key.Trim(), value.Trim());
        }

        string GetValue(string key) {
            if (!_configDict.ContainsKey(key)) {
                Debug.LogError("错误的配置项：" + key);
                return string.Empty;
            }
            return _configDict[key];
        }


        public string LoginServerIP {
            get { return GetValue("LOGIN_SERVER_IP"); }
        }

        public int LoginServerPort {
            get { return int.Parse(GetValue("LOGIN_SERVER_PORT")); }
        }

        public string LoginNoticePath {
            get { return GetValue("LOGIN_NOTICE_PATH"); }
        }
    }
}