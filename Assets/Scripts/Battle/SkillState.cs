using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    
    public class StateEffectNode {
        public string effectid;
        public GameObject effect;
        public int count;

        public StateEffectNode (string prefabName, GameObject go) {
            effectid = prefabName;
            effect = go;
            count = 1;
        }
    }

    public class SkillState {
        protected BattleUnit _caster = null; // 施法单位
        protected BattleUnit _target = null; // 状态作用目标
        protected BattleSkill _skill = null; // 技能
        protected TSkillState _configData = null; // 配置数据

        public SkillState (TSkillState data) {
            _configData = data;
            Parse ();
        }

        public int id {get {return _configData.id; }}
        /// 是否是永久性状态
        public bool IsPermanent {get {return _configData.isPermanent; }}
        /// 同时存在的最大个数
        public int MaxActivityNum {get {return _configData.maxActivityNum; }}
        /// 刷新规则
        public SkillStateReplaceRule ReplaceRule {get {return _configData.replaceRule; }}
        /// 状态类型
        public SkillStateType Type {get {return _configData.type; }}
        /// 状态种类
        public SkillStateParamType ParamType {get {return _configData.paramType;}}

        // 已作用时间
        protected float _timeSinceStart = 0f;
        // 配置的作用时间
        protected float _duration = 0f;
        public float Duration {get {return IsPermanent ? float.MaxValue : _duration;}}

        /// 状态特效ID
        public string EffectID {get; protected set;}

        protected Continuation _cont = null;

        protected virtual void RefreshDuration (float duration) {
            if (IsPermanent) {
                return;
            }
            _timeSinceStart = 0f;
        }

        // 获取施法者技能等级
        protected float CasterSkillLevel {get { return 0; }}

        // 处理技能升级步长：数值+（步长*技能等级-1）
        public float AddUpLevelFactor (float value, float skillLevel) {
            return value + (Mathf.Max (skillLevel-1f, 0f) * _configData.upLevelFactor);
        }

        public virtual void Exit() {
            Continuation.ExitContinuation (_cont);
            OnFinish ();
        }

        public bool IsControlState () {
            return (null != _configData) && (_configData.type == SkillStateType.Control);
        }

        // 各子类根据构造函数里传入的TSkillStateData做参数解释
        protected virtual void Parse () {}

        protected void OnFinish () {
            Debuff ();
            RemoveFromTarget ();
        }

        protected void RemoveFromTarget () {
            if (null == _target) {
                return;
            }
            _target.RemoveSkillState (this);
        }

        /// 等待状态作用时间结束
        protected virtual IEnumerator WaitToExit () {
            if (IsPermanent) {
                yield return Continuation.WaitForSignal (BattleManager.Instance.UpdateSignal);
                yield break;
            }
            _timeSinceStart = 0f;
            while (IsTargetAvailable (_target) && _timeSinceStart < _duration) {
                yield return Continuation.WaitForSignal (BattleManager.Instance.UpdateSignal);
                _timeSinceStart += Time.unscaledDeltaTime;
            }
        }

        // 判断目标单位是否有效
        protected bool IsTargetAvailable (BattleUnit target) {
            return (target != null && !target.IsDead);
        }

        protected bool IsReachMaximum () {
            return (MaxActivityNum > 0) && (_target.GetStateCount (id) >= MaxActivityNum);
        }

        // 退出最早的一个相关的状态
        protected void ExitFirstStateOnMaximum () {
            SkillState firstState = _target.GetState (id);
            if (null == firstState) {
                return;
            }
            firstState.Exit ();
        }

        // 刷新效果的叠加规则
        protected void RefreshOtherState () {
            // 刷新其他状态的作用时间
            for (int i=0; i<_target.CurStateList.Count; i++) {
                SkillState state = _target.CurStateList[i];
                if (null != state && state.id == id) {
                    state.RefreshDuration (_duration);
                }
            }
        }
            
        // 判断状态是否命中
        bool IsHit (BattleUnit caster, BattleSkill skill, BattleUnit target) {
            // 武将出生的初始状态没有skill数据
            if (skill == null) {
                return true;
            }
            // 自己对自己上buff肯定命中，对自己队友上buff也肯定命中
            if (target == caster || caster.UnitType == target.UnitType) {
                return true;
            }
            // More To Do
            return true;
        }

        /// 实际的处理函数，由子类重载实现状态的具体逻辑。由Use函数调用。
        protected virtual IEnumerator Start () {
            yield break;
        }

        /// debuff虚函数，由子类重载实现debuff的具体逻辑，在状态退出时被调用。
        protected virtual void Debuff () {}

        /// 上状态的统一入口
        public bool Use (BattleUnit caster, BattleSkill skill, BattleUnit target, string effectid="") {
            if (!IsTargetAvailable (target)) {
                return false;
            }
            // 判断是否命中
            if (!IsHit (caster, skill, target)) {
                return false;
            }
            // 判断状态能否作用在目标上
            if (!target.IsStateUsable (this)) {
                return false;
            }
            _caster = caster;
            _target = target;
            _skill = skill;
            if (IsReachMaximum ()) {
                ExitFirstStateOnMaximum ();
            }
            if (ReplaceRule == SkillStateReplaceRule.Refresh) {
                RefreshOtherState ();
            }
            EffectID = effectid;
            _target.AddSkillState (this);
            _cont = Continuation.Start (Start(), OnFinish);
            return true;
        }
    }
}
