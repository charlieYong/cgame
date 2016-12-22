using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


namespace ZR_MasterTools
{
    public class ConfigTableRow { }

    // 索引
    public class IndexNode
    {
        public string index;
        public int offset;
        public int length;

        public IndexNode(string idx, int position, int len = 0) {
            index = idx;
            offset = position;
            length = len;
        }
    }

    // 配置缓存
    public class CacheNode
    {
        public string index { get; private set; }
        // 上次使用的时间戳
        public int lastTimestamp { get; private set; }

        ConfigTableRow data = null;
        public ConfigTableRow Data {
            get {
                lastTimestamp = TimeManager.GetUnixTime();
                return data;
            }
        }

        public CacheNode(string idx, ConfigTableRow row) {
            index = idx;
            lastTimestamp = TimeManager.GetUnixTime();
            data = row;
        }
    }

    /*
     * 新的配置文件类，支持以下两种加载和读取方式，均为使用时才加载：
     * 1，支持全表加载到内存方式（以前的配表方式）（占内存但读取快）
     * 2，不加载全表数据到内存，只生成内存索引数据，读取配置数据时再根据索引读取文件对应类容（占用内存不大，读取因为涉及到IO相对第一种方式较慢）
     * 
     * 使用方法：
     * 1，每个配置文件需要创建一个继承于ConfigTableController的类
     * 2，实现Init方法：设置表和是否对查询结果进行缓存，默认会进行缓存
     * 3，实现LoadFile方法：选择全表加载还是索引表的方式
     * 4，实现配置解析方法：将文本数据转化为对应的配置类
     * 5，实现配置数据获取方法
     * 
     * 具体例子参见ConfigTableXXXX
     */

    /// 配置文件基类
    public class ConfigTableController<T> where T : ConfigTableController<T>, new()
    {
        protected static T _instance = null;
        public static T Instance {
            get {
                if (null == _instance) {
                    _instance = new T();
                    _instance.Init();
                    _instance.LoadFile();
                }
                return _instance;
            }
        }

        public static void Clear() {
            _instance = null;
        }

        // 是否需要对数据进行压缩
        protected bool _needCompress = true;

        /// 配置文件行解析函数定义
        public delegate ConfigTableRow ConfigTableRowParser(string[] cols);

        // 配置表名
        protected string _filename = string.Empty;
        // 一个配置多个文件的情况（分表）
        protected string[] _fileList = null;
        protected string _CurrentFileName { private set; get; }
        // 索引数据
        protected Dictionary<string, IndexNode> _indexDict = new Dictionary<string, IndexNode>();
        // 是否开启缓存
        protected bool _isCacheEnabled = true;
        // 最大的查询缓存个数
        protected int _maxCacheNum = 15;
        // 查询缓存
        protected List<CacheNode> _cacheList = new List<CacheNode>();

        // 初始化
        protected virtual void Init() { }

        /// 配置加载
        protected virtual void LoadFile() { }

        /// 空函数，用于主动触发单例的实例化
        public void Load() { }

        /// 配置表行解析，由子类实现，在首次加载整个配置文件时调用，可以做一些数据的预处理
        protected virtual void OnParseRow(string[] cols) { }

        /// 加载整张表到内存（需要实现ParseRow进行具体的数据解析）
        protected void LoadAll() {
            for (int i = 0; i < _fileList.Length; i++) {
                _CurrentFileName = _fileList[i];
                string text = Utils.ReadConfigFile(_CurrentFileName);
                if (string.IsNullOrEmpty(text)) {
                    Debug.LogWarning("空的配置文件：" + _CurrentFileName);
                    return;
                }
                string[] rowList = text.Split('\n');
                for (int j = 0; j < rowList.Length; j++) {
                    string row = rowList[j];
                    if (string.IsNullOrEmpty(row)) {
                        continue;
                    }
                    OnParseRow(row.Split('\t'));
                }
            }
        }

        // 将名字进行md5编码，以防用户可以明显看出是配置数据
        protected static string GetIndexDataPath(string filename) {
            return string.Format("{0}/{1}.bytes", Application.persistentDataPath, Utils.MD5String(filename));
        }

        /// 默认的获取每行index方法（以第一列的字符串作为index，默认情况下是ID，特殊配置要重载这个函数）
        protected virtual string ExtractIndexFromRow(string row) {
            return row.Substring(0, row.IndexOf('\t')).Trim();
        }

        /// 生成带索引记录的配置文件
        protected void CreateConfigFileWithIndex() {
            using (FileStream fs = new FileStream(GetIndexDataPath(_filename), FileMode.Create, FileAccess.Write)) {
                for (int i = 0; i < _fileList.Length; i++) {
                    _CurrentFileName = _fileList[i];
                    string text = Utils.ReadConfigFile(_CurrentFileName);
                    if (string.IsNullOrEmpty(text)) {
                        Debug.LogWarning("空的配置文件：" + _CurrentFileName);
                        continue;
                    }
                    string[] rowList = text.Split('\n');
                    for (int j = 0; j < rowList.Length; j++) {
                        if (string.IsNullOrEmpty(rowList[j])) {
                            continue;
                        }
                        string row = rowList[j];
                        string index = ExtractIndexFromRow(row);
                        if (_indexDict.ContainsKey(index)) {
                            Debug.LogWarning(string.Format("重复记录：{0}, {1}", _filename, index));
                            continue;
                        }
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(row);
                        if (_needCompress) {
                            data = LZFCompression.CompressBytes(data);
                        }
                        IndexNode node = new IndexNode(index, (int)fs.Position, data.Length);
                        fs.Write(data, 0, data.Length);
                        _indexDict.Add(index, node);
                        // 每行的解析回调，可以做一些数据的预处理
                        OnParseRow(row.Split('\t'));
                    }
                }
                fs.Flush();
#if UNITY_IOS
            // ios平台需要将文件标示为No back up
            iPhone.SetNoBackupFlag (GetIndexDataPath (_filename));
#endif
            }
        }

        /// 从内存缓存中获取配置数据 
        protected ConfigTableRow FindFromCache(string index) {
            if (null == _cacheList) {
                return null;
            }
            for (int i = 0; i < _cacheList.Count; i++) {
                if (_cacheList[i].index == index) {
                    return _cacheList[i].Data;
                }
            }
            return null;
        }

        protected void AddCache(string index, ConfigTableRow row) {
            if (!_isCacheEnabled || null != FindFromCache(index)) {
                return;
            }
            CacheNode node = new CacheNode(index, row);
            if (_cacheList.Count >= _maxCacheNum) {
                // 缓存数超过配置上线，替换掉最旧没被访问的节点
                int idx = -1;
                int queryTime = int.MaxValue;
                for (int i = 0; i < _cacheList.Count; i++) {
                    if (_cacheList[i].lastTimestamp < queryTime) {
                        queryTime = _cacheList[i].lastTimestamp;
                        idx = i;
                    }
                }
                if (idx >= 0 && idx < _cacheList.Count) {
                    _cacheList[idx] = node;
                    return;
                }
            }
            _cacheList.Add(node);
        }

        /// 根据索引查找配置数据，parser为配置文件行解析方法
        protected ConfigTableRow GetDataByIndex(string index, ConfigTableRowParser parser) {
            if (!_indexDict.ContainsKey(index)) {
                Debug.LogWarning(string.Format("获取不存在的记录：{0}, {1}", _filename, index));
                return null;
            }
            // 先从缓存里查找
            ConfigTableRow row = FindFromCache(index);
            if (null != row) {
                return row;
            }
            // 再从系统文件里查找
            IndexNode idx = _indexDict[index];
            string content = string.Empty;
            using (FileStream fs = new FileStream(GetIndexDataPath(_filename), FileMode.Open, FileAccess.Read)) {
                byte[] data = new byte[idx.length];
                fs.Seek((int)idx.offset, SeekOrigin.Begin);
                fs.Read(data, 0, idx.length);
                if (_needCompress) {
                    data = LZFCompression.DecompressBytes(data);
                }
                content = System.Text.Encoding.UTF8.GetString(data);
            }
            row = parser(content.Split('\t'));
            if (null == row) {
                Debug.LogWarning("配置文件解析异常：file=" + _filename + ", index=" + index);
                return null;
            }
            AddCache(index, row);
            return row;
        }
    }
}