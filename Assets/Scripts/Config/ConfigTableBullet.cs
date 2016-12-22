using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame {
    public class TBullet : ConfigTableRow {
        public int id;
        public int type;
        public string param;
        public string prefab;
        public int frequency;
        public int amount;
        public string effectOnHit;
    }

    public class ConfigTableBullet : ConfigTableController<ConfigTableBullet> {

        protected override void Init () {
            base.Init ();
            _filename = "bullet";
            _fileList = new string[] {_filename};
        }

        protected override void LoadFile () {
            base.LoadFile ();
            CreateConfigFileWithIndex ();
        }

        ConfigTableRow ParseRow (string[] s) {
            TBullet data = new TBullet ();
            int i = 0;
            data.id = int.Parse (s[i++]);
            data.type = int.Parse (s[i++]);
            data.param = s[i++];
            data.prefab = s[i++];
            data.frequency = int.Parse (s[i++]);
            data.amount = int.Parse (s[i++]);
            data.effectOnHit = s[i++];
            return data;
        }

        public TBullet GetConfigByID (int id) {
            return GetDataByIndex (id.ToString (), ParseRow) as TBullet;
        }
    }
}