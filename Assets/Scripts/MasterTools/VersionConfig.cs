using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    public class VersionConfig
    {
        /// Resources目录下的路径
        public static readonly string FilePath = "VersionConfigs/version";
        /// AssetBundle更新包的chunkname
        public static string BundleName {
            get {
                return BundleMgr.ResourcePathToBundleName(FilePath);
            }
        }

        // 单例
        static VersionConfig _instance = null;
        public static VersionConfig Instance {
            get {
                if (null == _instance) {
                    _instance = new VersionConfig();
                }
                return _instance;
            }
        }

        public static void Clear() {
            _instance = null;
        }

        string _basePatchUrl = string.Empty;

        private VersionConfig() {
            TextAsset asset = BundleMgr.Load<TextAsset>(FilePath);
            if (null == asset) {
                Debug.LogError("缺少版本配置文件");
                return;
            }
            string[] rowList = asset.text.Split('\n');
            for (int i = 0; i < rowList.Length; i++) {
                string[] items = Utils.ParseConfigRow(rowList[i]);
                if (null == items) {
                    continue;
                }
                switch (items[0].Trim()) {
                    case "version":
                        CurVersion = items[1].Trim();
                        VersionNum = CurVersion.Replace(".", "");
                        ParseVersionCode();
                        break;
                    case "patch_url":
                        _basePatchUrl = items[1].Trim();
                        break;
                }
            }
        }

        void ParseVersionCode() {
            string[] items = CurVersion.Split('.');
            if (items.Length != 3) {
                Debug.LogWarning("非法的版本号：" + CurVersion);
                VersionCode = 0;
                return;
            }
            VersionCode = 10000 * int.Parse(items[0]) + 100 * int.Parse(items[1]) + int.Parse(items[2]);
        }

        /// 当前版本
        public string CurVersion { get; private set; }
        /// Patch文件下载地址 （ios平台上会缓存http的txt请求，因此需要在url后面加上随机参数）
        public string PatchFileUrl {
            get {
                return string.Format("{0}?t={1}", _basePatchUrl, TimeManager.GetUnixTimeTotalMilliseconds());
            }
        }
        /// 版本号的数字值（用于配置文件命名）
        public string VersionNum { get; private set; }
        /// 用于后台校验版本的版本好，规则：a.b.c => 10000 * a + 100 * b + c
        public int VersionCode { get; private set; }
    }
}