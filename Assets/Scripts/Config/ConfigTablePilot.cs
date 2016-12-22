using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;


namespace Sgame
{
    public enum PilotType
    {
        Light = 1,
        Medium = 2,
        Heavy = 3,
        Balance = 4
    }

    public class TPilot : ConfigTableRow
    {
        public int id;
        public string name;
        public int needMilitaryRank;
        public TCostRes recruitCost;
        public PilotType type;
        public int additionOfLight;
        public int additionOfMedium;
        public int additionOfHeavy;
        public string nameTextureID;
        public string iconID;
        public string modelName;
        public string voiceActorTextureID;
        public string intro;
        public string radarMapID;
        public List<TPilotCharacterExpression> characterList = new List<TPilotCharacterExpression>();
    }

    public class TPilotCharacterExpression
    {
        public int id;
        public string expression;

        public TPilotCharacterExpression(string configText) {
            string[] info = configText.Split(':');
            id = int.Parse(info[0]);
            expression = info[1];
        }
    }

    public class ConfigTablePilot : ConfigTableController<ConfigTablePilot>
    {

        protected override void Init() {
            base.Init();
            _filename = "pilot";
            _fileList = new string[] { "pilot" };
        }

        protected override void LoadFile() {
            base.LoadFile();
            CreateConfigFileWithIndex();
        }


        ConfigTableRow ParseRow(string[] cols) {
            int i = 0;
            TPilot config = new TPilot();
            config.id = int.Parse(cols[i++]);
            config.name = cols[i++];
            config.needMilitaryRank = int.Parse(cols[i++]);
            config.recruitCost = new TCostRes(cols[i++], '+');
            config.type = (PilotType)int.Parse(cols[i++]);
            config.additionOfLight = int.Parse(cols[i++]);
            config.additionOfMedium = int.Parse(cols[i++]);
            config.additionOfHeavy = int.Parse(cols[i++]);
            config.nameTextureID = cols[i++];
            config.iconID = cols[i++];
            config.modelName = cols[i++];
            config.voiceActorTextureID = cols[i++];
            config.intro = cols[i++];
            config.radarMapID = cols[i++];
            string characterText = cols[i++];
            if(!string.IsNullOrEmpty(characterText)){
                string[] characterInfo = characterText.Split('|');
                for (int j = 0; j < characterInfo.Length; j++) {
                    config.characterList.Add(new TPilotCharacterExpression(characterInfo[j]));
                }
            }
            return config;
        }


        public TPilot GetConfigByID(int id) {
            ConfigTableRow data = GetDataByIndex(id.ToString(), ParseRow);
            return data as TPilot;
        }

        public Dictionary<int, TPilot> GetConfigDict() {
            Dictionary<int, TPilot> configDict = new Dictionary<int, TPilot>();
            foreach (string key in _indexDict.Keys) {
                int id = int.Parse(key);
                configDict.Add(id, GetConfigByID(id));
            }
            return configDict;
        }
    }
}