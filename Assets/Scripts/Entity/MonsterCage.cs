using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sgame {
    [System.Serializable]
    public class MonsterCage {
        // 笼子位置
        public Vector3 position;
        // 行为id
        public int motionid;
        // 怪物组
        public List<MonsterGroup> monsterGroupList;
    }

    [System.Serializable]
    public class MonsterGroup {
        // 出场方式
        public int showType;
        // 概率值
        public int percent;
        // 怪物列表
        public List<MonsterItemOnScene> monsterList;
    }

    [System.Serializable]
    public class MonsterItemOnScene {
        // 概率值
        public int percent;
        // monster id
        public int monsterid;
        // 位置信息
        public Vector3 position;
        // 旋转信息
        public Vector3 eulerAngels;
    }

    public class MonsterCageHelper {
        // 测试函数，创建一个笼子
        public MonsterCage CreateTestMonsterCage (Transform t) {
            MonsterCage c = new MonsterCage ();
            c.position = t.position;
            c.motionid = 1;
            c.monsterGroupList = new List<MonsterGroup> ();

            for (int i=0; i<2; i++) {
                MonsterGroup g = new MonsterGroup ();
                g.showType = i;
                g.percent = 100;
                g.monsterList = new List<MonsterItemOnScene> ();
                for (int j=0; j<3; j++) {
                    MonsterItemOnScene monster = new MonsterItemOnScene ();
                    monster.percent = 1000;
                    monster.monsterid = j;
                    monster.position = t.position;
                    monster.eulerAngels = t.eulerAngles;

                    g.monsterList.Add (monster);
                }
                c.monsterGroupList.Add (g);
            }
            return c;
        }

        public string MonsterCageToJson (MonsterCage c) {
            return JsonUtility.ToJson (c);
        }

        public MonsterCage MonsterCageFromJson (string json) {
            return JsonUtility.FromJson<MonsterCage> (json);
        }
    }
}
