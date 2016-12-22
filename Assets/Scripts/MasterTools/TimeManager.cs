using UnityEngine;
using System.Collections;
using System;


namespace ZR_MasterTools
{
    public class TimeManager : ScriptableObject
    {
        static DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int _timeOffsetBetweenClientAndServer = 0;    // 保存客户端和服务器时间差
        public static void CacheTimeOffset(int nowServerTimestamp) {
            _timeOffsetBetweenClientAndServer = GetUnixTime() - nowServerTimestamp;
        }

        #region 获取指定时间的不同格式
        public static DateTime GetDateTime(int unixTime) {
            return _unixEpoch.AddSeconds(unixTime);
        }

        public static int GetUnixTime(DateTime dt) {
            return (int)(dt - _unixEpoch).TotalSeconds;
        }

        /// 由时间文本（格式：YYYY-MM-DD hh:mm:ss）获取对应时间戳
        public static int GetTimestampByTimeString(string timeString) {
            DateTime time;
            if (DateTime.TryParse(timeString, out time)) {
                return GetUnixTime(time.ToUniversalTime());
            }
            return 0;
        }
        #endregion

        #region 获取当前时间
        // 获取时间戳
        public static int GetUnixTime() {
            return (int)(DateTime.UtcNow - _unixEpoch).TotalSeconds;
        }

        // 获取毫秒数
        public static double GetUnixTimeTotalMilliseconds() {
            return (DateTime.UtcNow - _unixEpoch).TotalMilliseconds;
        }

        // 获取当前服务器时间的时间文本
        public static string Now(string format = "{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}") {
            return GetTimeString(GetServerTimestamp(), format);
        }

        // 获取服务器时间戳
        public static int GetServerTimestamp() {
            return GetUnixTime() - _timeOffsetBetweenClientAndServer;
        }
        #endregion

        #region 获取时间文本
        public static string GetTimeString(int unixTime, string format = "{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}") {
            return GetTimeString(GetDateTime(unixTime), format);
        }

        public static string GetTimeString(DateTime inputTime, string format = "{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}") {
            DateTime time = inputTime.ToLocalTime();
            object[] args = new object[] { time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second };
            return string.Format(format, args);
        }

        public static string GetTimeString(TimeSpan timeSpan, string format = "{0:D2}:{1:D2}:{2:D2}") {
            object[] args = new object[] { timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds };
            return string.Format(format, args);
        }

        // 获取时间格式文本，isShowFull为true时显示天时分秒，为false时只显示两项或者一项，即（天时）或（时分）或（分秒）或（秒）
        public static string GetTimeTextWithStyle(int second, bool isShowFull = false) {
            TimeSpan timespan = TimeSpan.FromSeconds(second);
            if (isShowFull) {
                return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                timespan.Days, ConfigTableGameText.Instance.GetText("D"),
                timespan.Hours, ConfigTableGameText.Instance.GetText("H"),
                timespan.Minutes, ConfigTableGameText.Instance.GetText("M"),
                timespan.Seconds, ConfigTableGameText.Instance.GetText("S")
            );
            }
            return string.Format("{0}{1}{2}{3}",
                (timespan.Days > 0 ? string.Format("{0}{1}", timespan.Days, ConfigTableGameText.Instance.GetText("D")) : string.Empty),
                (timespan.Hours > 0 ? string.Format("{0}{1}", timespan.Hours, ConfigTableGameText.Instance.GetText("H")) : string.Empty),
                (timespan.Days <= 0 && timespan.Minutes > 0 ? string.Format("{0}{1}", timespan.Minutes, ConfigTableGameText.Instance.GetText("M")) : string.Empty),
                (timespan.Days <= 0 && timespan.Hours <= 0 && timespan.Seconds > 0 ? string.Format("{0}{1}", timespan.Seconds, ConfigTableGameText.Instance.GetText("S")) : string.Empty));
        }
        #endregion
    }
}