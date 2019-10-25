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

namespace GuiltyNature
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool needNoImpression = true;
        public bool ququEnabled = true;
        public bool rapedEnabled = true;
        public bool moreRape = true;
        public bool increasing = true;
        public int addParent = 0;
        public bool cd = true;
        public bool ignoreBlood = true;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string resBasePath;
        public static string folderName = "Texture";
        public static string[] addParentText = { "从不", "仅当与对方有亲密关系时", "一直" };
        public static int DateKey = 0;//记录当前获取key值，用于报错
        public static string DateName = "";//记录当前获取字典名，用于报错

        static Stack<string> FailedLog = new Stack<string>();

        public static void Log(object obj)
        {
            string text = obj.ToString();
            try
            {
                while (FailedLog.Count > 0)
                {
                    Logger.Log(FailedLog.Peek());
                    FailedLog.Pop();
                }
                Logger.Log(text);
            }
            catch (IOException)
            {
                FailedLog.Push(text);
            }
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            resBasePath = modEntry.Path;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#F28234FF>本mod要求太吾版本不低于2.5.X</color>");
            GUILayout.Label("<color=#F28234FF>在已积有被欺侮事件的情况下卸载本mod会导致回合开始时报错，但不影响游戏继续</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("功能1：相虫失败时可以选择将对方变成蛐蛐");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.needNoImpression = GUILayout.Toggle(settings.needNoImpression, "无需印象");
            bool flag = GUILayout.Toggle(settings.ququEnabled, "将【取其性命】改为【化为促织】");
            if (flag != settings.ququEnabled)
            {
                QuquEvent.Switch();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("功能2：");
            settings.rapedEnabled = GUILayout.Toggle(settings.rapedEnabled, "允许触发太吾被欺侮事件");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            bool flag2 = GUILayout.Toggle(settings.moreRape, "提高欺侮触发率");
            if (flag2 != settings.moreRape)
            {
                settings.moreRape = flag2;
                RapedEvent.ShiftProb(settings.moreRape);
            }
            settings.cd = GUILayout.Toggle(settings.cd, "限制同一个人过于频繁的欺侮");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.increasing = GUILayout.Toggle(settings.increasing, "【恶性循环】你的软弱会让恶人更加肆无忌惮，除非…永绝后患！");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("将欺侮者加入孩子的父母关系列表");
            settings.addParent = GUILayout.SelectionGrid(settings.addParent, addParentText, 3);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("功能3：互动-敌对-乱情瘴，可令对方爱上或遗忘指定的人");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            settings.ignoreBlood = GUILayout.Toggle(settings.ignoreBlood, "功能4：允许血亲相恋（包括太吾行为、NPC行为、男媒女妁）");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            foreach (GameEvent gameEvent in GameEvent.All)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(gameEvent.IsAdded ? 
                    $"<color=#63CED0FF>{gameEvent.Name}事件注册成功</color>"
                    : $"<color=#E4504DFF>{gameEvent.Name}事件注册失败</color>\n" + gameEvent.Log
                );
                GUILayout.EndHorizontal();
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    public static class Util
    {
        public static void ShowTips(object text)
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
        public static string ColorName(string name) => DateFile.instance.SetColoer(10002, name);
        public static string GetActorName(int actorId, bool fullName = true, bool showGang = true)
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
            int statusKey = num20 < 0 ? (isMale ? 1002 : 1003) : 1001;
            string statusName = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(gang, num20)][statusKey];
            statusName = DateFile.instance.SetColoer(20001 + grade, statusName, false);

            return gangName + statusName + " " + actorName;
        }
        public static string Color(string text, int colorId) => $"{DateFile.instance.massageDate[colorId][0]}{text}</color>";
        public static int ToInt(this string s) => Convert.ToInt32(s);
        public static decimal ToDecimal(this string s) => decimal.Parse(s);
    }//通用方法

    public static class Reflection
    {
        static MethodInfo aiCantMove = typeof(PeopleLifeAI).GetMethod("AICantMove", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void AICantMove(int actorId)
        {
            aiCantMove.Invoke(PeopleLifeAI.instance, new object[] { actorId });
        }

        static MethodInfo aiNeedMove = typeof(PeopleLifeAI).GetMethod("AINeedMove", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void AINeedMove(int actorId, int fromPartId, int toPartId, int toPlaceId, int toActorId)
        {
            aiNeedMove.Invoke(PeopleLifeAI.instance, new object[] { actorId, fromPartId, toPartId, toPlaceId, toActorId });
        }
    }//反射私有字段/方法

    public static class ModDate
    {
        public static string ModName = "GuiltyNature";
        public static string ququ = "ququ";
        public static string actor = "actor";
        public static string regular = "regular";
        public static string CD = "CD";
        public static bool TryGetValue(string key, out string value, string suffix)
        {
            if (Get(out var myModDict, suffix))
                return myModDict.TryGetValue(key, out value);
            value = "";
            return false;
        }
        public static bool Get(out Dictionary<string, string> myModDict, string suffix)
        {
            bool exist = true;
            var modDict = DateFile.instance.modDate;
            if (modDict == null)
            {
                myModDict = null;
                return false;
            }
            string key = ModName + suffix;
            if (!modDict.ContainsKey(key))
            {
                modDict.Add(key, new Dictionary<string, string> { });
                exist = false;
            }
            myModDict = modDict[key];
            return exist;
        }
        public static bool Add(string suffix, string key, string value)
        {
            Get(out var myModDict, suffix);
            return Add(myModDict, key, value);
        }
        public static bool Add(Dictionary<string, string> myModDict, string key, string value)
        {
            bool exist = true;
            if (myModDict.ContainsKey(key))
            {
                myModDict[key] = value;
            }
            else
            {
                myModDict.Add(key, value);
                exist = false;
            }
            return exist;
        }
        public static bool Remove(string suffix, string key)
        {
            Get(out var modDict, suffix);
            return Remove(modDict, key);
        }
        public static bool Remove(Dictionary<string, string> myModDict, string key)
        {
            bool exist = true;
            if (myModDict.ContainsKey(key))
            {
                myModDict.Remove(key);
            }
            else
            {
                exist = false;
            }
            return exist;
        }
    }//读写存档信息

    public abstract class GameEvent
    {
        public static List<GameEvent> All = new List<GameEvent>();

        public static int Key(int key)
        {
            Main.DateKey = key;
            return key;
        }
        public enum DateTyp
        {
            Event,
            EnemyTeam
        }
        public static Dictionary<int, Dictionary<int, string>> GetDate(DateTyp dt)
        {
            Dictionary<int, Dictionary<int, string>> result;
            switch (dt)
            {
                case DateTyp.Event:
                    Main.DateName = "EventDate";
                    result = DateFile.instance.eventDate;
                    break;
                default:
                    Main.DateName = "EnemyTeamDate";
                    result = DateFile.instance.enemyTeamDate;
                    break;
            }
            if (result == null)
            {
                throw new NullReferenceException($"{Main.DateName} is null");
            }
            return result;
        }

        public abstract void Add();
        public bool IsAdded;
        public string Log = "未曾执行加载"; // 用于记录加载成功或报错信息
        public string Name;

        void TryAdd()
        {
            if (IsAdded) return;
            try
            {
                Add();
            }
            catch (Exception e)
            {
                string message = e.Message + '\n' + e.StackTrace;
                if (e is KeyNotFoundException)
                {
                    string s3 = $"缺少数据: EventDate[{Main.DateKey}]";
                    Log = s3;
                }
                else
                {
                    Log = message;
                }
                IsAdded = false;
                return;
            }
            IsAdded = true;
            return;
        }
        public static void AddAll()
        {
            lock (DateFile.instance) //锁线程，防止与基础资源框架等同时运行
            {
                QuquEvent.Instance.TryAdd();
                RapedEvent.Instance.TryAdd();
                ConfuseEvent.Instance.TryAdd();
            }
        }
    }

    public class QuquEvent : GameEvent
    {
        static QuquEvent _instance;
        public static QuquEvent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new QuquEvent()
                    {
                        Name = "相人",
                        IsAdded = false,
                    };
                    All.Add(_instance);
                }
                return _instance;
            }
        }

        public override void Add()
        {
            var rows = GetDate(DateTyp.Event);
            int newRowId = EventId;
            while (rows.ContainsKey(newRowId)) newRowId++;
            EventId = newRowId;
            Dictionary<int, string> newRow = new Dictionary<int, string>(rows[Key(957300001)])
            {
                [0] = "相虫·相人",
                [3] = "我看你倒是神采非凡，不如…？ <color=#8E8E8EFF>（掏出剑柄……）</color>",
                [4] = "1",
                [8] = $"END&{newRowId}"
            };//相人选项
            rows.Add(newRowId, newRow);
            foreach (int i in new int[] { 9574, 9575, 9576 })
            {
                rows[Key(i)][5] = $"{newRowId}|" + rows[i][5];
            }//三种相虫失败
            if (Main.settings.ququEnabled) ChangeKillOption(true);
            Log = $"注册相虫事件，id为{newRowId}\n";
            return;
        }

        public static int EventId = 957400009;
        public static void Switch()
        {
            Main.settings.ququEnabled = !Main.settings.ququEnabled;
            if (DateFile.instance == null) return;
            ChangeKillOption(Main.settings.ququEnabled);
        }
        static void ChangeKillOption(bool forward)
        {
            var rows = GetDate(DateTyp.Event);
            if (forward)
            {
                foreach (int i in new int[] { 917800001, 12400001, 17700001 })
                {
                    Key(i);
                    rows[i][3] = "（将其化为促织……）";
                    rows[i][8] = $"END&{EventId}";
                }//袭击获胜，被袭击获胜，失心人救治失败
                foreach (int i in new int[] { 25200005 })
                {
                    Key(i);
                    rows[i][3] = "（将其化为促织……）";
                    rows[i][7] = "-1";
                    rows[i][8] = "GN&4|" + $"END&{EventId}";
                }//失心人救治唯我选项
            }
            else
            {
                foreach (int i in new int[] { 917800001, 12400001 })
                {
                    rows[Key(i)][3] = "（取其性命！）";
                }
                rows[Key(917800001)][8] = "END&91781&1";
                rows[Key(12400001)][8] = "END&1241&1";
                rows[Key(17700001)][3] = "（放弃救治，取其性命！）";
                rows[Key(17700001)][8] = "END&1241&1";
                foreach (int i in new int[] { 25200005 })
                {
                    Key(i);
                    rows[i][3] = "（为其了断……）";
                    rows[i][7] = "257";
                    rows[i][8] = "GN&4|END&2521&4";
                }//失心人救治唯我选项
            }
        }

        public static void EndEvent(int actorId)
        {
            //蛐蛐
            int num20 = DateFile.instance.GetActorDate(actorId, 20).ToInt();
            int level = 10 - Mathf.Abs(num20);//根据人物品级决定蛐蛐品级
            level = Mathf.Clamp(level, 1, 9);
            int 杂学 = DateFile.instance.GetActorValue(actorId, 516, false);//杂学面板资质，决定蛐蛐稀有度
            int 内功 = DateFile.instance.GetActorValue(actorId, 601, false);//内功面板资质，决定蛐蛐另一半的品级
            bool isMale = DateFile.instance.GetActorDate(actorId, 14) == "1";//根据性别决定[颜色和部位哪个品级更高]
            int colorId = 0;
            int partId = 0;
            const int colorLimit = 6;
            const int partLimit = 7;
            List<int> allQuqu = new List<int>(DateFile.instance.cricketDate.Keys);
            //促织王
            if (level >= 8)
            {
                Dictionary<int, List<int>> colorPools = new Dictionary<int, List<int>> { };
                foreach (int ququId in allQuqu)
                {
                    bool match = DateFile.instance.cricketDate[ququId][4].ToInt() == 0 &&
                        DateFile.instance.cricketDate[ququId][1].ToInt() == level;//一二品的颜色
                    if (match)//根据稀有度分配奖池
                    {
                        int rarity = DateFile.instance.cricketDate[ququId][6].ToInt();
                        if (ququId == 20) rarity = 5;//三太子和梅花翅一档
                        else if (ququId == 18 || ququId == 19) rarity = 1;//八败、三段棉、天蓝青一档
                        AddToList(colorPools, rarity, ququId);
                    }
                }
                colorId = MatchQuquId(colorPools, 杂学, actorId);
            }
            //普通蛐蛐
            else
            {
                Dictionary<int, List<int>> colorPools = new Dictionary<int, List<int>> { };
                Dictionary<int, List<int>> partPools = new Dictionary<int, List<int>> { };
                bool colorBetter = (level == 7 || isMale) ? false : true;
                int grade2 = GetLevel(内功, Math.Min(level, colorBetter ? partLimit : colorLimit)) + 1;
                for (int i = 0; i < allQuqu.Count; i++)
                {
                    int ququId = allQuqu[i];
                    if (ququId == 0) continue;//排除呆物
                    bool isColor = DateFile.instance.cricketDate[ququId][4].ToInt() == 0;
                    bool isBetter = isColor == colorBetter;
                    int grade0 = isBetter ? level : grade2;
                    var pools0 = isColor ? colorPools : partPools;
                    bool match = DateFile.instance.cricketDate[ququId][1].ToInt() == grade0;
                    if (match)
                    {
                        int rarity = DateFile.instance.cricketDate[ququId][6].ToInt();
                        AddToList(pools0, rarity, ququId);
                    }
                }
                colorId = MatchQuquId(colorPools, 杂学, actorId);
                partId = MatchQuquId(partPools, 杂学, actorId);
            }
            //生成物品
            int itemId = DateFile.instance.MakeNewItem(10000, 0, 0, 50, 20);
            GetQuquWindow.instance.MakeQuqu(itemId, colorId, partId);
            //名字信息
            string ququName = DateFile.instance.GetItemDate(itemId, 0, false);
            string actorName = DateFile.instance.GetActorName(actorId);
            int gang = DateFile.instance.GetActorDate(actorId, 19, false).ToInt();
            string gangName = DateFile.instance.GetGangDate(gang, 0);
            int gangValueId = DateFile.instance.GetGangValueId(gang, 10 - level);
            string gangLevelName = DateFile.instance.presetGangGroupDateValue
                [DateFile.instance.GetGangValueId(gang, num20)]
                [num20 < 0 ? (isMale ? 1002 : 1003) : 1001];
            gangLevelName = DateFile.instance.SetColoer(20001 + level, gangLevelName, false);
            //记录蛐蛐
            string[] ququInfo =
            {
                gangName + gangLevelName + " " + actorName,
                DateFile.instance.GetActorDate(actorId, 13, true), //蛐蛐寿命 = 人物健康上限
                actorId.ToString(),
            };
            ModDate.Add(ModDate.ququ, itemId.ToString(), string.Join("|", ququInfo));
            Main.Log($"{actorName}>>>{ququName}");
            //记录人
            string[] actorInfo =
            {
                colorId.ToString(),
                partId.ToString(),
                itemId.ToString(),
            };
            ModDate.Add(ModDate.actor, actorId.ToString(), string.Join("|", actorInfo));
            //给予物品
            GameData.Items.SetItemProperty(itemId, 2007, DateFile.instance.GetActorDate(actorId, 11, true)); //蛰龄 = 人物年龄
            DateFile.instance.GetItem(DateFile.instance.mianActorId, itemId, 1, false, 0, 0);
            //移除人
            GameData.Characters.SetCharProperty(actorId, 12, "0");//健康
            DateFile.instance.RemoveActor(new List<int> { actorId }, true, false);
            DateFile.instance.MoveOutPlace(actorId);
            UIDate.instance.UpdateManpower();
            WorldMapSystem.instance.UpdatePlaceActor(DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, true);
        }
        static int GetLevel(int attr, int count, int upLimit = 120)
        {
            int div = upLimit * 100 / count;
            int index = Mathf.Clamp(attr * 100 / div, 0, count - 1);
            return index;
        }
        static void AddToList(Dictionary<int, List<int>> dict, int key, int item)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(item);
            }
            else
            {
                dict.Add(key, new List<int> { item });
            }
        }
        static int MatchQuquId(Dictionary<int, List<int>> pools, int attr, int actorId)
        {
            List<int> keys = pools.Keys.ToList();
            keys.Sort((a, b) => (a > b ? -1 : 1));
            int poolKey = keys[GetLevel(attr, keys.Count)];
            List<int> pool = pools[poolKey];
            //匹配七元
            var ququsDate = DateFile.instance.cricketDate;
            int[] actorQiYuan = DateFile.instance.GetActorResources(actorId);
            Dictionary<int, float> similarity = new Dictionary<int, float> { };
            float max = 0;
            int best = 0;
            foreach (int ququId in pool)
            {
                int[] ququQiYuan = new int[7];
                var ququDate = ququsDate[ququId];
                for (int i = 0; i < 7; i++)
                {
                    ququQiYuan[i] = ququDate[52001 + i].ToInt();
                }
                similarity[ququId] = GetSimilarity(actorQiYuan, ququQiYuan);
                if (similarity[ququId] > max)
                {
                    max = similarity[ququId];
                    best = ququId;
                }
            }
            return best;
        }
        static float GetSimilarity(int[] actor, int[] ququ)
        {
            int length = 7;
            float sum1 = actor.Sum();
            sum1 = Mathf.Max(0.1f, sum1);
            float bonus1 = sum1 / 30f;
            sum1 += length * bonus1;
            float[] array1 = actor.Select(i => (i + bonus1) / sum1).ToArray();

            float sum2 = ququ.Sum();
            sum2 = Mathf.Max(0.1f, sum2);
            float bonus2 = sum2 / 30f;
            sum2 += length * bonus2;
            float[] array2 = ququ.Select(i => (i + bonus2) / sum2).ToArray();

            float similarity = 1;
            for (int i = 0; i < 7; i++)
            {
                float n1 = array1[i];
                float n2 = array2[i];
                similarity *= n1 > n2 ? n2 / n1 : n1 / n2;
            }
            return similarity;
        }
        
    }//相虫相人

    public class RapedEvent:GameEvent
    {
        static RapedEvent _instance;
        public static RapedEvent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RapedEvent()
                    {
                        Name = "被欺侮",
                        IsAdded = false,
                    };
                    All.Add(_instance);
                }
                return _instance;
            }
        }

        public override void Add()
        {
            Log = "";
            ShiftProb(Main.settings.moreRape);
            var eventDate = GetDate(DateTyp.Event);
            ID.menu0 = CheckEventId(ID.menu0);
            ID.option1 = CheckEventId(ID.menu0 * 100000 + 1);
            ID.option2 = CheckEventId(ID.option1 + 1);
            ID.menu1 = CheckEventId(ID.menu1);
            ID.option11 = CheckEventId(ID.menu1 * 100000 + 1);
            ID.menu3 = CheckEventId(ID.menu3);
            Dictionary<int, string> menu0_Date = new Dictionary<int, string>(eventDate[Key(201)])
            {
                [0] = "遭遇欺侮",
                [3] = "当MN行至偏僻无人之处时，D0忽然面色不善的拦住了MN的去路……\n在三言两语的交谈之后，D0终于言明：已无法抑制对MN的爱欲……",
                [4] = "1",
                [5] = $"{ID.option1}|{ID.option2}",
            };//遭遇欺侮
            eventDate.Add(ID.menu0, menu0_Date);
            Dictionary<int, string> option1_Date = new Dictionary<int, string>(eventDate[Key(20100001)])
            {
                [3] = "（任其宣泄……）",
                [4] = "1",
                [7] = $"{ID.menu1}",
                [8] = $"END&{ID.option1}",
            };//任其宣泄
            eventDate.Add(ID.option1, option1_Date);
            Dictionary<int, string> option2_Date = new Dictionary<int, string>(eventDate[Key(20100002)])
            {
                [4] = "1",
            };//恕难从命
            eventDate.Add(ID.option2, option2_Date);
            Dictionary<int, string> menu1_Date = new Dictionary<int, string>(eventDate[Key(203)])
            {
                [0] = "被欺侮",
                [3] = "D0以卑劣手段欺侮了MN……",
                [4] = "1",
                [5] = $"20300001|20300002|20300003|20300004|20300005|{ID.option11}"
            };//被欺侮
            eventDate.Add(ID.menu1, menu1_Date);
            Dictionary<int, string> option11_Date = new Dictionary<int, string>(eventDate[Key(922200001)])
            {
                [3] = "（倾诉爱意……）<color=#4B4B4BFF>（在剑柄的强迫下）</color>",
                [8] = "END&90011&6"
            };//倾诉爱意
            eventDate.Add(ID.option11, option11_Date);
            Dictionary<int, string> menu3_Date = new Dictionary<int, string>(eventDate[Key(115)])
            {
                [0] = "反抗失败",
                [3] = "MN试图反抗D0，但失败了……",
                [5] = $"{ID.option1}",
            };//反抗失败
            eventDate.Add(ID.menu3, menu3_Date);

            ID.menu2 = CheckEventId(ID.menu2);
            ID.option21 = CheckEventId(ID.menu2 * 100000 + 1);
            Dictionary<int, string> menu2_Date = new Dictionary<int, string>(eventDate[Key(124)])
            {
                [0] = "反抗欺侮获胜",
                [5] = $"{ID.option21}|12400002"
            };//反抗获胜
            eventDate.Add(ID.menu2, menu2_Date);
            Dictionary<int, string> option21_Date = new Dictionary<int, string>(eventDate[Key(900700001)])
            {
                [3] = "（没收工具......）",
                [8] = $"END&{ID.option21}"
            };//没收工具
            eventDate.Add(ID.option21, option21_Date);
            //战斗信息
            var enemyTeamDate = GetDate(DateTyp.EnemyTeam);
            while (enemyTeamDate.ContainsKey(ID.enemyTeam)) ID.enemyTeam++;
            Dictionary<int, string> newRow = new Dictionary<int, string>(enemyTeamDate[Key(102)])
            {
                [23] = "75",//血线
                [98] = "1",//BGM
                [101] = $"{ID.menu2}&1",//胜利事件
                [102] = $"{ID.menu3}&1",//失败事件
                [8] = "0",
                [11] = "0"
            };
            enemyTeamDate.Add(ID.enemyTeam, newRow);
            Log += $"注册被欺侮战斗，id为{ID.enemyTeam}\n";
            eventDate[ID.option2][8] = $"BAT&{ID.enemyTeam}&0";
            
            IsAdded = true;
        }

        static Sprite LoadSprite(string imageName)
        {
            string path = Path.Combine(Path.Combine(Main.resBasePath, Main.folderName), imageName);
            if (!File.Exists(path))
            {
                Util.ShowTips($"图片路径[{path}]不存在");
                return null;
            }
            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Main.Log($"成功加载图片[{path}]\n大小为{texture.width}*{texture.height}");
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);
        }
        public static Sprite[,,] Sprites = new Sprite[2, 2, 2];

        public static class ID
        {
            public static int menu0 = 999;
            public static int option1 = 99900001;
            public static int option2 = 99900002;
            public static int menu1 = 998;
            public static int menu2 = 997;
            public static int option11 = 99800001;
            public static int option21 = 99700001;
            public static int option22 = 99700002;
            public static int menu3 = 996;
            public static int enemyTeam = 170;

            public static int enticeOpiton = 900100008;
            public static int enticeMenu = 9388;
        }
        public static void ShiftProb(bool add = true)
        {
            if (DateFile.instance == null) return;
            var goodnessDate = DateFile.instance.goodnessDate;
            if (add)
            {
                DateFile.instance.goodnessDate[0][25] = "15";
                DateFile.instance.goodnessDate[3][25] = "45";
                DateFile.instance.goodnessDate[4][25] = "30";
            }
            else
            {
                DateFile.instance.goodnessDate[0][25] = "1";
                DateFile.instance.goodnessDate[3][25] = "3";
                DateFile.instance.goodnessDate[4][25] = "2";
            }
        }
        public static void DoRape(int raperId, int victimId, bool reverse = false)
        {
            int raperGender = DateFile.instance.GetActorDate(raperId, 14).ToInt();
            int raperGoodness = DateFile.instance.GetActorGoodness(raperId);
            int raperMoodChange = DateFile.instance.goodnessDate[raperGoodness][102].ToInt() * 10;
            int victimGender = DateFile.instance.GetActorDate(victimId, 14).ToInt();

            Cnt(raperId);
            DateFile.instance.SetActorMood(raperId, raperMoodChange);
            bool love = DateFile.instance.GetActorSocial(victimId, 312, false, false).Contains(raperId)//倾心爱慕
                || DateFile.instance.GetActorSocial(victimId, 306, false, false).Contains(raperId)//两情相悦
                || DateFile.instance.GetActorSocial(victimId, 309, false, false).Contains(raperId);//结发夫妻
            int XXChangePart = 9;
            if (love)
            {
                DateFile.instance.SetActorMood(victimId, UnityEngine.Random.Range(-10, 11), XXChangePart);
                bool hate = UnityEngine.Random.Range(0, 100) < 50;
                if (hate)
                {
                    DateFile.instance.AddSocial(victimId, raperId, 402);
                }
                PeopleLifeAI.instance.AISetMassage(97, victimId, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], raperId, true);
            }
            else
            {
                DateFile.instance.SetActorMood(victimId, -50, XXChangePart);
                DateFile.instance.AddSocial(victimId, raperId, 402);
                PeopleLifeAI.instance.AISetMassage(96, victimId, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], raperId, true);
            }
            DateFile.instance.ChangeActorFeature(raperId, 4001, 4002);
            DateFile.instance.ChangeActorFeature(victimId, 4001, 4002);
            bool sameGender = raperGender == victimGender;
            if (!sameGender)
            {
                bool addParent = Main.settings.addParent == 2 || (Main.settings.addParent == 1 && love);
                if (raperGender == 1)
                {
                    PeopleLifeAI.instance.AISetChildren(raperId, victimId, addParent ? 1 : 0, 1);
                }
                else
                {
                    PeopleLifeAI.instance.AISetChildren(victimId, raperId, 1, addParent ? 1 : 0);
                }
                /*bool preg = PeopleLifeAI.instance.AISetChildren((raperGender == 1) ? raperId : victimId, (raperGender == 1) ? victimId : raperId,
                    (raperGender == 1) ? 0 : 1, (raperGender == 1) ? 1 : 0);
                if (preg) Main.Log("身怀六甲");*/
            }
        }
        public static void Spay(int actorId)
        {
            bool isMale = DateFile.instance.GetActorDate(actorId, 14).ToInt() == 1;
            string actorName = DateFile.instance.GetActorName(actorId);
            int itemId = isMale ? 3413 : 3427;
            int featureId = isMale ? 1001 : 1002;
            int mainId = DateFile.instance.mianActorId;

            if(ModDate.Get(out var modDate, ModDate.regular))
            {
                ModDate.Remove(modDate, actorId.ToString());
            }

            DateFile.instance.AddActorFeature(actorId, featureId);
            DateFile.instance.GetItem(DateFile.instance.mianActorId, itemId, 1, true, 0);
            DateFile.instance.SetActorMood(mainId, 140);
        }
        int CheckEventId(int eventId)
        {
            var eventDate = DateFile.instance.eventDate;
            while (eventDate.ContainsKey(eventId)) eventId++;
            Log += $"注册被欺侮事件，id为{eventId}\n";
            return eventId;
        }
        public static void Cnt(int actorId, int add = 1)
        {
            string key = actorId.ToString();
            string actorName = DateFile.instance.GetActorName(actorId);
            ModDate.Get(out var modDate, ModDate.regular);
            string cnt;
            if (modDate.ContainsKey(key))
            {
                cnt = (modDate[key].ToInt() + add).ToString();
                modDate[key] = cnt;
            }
            else
            {
                cnt = add.ToString();
                modDate.Add(key, cnt);
            }
            Main.Log($"{actorName} 累计第{cnt}次");
        }

        static List<int> Losers = new List<int>();
        static List<int> Winners = new List<int>();
        static bool XX = false; //太吾入魔
        //换季前置处理
        [HarmonyPatch(typeof(UIDate), "ChangeTrun")]
        public static class GuiltyNature_ChangeTrun_Patch
        {
            static void Prefix()
            {
                Losers = new List<int>();
                Winners = new List<int>();

                ModDate.Get(out var CDdate, ModDate.CD);
                var keys = CDdate.Keys.ToList();
                foreach(var key in keys)
                {
                    int value = CDdate[key].ToInt();
                    value /= 2;
                    if (value <= 10)
                    {
                        CDdate.Remove(key);
                    }
                    else
                    {
                        CDdate[key] = value.ToString();
                    }
                }
                
                XX = DateFile.instance.GetLifeDate(DateFile.instance.mianActorId, 501, 0) >= 200;
            }
        }
        //被欺侮，将必定失败的换季事件变为回合开始事件
        [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
        public static class GuiltyNature_DoTrunAIChange_Patch
        {
            static void Postfix(PeopleLifeAI __instance, int actorId)
            {
                if (!Main.enabled || !Main.settings.rapedEnabled || !Instance.IsAdded) return;
                bool trigglable = UIDate.instance.trunChangeBattleEnemys.Count <= 0 && !DateFile.instance.setNewMianActor && !DateFile.instance.doMapMoveing;
                if (!trigglable || DateFile.instance.gameOver) return;
                if (XX) return;
                if (Losers.Contains(actorId) || Winners.Contains(actorId)) return;//重复触发

                List<int> features = DateFile.instance.GetActorFeature(actorId);
                bool disabled = (features.Contains(1001) || features.Contains(1002));
                string actorName = DateFile.instance.GetActorName(actorId);
                int mianId = DateFile.instance.mianActorId;
                int[] eventInfo = new int[]
                {
                    0,
                    actorId,
                    ID.menu0,
                    actorId
                };
                int length = __instance.aiTrunEvents.Count;
                int turnEventIndex = GetIndex(__instance.aiTrunEvents);
                if (turnEventIndex >= 0)
                {
                    int[] aiTurnEvent = __instance.aiTrunEvents[turnEventIndex];
                    if (aiTurnEvent.Length >= 4 && aiTurnEvent[0] == 233 && actorId == aiTurnEvent[3])
                    {
                        if (disabled)
                        {
                            __instance.aiTrunEvents.RemoveAt(length - 1);
                            Losers.Add(actorId);
                            Main.Log($"移除无稽之谈，发起者为：{actorName}");
                            return;
                        }
                        else
                        {
                            if (InCD(actorId))
                            {
                                //__instance.aiTrunEvents.RemoveAt(length - 1);
                                Main.Log($"取消频繁的雷普，发起者为：{actorName}");
                                return;
                            }
                            if (Success(actorId))
                            {
                                /*var method = typeof(PeopleLifeAI).GetMethod("AISetEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                                                        method.Invoke(__instance, new object[] { 0, eventInfo});*/
                                DateFile.instance.SetEvent(eventInfo, true, false);
                                __instance.aiTrunEvents.RemoveAt(length - 1);
                                Main.Log($"转化雷普事件，发起者为：{actorName}");
                                return;
                            }
                            Main.Log($"雷普被识破，发起者为：{actorName}");
                        }
                    }

                }
                //回头客
                if (Main.settings.increasing && ModDate.Get(out var modDate, ModDate.regular) && modDate.ContainsKey(actorId.ToString()))
                {
                    //检查有没有传剑
                    string mianKey = "mian";
                    bool mianChanged = false;
                    string mianValue = mianId.ToString();
                    if (modDate.ContainsKey(mianKey))
                    {
                        if (modDate[mianKey] != mianValue)
                        {
                            mianChanged = true;
                            modDate.Clear();//如果传剑就清空回头客数据
                            modDate.Add(mianKey, mianValue);
                        }
                    }
                    else
                    {
                        modDate.Add(mianKey, mianValue);
                    }
                    if (!modDate.ContainsKey(actorId.ToString())) return;//虽说理论上不可能，但安全起见还是判断一下吧
                    if (!mianChanged)
                    {
                        //仁善刚正不欺侮
                        int goodness = DateFile.instance.GetActorGoodness(actorId);
                        if (goodness == 1 || goodness == 2)
                        {
                            ModDate.Remove(modDate, actorId.ToString());
                            Main.Log($"正直的{actorName}抑制了自己的冲动");
                            return;
                        }
                        //根据累计欺侮次数计算触发率
                        int times = modDate[actorId.ToString()].ToInt();
                        if (UnityEngine.Random.Range(0f, 1f) > Mathf.Pow(0.7f, times))
                        {
                            var actorLife = DateFile.instance.actorLife;
                            int mainPart = DateFile.instance.mianPartId;
                            int mainPlace = DateFile.instance.mianPlaceId;
                            int actorPart = mainPart;
                            int actorPlace = mainPlace;
                            bool onMap = actorLife.ContainsKey(actorId) && actorLife[actorId].ContainsKey(11);
                            if (onMap)
                            {
                                var locPair = DateFile.instance.actorLife[actorId][11];
                                if (locPair != null && locPair.Count >= 2)
                                {
                                    actorPart = locPair[0];
                                    actorPlace = locPair[1];
                                }
                            }
                            bool sameLoc = actorPart == mainPart && actorPlace == mainPlace;
                            if (!sameLoc)
                            {
                                /*DateFile.instance.MoveToPlace(mainPart, mainPlace, actorId, true);
                                Reflection.AICantMove(actorId);*/
                                try
                                {
                                    Reflection.AINeedMove(actorId, actorPart, mainPart, mainPlace, mianId);
                                }
                                catch (Exception ex)
                                {
                                    Util.ShowTips("本错误不影响游戏进行，但请联系作者");
                                    Util.ShowTips(Util.Color("恶性循环报错：" + ex.Message, 20010));
                                    Main.Log(ex.Message);
                                    Main.Log(ex.StackTrace);
                                    return;
                                }
                                Main.Log($"回头客追随，发起者为：{actorName}");
                                return;
                            }
                            if (InCD(actorId))
                            {
                                PeopleLifeAI.instance.aiTrunEvents.Add(new int[]
                                {
                                    233,
                                    actorPart,
                                    actorPlace,
                                    actorId
                                });
                                Main.Log($"取消频繁的雷普，发起者为：{actorName}");
                                return;
                            }
                            if (!Success(actorId, true))
                            {
                                PeopleLifeAI.instance.aiTrunEvents.Add(new int[]
                                {
                                    233,
                                    actorPart,
                                    actorPlace,
                                    actorId
                                });
                                Main.Log($"回头客雷普被识破，发起者为：{actorName}");
                            }
                            else
                            {
                                DateFile.instance.SetEvent(eventInfo, true, false);
                                try
                                {
                                    Reflection.AICantMove(actorId);
                                }
                                catch (Exception ex)
                                {
                                    Util.ShowTips("本错误不影响游戏进行，但请联系作者");
                                    Util.ShowTips(Util.Color("恶性循环报错：" + ex.Message, 20010));
                                    Main.Log(ex.Message);
                                    Main.Log(ex.StackTrace);
                                    return;
                                }
                                Main.Log($"添加回头客雷普事件，发起者为：{actorName}");
                            }
                            return;

                        }
                    }
                }
            }
            static int GetIndex(List<int[]> aiTurnEvents)
            {
                int length = aiTurnEvents.Count;
                if (length <= 0) return -1;
                int index = length - 1;
                while (aiTurnEvents[index].Length < 4)//忽略丢东西事件
                {
                    index--;
                    if (index < 0) return -1;
                }
                return index;
            }
            static bool InCD(int actorId)
            {
                if (!Main.settings.cd) return false;
                if (ModDate.TryGetValue(actorId.ToString(), out string value, ModDate.CD))
                {
                    if (int.TryParse(value, out int n))
                    {
                        if (UnityEngine.Random.Range(0, 100) < n)
                        {
                            Losers.Add(actorId);
                            return true;
                        }
                    }
                }
                return false;
            }
            static bool Success(int actorId, bool regular = false)
            {
                int mianId = DateFile.instance.mianActorId;
                
                int cnt = Winners.Count;
                float randInt = UnityEngine.Random.Range(0, (int)Mathf.Pow(2, cnt));
                if (regular) randInt -= 1;
                if (randInt <= 0)
                {
                    Winners.Add(actorId);
                    ModDate.Add(ModDate.CD, actorId.ToString(), "150");
                    string name = Util.ColorName(Util.GetActorName(actorId, false, false));
                    Util.ShowTips($"{name}的眼神似乎有点怪…");
                    return true;
                }

                faild:;
                Losers.Add(actorId);
                return false;
            }
        }
        //修改Event图片
        [HarmonyPatch(typeof(ui_MessageWindow), "SetMassageWindow")]
        public static class GuiltyNature_SetMassageWindow_Patch
        {
            static void Postfix(MessageEventManager __instance, int[] baseEventDate)
            {
                if (!Main.enabled || !Main.settings.rapedEnabled || !RapedEvent.Instance.IsAdded) return;
                int eventId = baseEventDate[2];
                bool isMenu0 = eventId == RapedEvent.ID.menu0;
                bool isMenu1 = eventId == RapedEvent.ID.menu1;
                if (!isMenu0 && !isMenu1) return;

                int mianId = DateFile.instance.mianActorId;
                bool mianMale = DateFile.instance.GetActorDate(mianId, 14, false) == "1";
                if (DateFile.instance.GetActorDate(mianId, 17, false) == "1") mianMale = !mianMale;

                int actorId = baseEventDate[1];
                bool actorMale = DateFile.instance.GetActorDate(actorId, 14, false) == "1";
                if (DateFile.instance.GetActorDate(actorId, 17, false) == "1") actorMale = !actorMale;

                bool victimMale = mianMale;
                bool raperMale = actorMale;
                int i = raperMale ? 1 : 0;
                int j = mianMale ? 1 : 0;
                int k = isMenu0 ? 0 : 1;

                Sprite sprite = Sprites[i, j, k];
                if (sprite == null)
                {
                    sprite = LoadSprite($"{i}_{j}_{k}.png");//懒加载
                    if (sprite == null)
                    {
                        return;
                    }
                    Sprites[i, j, k] = sprite;
                }
                ui_MessageWindow.Instance.eventBackImage.sprite = sprite;
            }
        }
    }//被欺侮

    public class ConfuseEvent:GameEvent
    {
        static ConfuseEvent _instance;
        public static ConfuseEvent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfuseEvent()
                    {
                        Name = "乱情瘴",
                        IsAdded = false,
                    };
                    All.Add(_instance);
                }
                return _instance;
            }
        }

        public override void Add()
        {
            Log = "";
            var rows = GetDate(DateTyp.Event);
            ID.option = CheckEventId(ID.option);
            ID.input = CheckEventId(ID.option + 1);
            ID.option1 = CheckEventId(ID.input + 1);
            ID.menu1 = CheckEventId(ID.option1 + 1);
            var option_Date = new Dictionary<int, string>(rows[Key(931300001)])
            {
                [3] = "（乱情瘴……）",
                [6] = "GN&3|TIME&3",
                [7] = $"{ID.input}"
            };
            rows.Add(ID.option, option_Date);
            var input_Date = new Dictionary<int, string>(rows[Key(9356)])
            {
                [0] = "乱情瘴输入",
                [3] = "输入指定目标的姓名……",
                [5] = $"{ID.option1}|900700001"
            };
            rows.Add(ID.input, input_Date);
            var option1_Date = new Dictionary<int, string>(rows[Key(935600001)])
            {
                [3] = "便是此人！<color=#4B4B4BFF>（消耗行动力：3）</color>",
                [7] = $"{ID.menu1}",
                [8] = $"TIME&3|END&{ID.option1}"
            };
            rows.Add(ID.option1, option1_Date);
            var menu1_Date = new Dictionary<int, string>(rows[Key(9357)])
            {
                [0] = "乱情瘴完成",
                [3] = "啊…原来是ta么"
            };
            rows.Add(ID.menu1, menu1_Date);
            
            rows[Key(9005)][5] += $"|{ID.option}";
        }

        public const int GongfaId = 21202;
        public static class ID
        {
            public static int option = 900500100;
            public static int input = 900500101;
            public static int option1 = 900500102;
            public static int menu1 = 900500103;
        }
        int CheckEventId(int eventId)
        {
            var eventDate = DateFile.instance.eventDate;
            while (eventDate.ContainsKey(eventId)) eventId++;
            Log += $"注册乱情障事件，id为{eventId}\n";
            return eventId;
        }
        public static void EndEvent(int eventActor, string inputName)
        {
            int mianId = DateFile.instance.mianActorId;
            string coloredEventActorName = Util.ColorName(DateFile.instance.GetActorName(eventActor));
            string coloredInputName = Util.ColorName(inputName);
            int partId = DateFile.instance.mianPartId;
            int placeId = DateFile.instance.mianPlaceId;

            //断情
            var matchActors = DateFile.instance.GetActorSocial(eventActor, 306, true)//两情相悦
                .Union(DateFile.instance.GetActorSocial(eventActor, 312, true))//倾心爱慕
                .Where(actorId => Match(actorId, inputName));
            if (matchActors.Count() > 0)
            {
                foreach (int actorId in matchActors)
                {
                    DateFile.instance.RemoveActorSocial(eventActor, actorId, 306);
                    DateFile.instance.RemoveActorSocial(eventActor, actorId, 312);
                    PeopleLifeAI.instance.AISetMassage(42, eventActor, partId, placeId, new int[1], actorId, true);
                    if (actorId == mianId)
                    {
                        ModDate.Remove(ModDate.regular, eventActor.ToString());//移除恶性循环
                    }
                }
                Util.ShowTips($"{coloredEventActorName}与{coloredInputName}断绝了彼此的情爱。");
                goto end;
            }
            //生情
            matchActors = GameData.Characters.GetAllCharIds()
                .Where(actorId => (GameData.Characters.GetCharProperty(actorId, 26) ?? "0") == "0" //活着
                    && actorId != eventActor //不是自己
                    && Match(actorId, inputName));
            if (matchActors.Count() > 0)
            {
                foreach (int actorId in matchActors)
                {
                    DateFile.instance.AddSocial(eventActor, actorId, 312);
                    PeopleLifeAI.instance.AISetMassage(46, eventActor, partId, placeId, new int[1], actorId, true);
                    if (actorId == mianId)
                    {
                        RapedEvent.Cnt(actorId);//叠一层恶性循环
                    }
                }
                Util.ShowTips($"{coloredEventActorName}对{coloredInputName}心生爱慕之情。");
            }
            else
            {
                Util.ShowTips($"没有名为{coloredInputName}的人。");
            }

            end:;

            //对方变叛逆
            DateFile.instance.SetActorGoodness(eventActor, 750);
            //加入魔
            DateFile.instance.SetActorXXChange(mianId, 5, false, false);
            //太吾立场变动，附带心情变化
            MessageEventManager.Instance.ChangeGoodnees(3, mianId, 25);
        }
        static bool Match(int actorId, string name)
        {
            string actorName = DateFile.instance.GetActorName(actorId, false, true);
            string actorName2 = DateFile.instance.GetActorName(actorId, false, false);
            bool result =  actorName == name || actorName2 == name;
            return result;
        }
    }//乱情瘴
    
    //Event条件
    [HarmonyPatch(typeof(MessageEventManager), "GetEventIF")]
    public static class GuiltyNature_GetEventIF_Patch
    {
        static bool Prefix(ref bool __result, int actorId, int eventActor, int eventId)
        {
            if (!Main.enabled || !Main.settings.needNoImpression) return true;
            //乱情瘴
            if(eventId == ConfuseEvent.ID.option)
            {
                var gongfas = DateFile.instance.actorGongFas[actorId];
                int gongfaId = ConfuseEvent.GongfaId;
                if (gongfas.ContainsKey(gongfaId))
                {
                    int gongfaLevel = DateFile.instance.GetGongFaLevel(actorId, gongfaId, 0);
                    int gongfaFlevel = DateFile.instance.GetGongFaFLevel(actorId, gongfaId);
                    if (gongfaLevel >= 100 && gongfaFlevel >= 10)
                    {
                        int poisonId = 5;
                        int poison = DateFile.instance.Poison(eventActor, poisonId, false);
                        int resist = DateFile.instance.MaxPoison(eventActor, poisonId);
                        if (poison > resist)
                        {
                            if (DateFile.instance.dayTime >= 3)
                            {
                                __result = true;
                                return false;
                            }
                        }
                    }
                }
                __result = false;
                return false;
            }
            //衣装
            string require = DateFile.instance.eventDate[eventId][6];
            bool flag = require == "" || require == "0";
            if (flag) return true;
            string[] array = DateFile.instance.eventDate[eventId][6].Split('|');
            bool[] array2 = new bool[array.Length];
            bool flag_or;
            bool flag_and = true;
            bool isLIFEF = false;
            for (int i = 0; i < array.Length; i++)
            {
                string[] array3 = array[i].Split(new char[]
                {
                    '#'
                });
                flag_or = false;
                for (int j = 0; j < array3.Length; j++)
                {
                    string[] array5 = array3[j].Split(new char[]
                    {
                        '&'
                    });
                    string text = array5[0];
                    if (text == "LIFEF")
                    {
                        //if (DateFile.instance.GetLifeDate(eventActor, 1001, 0) == array5[1]).ToInt()
                        flag_or = true;
                        isLIFEF = true;
                    }
                    else if (text == "NOF")
                    {
                        if (!DateFile.instance.GetFamily(true, true).Contains(eventActor))
                            flag_or = true;
                    }
                    else flag_or = true;
                }
                if (!flag_or)
                {
                    flag_and = false;
                    break;
                }
            }
            if (isLIFEF && flag_and)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    //加载eventDate
    [HarmonyPatch(typeof(ArchiveSystem.LoadGame), "LoadReadonlyData")] //可通过ArchiveSystem.LoadGame.LoadedReadonlyData()判断只读数据加载完成
    [HarmonyPriority(Priority.Last)]
    public static class GuiltyNature_LoadReadonlyData_Patch
    {
        static void Postfix()
        {
            if (ArchiveSystem.LoadGame.LoadedReadonlyData()) 
            {
                GameEvent.AddAll();
            }
        }
    }

    //处理Event
    [HarmonyPatch(typeof(MessageEventManager), "EndEvent")]
    public static class GuiltyNature_EndEvent_Patch
    {
        static bool Prefix(MessageEventManager __instance)
        {
            if (__instance.EventValue.Count > 0 && __instance.EventValue[0] != 0 && __instance.MainEventData.Length > 1)
            {
                int eventId = __instance.EventValue[0];
                int eventActor = __instance.MainEventData[1];
                if (eventId == QuquEvent.EventId)
                {
                    Main.Log("相虫事件跳转");
                    QuquEvent.EndEvent(eventActor);
                }
                else if (eventId == RapedEvent.ID.option1)
                {
                    Main.Log("被欺侮事件跳转");
                    RapedEvent.DoRape(eventActor, DateFile.instance.mianActorId);
                }
                else if (eventId == RapedEvent.ID.option21)
                {
                    Main.Log("没收工具事件跳转");
                    RapedEvent.Spay(eventActor);
                }
                else if (eventId == ConfuseEvent.ID.option1)
                {
                    Main.Log("乱情瘴跳转");
                    string name = ui_MessageWindow.inputText;
                    ConfuseEvent.EndEvent(eventActor, name);
                }
                else return true;
                __instance.EventValue = new List<int>();
                return false;
            }
            return true;
        }

    }
    //蛐蛐信息显示人名
    [HarmonyPatch(typeof(DateFile), "GetItemDate")]
    public static class GuiltyNature_GetItemDate_Patch
    {
        static void Postfix(ref string __result, int id, int index, bool otherMassage)
        {
            if (index != 0 || !otherMassage) return; //查询内容不是物品名称 || 物品名称不带品级等额外信息
            if (__result.Length < 2) return; //排除"0"
            var dict = GameData.Items.GetItem(id);
            if (dict == null) return; //排除预设物品
            if (!DateFile.instance.presetitemDate.TryGetValue(dict[999].ToInt(), out var dict2)) return; //排除异常物品
            if (dict2[2001] != "1") return; //非蛐蛐
            //获取数据
            if (ModDate.Get(out var modDate, ModDate.ququ) && modDate.ContainsKey(id.ToString()))
            {
                var actorInfo = modDate[id.ToString()].Split('|');
                __result += "\n" + actorInfo[0];
            }
        }
    }
    //蛐蛐寿命
    [HarmonyPatch(typeof(GetQuquWindow), "GetQuquDate")]
    public static class GuiltyNature_GetQuquDate_Patch
    {
        static bool Prefix(ref int __result, int itemId, int index)
        {
            if (index != 98) return true; //查询目标不为寿命
            var dict = GameData.Items.GetItem(itemId);
            if (dict == null) return true; //排除预设物品（异常数据）
            //获取数据
            if (ModDate.Get(out var modDate, ModDate.ququ) && modDate.ContainsKey(itemId.ToString()))
            {
                var actorInfo = modDate[itemId.ToString()].Split('|');
                __result = actorInfo[1].ToInt();
                return false;
            }
            return true;
        }
    }
    //化虫者头像显示为蛐蛐
    [HarmonyPatch(typeof(ActorFace), "UpdateFace")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int[]), typeof(int[]), typeof(int), typeof(bool), typeof(bool) })]
    public static class GuiltyNature_UpdateFace_Patch
    {
        static readonly Color NoColor = new Color(1f, 1f, 1f, 1f);
        static bool Prefix(ActorFace __instance, int actorId)
        {
            if (ModDate.TryGetValue(actorId.ToString(), out string value, ModDate.actor))
            {
                var info = value.Split('|');
                int colorId = info[0].ToInt();
                int partId = info[1].ToInt();
                __instance.ageImage.gameObject.SetActive(false);
                __instance.nose.gameObject.SetActive(false);
                __instance.faceOther.gameObject.SetActive(false);
                __instance.eye.gameObject.SetActive(false);
                __instance.eyePupil.gameObject.SetActive(false);
                __instance.eyebrows.gameObject.SetActive(false);
                __instance.mouth.gameObject.SetActive(false);
                __instance.beard.gameObject.SetActive(false);
                __instance.hair1.gameObject.SetActive(false);
                __instance.hair2.gameObject.SetActive(false);
                __instance.hairOther.gameObject.SetActive(false);
                __instance.clothes.gameObject.SetActive(false);
                __instance.clothesColor.gameObject.SetActive(false);
                __instance.body.gameObject.SetActive(true);
                var ququDate = DateFile.instance.cricketDate;
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(__instance.body, "cricketImage", new int[] {
                    partId > 0 ? ququDate[colorId][96].ToInt() : 36,
                    (ququDate[partId > 0 ? partId : colorId][96]).ToInt()
                });
                __instance.body.color = NoColor;

                return false;
            }
            return true;
        }
    }
    //修改Event选项条件文本
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class GuiltyNature_WindowSwitch_Patch
    {
        static void Postfix(WindowManage __instance, GameObject tips)
        {
            if (!Main.enabled) return;
            if (tips == null) return;
            string[] array = tips.name.Split(new char[]
            {
                ','
            });
            int id = (array.Length > 1) ? array[1].ToInt() : 0;
            string tag = tips.tag;
            if(tag == "MassageChooseNeed")
            {
                var eventDate = DateFile.instance.eventDate[id];
                if (id == ConfuseEvent.ID.option)
                {
                    string text = WrapString("[功法:乱情瘴]大成") + WrapString("对方所中幻毒超过毒抗");
                    __instance.informationMassage.text += text;
                }
            }
        }
        static string WrapString(string text)
        {
            return WindowManage.instance.Dit() + text + DateFile.instance.massageDate[157][1] + "\n";
        }
    }
    //婚恋无视血缘
    [HarmonyPatch(typeof(DateFile), "GetLifeDate")]
    public static class GuiltyNature_GetLifeDate_Patch
    {
        static bool Prefix(int actorId, int socialTyp, ref int __result)
        {
            if (!Main.enabled || !Main.settings.ignoreBlood) return true;
            if (socialTyp != 601 && socialTyp != 602) return true;
            if (!IsMatch()) return true;

            __result = actorId;
            return false;
        }

        static bool IsMatch()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(false);
            var frames = st.GetFrames();
            var len = frames.Length;
            int index = 3;
            string name;
            do 
            {
                var frame = frames[index++];
                var method = frame.GetMethod();
                name = method.Name;

                if (name.IndexOf("Child", comparisonType: StringComparison.OrdinalIgnoreCase) >= 0) return false; //遗传
                if (name.IndexOf("Favor", comparisonType: StringComparison.OrdinalIgnoreCase) >= 0) return false; //好感
                if (name.IndexOf("Switch", comparisonType: StringComparison.OrdinalIgnoreCase) >= 0) return false; //信息显示
                if (name.IndexOf("AI", comparisonType: StringComparison.OrdinalIgnoreCase) >= 0) return true; //NPC行为
                if (name.IndexOf("GetActor", comparisonType: StringComparison.OrdinalIgnoreCase) >= 0) return true; //对话选人框
                if (Regex.IsMatch(name, "M.ssageWindow")) return true; //对话选项
            } while (index < len);
            

            Main.Log("no match");
            for (int i = 0; i < len; i++) Main.Log($"Names[{i}] = {frames[i].GetMethod().Name}");
            return false;
        }
    }

}