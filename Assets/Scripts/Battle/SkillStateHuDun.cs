using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 护盾破碎的类型
    public enum HuDunExitType {
        Invalid,
        Duration = 1,
        Capacity = 2,
        Any = 3,
        Max
    }

    // 护盾
    public class SkillState_HuDun : SkillState {
        // 可吸收总量
        float _capacity = 0f;
        public float Capacity {get {return _capacity;}}
        // 每次吸收伤害转化为生命的千分比
        float _toHpFactor = 0f;
        public float ToHpFactor {get {return _toHpFactor;}}
        // 触发的buff
        int _buffid = 0;
        public int BuffID = 0;

        // 剩余可吸收量
        float _capacityLeft = 0f;
        public float CapacityLeft {get {return _capacity;}}

        public float CapacityPercentage {get {return _capacityLeft/_capacity;}}

        public SkillState_HuDun (TSkillState config) : base(config) {}

        // 格式：护盾持续时间|可吸收伤害值|吸收伤害生命转化率|护盾结束时触发的状态id（可选）
        protected override void Parse() {
            base.Parse();
            if (_configData.paramList.Count < 4) {
                return;
            }
            _duration = float.Parse (_configData.paramList[0]) / 1000f;
            _capacity = float.Parse (_configData.paramList[1]);
            _toHpFactor = float.Parse (_configData.paramList[2]) / 1000f;
            if (_configData.paramList.Count >= 4) {
                _buffid = int.Parse (_configData.paramList[3]);
            }
            _capacityLeft = _capacity;
        }

        void SetupCapacity () {
            _capacity = AddUpLevelFactor(_capacity, this.CasterSkillLevel);
            _capacityLeft = _capacity;         
        }

        // 增益类效果，可以抵御一定量的伤害，同时可以支持免疫控制与否。注：抵御的伤害用完后，状态会自动消失。
        protected override IEnumerator Start () {
            SetupCapacity ();
            _target.AddHuDun (this);
            _timeSinceStart = 0f;
            while (_capacityLeft > 0f && _duration > _timeSinceStart) {
                yield return Continuation.WaitForSignal (BattleManager.Instance.UpdateSignal);
                _timeSinceStart += Time.unscaledDeltaTime;
            }
        }

        /// 获取护盾的退出方式
        public HuDunExitType GetExitType () {
            if (_timeSinceStart >= _duration) {
                return HuDunExitType.Duration;
            }
            if (_capacityLeft <= 0) {
                return HuDunExitType.Capacity;
            }
            // 如目标死亡等导致状态退出
            return HuDunExitType.Invalid;
        }

        protected override void Debuff () {
            base.Debuff();
            if (_buffid > 0) {
                ConfigTableSkillState.Instance.GetSkillState (_buffid).Use (_caster, _skill, _caster);
            }
            _duration = 0;
            _capacityLeft = 0;
            _target.RemoveHuDun (this);
        }

        /// 吸收伤害，返回吸收值
        public float AbsorbDamage (float damage) {
            if (0 >= _capacityLeft) {
                return 0f;
            }
            float value = Mathf.Min (damage, _capacityLeft);
            _capacityLeft = Mathf.Max (0, _capacityLeft - value);
            // 处理将伤害转化为生命值
            if (_toHpFactor > 0f) {
                _target.Heal ((int)(value*_toHpFactor));
            }
            return value;
        }
    }
}