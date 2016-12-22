using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame
{
    public class TPilotIntimacy : ConfigTableRow
    {
        public int level;
        public int levelUpExp;
    }


    public class ConfigTablePilotIntimacy : ConfigTableController<ConfigTablePilotIntimacy>
    {

        protected override void Init() {
            base.Init();
            _filename = "pilot_intimacy";
            _fileList = new string[] { "pilot_intimacy" };
        }

        protected override void LoadFile() {
            base.LoadFile();
            CreateConfigFileWithIndex();
        }


        ConfigTableRow ParseRow(string[] cols) {
            int i = 0;
            TPilotIntimacy config = new TPilotIntimacy();
            config.level = int.Parse(cols[i++]);
            config.levelUpExp = int.Parse(cols[i++]);
            return config;
        }

        public TPilotIntimacy GetConfigByLevel(int level) {
            ConfigTableRow data = GetDataByIndex(level.ToString(), ParseRow);
            return data as TPilotIntimacy;
        }
    }
}