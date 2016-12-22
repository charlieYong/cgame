using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using Sgame.UI;


namespace Sgame
{
    public class FakeSDK : PlatformBase
    {
        static readonly string _FakeUUIDKey = "Player UUID";

        public FakeSDK() {
            ChannelID = "{F52F35C5-A04A1876}";
            AuthToken = "M0US0mrrg46QIS45";
            AppID = "%7bCC7FCB91%2d516D9AD6%7d";
            UserID = GetUUID();
            UserName = RandomUserName.GetName(!IsFemale);
        }

        static int GetRandomUUID() {
            return TimeManager.GetUnixTime() + UnityEngine.Random.Range(1, 10000);
        }

        string GetUUID() {
            // 手机平台使用设备号做uid
            if (Application.isMobilePlatform && !string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier)) {
                return SystemInfo.deviceUniqueIdentifier;
            }
            //return "5cf2f646d186d8a7ed95f542046ffe15";
            // PC或者Editor则使用随机数
            int uid = PlayerPrefs.GetInt(_FakeUUIDKey);
            if (0 == uid) {
                uid = GetRandomUUID();
                PlayerPrefs.SetInt(_FakeUUIDKey, uid);
                PlayerPrefs.Save();
            }
            return uid.ToString();
        }

        public static void ResetUUID() {
            int uid = GetRandomUUID();
            PlayerPrefs.SetInt(_FakeUUIDKey, uid);
            PlayerPrefs.Save();
            UITips.ShowTips("重设账号成功，重启生效");
        }

        public override void Login() {
            SDKProxy.Instance.OnLoginResult(true, string.Empty);
        }
    }
}