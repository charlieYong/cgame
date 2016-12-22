using UnityEngine;
using System.Collections;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 怪物类
    public class BattleMonster : BattleUnit {
        // 初始化接口
        protected override void OnAwake () {
            base.OnAwake ();
            UnitType = BattleUnitType.Monster;
        }

        public void InitForBattle (TMonster config) {
            MonsterConfig = config;
            Level = config.level;
            InitUnitProperty (config.propertyDict);
            InitNormalSkill (config.normalSkillID);
            FightPower = GetUnitProperty (E_Fighter_Property.Attack);
            FullHP = CurHP = config.propertyDict[E_Fighter_Property.HP];
        }

        // 碰撞处理
        protected override void OnColliderTrigger (Collider co) {
            base.OnColliderTrigger (co);
        }

        // 受击回调
        protected override void OnUnitHurt (int delta) {
            base.OnUnitHurt (delta);
            // 受击特效
        }

        // 加血回调
        protected override void OnUnitHeal (int delta) {
            base.OnUnitHeal (delta);
        }

        // 死亡回调
        protected override IEnumerator OnUnitDeadTask () {
            GameObject effect = BattleResManager.Instance.CreateEffect ("monster/Ganeraleffect1");
            Utils.SetTransform (Trans, effect.transform, false);
            CleanOnDead ();
            yield break;
        }
    }
}
