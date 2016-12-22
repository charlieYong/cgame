using UnityEngine;
using System.Collections;
using System.IO;


namespace ZR_MasterTools
{
    public class NetStreamWriter : INetMessageWriter
    {
        private MemoryStreamEx m_Buffer = new MemoryStreamEx();

        private ProtoHeader header = new ProtoHeader();

        byte[] INetMessageWriter.MakeDataStream(int cmd, MemoryStream mem, ushort seq) {
            //Debug.Log (string.Format ("Packet CmdID={0}, Seq={1}", ((CmdID)cmd).ToString (), seq));
            this.m_Buffer.Clear();

            header.m_nPackLen = ProtoHeader.HEAD_LEN + ((mem == null) ? 0 : ((int)mem.Length));
            header.m_nCmd = (ushort)cmd;
            header.m_nSeq = seq; // 0 for async packet
            header.m_nTmstamp = TimeManager.GetServerTimestamp();
            header.m_nUin = NetworkMgr.Instance.GameSpecificLogic.GetProtoHeaderUin();

            byte[] headerBytes = new byte[ProtoHeader.HEAD_LEN];
            header.write2Bytes(headerBytes);
            this.m_Buffer.Write(headerBytes, 0, headerBytes.Length);

            if (mem != null) {
                byte[] data = mem.ToArray();
                int dataLen = data.Length;
                // 处理加密
                //string encodeKey = null;
                //if (NetworkMgr.Instance.GameSpecificLogic.NeedEncodePacket(cmd, out encodeKey)) {
                //    NetworkMgr.Instance.GameSpecificLogic.EncodePacketBody(data, dataLen, encodeKey);
                //}
                this.m_Buffer.Write(data, 0, dataLen);
            }
            //Test(m_Buffer);
            return this.m_Buffer.ToArray();
        }

        void INetMessageWriter.Reset() {
            this.m_Buffer.Clear();
        }
    }
}