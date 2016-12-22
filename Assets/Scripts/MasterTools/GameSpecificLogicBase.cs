using UnityEngine;
using System.Collections;
using System.Text;


namespace ZR_MasterTools
{
    /// <summary>
    /// 不同项目特殊的逻辑接口基类，游戏项目继承该类后实现对应逻辑
    /// </summary>
    public class GameSpecificLogicBase
    {
        #region 数据、逻辑相关
        /// 判断数据包体是否需要加密
        public virtual bool NeedEncodePacket(int packageKey, out string encodeKey) {
            encodeKey = string.Empty;
            return false;
        }

        /// 对数据包体进行加密
        public virtual void EncodePacketBody(byte[] data, int len, string key) {
            byte[] token = Encoding.ASCII.GetBytes(key);
            int tokenLen = token.Length;
            int j = 0, end = 0, step = 0, dir = 0;
            for (int i = 0; i < len; ) {
                j = (0 == dir) ? 0 : (tokenLen - 1);
                end = (0 == dir) ? tokenLen : -1;
                step = (0 == dir) ? 1 : -1;
                while (j != end) {
                    data[i] = (byte)(data[i] ^ token[j]);
                    i++;
                    j += step;
                    if (i >= len) {
                        return;
                    }
                }
                dir = 1 - dir;
            }
        }

        /// 获取包头uin
        public virtual ulong GetProtoHeaderUin() {
            return 0;
        }

        /// 获取数据包key对应的文本，主要用于log显示
        public virtual string GetPacketKeyText(int key) {
            return key.ToString();
        }

        /// 判断是否需要双倍回包时间，如支付包需要延长超时时间（服务器跟第三方服务器校验耗时较长）
        public virtual bool IsPacketNeedDoubleTimeout(int packetKey) {
            return false;
        }

        public virtual bool IsLoginToGamePacket(int packetKey) {
            return false;
        }
        #endregion



        #region 界面显示相关
        /// 显示转圈
        public virtual void ShowConnecting(bool show) { }

        /// 锁屏
        public virtual void EnableInteractive(bool enable) { }
        #endregion
    }
}