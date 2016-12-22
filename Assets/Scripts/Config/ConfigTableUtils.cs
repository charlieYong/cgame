using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using SLMS;


namespace Sgame
{

    public class TCostRes : ConfigTableRow
    {
        public ResType type;
        public int needNum;

        public TCostRes(ResType resType, int num) {
            type = resType;
            needNum = num;
        }

        public TCostRes(string text, char separator) {
            string[] cost = text.Split(separator);
            type = (ResType)int.Parse(cost[0]);
            needNum = int.Parse(cost[1]);
        }
    }
}