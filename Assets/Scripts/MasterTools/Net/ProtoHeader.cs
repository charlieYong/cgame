using UnityEngine;
using System.Collections;
using System.Net;
using System;


namespace ZR_MasterTools
{
    public class ProtoHeader
    {
        public int m_nPackLen;
        public ushort m_nCmd;
        public ushort m_nSeq;
        public int m_nTmstamp;
        public ulong m_nUin;

        public static readonly int HEAD_LEN = 20;

        public void reset() {
            m_nPackLen = 0;
            m_nCmd = 0;
            m_nSeq = 0;
            m_nTmstamp = 0;
            m_nUin = 0;
        }

        public void readFromBytes(byte[] buffer) {
            m_nPackLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));

            m_nCmd = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 4));

            m_nSeq = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 6));

            m_nTmstamp = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 8));

            m_nUin = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, 12));
        }

        public void write2Bytes(byte[] buffer) {
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(m_nPackLen)), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)m_nCmd)), 0, buffer, 4, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)m_nSeq)), 0, buffer, 6, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(m_nTmstamp)), 0, buffer, 8, 4);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)m_nUin)), 0, buffer, 12, 8);
        }
    }
}