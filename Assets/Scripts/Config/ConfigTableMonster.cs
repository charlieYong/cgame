using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    public enum MonsterType {
        Invalid = 0,   // 默认无效值
        Normal = 1,        // 普通小怪
        Special = 2,       // 精英怪
        Boss = 3,          // Boss
        Attackable = 4,    // 可被攻击物件
        Harmless = 5,      // 可被攻击但无敌物件
        NonAttackable = 6, // 不可被攻击物件

        Max,
    }

    public class TMonster : TFighterBaseConfig {
        public MonsterType type;
        public bool isShowHP;
        public int hpShowTime;
        public int level;
        public int damageOnHit;
        public int damagePercentOnHit;
        public int pointOnKill;
        public string dropsOnKill;
    }

    public class ConfigTableMonster : ConfigTableController<ConfigTableMonster> {

        protected override void Init () {
            base.Init ();
            _filename = "monster";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TMonster data = new TMonster ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.name = s[i++];
            data.prefab = s[i++];
            data.scale = int.Parse (s[i++]);
            data.desc = s[i++];
            data.type = (MonsterType)int.Parse (s[i++]);
            data.normalSkillID = int.Parse (s[i++]);
            data.isShowHP = (1 == int.Parse (s[i++]));
            data.hpShowTime = int.Parse (s[i++]);
            data.level = int.Parse (s[i++]);
            // 提取属性值
            i += ConfigTableFighter.ExtraFighterProperty (data, s, i, E_Fighter_Property.HP, E_Fighter_Property.Max);
            data.damageOnHit = int.Parse (s[i++]);
            data.damagePercentOnHit = int.Parse (s[i++]);
            data.pointOnKill = int.Parse (s[i++]);
            data.dropsOnKill = s[i++];

            return data;
        }

        public TMonster GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TMonster;
        }
    }
}