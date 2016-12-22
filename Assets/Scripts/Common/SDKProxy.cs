using UnityEngine;
using System.Collections;


namespace Sgame
{
    public class SDKProxy : MonoBehaviour
    {

        // 设备相关信息
        /// 机型
        public string DeviceModel { get; private set; }
        /// 系统类型
        public string DeviceSystem { get; private set; }
        // 系统版本号
        public string DeviceSysVersion { get; private set; }


        static SDKProxy _instance;
        public static SDKProxy Instance { get { return _instance; } }

        PlatformBase _sdk;
        public PlatformBase SDK { get { return _sdk; } }


        void Awake() {
            if (null != _instance) {
                Debug.LogError("Duplicated SDKManager!");
                Destroy(this);
                return;
            }
            _instance = this;

            _sdk = new FakeSDK();
            ParseDeviceInfo();
        }


        public void Login() {
            SceneMgr.Instance.ShowConnecting(true);
            _sdk.Login();
        }

        public void OnLoginResult(bool result, string info) {
            SceneMgr.Instance.ShowConnecting(false);
            if (result) {
                AuthMgr.Instance.CheckAuth();
            }
            else {
                Debug.LogWarning("登录SDK失败：" + info);
            }
            TriggerEvent<bool>.InvokeListener(TriggerEventType.LoginSDK_Result, result);
        }


        void ParseDeviceInfo() {
            DeviceModel = SystemInfo.deviceModel;
            string sysInfo = SystemInfo.operatingSystem;
            // Android OS 4.4.4 / API-19 (KTU84P/V7.1.2.0.KHKCNCK)
            if (sysInfo.Contains("Android")) {
                DeviceSystem = "Android";
                DeviceSysVersion = ExtraVersion(sysInfo);
            }
            // iPhone iOS 7.1 iPhone OS x.y
            else if (sysInfo.Contains("iPhone") || sysInfo.Contains("iOS")) {
                DeviceSystem = "iOS";
                DeviceSysVersion = ExtraVersion(sysInfo);
            }
            // 其他类型暂时不处理
            else {
                DeviceSystem = "0";
                DeviceSysVersion = "0";
            }
            //Debug.Log (
            //    string.Format ("机型={0} 系统={1} 系统版本={2}", DeviceModel, DeviceSystem, DeviceSysVersion)
            //);
        }

        string ExtraVersion(string info) {
            int start = -1;
            int end = info.Length;
            for (int i = 0; i < info.Length; i++) {
                if (-1 == start && char.IsDigit(info[i])) {
                    start = i;
                    continue;
                }
                if ((start != -1) && (!char.IsDigit(info[i])) && (info[i] != '.')) {
                    end = i;
                    break;
                }
            }
            if (start == -1 || end <= start || end - start > info.Length) {
                return info;
            }
            return info.Substring(start, end - start);
        }


        string CreateRoleTimestampKey(ulong id) {
            return string.Format("CreateRoleTimestamp_{0}", id);
        }

        public void CacheCreateRoleTimestamp(ulong playerID, int serverTimestamp) {
            PlayerPrefs.SetInt(CreateRoleTimestampKey(playerID), serverTimestamp);
            PlayerPrefs.Save();
        }

        //public int GetCreateRoleTimestamp() {
        //    string key = CreateRoleTimestampKey(PlayerMgr.Instance.player.id);
        //    if (PlayerPrefs.HasKey(key)) {
        //        return PlayerPrefs.GetInt(CreateRoleTimestampKey(PlayerMgr.Instance.player.id));
        //    }
        //    int now = DataManager.GetServerTimestamp();
        //    CacheCreateRoleTimestamp(PlayerMgr.Instance.player.id, now);
        //    return now;
        //}
    }
}