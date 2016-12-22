using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame
{
    public class TPilotGrowth : ConfigTableRow
    {
        public int level;
        public int levelUpExp;
        public int interactGetExp;
    }


    public class ConfigTablePilotGrowth : ConfigTableController<ConfigTablePilotGrowth>
    {
        protected override void Init() {
            base.Init();
            _filename = "pilot_growth";
            _fileList = new string[] { "pilot_growth" };
        }

        protected override void LoadFile() {
            base.LoadFile();
            CreateConfigFileWithIndex();
        }


        ConfigTableRow ParseRow(string[] cols) {
            int i = 0;
            TPilotGrowth config = new TPilotGrowth();
            config.level = int.Parse(cols[i++]);
            config.levelUpExp = int.Parse(cols[i++]);
            config.interactGetExp = int.Parse(cols[i++]);
            return config;
        }

        public TPilotGrowth GetConfigByLevel(int level) {
            ConfigTableRow data = GetDataByIndex(level.ToString(), ParseRow);
            return data as TPilotGrowth;
        }
    }
}