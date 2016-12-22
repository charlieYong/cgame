using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;


namespace Sgame
{
    public class PilotMgr : MonoSingleton<PilotMgr>
    {
        public Dictionary<int, Pilot> pilotDict = new Dictionary<int, Pilot>();


        public bool HasOwnPilot(int pilotID) {
            return pilotDict.ContainsKey(pilotID);
        }
    }
}