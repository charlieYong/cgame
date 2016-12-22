using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    public struct PropertyStateItem {
        public E_Fighter_Property property;
        public int value;
        // 实际作用的值
        public int deltaValue;

        public PropertyStateItem (int p, int v) {
            property = (E_Fighter_Property)p;
            value = v;
            deltaValue = 0;
        }

        public PropertyStateItem (string p, string v) {
            property = (E_Fighter_Property)int.Parse (p);
            value = int.Parse (v);
            deltaValue = 0;
        }
    }

    // 修改属性类型的状态
    public class SkillState_Property : SkillState {
        List<PropertyStateItem> _propertyList = new List<PropertyStateItem> ();

        public SkillState_Property (TSkillState config) : base (config) {}

        // 格式：duration|id,delta|id2,delta|...
        // id为属性值，delta为要修改的增加值，正值为增加，负值为降低。多个属性使用‘|’分隔
        protected override void Parse() {
            _duration = int.Parse (_configData.paramList[0]) / 1000f;
            for (int i=1; i<_configData.paramList.Count; i++) {
                string[] item = _configData.paramList[i].Split (',');
                _propertyList.Add (new PropertyStateItem (item[0], item[1]));
            }
        }

        public virtual float GetValue (float baseValue, float skillLevel) {
            return AddUpLevelFactor (baseValue, skillLevel);
        }

        protected override IEnumerator Start () {
            for (int i=0; i<_propertyList.Count; i++) {
                PropertyStateItem item = _propertyList[i];
                item.deltaValue = (int)GetValue (_target.GetUnitProperty (item.property), _skill.Level);
                _target.UpdateUnitProperty (item.property, item.deltaValue);
            }
            yield return Continuation.WaitForNextContinuation (WaitToExit ());
        }

        protected override void Debuff () {
            if (!IsTargetAvailable (_target)) {
                return;
            }
            for (int i=0; i<_propertyList.Count; i++) {
                PropertyStateItem item = _propertyList[i];
                _target.UpdateUnitProperty (item.property, -1 * item.deltaValue);
            }
        }
    }
}