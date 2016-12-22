using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    // 飞机/怪 属性类型
    public enum E_Fighter_Property {
        Invalid = 0,
        HP_Factor = 1,      // 生命成长
        Attack_Factor = 2,  // 攻击成长
        Defence_Factor = 3, // 防御成长
        HP = 4,             // 生命值
        Attack = 5,         // 攻击值
        Defence = 6,        // 防御值
        Transmit = 7,       // 穿透值
        TransmitRate = 8,   // 穿透率
        DamageBonus = 9,    // 伤害加成
        DamageReduce = 10,  // 伤害减免
        Hit = 11,           // 命中值
        Miss = 12,          // 闪避值
        HitRate = 13,       // 命中率
        MissRate = 14,      // 闪避率

        Max,
    }

    public class TFighterBaseConfig : ConfigTableRow {
        public int id;
        public string name;
        public string prefab;
        public int scale;
        public string desc;
        public int normalSkillID;
        public Dictionary<E_Fighter_Property, int> propertyDict = new Dictionary<E_Fighter_Property, int> ();
    }

    public class TFighter : TFighterBaseConfig {
        public int starLevel;
        public int maxLevel;
        public string dropWays;
        public int specialSkillID;
        public string skillList;
        public string initStateList;
    }

    public class ConfigTableFighter : ConfigTableController<ConfigTableFighter> {

        protected override void Init () {
            base.Init ();
            _filename = "fighter";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        // 要注意飞机表和怪物表里属性的顺序值和枚举值保持一致
        public static int ExtraFighterProperty (TFighterBaseConfig config, string[] items, int index) {
            return ExtraFighterProperty (config, items, index, E_Fighter_Property.HP_Factor, E_Fighter_Property.Max);
        }

        public static int ExtraFighterProperty (TFighterBaseConfig config, string[] items, int index, E_Fighter_Property start, E_Fighter_Property end) {
            int count = 0;
            for (int i=(int)start; i<(int)end-1; i++) {
                count += 1;
                config.propertyDict.Add ((E_Fighter_Property)i, int.Parse (items[index+count]));
            }
            return count;
        }

        ConfigTableRow ParseRow (string[] s) {
            TFighter data = new TFighter ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.name = s[i++];
            data.starLevel = int.Parse (s[i++]);
            data.maxLevel = int.Parse (s[i++]);
            data.prefab = s[i++];
            data.scale = int.Parse (s[i++]);
            data.desc = s[i++];
            data.dropWays = s[i++];
            data.normalSkillID = int.Parse (s[i++]);
            data.specialSkillID = int.Parse (s[i++]);
            data.skillList = s[i++];
            data.initStateList = s[i++];
            // 提取属性值
            i += ExtraFighterProperty (data, s, i);

            return data;
        }

        public TFighter GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TFighter;
        }
    }
}