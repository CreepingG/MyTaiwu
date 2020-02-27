using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Text;

namespace WindUtil
{
    public static class S
    {
        public static string Dit() => WindowManage.instance.Dit();
        public static string Cut(int color = 20002) => WindowManage.instance.Cut(color);
        public static string Color(int color, string s) => DateFile.instance.SetColoer(color, s);
        public static string Color(int color, object s) => DateFile.instance.SetColoer(color, s.ToString());
        public static string ColorName(string name) => Color(10002, name);
        public static int ToInt(this string s) => Convert.ToInt32(s);
        public static decimal ToDecimal(this string s) => decimal.Parse(s);
    }

    public static class G
    {
        public static void Show(object text)
        {
            int id = -489819;
            var date = DateFile.instance.tipsMassageDate;
            if (!date.ContainsKey(id))
            {
                date.Add(id, new Dictionary<int, string>
                {
                    [1] = "3",
                    [2] = "0",
                    [99] = "D0"
                });
            }
            TipsWindow.instance.SetTips(id, new string[] { text.ToString() }, 300);
        }
        public static string GetActorName(int actorId, bool fullName = false, bool showGang = false)
        {
            string actorName = DateFile.instance.GetActorName(actorId, fullName, false);
            if (!showGang) return actorName;
            int gang = DateFile.instance.GetActorDate(actorId, 19, false).ToInt();
            string gangName = DateFile.instance.GetGangDate(gang, 0);
            int num20 = DateFile.instance.GetActorDate(actorId, 20, false).ToInt();
            int grade = 10 - Mathf.Abs(num20);
            grade = Mathf.Clamp(grade, 1, 9);
            int gangValueId = DateFile.instance.GetGangValueId(gang, 10 - grade);
            bool isMale = DateFile.instance.GetActorDate(actorId, 14) == "1";
            int levelKey = num20 < 0 ? (isMale ? 1002 : 1003) : 1001;
            string levelName = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(gang, num20)][levelKey];
            levelName = DateFile.instance.SetColoer(20001 + grade, levelName, false);

            return gangName + levelName + " " + actorName;
        }
    }
    
}
