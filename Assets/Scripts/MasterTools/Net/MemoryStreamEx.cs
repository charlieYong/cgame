using UnityEngine;
using System.Collections;
using System.IO;

namespace ZR_MasterTools
{
    public class MemoryStreamEx : MemoryStream
    {
        public void Clear() {
            this.SetLength(0L);
        }
    }
}