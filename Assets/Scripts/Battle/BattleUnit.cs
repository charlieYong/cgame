using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 战斗单位类型
    public enum BattleUnitType {
        Invalid = 0,
        Fighter,
        Monster,
        SceneMonster,

        Max
    }

    // 战机单位基类
    public class BattleUnit : MonoBehaviour {
        public GameObject GO {get; private set;}
        public Transform Trans {get; private set;}

        // 虚类，需要子类实现具体逻辑接口 -----------------------------
        // awake回调接口，子类实现
        protected virtual void OnAwake () {}
        // 碰撞回调接口，子类实现
        protected virtual void OnColliderTrigger (Collider co) {}
        // 战斗单位受伤回调
        protected virtual void OnUnitHurt (int delta) {}
        // 战斗单位回血回调
        protected virtual void OnUnitHeal (int delta) {}
        // 战斗单位死亡回调
        protected virtual IEnumerator OnUnitDeadTask () {yield break;}
        // -------------------------------------------------------

        // 单位类型
        public BattleUnitType UnitType {get; protected set;}
        // 战机配置
        public Fighter FighterConfig {get; protected set;}
        // 怪配置
        public TMonster MonsterConfig {get; protected set;}
        // 基础配置
        public TFighterBaseConfig BaseConfig {
            get {
                if (UnitType == BattleUnitType.Fighter) {
                    return FighterConfig.Config;
                }
                else {
                    return MonsterConfig;
                }
            }
        }
        // 单位等级
        public int Level {get; protected set;}
        // 单位战斗力
        public int FightPower {get; protected set;}
        // 单位是否是否
        public bool IsDead {get; protected set;}
        // 满血血量
        public int FullHP {get; protected set;}
        // 当前血量
        public int CurHP {get; protected set;}
        // 普通技能
        public BattleSkill NormalSkill {get; protected set;}

        public OneTimeTimerManager TimerMgr {get {return BattleManager.Instance.TimerMgr;}}

        // 战斗中需要用到的属性，初始化时赋值
        protected Dictionary<E_Fighter_Property, int> UnitPropertyDict = new Dictionary<E_Fighter_Property, int> ();

        // 挂在战斗单位身上的状态列表
        public readonly List<SkillState> CurStateList = new List<SkillState> ();
        // 状态特效记录表 effectid => {effectid, count, effectgo}
        Dictionary<string, StateEffectNode> _stateEffectDict = new Dictionary<string, StateEffectNode> ();

        // 死亡时的逻辑协程
        protected Continuation _deadTaskCont = null;

        // 当前接收到的消息
        UnitMessage _curMessage = null;
        // 当前逻辑
        Continuation _mainCont = null;

        // 判断是否为攻击目标，由子类实现具体逻辑
        public virtual bool IsTarget (BattleUnit unit) {
            if (null == unit) {
                return false;
            }
            // 用户战机
            if (UnitType == BattleUnitType.Fighter) {
                return (unit.UnitType == BattleUnitType.Monster);
            }
            // 怪
            else {
                return (unit.UnitType == BattleUnitType.Fighter);
            }
        }

        public void SetUnitType (BattleUnitType type) {
            UnitType = type;
        }

        void Awake () {
            GO = gameObject;
            Trans = transform;
            OnAwake ();
        }

        void OnDestroy () {
            //Debug.Log (GO.name + "was destroyed, id=" + BaseConfig.id + ", type=" + UnitType.ToString ());
        }

        // 掉血
        public void Hurt (int deltaHP) {
            CurHP = Mathf.Max (0, CurHP - deltaHP);
            OnUnitHurt (deltaHP);
            if (CurHP <= 0) {
                IsDead = true;
                _deadTaskCont = Continuation.Start (OnUnitDeadTask ());
            }
        }

        // 加血
        public void Heal (int deltaHP) {
            CurHP = Mathf.Min (FullHP, CurHP + deltaHP);
            OnUnitHeal (deltaHP);
        }

        // 死亡时做的一些清理操作（死亡逻辑执行完后调用）
        protected void CleanOnDead () {
            Continuation.ExitContinuation (_mainCont);
            Utils.DestroyGo (GO);
        }

        // 碰撞处理
        void OnTriggerEnter (Collider co) {
            GameObject go = co.gameObject;
            if (null == go) {
                return;
            }
            OnColliderTrigger (co);
        }

        protected void InitUnitProperty (Dictionary<E_Fighter_Property, int> dict) {
            foreach (var property in dict) {
                UnitPropertyDict.Add (property.Key, property.Value);
            }
        }

        // 修改战斗单位属性
        public void UpdateUnitProperty (E_Fighter_Property property, int delta) {
            if (!UnitPropertyDict.ContainsKey (property)) {
                UnitPropertyDict[property] = 0;
            }
            UnitPropertyDict[property] = Mathf.Max (0, UnitPropertyDict[property] + delta);
        }

        // 获取战斗单位属性（绝对值类型）
        public int GetUnitProperty (E_Fighter_Property property) {
            if (UnitPropertyDict.ContainsKey (property)) {
                return UnitPropertyDict[property];
            }
            return 0;
        }

        // 获取战斗单位属性（千分比类型，返回已经处理过的值）
        public float GetUnitPropertyOfPercent (E_Fighter_Property property) {
            if (UnitPropertyDict.ContainsKey (property)) {
                return UnitPropertyDict[property] / 1000f;
            }
            return 0f;
        }

        public int GetStateCount (int stateid) {
            int total = 0;
            for (int i=0; i<CurStateList.Count; i++) {
                if (null != CurStateList[i] && CurStateList[i].id == stateid) {
                    total += 1;
                }
            }
            return total;
        }

        public SkillState GetState (int stateid) {
            for (int i=0; i<CurStateList.Count; i++) {
                if (null != CurStateList[i] && CurStateList[i].id == stateid) {
                    return CurStateList[i];
                }
            }
            return null;
        }

        /// 添加buff
        public void AddSkillState (SkillState state) {
            CurStateList.Add (state);
            string effectid = state.EffectID;
            if (string.IsNullOrEmpty (effectid)) {
                return;
            }
            // 单位身上已存在此特效，引用数+1
            if (_stateEffectDict.ContainsKey (effectid)) {
                _stateEffectDict[effectid].count += 1;
                return;
            }
            // 单位身上还不存在此特效，生成特效，存到字典
            GameObject effect = BattleResManager.Instance.CreateEffect (effectid);
            if (null == effect) {
                return;
            }
            Utils.SetTransform (Trans, effect.transform, true);
            _stateEffectDict.Add (effectid, new StateEffectNode (effectid, effect));
        }

        /// 删除状态（只能由SkillState调用）
        public void RemoveSkillState (SkillState state) {
            if (null == state) {
                return;
            }
            for (int i=0; i<CurStateList.Count; i++) {
                if (state == CurStateList[i]) {
                    if (!string.IsNullOrEmpty (state.EffectID) && _stateEffectDict.ContainsKey (state.EffectID)) {
                        _stateEffectDict[state.EffectID].count -= 1;
                        if (_stateEffectDict[state.EffectID].count <= 0) {
                            RemoveFromStateDict (state.EffectID);
                        }
                    }
                    CurStateList[i] = null;
                }
            }
        }

        void RemoveFromStateDict (string effectid) {
            if (!_stateEffectDict.ContainsKey (effectid)) {
                return;
            }
            GameObject effect = _stateEffectDict[effectid].effect;
            if (null != effect) {
                GameObject.Destroy (effect);
            }
            _stateEffectDict.Remove (effectid);
        }

        /// 删除武将身上所有的buff
        public void ClearStateList (bool isClearPermanent=true) {
            for (int i=0; i<CurStateList.Count; i++) {
                SkillState state = CurStateList[i];
                if (null == state || (state.IsPermanent && !isClearPermanent)) {
                    continue;
                }
                state.Exit ();
            }
            if (isClearPermanent) {
                CurStateList.Clear ();
            }
        }

        /// 检查状态能否作用于战斗单位身上
        public bool IsStateUsable (SkillState s) {
            for (int i=0; i<CurStateList.Count; i++) {
                SkillState state = CurStateList[i];
                if (null != state && (state is SkillState_ImmuneState)) {
                    if ((state as SkillState_ImmuneState).IsStateImmuned (s)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// 判断是否处于某种Buff状态
        public bool IsInSkillState<T> () where T : SkillState {
            return null != GetSkillState<T> ();
        }

        /// 获取单位身上指定类型的状态
        public T GetSkillState<T> () where T : SkillState {
            for (int i=0; i<CurStateList.Count; i++) {
                SkillState state = CurStateList[i];
                if (null != state && (state is T)) {
                    return state as T;
                }
            }
            return null;
        }

        // 护盾 
        int _huDunCount = 0;
        // 是否存在护盾
        public bool IsInHuDunState {get {return _huDunCount > 0;}}
        // 当前护盾列表
        List<SkillState_HuDun> _hunDunList = new List<SkillState_HuDun> ();

        public void AddHuDun (SkillState_HuDun state) {
            _hunDunList.Add (state);
            _huDunCount ++;
        }

        // 删除护盾，不能直接Remove，只能置空
        public void RemoveHuDun (SkillState_HuDun state) {
            for (int i=0; i<_hunDunList.Count; i++) {
                if (state == _hunDunList[i]) {
                    _hunDunList[i] = null;
                    _huDunCount --;
                }
            }
        }

        /// 护盾吸收伤害，返回总的吸收伤害值
        public float AbsorbDamageByHuDun (float damage) {
            if (!IsInHuDunState) {
                return 0f;
            }
            float total = 0f;
            float current = damage;
            for (int i=0; i<_hunDunList.Count; i++) {
                if (null == _hunDunList[i]) {
                    continue;
                }
                float absorb = _hunDunList[i].AbsorbDamage (current);
                total += absorb;
                current -= absorb;
                if (current <= 0f) {
                    break;
                }
            }
            return total;
        }

        public float HunDuLeftPercentage {
            get {
                if (!IsInHuDunState) {
                    return 0f;
                }
                float percent = 0f;
                for (int i=0; i<_hunDunList.Count; i++) {
                    if (null == _hunDunList[i]) {
                        continue;
                    }
                    if (_hunDunList[i].CapacityPercentage > percent) {
                        percent = _hunDunList[i].CapacityPercentage;
                    }
                }
                return percent;
            }
        }

        public void InitNormalSkill (int skillid) {
            if (skillid <= 0) {
                NormalSkill = null;
                return;
            }
            NormalSkill = new BattleSkill (this, skillid);
        }

        public void StartBattle () {
            SendUnitMessage (new AttackMessage ());
        }

        public void SendUnitMessage (UnitMessage msg) {
            _curMessage = msg;
            Continuation.ExitContinuation (_mainCont);
            _mainCont = Continuation.Start (ExecUnitMesaage ());
        }

        IEnumerator ExecUnitMesaage () {
            while (true) {
                if (_curMessage is IdleMessage) {
                    yield return new WaitForSeconds (1f);
                }
                else if (_curMessage is AttackMessage) {
                    yield return Continuation.WaitForNextContinuation (ExecSkillTask (NormalSkill, true));
                }
                else {
                    yield return null;
                }
            }
        }

        // 执行技能
        IEnumerator ExecSkillTask (BattleSkill skill, bool isLoop=false) {
            while (true) {
                yield return Continuation.WaitForNextContinuation (skill.Use ());
                if (!isLoop) {
                    break;
                }
                yield return Continuation.WaitForSeconds (skill.CoolDown, TimerMgr);
            }
        }

        // 发射节点缓存字典
        protected Dictionary<string, Transform> ShootJointCacheDict = new Dictionary<string, Transform> ();
        // 获取发射节点
        protected Transform GetJointTrans (string jointName) {
            if (string.IsNullOrEmpty (jointName)) {
                return Trans;
            }
            if (ShootJointCacheDict.ContainsKey (jointName)) {
                return ShootJointCacheDict[jointName];
            }
            Transform t = Trans.Find (jointName);
            if (null == t) {
                return Trans;
            }
            ShootJointCacheDict.Add (jointName, t);
            return t;
        }

        // 发射子弹
        public IEnumerator ShootBullet (BattleSkill skill, int bulletid, string joint, string effect, List<int> aiListOnHit) {
            TBullet bulletConfig = ConfigTableBullet.Instance.GetConfigByID (bulletid);
            if (null == bulletConfig) {
                yield break;
            }
            // 设置发射位置
            Transform t = GetJointTrans (joint);
            // 创建发射特效
            /*
            if (!string.IsNullOrEmpty (effect)) {
                GameObject effectGo = BattleResManager.Instance.CreateBulletEffectGo (effect);
                if (null != effectGo) {
                    effectGo.transform.position = t.position;
                }
            }
            */
            int count = 0;
            while (true) {
                CreateBullet (skill, bulletConfig, t.position, aiListOnHit);
                count ++;
                if (count >= bulletConfig.amount) {
                    break;
                }
                if (bulletConfig.frequency > 0) {
                    yield return Continuation.WaitForSeconds (bulletConfig.frequency / 1000f, TimerMgr);
                } else {
                    yield return Continuation.WaitForSeconds (5/60f, TimerMgr);
                }
            }
        }

        void CreateBullet (BattleSkill skill, TBullet cfg, Vector3 position, List<int> aiListOnHit) {
            GameObject bulletGo = BattleResManager.Instance.CreateBulletGo (cfg.prefab);
            if (null == bulletGo) {
                return;
            }
            // 设置发射位置
            bulletGo.transform.position = position;
            BulletController controller = bulletGo.AddComponent<BulletController> ();
            controller.Init (this, skill, cfg);
            controller.Fire (Trans.forward, aiListOnHit);
        }
    }
}
