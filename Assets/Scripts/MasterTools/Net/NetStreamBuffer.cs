using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    internal class NetStreamBuffer
    {
        private int m_nActivedStreamPosition;
        private const int m_nMaxStreamCount = 2;
        private ArrayList m_streamList = new ArrayList();

        public NetStreamBuffer() {
            for (int i = 0; i < 2; i++) {
                this.m_streamList.Add(new MemoryStreamEx());
            }
        }

        public MemoryStreamEx GetActivedStream() {
            return (MemoryStreamEx)this.m_streamList[this.m_nActivedStreamPosition];
        }

        public MemoryStreamEx MoveStream(int index) {
            MemoryStreamEx ex = (MemoryStreamEx)this.m_streamList[this.m_nActivedStreamPosition];
            if (index > 0) {
                if (index < ex.Length) {
                    this.m_nActivedStreamPosition = (this.m_nActivedStreamPosition + 1) % 2;
                    MemoryStreamEx ex2 = (MemoryStreamEx)this.m_streamList[this.m_nActivedStreamPosition];
                    ex2.Clear();
                    ex2.Write(ex.GetBuffer(), index, ((int)ex.Length) - index);
                    ex.Clear();
                    return ex2;
                }
                ex.Clear();
            }
            return ex;
        }

        public void Reset() {
            for (int i = 0; i < 2; i++) {
                ((MemoryStreamEx)this.m_streamList[i]).Clear();
            }
        }
    }
}