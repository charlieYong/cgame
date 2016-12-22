using UnityEngine;
using System.Collections;

namespace Sgame.Battle {
    // 战机类
    public class BattleFighter : BattleUnit {
        // 初始化接口
        protected override void OnAwake () {
            base.OnAwake ();
            UnitType = BattleUnitType.Fighter;
        }

        public void InitForBattle (Fighter fighterConfig) {
            InitConfig (fighterConfig);
            InitFighterCollider ();
            GO.AddComponent<FighterOperator> ();
        }

        void InitConfig (Fighter config) {
            FighterConfig = config;
            Level = config.lv;
            FullHP = CurHP = FighterConfig.Config.propertyDict[E_Fighter_Property.HP];
            FightPower = FighterConfig.fightPower;
            InitUnitProperty (FighterConfig.Config.propertyDict);
            InitNormalSkill (config.Config.normalSkillID);
        }

        void InitFighterCollider () {
            BoxCollider box = GO.AddComponent<BoxCollider> ();
            box.isTrigger = true;
            box.center = new Vector3 (0, 0, 0.5f);
            box.size = new Vector3 (2f, 1f, 3f);
            Rigidbody rigid = GO.AddComponent<Rigidbody> ();
            rigid.useGravity = false;
            rigid.isKinematic = true;
        }

        // 碰撞处理
        protected override void OnColliderTrigger (Collider co) {
            base.OnColliderTrigger (co);
        }
    }
}
