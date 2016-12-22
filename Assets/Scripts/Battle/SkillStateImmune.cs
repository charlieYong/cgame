using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 免疫伤害状态
    public class SkillState_ImmuneHarm : SkillState {

        public SkillState_ImmuneHarm (TSkillState config) : base (config) {}

        // 格式：时间值
        protected override void Parse() {
            base.Parse();
            if (_configData.paramList.Count != 2) {
                return;
            }
            _duration = float.Parse (_configData.paramList[1]) / 1000f;
        }

        protected override IEnumerator Start () {
            yield return Continuation.WaitForNextContinuation (WaitToExit ());
        }

        protected override void Debuff() {
            base.Debuff();
            _duration = 0;
        }
    }

    // 免疫状态类型
    public class SkillState_ImmuneState : SkillState {
        List<SkillStateType> _immuneTypeList = null;

        public SkillState_ImmuneState (TSkillState config) : base (config) {}

        // 格式：时间|免疫的状态类型1|免疫的状态类型2|...
        protected override void Parse() {
            base.Parse();
            if (_configData.paramList.Count < 2) {
                return;
            }
            _duration = float.Parse (_configData.paramList[0]) / 1000f;
            _immuneTypeList = new List<SkillStateType> ();
            for (int i=1; i<_configData.paramList.Count; i++) {
                _immuneTypeList.Add ((SkillStateType)int.Parse (_configData.paramList[i]));
            }
        }

        public bool IsStateImmuned (SkillState state) {
            if (null != _immuneTypeList) {
                for (int i=0; i<_immuneTypeList.Count; i++) {
                    if (state.Type == _immuneTypeList[i]) {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override IEnumerator Start () {
            yield return Continuation.WaitForNextContinuation (WaitToExit ());
        }

        protected override void Debuff() {
            base.Debuff();
            _immuneTypeList = null;
        }
    }
}