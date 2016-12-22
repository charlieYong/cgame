using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ZR_MasterTools
{
    public class ConfigTableGameText : ConfigTableController<ConfigTableGameText>
    {
        // Key => 文本
        Dictionary<string, string> _textDict = new Dictionary<string, string>();

        protected override void Init() {
            base.Init();
            _filename = "game_text";
            _fileList = new string[] { _filename };
        }

        protected override void LoadFile() {
            base.LoadFile();
            LoadAll();
        }

        // 行格式：key  text
        protected override void OnParseRow(string[] s) {
            // 将‘|’转化为换行
            string key = s[0].Trim();
            if (_textDict.ContainsKey(key)) {
                Debug.LogWarning("GameText 重复的字符配置：" + key);
                return;
            }
            _textDict[key] = s[1].Trim().Replace("|", "\n");
        }

        /// 使用key获取对应的文字描述
        public string GetText(string key) {
            return _textDict.ContainsKey(key) ? _textDict[key] : string.Empty;
        }

        public bool IsContainsKey(string key) {
            return _textDict.ContainsKey(key);
        }
    }
}