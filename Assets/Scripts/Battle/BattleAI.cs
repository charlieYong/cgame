using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    public enum BattleAIType {
        Invalid = 0,
        Bullet = 1,     // 发射子弹

        Max
    }

    public class BattleAI {
        // 施法单位
        protected BattleUnit _caster = null;
        // 目标单位
        protected BattleUnit _target = null;
        // 技能
        protected BattleSkill _skill = null;
        // 配置数据
        protected TAI _config = null;

        // 由子类实现具体逻辑
        public virtual IEnumerator Execute (BattleSkill skill, BattleUnit caster, BattleUnit target=null) {
            yield break;
        }
    }

    // 发射子弹ai
    public class BattleAIBullet : BattleAI {
        int _bulletid;
        string _shootJoint;
        string _shootEffect;
        List<int> _aiListOnHit = null;

        public BattleAIBullet (TAI data) {
            _config = data;
            ParseConfig ();
        }

        // id|发射点|发射特效|命中ai列表（逗号分割）
        void ParseConfig () {
            string[] items = _config.param.Split ('|');
            if (items.Length != 4) {
                Debug.LogWarning ("invalid config of bullet ai, id=" + _config.id);
                return;
            }
            _bulletid = int.Parse (items[0]);
            _shootJoint = items[1];
            _shootEffect = items[2];
            _aiListOnHit = Utils.StringToNumberList (',', items[3]);
        }

        public override IEnumerator Execute (BattleSkill skill, BattleUnit caster, BattleUnit target=null) {
            _skill = skill;
            _caster = caster;
            _target = target;
            yield return Continuation.WaitForNextContinuation (
                caster.ShootBullet (_skill, _bulletid, _shootJoint, _shootEffect, _aiListOnHit)
            );
        }
    }
}
