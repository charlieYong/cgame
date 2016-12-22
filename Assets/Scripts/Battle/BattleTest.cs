using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;
using Sgame;
using Sgame.Battle;

public class BattleTest : MonoBehaviour {
    void Start () {
        //CreateTestMonster ();
        StartEndlessBattle ();
    }
        
    void StartEndlessBattle () {
        TEndlessPve config = ConfigTableEndlessPve.Instance.GetConfigByID (10001);
        BattleParams param = new BattleParams ();
        param.battleFieldID = config.id;
        param.config = config;
        param.type = BattleType.Endless;
        param.onFinish = OnEndlessBattleEnd;
        param.fighter = new Fighter (1, 1, 2);

        BattleDataManager.Instance.StartBattle (param);
    }

    void OnEndlessBattleEnd (bool result) {
        Debug.Log ("endless battle end, result:" + result.ToString ());
    }

    void CreateTestMonster () {
        Transform t = transform;
        // 笼子
        MonsterCage c = new MonsterCage ();
        c.motionid = 0;
        c.position = transform.position;
        c.monsterGroupList = new List<MonsterGroup> ();
        // 怪物组
        MonsterGroup group = new MonsterGroup ();
        group.percent = 1000;
        group.showType = 1;
        group.monsterList = new List<MonsterItemOnScene> ();
        // 怪物
        for (int i=1; i<=5; i++) {
            MonsterItemOnScene monster = new MonsterItemOnScene ();
            monster.monsterid = i;
            monster.percent = 1000;
            monster.position = t.position + (Vector3.left * 3 * i) + (Vector3.up * 4 * (i%2));
            monster.eulerAngels = t.eulerAngles;
            group.monsterList.Add (monster);
        }
        c.monsterGroupList.Add (group);

        Debug.Log (JsonUtility.ToJson (c));
    }
}
