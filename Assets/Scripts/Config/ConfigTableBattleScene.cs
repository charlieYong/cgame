using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    public enum BattleSceneMoveType {
        Invalid = 0,
        Line = 1,     // 直线匀速移动

        Max
    }

    public class TBattleScene : ConfigTableRow {
        public int id;
        // 战斗场景名
        public string scene;
        // 场景组件挂载点顶层对象
        public string componentRootGo;
        // 移动的顶层对象
        public string moveRootGo;
        // 移动类型
        public BattleSceneMoveType moveType;
        // 移动速度
        public int moveSpeed;
        // 特殊参数（用于扩展）
        public string extraParams;
    }

    public class ConfigTableBattleScene : ConfigTableController<ConfigTableBattleScene> {

        protected override void Init () {
            base.Init ();
            _filename = "battle_scene";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TBattleScene data = new TBattleScene ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.scene = s[i++];
            data.componentRootGo = s[i++];
            data.moveRootGo = s[i++];
            data.moveType = (BattleSceneMoveType)int.Parse (s[i++]);
            data.moveSpeed = int.Parse (s[i++]);
            data.extraParams = s[i++];
            return data;
        }

        public TBattleScene GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TBattleScene;
        }
    }
}