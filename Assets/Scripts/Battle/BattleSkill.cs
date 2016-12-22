using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 技能状态
    public enum BattleSkillState {
        Invalid,
        Active,
        Using,
        NotActive,

        Max
    }

    public class BattleSkill {
        // 当前等级
        public int Level {get; private set;}
        // 当前状态
        public BattleSkillState State {get; private set;}
        // 配置
        public TSkill config {get; private set;}
        // cd配置值
        public float CoolDown = 0f;
        // cd剩余时间
        public float CDLeft {get; private set;}
        // cd百分比
        public float CDLeftPercent {get {return CDLeft / CoolDown;}}

        // 技能攻击ai列表
        List<TAI> _attackAIList = new List<TAI> ();
        // 技能移动ai列表
        List<TAI> _motionAIList = new List<TAI> ();
        // 使用技能时触发的状态列表
        List<TSkillState> _stateList = new List<TSkillState> ();
        // 当前战斗单位
        BattleUnit _caster = null;

        public BattleSkill (BattleUnit caster, int skillid) {
            Init (caster, skillid);
        }

        public void Reset () {
            CDLeft = CoolDown;
            State = BattleSkillState.NotActive;
        }

        public void Init (BattleUnit caster, int skillid) {
            _caster = caster;
            State = BattleSkillState.NotActive;
            config = ConfigTableSkill.Instance.GetConfigByID (skillid);
            CoolDown = config.cooldown/1000f;
            if (null == config) {
                Debug.LogWarning ("no skill config, id=" + skillid);
                return;
            }
            for (int i=0; i<config.attackAIList.Count; i++) {
                _attackAIList.Add (ConfigTableAI.Instance.GetConfigByID (config.attackAIList[i]));
            }

            for (int i=0; i<config.motionAIList.Count; i++) {
                _motionAIList.Add (ConfigTableAI.Instance.GetConfigByID (config.motionAIList[i]));
            }
            for (int i=0; i<config.stateList.Count; i++) {
                _stateList.Add (ConfigTableSkillState.Instance.GetConfigByID (config.stateList[i]));
            }
        }

        // 使用技能：这里执行具体的技能逻辑
        public IEnumerator Use () {
            State = BattleSkillState.Using;
            CDLeft = CoolDown;
            ExecStateList ();
            for (int i=0; i<_attackAIList.Count; i++) {
                yield return Continuation.WaitForNextContinuation (_attackAIList[i].GetBattleAI ().Execute (this, _caster));
            }
        }

        void ExecStateList () {
            if (null == _stateList || _stateList.Count <= 0) {
                return;
            }
            for (int i=0; i<_stateList.Count; i++) {
                _stateList[i].GetState ().Use (_caster, this, null);
            }
        }

        public void Update () {
            if (State != BattleSkillState.NotActive) {
                return;
            }
            CDLeft -= Time.deltaTime;
            if (CDLeft <= 0) {
                CDLeft = 0;
                State = BattleSkillState.Active;
            }
        }

        // 实际命中率=攻方命中值 / (守方等级 * a + 攻方命中值+ b) + 攻方命中率 - 守方闪避值 / (守方等级 * a + 守方闪避值+ b)-守方闪避率
        public float CalHitRate (BattleUnit target) {
            int pCasterHit = _caster.GetUnitProperty (E_Fighter_Property.Hit);
            float pCasterHitPercent = _caster.GetUnitPropertyOfPercent (E_Fighter_Property.HitRate);
            int pTargetMiss = target.GetUnitProperty (E_Fighter_Property.Miss);
            float pTargetMissPercent = target.GetUnitPropertyOfPercent (E_Fighter_Property.MissRate);
            // p1: 攻方命中值 / (守方等级 * a + 攻方命中值+ b)
            float p1 = pCasterHit / (target.Level * Utils.LevelFactor + pCasterHit + Utils.HitDelta);
            // p2: 守方闪避值 / (守方等级 * a + 守方闪避值+ b)
            float p3 = pTargetMiss / (target.Level * Utils.LevelFactor + pTargetMiss + Utils.HitDelta);
            return (p1 + pCasterHitPercent - p3 - pTargetMissPercent);
        }

        // 计算伤害值
        public float CalDamage (BattleUnit target) {
            // 伤害① = if（命中， Σ攻方攻击力 * 技能伤害系数  + 技能附加伤害值 ，0）
            float d1 = _caster.FightPower * config.damageFactor + config.extraDamage;
            //Debug.Log ("d1=" + d1);
            // 伤害② = 伤害① * {1-max(0,[守方装甲值 *（1-攻方装甲穿透率）-攻方装甲穿透值])/[max(0,守方装甲值 *（1-攻方装甲穿透率）-攻方装甲穿透值)+攻击者的级别*a+b] }
            int defence = target.GetUnitProperty (E_Fighter_Property.Defence);
            int transmit = _caster.GetUnitProperty (E_Fighter_Property.Transmit);
            int transmitRate = _caster.GetUnitProperty (E_Fighter_Property.TransmitRate);
            // max(0,[守方装甲值 *（1-攻方装甲穿透率）-攻方装甲穿透值])
            float p = Mathf.Max (0f, defence * (1f-transmitRate) - transmit);
            float d2 = d1 * (1f - p/(p + _caster.Level * Utils.LevelFactor + Utils.DamageDelta));
            //Debug.Log ("d2=" + d2);
            // 伤害③ = max[ 1，伤害② * （100% + Σ攻方伤害加深百分比 - Σ守方伤害减免百分比）]
            float damageBonusRate = _caster.GetUnitPropertyOfPercent (E_Fighter_Property.DamageBonus);
            float damageReduceRate = target.GetUnitPropertyOfPercent (E_Fighter_Property.DamageReduce);
            float d3 = Mathf.Max (1f, d2 * (1f + damageBonusRate - damageReduceRate));
            //Debug.Log ("d3=" + d3);
            // 伤害④ = max（1，伤害③+ Σ攻方附加固定伤害 - Σ守方减免固定伤害 + 真实伤害）
            return Mathf.Max (1f, d3 + config.extraDamage - 0f + 0f);
        }

        // 子弹碰撞到目标，处理伤害逻辑
        public bool HitOnTarget (BattleUnit target) {
            // 处理免疫状态
            if (target.IsInSkillState<SkillState_ImmuneHarm> ()) {
                return false;
            }
            // 计算命中率
            if (CalHitRate (target) <= 0) {
                return false;
            }
            // 计算伤害值
            float damage = CalDamage (target);
            // 处理防伤害盾
            damage -= target.AbsorbDamageByHuDun (damage);
            if (damage > 0) {
                target.Hurt ((int)damage);
            }
            return true;
        }
    }
}