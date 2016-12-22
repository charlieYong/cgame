using UnityEngine;
using System.Collections;


namespace Sgame
{
    public sealed class GlobalConfig
    {
        // 各种Resources路径配置
        public static readonly string ConfigPath = "Configs/";
        public static readonly string UIWinPath = "UI/";
        public static readonly string UIItemPath = "UI/Item/";
        public static readonly string ModelPrefabPath = "Models/Pilots/";

        public static readonly string LoginTexturePath = "Textures/Login/";
        public static readonly string PilotTexturePath = "Textures/Pilot/";
        public static readonly string PilotIconTexturePath = "Textures/PilotIcon/";

#if UNITY_STANDALONE_WIN
    //仅用于测试
    static string _TestConfigPath = "";
    public static string TestConfigPath
    {
        get
        {
            if (_TestConfigPath == "")
            {
                string file = Application.dataPath;
                file = file.Substring(0, file.LastIndexOf("/") + 1);
                file = @"" + file.Replace("/", "\\") + "Configs\\";
                _TestConfigPath = file;
            }
            return _TestConfigPath;
        }
    }
#endif


        public static readonly int MaxNameLength = 7;
        public static readonly int MinNameLength = 2;
    }
}