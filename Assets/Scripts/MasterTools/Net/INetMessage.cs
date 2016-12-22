using UnityEngine;
using System.Collections;
using System.IO;

namespace ZR_MasterTools
{
    public interface INetMessageWriter
    {
        byte[] MakeDataStream(int cmd, MemoryStream mem, ushort seq);
        void Reset();
    }

    public interface INetMessageReader
    {
        void ReadData(byte[] data, int size);
        void Reset();
    }

    public interface IMessageHandler
    {
        void HandleMessage(object messageKey, MemoryStream data);
    }
}