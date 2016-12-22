using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    /// 游戏文字类，配合UILabel使用
    /// 游戏运行时将Key对应的Text赋值给UILabel从而实现界面文本的可配置化以及各种语言的本地化（替换配置表即可替换对应的语言文本）
    public class GameTextHelper : MonoBehaviour
    {
        public string Key;

        void Start() {
            UILabel label = gameObject.GetComponent<UILabel>();
            if ((null != label) && !string.IsNullOrEmpty(Key)) {
                label.text = ConfigTableGameText.Instance.GetText(Key);
            }
        }

        public string value {
            get {
                return ConfigTableGameText.Instance.GetText(Key);
            }
        }
    }
}