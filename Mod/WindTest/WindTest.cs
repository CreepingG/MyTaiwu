using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Timers;

namespace WindTest
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
        public int actorDamage = 0;
        public int enemyDamage = 0;
        public bool lockTime = false;
        public bool skip = true;
        public bool maxWeight = true;
        public bool noFeature = false;
        public bool lockXX = false;
        public GameTuple Item = new GameTuple();
        public GameTuple Actor = new GameTuple();
        public bool partMatch = true;
        public int XXValue = 101;
        public bool ignoreEvents = false;
        public bool ignoreCreepings = true;
        public string input = "";
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string[] damageName = { "不变", "被秒", "免伤" };
        public static Timer timer;
        public static string inputItemId = "";
        public static string inputItemNumber = "1";
        public static string result = "";
        public static int WatchingItem = 0;
        static int _watchingActor = -1;
        public static int WatchingActor
        {
            get => _watchingActor > 0 ? _watchingActor : DateFile.instance.mianActorId;
            set
            {
                _watchingActor = value;
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
            /*timer = new Timer
            {
                Interval = 500d
            };
            timer.Elapsed += (object sender, ElapsedEventArgs e) => { "do something";};
            timer.Start();*/
            new GEventRegister();
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
            GUILayout.Label("太吾所受伤害：");
            settings.actorDamage = GUILayout.SelectionGrid(settings.actorDamage, damageName, 3);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("敌人所受伤害：");
            settings.enemyDamage = GUILayout.SelectionGrid(settings.enemyDamage, damageName, 3);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.skip = GUILayout.Toggle(settings.skip, "跳过测试版说明");
            settings.lockTime = GUILayout.Toggle(settings.lockTime, "行动力锁99");
            settings.maxWeight = GUILayout.Toggle(settings.maxWeight, "最大负重");
            settings.ignoreEvents = GUILayout.Toggle(settings.ignoreEvents, "无视时节开始的乞讨事件");
            settings.ignoreCreepings = GUILayout.Toggle(settings.ignoreCreepings, "自动驱逐外道（不打断移动）");
            GUILayout.EndHorizontal();
            /*GUILayout.BeginHorizontal();
            settings.input = GUILayout.TextField(settings.input);
            GUILayout.EndHorizontal();*/

            if (DateFile.instance != null && DateFile.instance.mianActorId > 0)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                bool flag = GUILayout.Button("获取物品", GUILayout.Width(100));
                GUILayout.Label("物品ID：", GUILayout.Width(70));
                inputItemId = GUILayout.TextField(inputItemId, GUILayout.Width(100));
                GUILayout.Label("数量：", GUILayout.Width(40));
                inputItemNumber = GUILayout.TextField(inputItemNumber, GUILayout.Width(40));
                if (flag)
                {
                    if (int.TryParse(inputItemId, out int itemId))
                    {
                        if (int.TryParse(inputItemNumber, out int n))
                        {
                            n = Mathf.Max(n, 1);
                        }
                        else
                        {
                            n = 1;
                        }
                        result = GiveItem(itemId, n);
                    }
                    else
                    {
                        result = "输入应为正整数";
                    }
                }
                GUILayout.Label(result);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("修改物品 （将鼠标移至物品图标上即可获取物品id）");
                GUILayout.EndHorizontal();
                SetGUI(Item.Instance);

                GUILayout.BeginHorizontal("Box");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("修改人物 （打开人物面板即可获取人物id，否则将获取太吾id）");
                GUILayout.EndHorizontal();
                SetGUI(Actor.Instance);
                
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("存档未载入");
                GUILayout.EndHorizontal();
            }
        }
        static void SetGUI(GameEntity ge)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"修改{ge.Title}: id = {ge.ID}, 名称 = {ge.GetName(ge.ID)}");
            GUILayout.Label("序号 =", GUILayout.Width(50));
            if (int.TryParse(
                GUILayout.TextField(ge.setting.key.ToString(), GUILayout.Width(40)),
                out int newKey
            ))
                ge.setting.key = newKey;
            GUILayout.Label("值 =", GUILayout.Width(50));
            ge.setting.value = GUILayout.TextField(ge.setting.value, GUILayout.Width(400));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("按序号查询", GUILayout.Width(100)))
                ge.setting.result = ExecuteBtn(ge, BtnTyp.FindKey);
            if (GUILayout.Button("按值查询", GUILayout.Width(80)))
                ge.setting.result = ExecuteBtn(ge, BtnTyp.FindValue);
            settings.partMatch = GUILayout.Toggle(settings.partMatch, "部分匹配", GUILayout.Width(80));
            if (GUILayout.Button("修改值", GUILayout.Width(60)))
                ge.setting.result = ExecuteBtn(ge, BtnTyp.SetValue);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.TextField(ge.setting.result);
            GUILayout.EndHorizontal();
        }
        static bool Match(string findStr, string patternStr)
        {
            bool fullMatch = findStr == patternStr;
            bool partMatch = patternStr != "0" && findStr.Contains(patternStr);
            return fullMatch || (settings.partMatch && partMatch);
        }
        enum BtnTyp { None, FindKey, FindValue, SetValue }
        static string ExecuteBtn(GameEntity ge, BtnTyp btn)
        {
            int inputKey = ge.setting.key;
            string inputValue = ge.setting.value;
            int id = ge.ID;
            var keys = ge.Keys;
            string name = ge.GetName(id);

            if (btn == BtnTyp.FindKey)
            {
                if (keys.Contains(inputKey))
                {
                    var v0 = ge.GetProp(id, inputKey, false);
                    var v1 = ge.GetProp(id, inputKey);
                    ge.setting.value = v0;
                    var show = v0 == v1 ? v0 : $"{v1}（{v0}）";
                    return $"{name}[{inputKey}] 为 {show}";
                }
                else
                    return $"错误的键";
            }
            else if (btn == BtnTyp.FindValue)
            {
                int origin = keys.IndexOf(inputKey);
                int index = origin;
                while (true)
                {
                    if (++index >= keys.Count) index = 0;
                    if (index == origin) break;
                    int key = keys[index];
                    var v0 = ge.GetProp(id, key, false);
                    var v1 = ge.GetProp(id, key);
                    if (Match(v0, inputValue))
                    {
                        ge.setting.key = key;
                        var show = v0 == v1 ? v0 : $"{v1}（{v0}）";
                        return $"{name}[{key}] 为 {show}，再次点击可查找下一个匹配项";
                    }
                }
                // 没找到
                if (Match(ge.GetProp(id, inputKey, false), inputValue))//初始位置为匹配项
                {
                    return ge.setting.result = $"{name}[{inputKey}] 为 {inputValue}，没有更多的匹配项";
                }
                ge.setting.key = -1;
                return "没有找到匹配项";
            }
            else if (btn == BtnTyp.SetValue)
            {
                if (!keys.Contains(inputKey)) return "错误的键";
                if (!ge.Has(id)) return "该物品无法被修改";
                var oldValue = ge.GetProp(id, inputKey, false);
                if (oldValue != inputValue)
                {
                    ge.SetProp(id, inputKey, inputValue);
                    return $"修改{name}[{inputKey}] {oldValue} => {inputValue}";
                }
                return "无效操作，因为实际值与输入值相同";
            }
            throw new Exception("Wrong BtnTyp");
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        //计时器到期后行动力改99
        static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
        }
        //给予物品
        public static string GiveItem(int itemId, int n = 1)
        {
            if (!Main.enabled) return "Mod未开启";
            if (DateFile.instance == null) return "存档未载入";
            int mainId = DateFile.instance.mianActorId;
            if (mainId <= 0) return "存档未载入";
            if (!DateFile.instance.presetitemDate.ContainsKey(itemId)) return "该ID没有对应的物品";
            if (n == 1)
            {
                int resultId = DateFile.instance.GetItem(mainId, itemId, n, true, 0);
                if (resultId == 0) return "无法获得该物品";
                string itemName = DateFile.instance.GetItemDate(resultId, 0, true).Replace("\n", @"\n");
                return $"成功获得物品：{itemName}";
            }
            else
            {
                if (int.Parse(DateFile.instance.GetItemDate(itemId, 4, true)) <= 0) return "无法获得该物品";
                List<int[]> itemList = new List<int[]>();
                int[] pair = { itemId, n };
                itemList.Add(pair);
                DateFile.instance.GetItem(mainId, itemList, true, 0);
                string itemName = DateFile.instance.GetItemDate(itemId, 0, true).Replace("\n", @"\n");
                return $"成功获得物品：{itemName} * {n}";
            }
        }

        public static void Say(object o)
        {
            var s = o.ToString();
            Logger.Log(s);
            Util.Show(s);
        }
    }

    public static class TestAction
    {
        public static void InvokeTest()
        {
            var name = "GuiltyNature.Main";
            var typ = AccessTools.TypeByName(name);
            if (typ is null)
            {
                Main.Logger.Log("typ is null");
                return;
            }
            var info = typ.GetField("enabled", AccessTools.all);
            Main.Logger.Log(((bool)info.GetValue(null)).ToString());
        }
        
        public static void Pregnant(int actorId = -1)
        {
            if (actorId == -1) actorId = DateFile.instance.mianActorId;
            if ((GameData.Characters.GetCharProperty(actorId, 26) ?? "0") == "0") //没死
            {
                DateFile.instance.ChangeActorFeature(actorId, 4001, 4002);
                DateFile.instance.ChangeActorFeature(actorId, 4002, 4003);
                DateFile.instance.actorLife[actorId].Add(901, new List<int>
                {
                    1,
                    0,
                    actorId,
                    0,
                    1
                });
            }
        }

        public static void PrintCombatPower(int charId)
        {
            Action<string> Log = Main.Logger.Log;
            Log($"被评选手\t{Util.GetActorName(charId)}");
            Log($"记录总分\t{GameData.Characters.GetCharProperty(charId, 993)}");
            int 战力评分 = 0;
            int 摧破平均伤害 = 0;
            int 摧破数 = 0;
            int prev = 0;
            Dictionary<int, int[]> actorEquipGongFa = DateFile.instance.GetActorEquipGongFa(charId);
            for (int i = 0; i < 5; i++)
            {
                int[] array = actorEquipGongFa[i];
                foreach (int num4 in array)
                {
                    if (num4 > 0)
                    {
                        // 功法得分
                        战力评分 += DateFile.instance.GetGongFaBaseScore(charId, num4);
                        if (int.Parse(DateFile.instance.gongFaDate[num4][6]) == 1) //摧破
                        {
                            摧破数++;
                            摧破平均伤害 += BattleVaule.instance.SetGongFaSpDamage(charId, num4);
                        }
                    }
                }
            }
            if (摧破数 > 0)
            {
                摧破平均伤害 /= 摧破数;
            }

            Log($"功法得分\t{战力评分 - prev}");
            prev = 战力评分;

            // 真气*40
            战力评分 += DateFile.instance.GetMaxGongFaSp(charId, 0) * 40;
            战力评分 += DateFile.instance.GetMaxGongFaSp(charId, 1) * 40;
            战力评分 += DateFile.instance.GetMaxGongFaSp(charId, 2) * 40;
            战力评分 += DateFile.instance.GetMaxGongFaSp(charId, 3) * 40;

            Log($"真气得分\t{战力评分 - prev}");
            prev = 战力评分;

            // 精纯×500
            战力评分 += BattleSystem.GetQiLevel(charId, applyEffect: false) * 500;

            Log($"精纯得分\t{战力评分 - prev}");
            prev = 战力评分;

            // 主要属性×3 防御属性×3
            for (int k = 0; k < 6; k++)
            {
                战力评分 += DateFile.instance.BaseAttr(charId, k, 0) * 3;
                战力评分 += BattleVaule.instance.GetDeferDefuse(true, charId, false, k, 0) * 3;
            }
            // 提气速度、架势速度、提气消耗、架势消耗、力道发挥、精妙发挥、迅疾发挥、造成外伤、造成内伤、破体强度、破气强度
            // 偏离100%的部分 ×5
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 1101)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 1102)) - 100) * 5;
            战力评分 -= (int.Parse(DateFile.instance.GetActorDate(charId, 1103)) - 100) * 5;
            战力评分 -= (int.Parse(DateFile.instance.GetActorDate(charId, 1104)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 92)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 93)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 94)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 95)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 96)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 97)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 98)) - 100) * 5;
            // 守御效率 2
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 22)) - 100) * 2;
            // 攻击速度、武器切换、施展速度 5
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 1107)) - 100) * 5;
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 1108)) - 100) * 5;
            战力评分 += (BattleVaule.instance.GetActorCastSpeed(charId) - 100) * 5;
            // 移动速度 2
            战力评分 += (BattleVaule.instance.GetMoveSpeed(false, charId, false) - 100) * 2;
            // 移动距离 5
            战力评分 += (int.Parse(DateFile.instance.GetActorDate(charId, 1111)) - 100) * 5;

            Log($"属性得分\t{战力评分 - prev}");
            prev = 战力评分;

            // 技艺资质 2
            for (int l = 0; l < 16; l++)
            {
                战力评分 += int.Parse(DateFile.instance.GetActorDate(charId, 501 + l)) * 2;
            }
            //武学资质 4
            for (int m = 0; m < 14; m++)
            {
                战力评分 += int.Parse(DateFile.instance.GetActorDate(charId, 601 + m)) * 4;
            }

            Log($"资质得分\t{战力评分 - prev}");
            prev = 战力评分;

            // 装备品级 * 50
            for (int n = 0; n < 12; n++)
            {
                int num5 = int.Parse(DateFile.instance.GetActorDate(charId, 301 + n, applyBonus: false));
                if (num5 > 0)
                {
                    战力评分 += int.Parse(DateFile.instance.GetItemDate(num5, 8)) * 50;
                }
            }

            Log($"装备得分\t{战力评分 - prev}");
            prev = 战力评分;

            int num6 = 0;
            int num7 = 0;
            for (int num8 = 0; num8 < 6; num8++)
            {
                num6 += DateFile.instance.Poison(charId, num8);
                num7 += DateFile.instance.MaxPoison(charId, num8);
            }
            // 所中毒素/毒抗
            int num9 = num6 * 100 / Mathf.Max(num7, 1);
            战力评分 = 战力评分 * Mathf.Max(100 - num9, 0) / 100;
            Log($"毒素影响\t-{num9}%");
            // 所受内外伤/内外伤上限
            int num10 = DateFile.instance.Hp(charId) + DateFile.instance.Sp(charId);
            int a = DateFile.instance.MaxHp(charId) + DateFile.instance.MaxSp(charId);
            int num11 = num10 * 100 / Mathf.Max(a, 1);
            Log($"伤势影响\t-{num9}%");
            战力评分 = 战力评分 * Mathf.Max(100 - num11, 0) / 100;
            Log($"实时总分\t{战力评分}");
        }

        public static void MakeStories()
        {
            var aggr = new DefaultDictionary<int, int>();
            var storyId = int.TryParse(Main.settings.input, out int num) ? num : 10001;
            DateFile.instance.AllMapRandSetStory(storyId, 6, true, 1);
            foreach (var part in DateFile.instance.worldMapState)
            {
                var cnt = part.Value.Count(place => place.Value[0] == storyId);
                var partId = part.Key;
                var name = DateFile.instance.placeWorldDate.ContainsKey(partId) ? DateFile.instance.placeWorldDate[partId][98] : partId.ToString();
                //Main.Logger.Log($"{name}::{cnt}");
                aggr[cnt] += 1;
            }
            var sum = aggr.Sum(kvp => kvp.Value);
            foreach (var kvp in aggr)
            {
                Main.Logger.Log($"{kvp.Key}::{kvp.Value * 100 / sum}%");
            }
        }

        public static void MakeChilrenAlone()
        {
            var actorId = int.TryParse(Main.settings.input, out int num) ? num : DateFile.instance.mianActorId;
            for (int i = 0; i < 20; i++)
            {
                foreach (var childId in DateFile.instance.MakeNewChildren(DateFile.instance.mianActorId, actorId, true, true))
                {
                    GameData.Characters.SetCharProperty(childId, 11, "20");
                    DateFile.instance.ActorFeaturesCacheReset(childId);
                    //DateFile.instance.NewActorFeatures(childId,null, DateFile.instance.mianActorId, DateFile.instance.mianActorId);
                }
            }
        }

        public static void RepeatMarry()
        {
            DateFile.instance.actorLife[DateFile.instance.mianActorId].Remove(901); //移除怀孕信息
            var actorId = int.TryParse(Main.settings.input, out int num) ? num : Actor.Instance.ID;
            for (int i = 0; i < 100; i++)
            {
                MessageEventManager.Instance.SetSpouseSocial(actorId, DateFile.instance.mianActorId, 0);
            }
        }

        public static void ChangeWariness()
        {
            int actorId = Main.settings.input.ToInt(Actor.Instance.ID);
            for (int goodness = 0; goodness < 5; goodness++)
            {
                Actor.Instance.SetProp(actorId, 16, (goodness * 250).ToString());
                Actor.Instance.SetProp(DateFile.instance.mianActorId, 16, (goodness * 250).ToString());
                Actor.Instance.SetProp(actorId, 3, (10000).ToString());
                string goodnessText = DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(actorId)];
                DateFile.instance.actorTalkFavor.Remove(actorId);
                int prevWariness = DateFile.instance.GetActorWariness(actorId);
                int prevFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.mianActorId, actorId);
                DateFile.instance.SetTalkFavor(actorId, 0, 1000);
                int curWariness = DateFile.instance.GetActorWariness(actorId);
                int curFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.mianActorId, actorId);
                Main.Logger.Log($"{goodnessText}");
                Main.Logger.Log($"好感：{prevFavor}→{curFavor}");
                Main.Logger.Log($"戒心：{prevWariness}→{curWariness}");
            }
        }
    }

    public static class Util
    {
        public static string Dit() => WindowManage.instance.Dit();
        public static string Cut(int color = 20002) => WindowManage.instance.Cut(color);
        public static string Color(int color, string s) => DateFile.instance.SetColoer(color, s);
        public static string Color(int color, object s) => DateFile.instance.SetColoer(color, s.ToString());
        public static string ColorName(string name) => Color(10002, name);
        public static int ToInt(this string s) => Convert.ToInt32(s);
        public static int ToInt(this string s, int ifNot) => int.TryParse(s, out int i) ? i : ifNot;
        public static decimal ToDecimal(this string s) => decimal.Parse(s);

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

    public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue Default;
        public new TValue this[TKey key]
        {
            get => ContainsKey(key) ? base[key] : Default;
            set => base[key] = value;
        }
    }

    public class GameTuple
    {
        public int key = -1;
        public string value = "";
        public string result = "";
    }

    abstract class GameEntity
    {
        public string Title;
        public abstract GameTuple setting { get; }
        public abstract int ID { get; }
        public abstract bool Has(int id);
        public Dictionary<int, Dictionary<int, string>> PresetData;
        List<int> _keys;
        public List<int> Keys
        {
            get
            {
                if (_keys == null) _keys = PresetData.First().Value.Keys.ToList();
                return _keys;
            }
        }
        public abstract string GetName(int id);
        public abstract Dictionary<int, string> GetData(int id);
        public abstract void SetData(int id, Dictionary<int, string> data);
        public abstract string GetProp(int id, int key, bool other = true);
        public abstract void SetProp(int id, int key, string value);
    }
    class Item : GameEntity
    {
        static Item _instance;
        static public Item Instance { get => _instance = (_instance ?? new Item { Title = "物品", PresetData = DateFile.instance.presetitemDate }); }
        public override GameTuple setting { get => Main.settings.Item; }
        public override int ID => Main.WatchingItem;
        public override bool Has(int id) => GameData.Items.GetItem(id) != null;
        public override string GetName(int id) => DateFile.instance.GetItemDate(id, 0, false);
        public override Dictionary<int, string> GetData(int id) => GameData.Items.GetItem(id) ?? PresetData[id];
        public override void SetData(int id, Dictionary<int, string> data) => GameData.Items.SetItem(id, data);
        public override string GetProp(int id, int key, bool other = true) => DateFile.instance.GetItemDate(id, key, other);
        public override void SetProp(int id, int key, string value) => GameData.Items.SetItemProperty(id, key, value);
    }
    class Actor : GameEntity
    {
        static Actor _instance;
        static public Actor Instance { get => _instance = (_instance ?? new Actor { Title = "人物", PresetData = DateFile.instance.presetActorDate }); }
        public override GameTuple setting { get => Main.settings.Actor; }
        public override int ID => ActorMenu.Exists ? ActorMenu.instance.actorId : DateFile.instance.mianActorId;
        public override bool Has(int id) => GameData.Characters.HasChar(id);
        public override string GetName(int id) => DateFile.instance.GetActorName(id);
        public override Dictionary<int, string> GetData(int id) => null;
        public override void SetData(int id, Dictionary<int, string> data) { }
        public override string GetProp(int id, int key, bool other = true) => DateFile.instance.GetActorDate(id, key, other);
        public override void SetProp(int id, int key, string value)
        {
            GameData.Characters.SetCharProperty(id, key, value);
            if (key == 101) DateFile.instance.ActorFeaturesCacheReset(id);
        }
    }

    class GEventRegister
    {
        public GEventRegister()
        {
            GEvent.Add(eEvents.TurnChangeFinish, IgnoreEvent);
            GEvent.Add(eEvents.LoadedSavedAndBaseData, IgnoreEvent);
        }

        void IgnoreEvent(params object[] args)
        {
            if (Main.enabled && Main.settings.ignoreEvents)
            {
                var len0 = DateFile.instance.eventId.Count;
                DateFile.instance.eventId = DateFile.instance.eventId.Where(arr =>
                {
                    var isBegging = arr.Length > 2 && (arr[2] >= 207 && arr[2] <= 224 || arr[2] == 230);
                    if (isBegging)
                    {
                        try
                        {
                            Main.Logger.Log($"忽略【{DateFile.instance.GetActorName(arr[1])}】的【{DateFile.instance.eventDate[arr[2]][3]}】事件");
                        }
                        catch (Exception)
                        {
                            Main.Logger.Log($"忽略【{arr[1]}】的【{arr[2]}】事件");
                        }
                    }
                    return !isBegging;
                }).ToList(); //筛选事件id范围
                var len1 = DateFile.instance.eventId.Count;
                if (len0 > len1)
                {
                    Util.Show($"忽略了{len0 - len1}个乞讨事件");
                }
            }
        }
    }

    class Patch
    {
        //攻击完成后秒杀
        [HarmonyPatch(typeof(BattleSystem), "ActionEventAttack")]
        class WindTest_ActionEventAttack_Patch
        {
            static void Postfix(BattleSystem __instance, bool isActor)
            {
                if (!Main.enabled) return;
                int attackerId = __instance.ActorId(isActor, true);
                int defenderId = __instance.ActorId(!isActor, false);
                isActor = !isActor;//attackerIsActor => defenderIsActor
                bool flag1 = isActor && Main.settings.actorDamage == 1;
                bool flag2 = !isActor && Main.settings.enemyDamage == 1;
                if (!flag1 && !flag2) return;
                int damageTyp = 0;
                int injuryId = 49;
                int injuryDate = int.Parse(DateFile.instance.injuryDate[injuryId][damageTyp + 1]);
                int maxHp = DateFile.instance.MaxHp(defenderId);
                int hp = DateFile.instance.Hp(defenderId);
                int needDamage = maxHp - hp;
                int injuryPower = needDamage * 100 / injuryDate;
                BattleSystem.instance.AddBattleInjury(isActor, defenderId, attackerId, injuryId, injuryPower);
                int index = isActor ? DateFile.instance.actorBattlerIdDate.IndexOf(defenderId) : DateFile.instance.enemyBattlerIdDate.IndexOf(defenderId);
                __instance.UpdateActorHpSpBar(index, isActor, true, false);

            }
        }
        //阻止新增伤口
        [HarmonyPatch(typeof(BattleSystem), "AddBattleInjury")]
        class WindTest_AddBattleInjury_Patch
        {
            static bool Prefix(BattleSystem __instance, bool isActor, int actorId, int attackerId, int injuryId, int injuryPower, ref List<int> __result)
            {
                if (!Main.enabled) return true;
                bool flag1 = isActor && Main.settings.actorDamage == 2;
                bool flag2 = !isActor && Main.settings.enemyDamage == 2;
                if (flag1 || flag2)
                {
                    __result = new List<int>
                {
                    injuryId,
                    injuryPower,
                    actorId
                };
                    return false;
                }
                return true;
            }
        }
        //阻止伤口扩大
        [HarmonyPatch(typeof(BattleSystem), "ChangeBattleInjury")]
        class WindTest_ChangeBattleInjury_Patch
        {
            static void Prefix(BattleSystem __instance, ref int actorId, ref int value)
            {
                if (!Main.enabled) return;
                bool isActor = actorId == DateFile.instance.mianActorId;
                bool flag1 = isActor && Main.settings.actorDamage == 2;
                bool flag2 = !isActor && Main.settings.enemyDamage == 2;
                if (flag1 || flag2) value = 0;
                return;
            }
        }
        //跳过测试版说明
        [HarmonyPatch(typeof(MainMenu), "PageReady")]
        class WindTest_PageReady_Patch
        {
            static void Prefix(ref bool ___showStartMassage)
            {
                if (!Main.enabled || !Main.settings.skip)
                {
                    return;
                }
                ___showStartMassage = false;
            }
        }
        //最大负重
        [HarmonyPatch(typeof(DateFile), "GetMaxItemSize")]
        class WindTest_GetMaxItemSize_Patch
        {
            static bool Prefix(ref int key, ref int __result)
            {
                if (!Main.enabled || !Main.settings.maxWeight) return true;
                if (key != DateFile.instance.mianActorId) return true;
                __result = 999999999;
                return false;
            }
        }
        //记录物品
        [HarmonyPatch(typeof(WindowManage), "ShowItemMassage")]
        class WindTest_ShowItemMassage_Patch
        {
            static void Postfix(int itemId, int showActorId)
            {
                Main.WatchingItem = itemId;
            }
        }
        //入邪
        [HarmonyPatch(typeof(DateFile), "GetLifeDate")]
        class WindTest_GetLifeDate_Patch
        {
            static bool Prefix(int actorId, int socialTyp, int index, ref int __result)
            {
                if (!Main.enabled || !Main.settings.lockXX) return true;
                if (socialTyp == 501 && index == 0)
                {
                    /*int prev = DateFile.instance.HaveLifeDate(actorId, socialTyp) && DateFile.instance.actorLife[actorId][socialTyp].Count > index ? 
                        DateFile.instance.actorLife[actorId][socialTyp][index] : -1;*/
                    __result = Main.settings.XXValue;
                    return false;
                }
                return true;
            }
        }
        //刷新行动力的时候
        [HarmonyPatch(typeof(ui_BottomLeft), "UpdateTime")]
        class WindTest_UpdateTime_Patch
        {
            static void Prefix()
            {
                if (!Main.enabled) return;

                if (Main.settings.lockTime)
                {
                    DateFile.instance.dayTime = 99;
                }

            }
        }
        // 拦截地图野怪逃跑并给予威望，不打断移动
        [HarmonyPatch(typeof(UIManager), "AddUI")]
        class WindTest_AddUI_Patch
        {
            static bool Prefix(string uiPrefabName, params object[] onShowArgs)
            {
                if (Main.enabled && Main.settings.ignoreCreepings && uiPrefabName == "ui_MessageWindow" && onShowArgs.Length > 0)
                {
                    var arr = onShowArgs[0] as int[];
                    if (arr?.Length > 2 && arr[2] == 112) //事件id
                    {
                        MessageEventManager.Instance.MainEventData = arr;
                        EndEvent113_3();
                        return false;
                    }
                }
                return true;
            }

            static MethodInfo _method;
            static void EndEvent113_3()
            {
                if (_method == null)
                {
                    _method = typeof(MessageEventManager).GetMethod("EndEvent113_3", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                _method.Invoke(MessageEventManager.Instance, new object[] { });
            }

        }

    }
}