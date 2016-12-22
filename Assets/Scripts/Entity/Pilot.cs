using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;


namespace Sgame
{
    public class Pilot
    {
        public TPilot Config { get; private set; }
        public int id;
        public int level;
        public int exp;
        public int intimacyLevel;
        public int intimacyExp;

        public Pilot(int pilotID) {
            id = pilotID;
            level = 0;
            exp = 0;
            intimacyLevel = 0;
            intimacyExp = 0;
            Config = ConfigTablePilot.Instance.GetConfigByID(id);
            if (null == Config) {
                Debug.Log("cannot find config of pilot: id = " + id);
            }
        }


        public string ModelPath { get { return GlobalConfig.ModelPrefabPath + Config.modelName; } }

        public string GetCharacterDescription() {
            string description = "";
            List<TPilotCharacterExpression> characterList = Config.characterList;
            for (int i = 0; i < characterList.Count; i++) {
                if (i > 0) {
                    description += "\n";
                }
                TPilotCharacter character = ConfigTablePilotCharacter.Instance.GetConfigByID(characterList[i].id);
                string expression = string.Format(characterList[i].expression, level);
                description += string.Format(character.description, ExpressionHelper.Evaluate(expression, MathParserTK.DivisionType.FloorToInt));
            }
            return description;
        }
    }

}