using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 战斗类型
    public enum BattleType {
        Invalid,
        Endless, // 无尽模式
        Max,
    }

    // 战斗参数类，由上层业务系统构造和传入
    public class BattleParams {
        public BattleType type;
        public int battleFieldID;
        public ConfigTableRow config;
        public System.Action<bool> onFinish;
        // 己方战机
        public Fighter fighter;
        // 帮手（双打模式时可用）
        public Fighter partner;
    }

    /// 战斗数据控制类
    public class BattleDataManager : DataSingleton<BattleDataManager> {
        public BattleParams Params {get; protected set;}
        public BattleType CurBattleType {get {return Params.type;}}

        // 开始战斗
        public void StartBattle (BattleParams obj) {
            Params = obj;
            BattleManager.Start ();
        }

    }
}
