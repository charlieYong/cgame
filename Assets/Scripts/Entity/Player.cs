using UnityEngine;
using System.Collections;



namespace Sgame
{
    public class Player
    {
        public long guid;
        public string name;
        public bool isFemale;
        public int level;
        public int exp;
        public int militaryRank = 1;

        public int energy;  // 体力
        public int diamondNum; // 钻石
        public int goldNum = 99999999;    // 金币
    }
}