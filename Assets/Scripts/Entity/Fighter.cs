using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sgame {
    public class Fighter {
        public TFighter Config {get; private set;}
        public int index;
        public int lv;
        // debug
        public int fightPower = 100;

        public Fighter (int idx, int id, int level) {
            index = idx;
            lv = level;
            Config = ConfigTableFighter.Instance.GetConfigByID (id);
            if (null == Config) {
                Debug.LogWarning ("cannot find config of fighter: id=" + id);
            }
        }
    }
}