using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    using Battle;

    public enum SkillStateType {
        None = 0,
        Harmful = 1, // 不良类
        Control = 2, // 控制类
        Helpful = 3, // 增益类

        Max
    }

    // 生命值改变类型
    public enum SkillStateHpValueType {
        None = 0,
        MaxHpPermillage = 1,   // 最大生命值千分比
        PAttackPermillage = 2, // 物理攻击千分比
        MAttackPermillage = 3, // 法术攻击千分比
        DamagePermillage = 4,  // 总伤害伤害千分比
        EachDamagePermillage = 5, // 单次伤害千分比

        Max
    }

    // 状态参数类型，用于识别参数代表的意思
    public enum SkillStateParamType {
        Property = 1,    // 属性增加
        Immune_Harm = 2,   // 免疫所有伤害
        Immune_State = 3,  // 免疫指定类型状态（控制等）
    }

    public enum SkillStateReplaceRule {
        None = 0,
        Normal = 1,  // 正常叠加，新替换旧
        Refresh = 2, // 刷新叠加，新替换旧，同时刷新所有buff作用时间
    }

    public enum SkillStateDurationType {
        None = 0,
        Duration = 1,
        Times = 2,
        DurationAndTimes = 3,
    }

    public class TSkillState : ConfigTableRow {
        public int id;
        public SkillStateType type;
        public SkillStateParamType paramType;
        public List<string> paramList;
        public float upLevelFactor; // 技能升级步长
        public bool isPermanent;
        public int maxActivityNum;
        public SkillStateReplaceRule replaceRule;

        /// 获取状态统一入口，根据配置的类型返回对应的状态处理对象
        public SkillState GetState () {
            SkillState state = null;
            switch (paramType) {
            case SkillStateParamType.Property:
                state = new SkillState_Property (this);
                break;
            case SkillStateParamType.Immune_Harm:
                state = new SkillState_ImmuneHarm (this);
                break;
            case SkillStateParamType.Immune_State:
                state = new SkillState_ImmuneState (this);
                break;
            default:
                Debug.LogWarning ("不支持的状态类型:" + this.ToString ());
                break;
            }
            return state;
        }
    }

    public class ConfigTableSkillState : ConfigTableController<ConfigTableSkillState> {
        // 配置初始化
        protected override void Init() {
            base.Init();
            _filename = "skill_state";
            _fileList = new string[] {"skill_state"};
        }

        protected override void LoadFile () {
            base.LoadFile();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] cols) {
            TSkillState data = new TSkillState ();
            int i=0;
            data.id = int.Parse (cols[i++]);
            data.paramType = (SkillStateParamType)int.Parse (cols[i++]);
            string arg = cols[i++];
            if (!string.IsNullOrEmpty (arg)) {
                data.paramList = new List<string> ();
                string[] args = arg.Split ('|');
                for (int k=0; k<args.Length; k++) {
                    data.paramList.Add (args[k].Trim ());
                }
            }
            data.upLevelFactor = float.Parse (cols[i++]);
            data.isPermanent = (1 == int.Parse (cols[i++]));
            data.maxActivityNum = int.Parse (cols[i++]);
            data.replaceRule = (SkillStateReplaceRule)int.Parse (cols[i++]);
            data.type = (SkillStateType)int.Parse (cols[i++]);
            return data;
        }

        /// 是否存在此id状态配置
        public bool IsContainState (int id) {
            return _indexDict.ContainsKey (id.ToString ());
        }

        /// 根据ID获取配置信息
        public TSkillState GetConfigByID (int id) {
            ConfigTableRow data = GetDataByIndex (id.ToString (), ParseRow);
            return data as TSkillState;
        }

        /// 根据ID获取技能状态对象
        public SkillState GetSkillState (int stateid) {
            TSkillState state = GetConfigByID (stateid);
            if (null == state) {
                Debug.LogError ("尝试获取不存在的状态，ID＝" + stateid);
                return null;
            }
            return state.GetState();
        }
    }
}