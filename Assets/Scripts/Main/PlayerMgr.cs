using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using SLMS;


namespace Sgame
{
    public class PlayerMgr : MonoSingleton<PlayerMgr>
    {
        public LastLevelData lastLevelData = new LastLevelData();
        Player _player = new Player();
        public Player player { get { return _player; } }



        void OnSynAttr(object data) {
            bool isUpLevel = false;
            scSynAttr syn = (scSynAttr)data;
            if (0 >= _player.guid) {
                _player.guid = (long)syn.guid;
            }
            if (syn.lvlSpecified) {
                isUpLevel = (0 != _player.level) && (syn.lvl > _player.level);
                if (isUpLevel) {
                    lastLevelData.Setup(_player);
                }
                _player.level = (int)syn.lvl;
            }
            if (syn.expSpecified) {
                _player.exp = (int)syn.exp;
            }
            if (syn.nameSpecified) {
                _player.name = syn.name;
            }
            if (syn.sexSpecified) {
                _player.isFemale = (1 == syn.sex);
            }
            if (syn.moneySpecified) {
                _player.goldNum = (int)syn.money;
            }
            if (syn.diamondSpecified) {
                _player.diamondNum = (int)syn.diamond;
            }
        }



        public class LastLevelData
        {
            public int level;
            public int energy;
            public int maxEnergy;

            public void Setup(Player player) {
                level = player.level;
                energy = player.energy;
            }
        }
    }
}