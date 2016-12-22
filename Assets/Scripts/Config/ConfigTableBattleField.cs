using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    public class TBattleField : ConfigTableRow {
        public int id;
        public int sceneid;
        // 怪物配置方案id
        public int monsterConfigID;
        // 场景组件配置id
        public int componentConfigId;
        // 战斗场景配置
        TBattleScene _battleScene = null;

        public TBattleScene BattleScene {
            get {
                if (null == _battleScene) {
                    _battleScene = ConfigTableBattleScene.Instance.GetConfigByID (sceneid);
                }
                return _battleScene;
            }
        }
    }

    public class ConfigTableBattleField : ConfigTableController<ConfigTableBattleField> {

        protected override void Init () {
            base.Init ();
            _filename = "battle_field";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TBattleField data = new TBattleField ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.sceneid = int.Parse (s[i++]);
            data.monsterConfigID = int.Parse (s[i++]);
            data.componentConfigId = int.Parse (s[i++]);
            return data;
        }

        public TBattleField GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TBattleField;
        }
    }
}