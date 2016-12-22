using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    using Battle;

    public enum BattleAIType {
        Invalid = 0,
        Bullet = 1,
    }

    public class TAI : ConfigTableRow {
        public int id;
        public BattleAIType type;
        public string param;

        public BattleAI GetBattleAI () {
            switch (type) {
            case BattleAIType.Bullet:
                return new BattleAIBullet (this);
            }
            return null;
        }
    }

    public class ConfigTableAI : ConfigTableController<ConfigTableAI> {

        protected override void Init () {
            base.Init ();
            _filename = "ai";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TAI data = new TAI ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.type = (BattleAIType)int.Parse (s[i++]);
            data.param = s[i++];
            return data;
        }

        public TAI GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TAI;
        }
    }
}