using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 战斗资源管理类
    public class BattleResManager : MonoBehaviour {
        // Resources目录路径
        const string BulletPrefabPath = "Effects/";
        const string MonsterPrefabPath = "Models/";
        const string EffectPrefabPath = "Effects/";

        // TODO 资源预加载和对象池复用

        public static BattleResManager Instance {get; private set;}

        // prefab缓存字典：path -> prefab
        Dictionary<string, GameObject> PrefabDict = new Dictionary<string, GameObject> ();

        void Awake () {
            Instance = this;
        }

        // 加载prefab
        GameObject LoadPrefab (string path) {
            if (PrefabDict.ContainsKey (path)) {
                return PrefabDict[path];
            }
            GameObject prefab = BundleMgr.Load<GameObject> (path);
            if (null == prefab) {
                Debug.LogWarning ("fail to load prefab of battle, path=" + path);
                // 测试使用
                return BundleMgr.Load<GameObject> ("Models/M2002");
                //return null;
            }
            PrefabDict.Add (path, prefab);
            return prefab;
        }

        // 根据prefab创建gameobject
        GameObject InstantGoFromPrefab (GameObject prefab) {
            if (null == prefab) {
                return null;
            }
            return GameObject.Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;            
        }

        // 创建怪物模型
        public GameObject CreateMonsterGo (string prefabName) {
            return InstantGoFromPrefab (LoadPrefab (MonsterPrefabPath + prefabName));
        }

        // 创建子弹模型
        public GameObject CreateBulletGo (string prefabName) {
            return InstantGoFromPrefab (LoadPrefab (BulletPrefabPath + prefabName));
        }

        // 创建子弹特效（临时）
        public GameObject CreateBulletEffectGo (string name) {
            return CreateBulletGo (name);
        }

        // 创建特效
        public GameObject CreateEffect (string effectName) {
            return InstantGoFromPrefab (LoadPrefab (EffectPrefabPath + effectName));
        }
    }
}
