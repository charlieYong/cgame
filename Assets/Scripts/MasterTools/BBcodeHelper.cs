using UnityEngine;
using System.Collections;


namespace ZR_MasterTools
{
    public class BBcodeHelper
    {
        public static string MarkUpText(string symbol, string text, string end = "-") {
            return string.Format("[{0}]{1}[{2}]", symbol, text, end);
        }

        public static string ColorText(Color color, string text) {
            return BBcodeHelper.MarkUpText(NGUIText.EncodeColor(color), text);
        }

        public static string BoldText(object text) {
            return BBcodeHelper.MarkUpText("b", text.ToString(), "/b");
        }

        public static string ItalicizeText(string text) {
            return BBcodeHelper.MarkUpText("i", text, "/i");
        }

        public static string UnderlineText(string text) {
            return BBcodeHelper.MarkUpText("u", text, "/u");
        }

        public static string Strikethrough(string text) {
            return BBcodeHelper.MarkUpText("s", text, "/s");
        }

        public static string ColorText(string color, string text, bool isBold = false) {
            string newText = MarkUpText(color, text);
            if (isBold) {
                newText = BoldText(newText);
            }
            return newText;
        }

        public static string ColorText_Resource(int needNum, int ownNum, int showNum, bool isBold = false, string enoughColor = "EDB070") {
            string newText = MarkUpText(needNum > ownNum ? BBcodeHelper.Red : enoughColor, showNum.ToString());
            if (isBold) {
                newText = BoldText(newText);
            }
            return newText;
        }

        // 颜色代码
        public static readonly string Red = "FF0000"; // ( 255, 0, 0 )
        public static readonly string OliveGreen = "8FC31F"; // ( 143, 195, 31 )
        public static readonly string DarkOrange = "DF6E37"; // ( 223, 110, 55 )
        public static readonly string Yellow = "FFD200"; // ( 255, 210, 0 )
        public static readonly string Brown = "D49E5F"; // ( 212, 158, 94 )
        public static readonly string Blue = "4FA49C"; // ( 79, 164, 156 )
        public static readonly string LightBlue = "00FFFF"; // ( 0, 255, 255 )
        public static readonly string LightYellow = "FFF247"; // ( 255, 242, 71 )
        public static readonly string OrangeRed = "D65116"; // ( 214, 81, 22 )
        public static readonly string Grey = "808080"; // ( 128, 128, 128 )
        public static readonly string LightGrey = "D1C0A5"; // ( 209, 192, 165 )
        public static readonly string White = "FFFFFF"; // ( 255, 255, 255 )
        public static readonly string Name = "FFEFCC"; // ( 255, 239, 204 )
        public static readonly string Purple = "FF00FF"; // ( 255, 0, 255 )
        public static readonly string Wheat = "F4D17D"; // ( 244, 209, 125 )
        public static readonly string Green = "00FF00"; // ( 0, 255, 0 )
        public static readonly string DarkBrown = "4F2B0A"; // ( 79, 43, 10 )
        public static readonly string DarkRed = "7D0000"; // ( 125， 0， 0 )
        public static readonly string Black = "000000"; // ( 0, 0, 0 )
        public static readonly string DarkGrey = "A0A0A0"; // (160, 160, 160）
        public static readonly string NormalYellow = "FFFF00";  // （255, 255, 0）
        public static readonly string Orange = "FF781E"; // （255, 120, 30）
        public static readonly string LockGrey = "53514D"; // （83， 81， 77）
        public static readonly string DarkGreen = "54FE26"; // （84， 254， 38）
    }
}