using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MyFunc
{
    class Universal
    {
        static T KeyOfMaxValue<T>(IEnumerable<T> enumerable, Func<T, int> Key2Value)//×îÓÅ
        {
            T result = enumerable.First();
            int maxValue = int.MinValue;
            foreach (T key in enumerable)
            {
                int value = Key2Value(key);
                if (value > maxValue)
                {
                    result = key;
                    maxValue = value;
                }
            }
            return result;
        }
        static void PrintAll<T>(IEnumerable<T> enumerable, Action<string> printAction, bool multilines = true)//´òÓ¡
        {
            if (multilines)
            {
                foreach (T key in enumerable)
                {
                    printAction(key.ToString());
                }
            }
            else
            {
                printAction(string.Join(" ", enumerable.Select(obj => obj.ToString()).ToArray()));
            }

        }
        static void SetValue(Type type, object instance, string fieldName, object value)
        {
            type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value);
        } 
        static object InvokeMethod(Type type, object instance, string methodName, params object[] args)
        {
            var methodInfo = Type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return methodInfo.Invoke(instance, args);
        }

		static string CheckMethodInfo(MethodInfo mi)
		{
			string result = "";
			var paras = mi.GetParameters();
            foreach(var para in paras)
            {
				result += $"{para.ParameterType.ToString()} {para.Name.ToString()}\n";
            }
            result += $"return {mi.ReturnParameter.ParameterType.ToString()}"
	        return result;
		}
        
    }
    class Taiwu
    {
        public static string GetItemName(string itemId)
        {
            return int.TryParse(itemId, out int id) ? GetItemName(id) : $"I2S Failed: itemId = {itemId}";
        }
        public static string GetItemName(int itemId)
        {
            string name = DateFile.instance.GetItemDate(itemId, 0, false);
            if (int.TryParse(DateFile.instance.GetItemDate(itemId, 8, false), out int grade))
            {
                name = DateFile.instance.SetColoer(20001 + grade, name);
            }
            return name;
        }

        public static string GetActorName(string actorId, bool showGang = true)
        {
            return int.TryParse(actorId, out int id) ? GetActorName(id, showGang) : $"I2S Failed: actorId = {actorId}";
        }
        public static string GetActorName(int actorId, bool showGang = true)
        {
            string actorName = DateFile.instance.GetActorName(actorId, true, false);
            if (!showGang) return actorName;

            int gang = int.Parse(DateFile.instance.GetActorDate(actorId, 19, false));
            string gangName = DateFile.instance.GetGangDate(gang, 0);
            int num20 = int.Parse(DateFile.instance.GetActorDate(actorId, 20, false));
            int grade = 10 - Mathf.Abs(num20);
            grade = Mathf.Clamp(grade, 1, 9);
            int gangValueId = DateFile.instance.GetGangValueId(gang, 10 - grade);
            bool isMale = DateFile.instance.GetActorDate(actorId, 14) == "1";
            int statusKey = num20 < 0 ? (isMale ? 1002 : 1003) : 1001;
            string statusName = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(gang, num20)][statusKey];
            statusName = DateFile.instance.SetColoer(20001 + grade, statusName, false);

            return gangName + statusName + " " + actorName;
        }
        public static string ColorName(string name) => DateFile.instance.SetColoer(10002, name);

        public static void ShowTips(string text, int time = 60)
        {
            var instance = TipsWindow.instance;
            if (instance.tipsIsShow)
            {
                instance.tipsMassage.text += $"\n{text}";
                time = Mathf.Max(instance.showTipsTime, time);
                instance.showTipsTime = time;
                var size = instance.tips.sizeDelta;
                instance.tips.sizeDelta = new Vector2(size.x, Mathf.Min(size.y + 20, 500));
            }
            else
            {
                instance.SetTips(0, new string[] { text }, time);
            }
        }

        public static List<int> GetActorLoc(int actorId, bool canBeEmpty = true)
        {
            var instance = DateFile.instance;
            var mianLoc = new List<int>
            {
                instance.mianPartId,
                instance.mianPlaceId
            };
            if (instance.GetFamily(true).Contains(actorId))
            {
                return mianLoc;
            }
            else
            {
                if (instance.HaveLifeDate(actorId, 11))
                {
                    return instance.GetLifeDate(actorId, 11, true);
                }
                else
                {
                    return canBeEmpty ? instance.EMPTY_LIFT_DATA_LIST : mianLoc;
                }
            }
        }
        
        public static IEnumerable<int> AllActors()
        {
            return DateFile.instance.actorsDate.Keys.Where(key => (DateFile.instance.GetActorDate(key, 26, false) == "0"));
        }
    }
}
