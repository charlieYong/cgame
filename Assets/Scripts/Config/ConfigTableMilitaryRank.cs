using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame
{
    public class TMilitaryRank : ConfigTableRow
    {
        public int level;
        public string name;
    }


    public class ConfigTableMilitaryRank : ConfigTableController<ConfigTableMilitaryRank>
    {
        protected override void Init() {
            base.Init();
            _filename = "military_rank";
            _fileList = new string[] { "military_rank" };
        }

        protected override void LoadFile() {
            base.LoadFile();
            CreateConfigFileWithIndex();
        }


        ConfigTableRow ParseRow(string[] cols) {
            int i = 0;
            TMilitaryRank config = new TMilitaryRank();
            config.level = int.Parse(cols[i++]);
            config.name = cols[i++];
            return config;
        }


        public TMilitaryRank GetConfigByLevel(int level) {
            ConfigTableRow data = GetDataByIndex(level.ToString(), ParseRow);
            return data as TMilitaryRank;
        }
    }
}