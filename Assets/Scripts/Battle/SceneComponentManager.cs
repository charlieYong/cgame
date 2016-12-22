using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 场景怪物配置管理类
    public class SceneComponentManager {
        int _configid;
        // 场景组件挂载点列表
        List<Transform> _componentTransList = new List<Transform> ();
        // 场景组件配置列表
        public readonly Dictionary<string, int> ComponentDict = new Dictionary<string, int> ();

        public SceneComponentManager (int id) {
            _configid = id;
            LoadSolution ();
        }

        string GetPathNameByID () {
            return string.Format ("SceneComponentConf/{0}", _configid);
        }

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
                // positionName monsterid
                string[] items = rowList[i].Split ('\t');
                if (items.Length != 2) {
                    Debug.LogWarning ("invalid config of scene solution:" + rowList[i]);
                    continue;
                }
                ComponentDict[items[0].Trim ()] = int.Parse (items[1]);
            }
        }

        // 设置挂载点负对象
        public void SetTransformRoot (Transform root) {
            for (int i=0; i<root.childCount; i++) {
                Transform child = root.GetChild (i);
                if (!child.gameObject.activeSelf) {
                    continue;
                }
                if (!ComponentDict.ContainsKey (child.name)) {
                    continue;
                }
                _componentTransList.Add (child);
            }
        }

        public void Update () {
            if (_componentTransList.Count <= 0) {
                return;
            }
            // 根据当前移动位置判断是否需要创建对象
            List<Transform> list = new List<Transform> ();
            for (int i=0; i<_componentTransList.Count; i++) {
                list.Add (_componentTransList[i]);
            }
            // remove first
            for (int i=0; i<list.Count; i++) {
                _componentTransList.Remove (list[i]);
            }
            Continuation.Start (CreateComponentList (list));
        }

        IEnumerator CreateComponentList (List<Transform> list) {
            for (int i=0; i<list.Count; i++) {
                Transform item = list[i];
                if (!ComponentDict.ContainsKey (item.name)) {
                    continue;
                }
                BattleManager.Instance.CreateSceneMonster (ComponentDict[item.name], item);
                yield return Continuation.WaitForSeconds (0.1f, BattleManager.Instance.TimerMgr);
            }
        }
    }
}
