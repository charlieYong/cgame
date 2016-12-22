using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


namespace ZR_MasterTools
{
    // 下载完成回调
    public delegate void WWWFinishCallback(MyWWW www, bool isTimeout);

    /// Unity默认的WWW不支持指定的超时时间，做简单的封装实现指定超时时间
    public class MyWWW
    {
        WWW _www = null;
        // 超时时间（秒），0表示不设置超时时间
        float _timeout = 0f;
        long _startTicks = 0;

        // 目前只需要用到最简单的URL方式。需要用到其他方式时再进行扩展。
        public MyWWW(string url, float timeout = 0f, bool isNoCache = true) {
            if (isNoCache) {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Pragma", "no-cache");
                headers.Add("Cache-Control", "no-cache");
                _www = new WWW(url, null, headers);
            }
            else {
                _www = new WWW(url);
            }
            _timeout = timeout;
            _startTicks = DateTime.Now.Ticks;
        }

        /// 获取当前www对象
        public WWW CurWWW { get { return _www; } }

        bool CheckTimeOut() {
            return (_timeout > 0f) && ((DateTime.Now.Ticks - _startTicks) / 10000000f > _timeout);
        }

        /// 等待下载完成
        public IEnumerator WaitForFinish(WWWFinishCallback onFinish) {
            bool isTimeout = false;
            while (!_www.isDone) {
                if (CheckTimeOut()) {
                    isTimeout = true;
                    _www = null;
                    break;
                }
                yield return null;
            }
            onFinish(this, isTimeout);
        }
    }

    public class DownloadHelper : MonoBehaviour
    {
        static DownloadHelper _instance;
        public static DownloadHelper Instance {
            get {
                if (null == _instance) {
                    GameObject go = new GameObject("DownloadHelper");
                    _instance = go.AddComponent<DownloadHelper>();
                }
                return _instance;
            }
        }

        public static void Clear() {
            if (null != _instance) {
                Destroy(_instance.gameObject);
            }
            _instance = null;
        }

        string _localCachePath = string.Empty;

        void Awake() {
            _localCachePath = string.Format("{0}/xgame_webdl", Application.persistentDataPath);
            if (!Directory.Exists(_localCachePath)) {
                Directory.CreateDirectory(_localCachePath);
            }
        }

        // 将url转换成本地文件路径
        string ConvertUrlToLocalPath(string url) {
            return string.Format("{0}/{1}", _localCachePath, Utils.MD5String(url));
        }

        // 如果本地有缓存，则返回缓存的路径，否则返回空
        string GetLocalCachePath(string url) {
            string path = ConvertUrlToLocalPath(url);
            return File.Exists(path) ? path : string.Empty;
        }

        public void DownloadByUrl(string url, WWWFinishCallback onFinish, float timeout = 10f, bool isNoCache = true) {
            MyWWW www = new MyWWW(url, timeout, isNoCache);
            StartCoroutine(www.WaitForFinish(onFinish));
        }

        /// 下载网络图片（本地带缓存，同一个链接本地已存在时不会再下载）
        public void DownloadTexture(string url, System.Action<Texture2D> onFinish, float timeout = 10f) {
            string localFilePrefix = "file://";
            // 下载完成时的回调函数
            WWWFinishCallback callback = (MyWWW mw, bool isTO) => {
                // 超时
                if (isTO) {
                    onFinish(null);
                    return;
                }
                // 将网络图片保存到本地
                if (!mw.CurWWW.url.StartsWith(localFilePrefix)) {
                    string cacheFile = ConvertUrlToLocalPath(url);
                    File.WriteAllBytes(cacheFile, mw.CurWWW.bytes);
                }
                onFinish(mw.CurWWW.texture);
            };
            string cachePath = GetLocalCachePath(url);
            if (!string.IsNullOrEmpty(cachePath)) {
                // 本地有缓存，则直接从本地获取
                MyWWW localWWW = new MyWWW(localFilePrefix + cachePath, timeout, false);
                StartCoroutine(localWWW.WaitForFinish(callback));
                return;
            }
            MyWWW webWWW = new MyWWW(url, timeout);
            StartCoroutine(webWWW.WaitForFinish(callback));
        }

        // 获取网络文本
        public void DownloadText(string url, System.Action<string> onFinish, float timeout = 10f) {
            // 下载完成时的回调函数
            WWWFinishCallback callback = (MyWWW mw, bool isTO) => {
                // 超时
                if (isTO) {
                    onFinish(string.Empty);
                    return;
                }
                onFinish(mw.CurWWW.text);
            };
            MyWWW webWWW = new MyWWW(url, timeout);
            StartCoroutine(webWWW.WaitForFinish(callback));
        }
    }
}