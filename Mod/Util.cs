using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using System.Text;

namespace Util
{
    public static class S
    {
        public static string Dit() => WindowManage.instance.Dit();
        public static string Cut(int color = 20002) => WindowManage.instance.Cut(color);
        public static string Color(int color, object s) => DateFile.instance.SetColoer(color, s.ToString());
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
    }
}
