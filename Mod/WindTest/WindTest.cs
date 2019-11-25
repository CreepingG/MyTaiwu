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
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
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
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string[] damageName = { "不变", "被秒", "免伤" };
        public static Timer timer = new Timer();
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
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            timer.Interval = 500d;
            timer.Elapsed += TimerElapsed;
            timer.Start();
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
            //settings.noFeature = GUILayout.Toggle(settings.noFeature, "无特性");
            settings.lockXX = GUILayout.Toggle(settings.lockXX, "锁入魔值");
            if (settings.lockXX && int.TryParse(GUILayout.TextField(settings.XXValue.ToString()), out int tmp)) settings.XXValue = tmp;
            settings.ignoreEvents = GUILayout.Toggle(settings.ignoreEvents, "无视时节开始事件与外道事件");
            //if (GUILayout.Button("当场怀孕")) Pregnant();
            //if (GUILayout.Button("test")) InvokeTest();
            GUILayout.EndHorizontal();
            
            if(DateFile.instance != null && DateFile.instance.mianActorId > 0)
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
                        int n;
                        if (int.TryParse(inputItemNumber, out n))
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
                    var value = ge.GetProp(id, inputKey);
                    ge.setting.value = value;
                    return $"{name}[{inputKey}] 为 {value}";
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
                    string value = ge.GetProp(id, key, false);
                    if (Match(value, inputValue))
                    {
                        ge.setting.key = key;
                        return $"{name}[{key}] 为 {value}，再次点击可查找下一个匹配项";
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
                if (resultId == 0 ) return "无法获得该物品";
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
        //怀孕
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

    //攻击完成后秒杀
    [HarmonyPatch(typeof(BattleSystem), "ActionEventAttack")]
    public static class WindTest_ActionEventAttack_Patch
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
            int index = isActor ? DateFile.instance.actorBattlerIdDate.IndexOf(defenderId) : DateFile.instance.enemyBattlerIdDate.IndexOf(defenderId);
            var method = typeof(BattleSystem).GetMethod("AddBattleInjury",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(__instance, new object[] { isActor, defenderId, injuryId, injuryPower, false});
            __instance.UpdateActorHpSpBar(index, isActor, true, false);

        }
    }
    //阻止新增伤口
    [HarmonyPatch(typeof(BattleSystem), "AddBattleInjury")]
    public static class WindTest_AddBattleInjury_Patch
    {
        static bool Prefix(BattleSystem __instance, ref bool isActor)
        {
            if (!Main.enabled) return true;
            bool flag1 = isActor && Main.settings.actorDamage == 2;
            bool flag2 = !isActor && Main.settings.enemyDamage == 2;
            if (flag1 || flag2) return false;
            return true;
        }
    }
    //阻止伤口扩大
    [HarmonyPatch(typeof(BattleSystem), "ChangeBattleInjury")]
    public static class WindTest_ChangeBattleInjury_Patch
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
    public static class WindTest_PageReady_Patch
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
    public static class WindTest_GetMaxItemSize_Patch
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
    public static class WindTest_ShowItemMassage_Patch
    {
        static void Postfix(int itemId, int showActorId)
        {
            Main.WatchingItem = itemId;
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
            if (Main.settings.ignoreEvents)
            {
                Main.Show($"忽略了{DateFile.instance.eventId.Count}个事件");
                DateFile.instance.eventId.Clear();
            }
        }
    }
    
    //入邪
    [HarmonyPatch(typeof(DateFile), "GetLifeDate")]
    public static class WindTest_GetLifeDate_Patch
    {
        static bool Prefix(int actorId, int socialTyp, int index, ref int __result)
        {
            if (!Main.enabled) return true;
            if (socialTyp == 501 && index == 0)
            {
                int prev = DateFile.instance.HaveLifeDate(actorId, socialTyp) && DateFile.instance.actorLife[actorId][socialTyp].Count > index ? 
                    DateFile.instance.actorLife[actorId][socialTyp][index] : -1;
                Main.Logger.Log(DateFile.instance.GetActorName(actorId) + " : " + prev.ToString());
                __result = Main.settings.XXValue;
                return false;
            }
            //印象
            /*if (socialTyp == 1001 && index == 1)
            {
                __result = 100;
                return false;
            }*/
            return true;
        }
    }

    //刷新行动力的时候
    [HarmonyPatch(typeof(ui_BottomLeft), "UpdateTime")]
    public static class WindTest_UpdateTime_Patch
    {
        /*static string GetMethodInfo(MethodBase m) => $"{m.DeclaringType}.{m.Name}({m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}").Join()})";
        string stack = new System.Diagnostics.StackTrace(false).GetFrames().Select(f => GetMethodInfo(f.GetMethod())).Join(null, "\n>");
        Main.Show(stack);
        Main.Logger.Log(stack);*/
        static void Prefix()
        {
            if (!Main.enabled) return;
            
            if (Main.settings.lockTime)
            {
                DateFile.instance.dayTime = 99;
            }
            
        }
    }

    // 攔截遭遇逃亡小怪事件
    [HarmonyPatch(typeof(ui_MessageWindow), "SetEventWindow")]
    public class WindTest_SetEventWindow_Patch
    {
        private static void Postfix(ui_MessageWindow __instance, int[] eventDate)
        {
            if (Main.enabled && eventDate.Length == 4 && eventDate[2] == 112)
            {
                GameObject choose = UnityEngine.Object.Instantiate<GameObject>(__instance.massageChoose1, Vector3.zero, Quaternion.identity);
                choose.name = "Choose,11200002";
                choose.GetComponent<Button>().onClick.Invoke();
                UnityEngine.Object.Destroy(choose);
            }
        }

        private static Button GetSkipButtonFromComponets(ui_MessageWindow instance)
        {
            var skipBtn = instance.GetComponentsInChildren<Button>().FirstOrDefault(btn => btn.name == "Choose,11200002");
            if (skipBtn == null)
            {
                Main.Logger.Log("Could't find the skip choose!");
                return null;
            }

            return skipBtn;
        }


    }

}