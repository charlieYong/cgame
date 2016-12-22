using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ZR_MasterTools
{
    public class RandomUserName
    {
        public enum NameType
        {
            FamilyName = 1,
            Male,
            Female,
            Max
        }

        /// 根据性别获取随机名字
        public static string GetName(bool male) {
            List<string> _familyNameList = new List<string>();
            List<string> _maleNameList = new List<string>();
            List<string> _femaleNameList = new List<string>();

            string text = Utils.ReadConfigFile("random_username");
            string[] rowList = text.Split('\n');
            for (int i = 0; i < rowList.Length; i++) {
                if (string.IsNullOrEmpty(rowList[i])) {
                    continue;
                }
                string[] cols = rowList[i].Split('\t');
                int type = int.Parse(cols[1]);
                string n = cols[0].Trim();
                switch ((NameType)type) {
                    case NameType.FamilyName:
                        _familyNameList.Add(n);
                        break;
                    case NameType.Male:
                        _maleNameList.Add(n);
                        break;
                    case NameType.Female:
                        _femaleNameList.Add(n);
                        break;
                }
            }

            if ((0 >= _familyNameList.Count) || (0 >= _maleNameList.Count) || (0 >= _femaleNameList.Count)) {
                Debug.LogWarning("NameList is Empty");
                return string.Empty;
            }
            int family = ThreadSafeRandom.ThisThreadsRandom.Next(0, _familyNameList.Count);
            int name;
            if (male) {
                name = ThreadSafeRandom.ThisThreadsRandom.Next(0, _maleNameList.Count);
                return string.Format("{0}{1}", _familyNameList[family], _maleNameList[name]);
            }
            name = ThreadSafeRandom.ThisThreadsRandom.Next(0, _femaleNameList.Count);
            return string.Format("{0}{1}", _familyNameList[family], _femaleNameList[name]);
        }
    }
}