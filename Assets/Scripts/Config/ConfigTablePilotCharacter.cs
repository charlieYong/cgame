using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame
{
    public class TPilotCharacter : ConfigTableRow
    {
        public int id;
        public string description;
    }

    public class ConfigTablePilotCharacter : ConfigTableController<ConfigTablePilotCharacter>
    {

        protected override void Init() {
            base.Init();
            _filename = "pilot_character";
            _fileList = new string[] { "pilot_character" };
        }

        protected override void LoadFile() {
            base.LoadFile();
            CreateConfigFileWithIndex();
        }


        ConfigTableRow ParseRow(string[] cols) {
            int i = 0;
            TPilotCharacter config = new TPilotCharacter();
            config.id = int.Parse(cols[i++]);
            config.description = cols[i++];
            return config;
        }


        public TPilotCharacter GetConfigByID(int id) {
            ConfigTableRow data = GetDataByIndex(id.ToString(), ParseRow);
            return data as TPilotCharacter;
        }
    }
}