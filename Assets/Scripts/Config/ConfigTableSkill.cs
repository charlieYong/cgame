using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    public class TSkill : ConfigTableRow {
        public int id;
        public string name;
        public string icon;
        public string desc;
        // 技能攻击逻辑ai列表
        public List<int> attackAIList;
        // 技能行为逻辑ai列表
        public List<int> motionAIList;
        public int cooldown;
        public List<int> stateList;
        public float damageFactor;
        public int extraDamage;
    }

    public class ConfigTableSkill : ConfigTableController<ConfigTableSkill> {

        protected override void Init () {
            base.Init ();
            _filename = "skill";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TSkill data = new TSkill ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.name = s[i++];
            data.icon = s[i++];
            data.desc = s[i++];
            data.attackAIList = Utils.StringToNumberList ('|', s[i++]);
            data.motionAIList = Utils.StringToNumberList ('|', s[i++]);
            data.cooldown = int.Parse (s[i++]);
            data.stateList = Utils.StringToNumberList ('|', s[i++]);
            data.damageFactor = int.Parse (s[i++]) / 1000f;
            data.extraDamage = int.Parse (s[i++]);
            return data;
        }

        public TSkill GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TSkill;
        }
    }
}