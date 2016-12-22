using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 场景怪物配置管理类
    public class SceneMonsterManager {
        int _configid;
        public readonly List<MonsterCage> CageList = new List<MonsterCage> ();

        public SceneMonsterManager (int sulotionid) {
            _configid = sulotionid;
            LoadSolution ();
        }

        // 配置文件
        string GetPathNameByID () {
            return "test_scene_monster";
        }

        // 根据id加载战斗场景怪物配置
        void LoadSolution () {
            string text = Utils.ReadConfigFile (GetPathNameByID ());
            if (string.IsNullOrEmpty (text)) {
                Debug.LogWarning ("no config of scene solution:" + GetPathNameByID ());
                return;
            }
            string[] rowList = text.Split ('\n');
            for (int i=0; i<rowList.Length; i++) {
                if (string.IsNullOrEmpty (rowList[i])) {
                    continue;
                }
                // id json
                string[] items = rowList[i].Split ('\t');
                if (items.Length != 2) {
                    continue;
                }
                CageList.Add (JsonUtility.FromJson<MonsterCage> (items[1]));
            }
            // TODO 这里根据距离做一次排序？
        }

        public void Update () {
            List<MonsterCage> list = new List<MonsterCage> ();
            for (int i=0; i<CageList.Count; i++) {
                MonsterCage cage = CageList[i];
                if (Utils.IsOnScreen (BattleManager.Instance.BattleCamera, cage.position, 0f)) {
                    list.Add (cage);
                }
                else {
                    // 找到第一个不在屏幕的笼子后，直接跳出（笼子按距离排序）
                    break;
                }
            }
            for (int i=0; i<list.Count; i++) {
                CageList.Remove (list[i]);
            }
            Continuation.Start (CreateMonsterCageList (list));
        }

        bool CheckRandomPercent (int percent) {
            return percent >= Random.Range (1, 1000);
        }

        IEnumerator CreateMonsterCageList (List<MonsterCage> list) {
            for (int i=0; i<list.Count; i++) {
                MonsterCage cage = list[i];
                // 判断生成概率
                int gRandom = Random.Range (1, 1000);
                int gPrev = 0;
                for (int j=0; j<cage.monsterGroupList.Count; j++) {
                    MonsterGroup gp = cage.monsterGroupList[j];
                    if (gp.percent + gPrev >= gRandom) {
                        // create group
                        for (int k=0; k<gp.monsterList.Count; k++) {
                            MonsterItemOnScene item = gp.monsterList[k];
                            if (CheckRandomPercent (item.percent)) {
                                BattleManager.Instance.CreateMonster (item);
                                yield return Continuation.WaitForSeconds (1/50f, BattleManager.Instance.TimerMgr);
                            }
                        }
                        break;
                    }
                    else {
                        gPrev += gp.percent;
                    }
                }
            }
        }
    }
}
