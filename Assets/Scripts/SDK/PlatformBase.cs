using UnityEngine;
using System.Collections;


namespace Sgame
{
    public class PlatformBase
    {

        protected const string UnityClassName = "com.unity3d.player.UnityPlayer";
        protected const string ActivityName = "currentActivity";


        // 渠道号
        public string ChannelID { get; protected set; }
        public string AppID { get; protected set; }

        // 登录授权Token
        public string AuthToken { get; protected set; }
        // 登录用户信息
        public string UserID { get; protected set; }
        public string UserName { get; protected set; }
        public bool IsFemale { get; protected set; }


        /// 设置Userid，有的渠道sdk授权时不会返回userid，需要服务器回传再设置
        public virtual void SetUserID(string userid) {
            if (string.IsNullOrEmpty(UserID) && !string.IsNullOrEmpty(userid)) {
                UserID = userid;
            }
        }


        public virtual void Login() { }
    }
}