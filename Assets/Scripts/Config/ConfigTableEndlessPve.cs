using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    public class PercentPairItem {
        public int value;
        // 千分数，如200，则表示200/1000
        public int percent;

        public PercentPairItem (string pair) {
            string[] items = pair.Split (':');
            value = int.Parse (items[0]);
            percent = int.Parse (items[1]);
        }

        public PercentPairItem (int v, int p) {
            value = v;
            percent = p;
        }
    }

    public class TEndlessPve : ConfigTableRow {
        public int id;
        public int transitionid;
        // pveid:percent
        public List<PercentPairItem> battleFieldPercentList;

        // 根据概率获取战场id
        public int GetRandomBattleFieldID (int curPveID=-1) {
            int p = Random.Range (1, 1000);
            int delta = 0;
            for (int i=0; i<battleFieldPercentList.Count; i++) {
                PercentPairItem pair = battleFieldPercentList[i];
                if ((pair.percent + delta) > p) {
                    if (pair.value == curPveID) {
                        // 如果随机到当前值，则取下一个值
                        return battleFieldPercentList[(i+1) % battleFieldPercentList.Count].value;
                    }
                    return pair.value;
                } else {
                    delta += pair.percent;
                }
            }
            // 没取到区间值？取最后一个值
            return battleFieldPercentList[battleFieldPercentList.Count - 1].value;
        }

        public TBattleField GetRandomBattleField (int curPveID=-1) {
            return ConfigTableBattleField.Instance.GetConfigByID (GetRandomBattleFieldID (curPveID));
        }
    }

    // 无尽模式配置表
    public class ConfigTableEndlessPve : ConfigTableController<ConfigTableEndlessPve> {

        protected override void Init () {
            base.Init ();
            _filename = "endless_pve";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TEndlessPve data = new TEndlessPve ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.transitionid = int.Parse (s[i++]);
            data.battleFieldPercentList = new List<PercentPairItem> ();
            foreach (string item in s[i++].Split ('|')) {
                data.battleFieldPercentList.Add (new PercentPairItem (item));
            }
            return data;
        }

        public TEndlessPve GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TEndlessPve;
        }
    }
}