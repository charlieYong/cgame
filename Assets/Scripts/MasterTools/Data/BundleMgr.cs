using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;


namespace ZR_MasterTools
{

    /*
     * 资源管理和更新控制类，更新逻辑如下：
     * 1，检查当前版本是否需要更新
     * 2，检查和下载当前版本的资源patch列表
     * 3，下载和提取patch文件的资源 
     */
    public class BundleMgr : MonoSingleton<BundleMgr>
    {
        // 当前版本的标识
        const string VersionMark = "version";
        // 当前patch版本
        public int PatchVersionNum { get; private set; }
        int _progress = 0;
        int _totol = 0;
        // 已下载大小
        int _downloadedSize = 0;
        // 一共需下载大小
        int _totalSize = 0;
        List<PatchFileNode> _patchList = null;

        // 已加载对象
        Dictionary<string, UnityEngine.Object> _bundleObjDict = new Dictionary<string, UnityEngine.Object>();

        protected override void OnAwake() {
            base.OnAwake();
            // 默认为-1，表示没有拉取到正确的版本号，后台做特殊处理
            PatchVersionNum = -1;
        }

        public void ClearCache() {
            _bundleObjDict.Clear();
        }

        public static string BundlesPath() {
            return Application.persistentDataPath;
        }

        // 拉取URL文本信息，以url内容(www.text或www.bytes)调用回调
        IEnumerator DownloadByUrl(string url, Action<string> onTextDownload, Action<byte[]> onDataDownload) {
            WWW www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) {
                Debug.LogWarning("download fail," + url + ":" + www.error);
                yield break;
            }
            // 这里要不要直接用www做参数调用回调？
            if (null != onTextDownload) {
                onTextDownload(www.text);
            }
            if (null != onDataDownload) {
                onDataDownload(www.bytes);
            }
        }

        void OnPatchListFinish(MyWWW www, bool isTimeOut) {
            if (isTimeOut || !string.IsNullOrEmpty(www.CurWWW.error)) {
                return;
            }
            _patchList = new List<PatchFileNode>();
            string[] patchList = www.CurWWW.text.Trim().Split('\n');
            for (int i = 0; i < patchList.Length; i++) {
                string record = patchList[i].Trim();
                if (string.IsNullOrEmpty(record)) {
                    continue;
                }
                string[] items = record.Split(' ');
                if (items.Length != 4) {
                    continue;
                }
                string url = items[0].Trim();
                string version = items[1].Trim();
                string md5 = items[2].Trim();
                int size = int.Parse(items[3].Trim());
                // 处理大版本升级: "version(固定字符串) x.x.x(最新版本号) x.x.x(上一个版本) x(patch版本号)"
                if (VersionMark == url) {
                    PatchVersionNum = size;
                    continue;
                }
                // 不是当前版本的不需要更新；已经更新过了的也不需要更新
                if (version != VersionConfig.Instance.CurVersion || _downloadedPatchMD5Set.Contains(md5)) {
                    continue;
                }
                _patchList.Add(new PatchFileNode(url, size, version, md5));
                _totalSize += size;
            }
        }

        /// 下载更新配置列表
        public IEnumerator WaitForFetchPatchList() {
            LoadDownloadedPatchMD5Set();
            PatchVersionNum = -1;
            MyWWW www = new MyWWW(VersionConfig.Instance.PatchFileUrl, 5F);
            yield return StartCoroutine(www.WaitForFinish(OnPatchListFinish));
        }

        public bool IsNeedDownloadPatch() {
            return (null != _patchList && _patchList.Count > 0);
        }

        /*
         * 1、最小单位为kb，小数位留两位即可，即最小为0.01kb
         * 2、若超过100kb的，转换成MB显示，即0.098≈0.1MB，小数留两位即可
         */
        public string GetTotalSizeText() {
            int min = 10;
            int k = 1024;
            int m = 1024 * 1024;
            if (_totalSize <= min) {
                return "0.01KB";
            }
            if (_totalSize < 100 * k) {
                return string.Format("{0:F2}KB", _totalSize / (float)k);
            }
            else {
                return string.Format("{0:F2}MB", _totalSize / (float)m);
            }
        }

        string GetSizeText(int size) {
            int min = 10;
            int k = 1024;
            int m = 1024 * 1024;
            if (size <= min) {
                return "0.01KB";
            }
            if (size < 100 * k) {
                return string.Format("{0:F2}KB", size / (float)k);
            }
            else {
                return string.Format("{0:F2}MB", size / (float)m);
            }
        }

        public string GetUpdateProcessText() {
            if (_totol <= 0 || _progress >= _totol) {
                return string.Empty;
            }
            return string.Format(ConfigTableGameText.Instance.GetText("UpdateProcessTips"), 
                GetSizeText(_totalSize), GetSizeText(_downloadedSize), CurUpdateProgress
            );
        }

        /// 当前的更新进度
        public float CurUpdateProgress {
            get { return (_totol <= 0) ? 1F : _progress / (float)_totol; }
        }

        string PatchMd5RecordFile {
            get {
                return string.Format("{0}/dowdloaded_patch_set.bytes", Application.persistentDataPath);
            }
        }

        HashSet<string> _downloadedPatchMD5Set = new HashSet<string>();
        // 加载已下载到本地的patch文件md5集合
        void LoadDownloadedPatchMD5Set() {
            _downloadedPatchMD5Set.Clear();
            if (!File.Exists(PatchMd5RecordFile)) {
                return;
            }
            string[] md5List = File.ReadAllLines(PatchMd5RecordFile);
            for (int i = 0; i < md5List.Length; i++) {
                string md5 = md5List[i].Trim();
                if (!string.IsNullOrEmpty(md5) && !_downloadedPatchMD5Set.Contains(md5)) {
                    _downloadedPatchMD5Set.Add(md5);
                }
            }
        }

        // 同步已下载的patch md5集合
        void SyncDownloadedPathcMD5Set() {
            if (_downloadedPatchMD5Set.Count <= 0) {
                return;
            }
            string[] records = new string[_downloadedPatchMD5Set.Count];
            _downloadedPatchMD5Set.CopyTo(records);
            File.WriteAllLines(PatchMd5RecordFile, records);
            _downloadedPatchMD5Set.Clear();
#if UNITY_IOS
        iPhone.SetNoBackupFlag (PatchMd5RecordFile);
#endif
        }

        /// 更新配置和资源
        public IEnumerator UpdateResources() {
            if (null == _patchList || _patchList.Count <= 0) {
                yield break;
            }
            if (!Utils.IsPersistentDataPathExists()) {
                yield break;
            }
            // 资源更新
            _progress = 0;
            _totol = _patchList.Count;
            for (int i = 0; i < _patchList.Count; i++) {
                PatchFileNode patch = _patchList[i];
                // 不是当前版本的不需要更新；已经更新过了的也不需要更新
                if (patch.version != VersionConfig.Instance.CurVersion || _downloadedPatchMD5Set.Contains(patch.md5)) {
                    continue;
                }
                byte[] data = null;
                yield return StartCoroutine(DownloadByUrl(patch.url, null, (bytes) => { data = bytes; }));
                if (null != data) {
                    _downloadedPatchMD5Set.Add(patch.md5);
                    ExtractBundleFromPatchFile(FileCompression.Decompress(data));
                }
                _progress++;
                _downloadedSize += patch.byteSize;
                // debug，用于展示下载进度
                //yield return new WaitForSeconds (1f);
            }
            SyncDownloadedPathcMD5Set();
            _patchList.Clear();
        }

        public static string PatchFilePath(string patchUrl) {
            return patchUrl.ToLower().Replace("/", "_").Replace(":", "_").Trim();
        }

        // 判断是否需要升级客户端
        bool IsNeedUpdateVersion(string lastestVersion) {
            VersionNode lastest = new VersionNode(lastestVersion);
            VersionNode current = new VersionNode(VersionConfig.Instance.CurVersion);
            return lastest.primary > current.primary;
        }

        // patch文件格式：PatchHeader(version;count)|BundleChunk(data;filename;md5)|BundleChunk|....
        void ExtractBundleFromPatchFile(byte[] data) {
            PatchHeader header;
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            try {
                header = formatter.Deserialize(stream) as PatchHeader;
            }
            catch (Exception exception) {
                Debug.LogWarning("Patch头解释错误：" + exception.Message);
                stream.Close();
                return;
            }
            //Debug.Log ("开始提取Bundle，数量：" + header.chunkCount);
            BundleChunk chunk;
            string chunkPath;
            for (int i = 0; i < header.chunkCount; i++) {
                try {
                    chunk = formatter.Deserialize(stream) as BundleChunk;
                    // MD5 verify
                    if (MD5Sum(chunk.data) != chunk.md5) {
                        Debug.LogWarning("BundleChunk文件MD5不一致：" + chunk.fileName);
                        continue;
                    }
                    chunkPath = GetBundlePath(chunk.fileName);
                    File.WriteAllBytes(chunkPath, chunk.data);
                    Debug.Log("提取Bundle成功：" + chunk.fileName);
#if UNITY_IOS
                iPhone.SetNoBackupFlag (chunkPath);
#endif
                }
                catch (Exception exception) {
                    Debug.LogWarning("Bundle提取错误：" + exception.Message);
                }
            }
            stream.Close();
        }

        public static string FileMD5Sum(string filepath) {
            return MD5Sum(File.ReadAllBytes(filepath));
        }

        public static string MD5Sum(byte[] data) {
            MD5 md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(data)).Replace("-", string.Empty);
        }

        [Serializable]
        public class BundleChunk
        {
            public byte[] data;
            public string fileName = string.Empty;
            public string md5 = string.Empty;
        }

        [Serializable]
        public class PatchHeader
        {
            public int chunkCount;
            public string version;
        }

        /*******    下面为资源加载接口    ***********/

        /// Resources路径和Bundle名字转换，如：Configs/general => Configs_general
        public static string ResourcePathToBundleName(string path) {
            return string.Format("abd_{0}", path.ToLower().Replace("/", "_").Trim());
        }

        /// 加载场景到内存中
        public AssetBundle LoadSceneResources(string sceneName) {
            string path = GetBundlePath(sceneName, true);
            if (!File.Exists(path)) {
                return null;
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if (null == bundle) {
                Debug.LogWarning("AssetBundle.CreateFromFile返回NULL：" + path);
                return null;
            }
            return bundle;
        }

        /// 加载资源（带缓存）。顺序：AssetBundle -> Resources.Load
        public T LoadResource<T>(string path) where T : UnityEngine.Object {
            string bundleName = ResourcePathToBundleName(path);
            if (_bundleObjDict.ContainsKey(bundleName)) {
                return _bundleObjDict[bundleName] as T;
            }
            UnityEngine.Object obj = LoadResourceFromAssetBundle(bundleName);
            if (null != obj) {
                _bundleObjDict[bundleName] = obj;
                return obj as T;
            }
            return Resources.Load<T>(path);
        }

        /// 加载资源。重复加载同一个AssetBundle也会消耗内存，因此优先用上面的LoadResource接口。
        public static T Load<T>(string path) where T : UnityEngine.Object {
            string bundleName = ResourcePathToBundleName(path);
            UnityEngine.Object obj = LoadResourceFromAssetBundle(bundleName);
            if (null != obj) {
                return obj as T;
            }
            return Resources.Load<T>(path);
        }

        /// 根据bundle名字获取资源在本地目录的路径
        public static string GetBundlePath(string bundleName, bool isScene = false) {
            // 如果是版本文件时，命名不带上版本号
            if (bundleName.Contains(VersionConfig.BundleName)) {
                return string.Format("{0}/{1}.{2}", BundlesPath(), bundleName, (isScene ? "unity3d" : "assetbundle"));
            }
            return string.Format(
                "{0}/{1}.{2}.{3}",
                BundlesPath(), bundleName, VersionConfig.Instance.VersionNum, (isScene ? "unity3d" : "assetbundle")
            );
        }

        static UnityEngine.Object LoadResourceFromAssetBundle(string bundleName) {
            string path = GetBundlePath(bundleName);
            if (!File.Exists(path)) {
                return null;
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if (null == bundle) {
                Debug.LogWarning("AssetBundle.CreateFromFile返回NULL");
                return null;
            }
            bundle.LoadAllAssets();
            UnityEngine.Object asset = bundle.mainAsset;
            bundle.Unload(false);
            return asset;
        }
    }

    public class VersionNode
    {
        // 大版本号（需更新安装包）
        public int primary { get; private set; }
        // 代码版本号（需更新安装包）
        public int code { get; private set; }
        // 资源版本号（不需要更新安装包，动态更新assetbundle包）
        public int resource { get; private set; }

        public VersionNode(int p, int c, int r) {
            primary = p;
            code = c;
            resource = r;
        }

        public VersionNode(string version) {
            string[] items = version.Split('.');
            if (items.Length != 3) {
                Debug.LogWarning("Wrong version format : " + version);
                return;
            }
            primary = int.Parse(items[0]);
            code = int.Parse(items[1]);
            resource = int.Parse(items[2]);
        }
    }

    public class PatchFileNode
    {
        public string url { get; private set; }
        public int byteSize { get; private set; }
        public string version { get; private set; }
        public string md5 { get; private set; }

        public PatchFileNode(string u, int s, string v, string m) {
            url = u;
            byteSize = s;
            version = v;
            md5 = m;
        }
    }
}