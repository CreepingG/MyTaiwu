using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace MakeYourWant
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool IMBA = false;
        public bool global = false;
        public bool needNoTime = false;
        public bool freeTyp = false;
        public bool changeRate = false;
        public int rate = 0;
        public bool changePoison = false;
        public int[] poison = new int[6] { 0, 0, 0, 0, 0, 0 };
        public bool 神兵 = false;
        public bool lastWish = false;
        public bool 传家宝 = false;
        //武器
        public bool SetWeapon = false;
        public bool WeaponMoreDuration = false;
        public bool AttackTypExtend = false;
        public int[] AttackTyp = {15,15,15,15,15,15};//式
        public int[] WeaponPower = { 3, 0, 0, 5 };
        public int WeaponEngraving = 0;//功法发挥
        //防具
        public bool SetArmor = false;
        public int ArmorPower = 0;
        public int ArmorAttr = 0;
        //饰品
        public bool SetJewelry = false;
        public bool JewelryPowerMustBe = false;
        public int[] JewelryAttr = { 0, 0, 0 };
        //食物
        public bool SetFood = false;
        public int[] FoodAttr = { 0, 0, 0 };
        //药品
        public bool setMedicine = false;

        public bool[] SetEnaled = { false, false, false, false, false };
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        static string[] attackTypName;
        static string[] attackTypId;
        static string[] AttackTypName
        {
            get
            {
                if (attackTypName == null)
                {
                    attackTypName = DateFile.instance.attackTypDate
                        .Select(kvp => kvp.Value).Where(v => v[1] != "0").Select(v => v[0])
                        .Add("?").ToArray();
                    attackTypId = DateFile.instance.attackTypDate
                        .Where(kvp => kvp.Value[1] != "0").Select(kvp => kvp.Key.ToString())
                        .Add("").ToArray();
                }
                return attackTypName;
            }
        }
        public static string[] AttackTypId
        {
            get
            {
                if (attackTypName == null)
                {
                    attackTypName = DateFile.instance.attackTypDate
                        .Select(kvp => kvp.Value).Where(v => v[1] != "0").Select(v => v[0])
                        .Add("?").ToArray();
                    attackTypId = DateFile.instance.attackTypDate
                        .Where(kvp => kvp.Value[1] != "0").Select(kvp => kvp.Key.ToString())
                        .Add("").ToArray();
                }
                return attackTypId;
            }
        }
        static readonly string[][] WeaponPowerName = {
            new string[] { "破", "辟", "杀", "其他" },
            new string[] { "掌", "剑", "刀", "毒", "长鞭", "软兵", "暗器",
            "奇门", "魔音", "金", "木", "玉" },
            new string[] { "武器杀", "材质杀" },
            new string[] { "厚重", "轻盈", "锋锐", "钝拙", "贵重", "随机" }
        };
        static readonly string[] WeaponEngravingName = { "随机", "力道", "精妙", "迅疾", "内息" };
        static readonly string[] ArmorPowerName = { "随机", "不变", "厚重", "轻盈", "藏锋", "百折", "贵重" };
        static readonly string[] ArmorAttrName = { "随机", "膂力", "体质", "灵敏", "根骨", "悟性", "定力" };
        static readonly string[][] AttrName = {
            new string[] { "随机", "技艺", "武学" },
            new string[] { "音律", "弈棋", "诗书", "绘画", "术数", "品鉴", "锻造", "制木",
                "医术", "毒术", "织锦", "巧匠", "道法", "佛学", "厨艺", "杂学" },
            new string[] { "内功", "身法", "绝技", "拳掌", "指法", "腿法", "暗器",
                "剑法", "刀法", "长兵", "奇门", "软兵", "御射", "乐器" }
        };
        static readonly string[] PoisonName = { "烈毒", "郁毒", "寒毒", "赤毒", "腐毒", "幻毒" };


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!ArchiveSystem.LoadGame.LoadedReadonlyData())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("等待游戏数据加载完成", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#F28234FF>跨月制作的装备在投入材料时决定</color>", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.SetWeapon = GUILayout.Toggle(settings.SetWeapon, "定制武器", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetWeapon)
            {
                GUILayout.BeginHorizontal();
	            GUILayout.Label("式排列", GUILayout.Width(100));
                string[] s = new string[6];
                for (int i = 0; i < 6; i = i + 1)
                {
                    s[i] = AttackTypName[settings.AttackTyp[i]];
                }
                GUILayout.Label(string.Concat("<size=15>",string.Join("|",s),"</size>"), GUILayout.Width(150));
                if (GUILayout.Button((settings.AttackTypExtend ? "收起" : "展开"), GUILayout.Width(100)))
                {
                    settings.AttackTypExtend = !settings.AttackTypExtend;
                }
                GUILayout.Label("<color=#E4504DFF>不会产生武器没有的式</color>", GUILayout.Width(210));
                settings.WeaponMoreDuration = GUILayout.Toggle(settings.WeaponMoreDuration, "较高耐久", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                if (settings.AttackTypExtend)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        GUILayout.BeginHorizontal();
                        settings.AttackTyp[i] = GUILayout.SelectionGrid(settings.AttackTyp[i], AttackTypName, AttackTypName.Length);
                        GUILayout.EndHorizontal();
                    }
                }
	            GUILayout.BeginHorizontal();
	            GUILayout.Label("词条", GUILayout.Width(100));
                settings.WeaponPower[0] = GUILayout.SelectionGrid(settings.WeaponPower[0], WeaponPowerName[0], 4);
		        GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                var index = settings.WeaponPower[0]==0 ? 1 : settings.WeaponPower[0];
                settings.WeaponPower[index] = GUILayout.SelectionGrid(settings.WeaponPower[index], WeaponPowerName[index], WeaponPowerName[index].Length);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
	            GUILayout.Label("功法发挥", GUILayout.Width(100));
                settings.WeaponEngraving = GUILayout.SelectionGrid(settings.WeaponEngraving, WeaponEngravingName, 5);
	            GUILayout.EndHorizontal();
		    }
            
            GUILayout.BeginHorizontal("Box");
            settings.SetArmor = GUILayout.Toggle(settings.SetArmor, "定制防具", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetArmor)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("词条", GUILayout.Width(100));
                settings.ArmorPower = GUILayout.SelectionGrid(settings.ArmorPower, ArmorPowerName, 7);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("额外属性", GUILayout.Width(100));
                settings.ArmorAttr = GUILayout.SelectionGrid(settings.ArmorAttr, ArmorAttrName, 7);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal("Box");
            settings.SetJewelry = GUILayout.Toggle(settings.SetJewelry, "定制饰品", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetJewelry)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("词条", GUILayout.Width(100));
                settings.JewelryPowerMustBe = GUILayout.Toggle(settings.JewelryPowerMustBe, "必出贵重", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("额外属性", GUILayout.Width(100));
                settings.JewelryAttr[0] = GUILayout.SelectionGrid(settings.JewelryAttr[0], AttrName[0], 3);
                GUILayout.EndHorizontal();
                var index = settings.JewelryAttr[0];
                if (index != 0)
                {
                    GUILayout.BeginHorizontal();
                    settings.JewelryAttr[index] = GUILayout.SelectionGrid(settings.JewelryAttr[index], AttrName[index], AttrName[index].Length / 2);
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.BeginHorizontal("Box");
            settings.SetFood = GUILayout.Toggle(settings.SetFood, "定制食物", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetFood)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("额外属性", GUILayout.Width(100));
                settings.FoodAttr[0] = GUILayout.SelectionGrid(settings.FoodAttr[0], AttrName[0], 3);
                GUILayout.EndHorizontal();
                var index = settings.FoodAttr[0];
                if (index != 0)
                {
                    GUILayout.BeginHorizontal();
                    settings.FoodAttr[index] = GUILayout.SelectionGrid(settings.FoodAttr[index], AttrName[index], AttrName[index].Length / 2);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.BeginHorizontal("Box");
            bool setDrug = GUILayout.Toggle(settings.setMedicine, "定制药品", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.setMedicine != setDrug)
            {
                settings.setMedicine = setDrug;
                MedicineSwitch();
            }
            if (setDrug)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("必定产出品阶更高的药品", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal("Box");
            settings.IMBA = GUILayout.Toggle(settings.IMBA, "IMBA模式（破坏游戏平衡，慎用）", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if(settings.IMBA)
            {
                GUILayout.BeginHorizontal();
                settings.global = GUILayout.Toggle(settings.global, "影响所有新产生的装备，包括商店出售的与NPC获得的（如果版本变化有小概率坏档，慎用）", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.needNoTime = GUILayout.Toggle(settings.needNoTime, "所有装备制造立刻完成", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.freeTyp = GUILayout.Toggle(settings.freeTyp, "式的定制不受限制", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.changeRate = GUILayout.Toggle(settings.changeRate, "改变内伤比例", GUILayout.Width(120));
                string rate = GUILayout.TextField(settings.rate.ToString(), GUILayout.Width(40));
                if (int.TryParse(rate, out int temp) && temp >= 0 && temp <= 100)
                {
                    settings.rate = temp;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.changePoison = GUILayout.Toggle(settings.changePoison, "含有毒素：", GUILayout.Width(100));
                for(int i = 0; i < 6; i++)
                {
                    GUILayout.Label("  " + PoisonName[i], GUILayout.Width(40));
                    string poison = GUILayout.TextField(settings.poison[i].ToString(), new GUILayoutOption[0]);
                    if (int.TryParse(poison, out int temp1) && temp1 >= 0)
                    {
                        settings.poison[i] = temp1;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.神兵 = GUILayout.Toggle(settings.神兵, "神兵（耐久不会因任何原因减少）", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.传家宝 = GUILayout.Toggle(settings.传家宝, "传家宝", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.lastWish = GUILayout.Toggle(settings.lastWish, "遗愿加成(必得双倍额外属性+必得两种副产品)", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static string[] medicineIds = new string[25];
        public static void MedicineSwitch()
        {
            if (DateFile.instance == null) return;
            var makeItemDate = DateFile.instance.makeItemDate;
            if (makeItemDate == null || makeItemDate.Count == 0) return;
            if(!makeItemDate.ContainsKey(901))
            {
                Main.Logger.Log("makeItemDate没有901项");
                return;
            }
            var makeDrugDate = makeItemDate[901];
            bool enabled = settings.setMedicine;
            if (enabled)
            {
                medicineIds[0] = "";
                for (int i = 1; i <= 24; i++)
                {
                    string oldStr = makeDrugDate[i];
                    try
                    {
                        List<int> ids = new List<int>(oldStr.Split('|').Select(id => int.Parse(id)));
                        medicineIds[i] = oldStr;
                        if (ids.Count <= 1)
                        {
                            Main.Logger.Log(GetItemName(oldStr));
                        }
                        else
                        {
                            int bestId = KeyOfMaxValue(ids, GetItemLevel);
                            makeDrugDate[i] = bestId.ToString();
                            string log = string.Join("|", ids.Select(id => GetItemName(id)).ToArray());
                            Main.Logger.Log($"{log} => {GetItemName(bestId)}");
                        }
                    }
                    catch (Exception)
                    {
                        Main.Logger.Log(oldStr);
                    }
                    
                }
            }
            else
            {
                if (medicineIds[0] == null) return;
                for (int i = 1; i <= 24; i++)
                {
                    string tmp = medicineIds[i];
                    medicineIds[i] = makeDrugDate[i];
                    makeDrugDate[i] = tmp;
                    string log = string.Join("|", tmp.Split('|').Select(id => int.Parse(id)).Select(id => GetItemName(id)).ToArray());
                    Main.Logger.Log($"{GetItemName(medicineIds[i])} => {log}");
                }
            }
        }
        static T KeyOfMaxValue<T>(IEnumerable<T> enumerable, Func<T,int> Key2Value)
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
        static int GetItemLevel(int id) => int.Parse(DateFile.instance.presetitemDate[id][8]);
        public static string GetItemName(string itemId, bool showTyp = false)
        {
            return int.TryParse(itemId, out int id) ? GetItemName(id, showTyp) : $"I2S Failed: itemId = {itemId}";
        }
        public static string GetItemName(int itemId, bool showTyp = false)
        {
            string name = DateFile.instance.presetitemDate[itemId][0];
            if (int.TryParse(DateFile.instance.presetitemDate[itemId][8], out int grade))
            {
                name = DateFile.instance.SetColoer(20001 + grade, name);
            }
            return name;
        }

    }

    [HarmonyPatch(typeof(MakeSystem), "SetMakeItem")]
    public static class MakeYourWant_SetMakeItem_Patch
    {
        static void Prefix()
        {
            if (!Main.enabled) return;

            bool lastWish = Main.settings.IMBA && Main.settings.lastWish;
            if (lastWish)
            {
                DateFile.instance.makePower = true;
            }

            if (Main.settings.setMedicine && Main.medicineIds[0] == null) Main.MedicineSwitch();

            // 应对玩家先打开制作面板，后开启mod的情况
            bool noTime = Main.settings.IMBA && Main.settings.needNoTime;
            if (noTime)
            {
                typeof(MakeSystem).GetField("useTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(MakeSystem.instance, 0);
            }
        }
    }
    [HarmonyPatch(typeof(DateFile), "GetItemDate")]
    public static class MakeYourWant_GetItemDate_Patch
    {
        static bool Prefix(DateFile __instance, ref string __result, ref int index)
        {
            if (!Main.enabled) return true;
            if (index == 47 && Main.settings.IMBA && Main.settings.needNoTime)
            {
                __result = "0";
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(DateFile), "MakeNewItem")]
    public static class MakeYourWant_MakeNewItem_Patch
    {
        readonly static MethodInfo changeNewItemSPower = typeof(DateFile).GetMethod("ChangeNewItemSPower", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void ChangeNewItemSPower(Dictionary<int, string> itemData, int baseItemId, int powerId)
        {
            changeNewItemSPower.Invoke(DateFile.instance, new object[] { itemData, baseItemId, powerId });
        }
        static bool Prefix(DateFile __instance, ref int __result,ref int presetItemId, ref int temporaryId, 
            ref int bookObbs, ref int buffObbs, ref int sBuffObbs)
        {
            if (!Main.enabled) return true;//mod未开启
            var presetItem = __instance.presetitemDate[presetItemId];
            int itemTyp = presetItem[5].ParseInt();
            int equipTyp = presetItem[1].ParseInt();//1武器，2防具，3饰品
            bool isFood = itemTyp == 34 || itemTyp == 35;
            if (isFood)//食物
            {
                if (presetItem[6].ParseInt() == 1) return true;//可堆叠，是野果
                if (!Main.settings.SetFood) return true;
            }
            else
            {
                if (presetItem[4] != "4") return true;//排除非装备
                if (equipTyp <= 0 || equipTyp >= 4) return true;//装备类型不符
                if ((equipTyp == 1 && !Main.settings.SetWeapon) || (equipTyp == 2 && !Main.settings.SetArmor) ||
                    (equipTyp == 3 && !Main.settings.SetJewelry)) return true;//装备类型对应选项未开启
                if (itemTyp == 42) return true;//排除奇书宝典
                if (itemTyp == 36) return true;//排除神兵
                if (itemTyp == 21 || presetItem[31].ParseInt() > 0) return true;//排除书籍
            }
            if (!Main.settings.IMBA || !Main.settings.global)//仅对玩家制造的物品生效
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(false);
                if (!st.GetFrames().Select(f => f.GetMethod().Name).Any(n => n.Contains("SetMakeItem")))
                    return true;
            }
            Dictionary<int, string> newItem = new Dictionary<int, string>();
            int newItemId;
            if (temporaryId < 0)
            {
                newItemId = temporaryId;
                GameData.Items.RemoveItem(temporaryId);
                __instance.itemsChangeDate.Remove(temporaryId);
            }
            else
            {
                __instance.newItemId++;
                newItemId = __instance.newItemId;
            }
            newItem[999] = presetItemId.ToString();//物品类型id
            int duration = presetItem[902].ParseInt();
            bool isFixed = duration <= 0;
            duration = duration.Abs();
            var seed = DateFile.instance.randSeed;
            //毒素
            if (Main.settings.IMBA && Main.settings.changePoison)
            {
                for (int i = 0; i < 6; i++)
                {
                    newItem[i + 71] = Main.settings.poison[i].ToString();
                }
            }
            //食物
            if (isFood)
            {
                //耐久
                int minimum = 50;
                duration = isFixed ? duration : 1 + duration * seed.Next(minimum, 101) / 100;
                newItem[902] = duration.ToString();//最大耐久
                newItem[901] = duration.ToString();//当前耐久
                if (Main.settings.FoodAttr[0] != 0 || seed.Next(0, 100) < buffObbs)
                {
                    int attrTyp = Main.settings.FoodAttr[0] != 0
                        ? Main.settings.FoodAttr[0] == 1
                            ? Main.settings.FoodAttr[1] + 50501
                            : Main.settings.FoodAttr[2] + 50601
                        : (seed.Next(0, 100) < 50)
                            ? seed.Next(0, 16) + 50501
                            : seed.Next(0, 14) + 50601;
                    newItem[attrTyp] = (Mathf.Max(presetItem[8].ParseInt() / 2, 1) * 20).ToString();
                }
            }
            //装备
            else
            {
                if (Main.settings.IMBA && Main.settings.传家宝)
                {
                    DateFile.instance.ChangItemDate(newItemId, 92, 900);
                    DateFile.instance.ChangItemDate(newItemId, 93, 1800);
                }
                if (Main.settings.IMBA && Main.settings.神兵)
                {
                    isFixed = true;
                    duration = 9;
                    newItem[5] = "36";
                }
                //武器
                if (equipTyp == 1)
                {
                    //耐久
                    if (!isFixed)
                    {
                        int minimum = Main.settings.WeaponMoreDuration ? 80 : 50;
                        duration = 1 + duration * seed.Next(minimum, 101) / 100;
                    }
                    newItem[902] = duration.ToString();//最大耐久
                    newItem[901] = duration.ToString();//当前耐久
                    //式
                    bool freeTyp = Main.settings.IMBA && Main.settings.freeTyp;
                    if (freeTyp || presetItem[606].ParseInt() > 0)
                    {
                        string attackTypText = "";
                        var newTyps = new string[6];
                        for (int i = 0; i < 6; i++)
                        {
                            newTyps[i] = Main.AttackTypId[Main.settings.AttackTyp[i]];
                        }
                        List<string> presetTyps = new List<string>(presetItem[7].Split('|'));
                        if (!freeTyp)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (presetTyps.Contains(newTyps[i])) presetTyps.Remove(newTyps[i]);
                                else newTyps[i] = "";
                            }
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            if (newTyps[i] == "")
                            {
                                string typ = presetTyps.Random();
                                newTyps[i] = typ;
                                presetTyps.Remove(typ);
                            }
                        }
                        attackTypText = string.Join("|", newTyps);
                        newItem[7] = attackTypText;
                    }
                    //内伤比例
                    if (Main.settings.IMBA && Main.settings.changeRate)
                    {
                        newItem[10] = Main.settings.rate.ToString();
                    }
                    //功法发挥
                    if (Main.settings.WeaponEngraving != 0 || seed.Next(0, 100) < buffObbs)
                    {
                        int carving = Main.settings.WeaponEngraving;
                        if (carving == 0)
                        {
                            carving = seed.Next(1, 4);
                        }
                        if (buffObbs == 999)
                        {
                            carving *= 100;
                        }
                        newItem[505] = carving.ToString();
                    }
                    //词条
                    int power = presetItem[504].ParseInt();
                    if ((Main.settings.WeaponPower[0] != 3 || Main.settings.WeaponPower[3] != 5) || (power != 0 && seed.Next(0, 100) < sBuffObbs))
                    {
                        if (Main.settings.WeaponPower[0] == 3)//其他
                        {
                            if (Main.settings.WeaponPower[3] == 5)//随机
                            {
                                List<int> powerList = new List<int>
                                {
                                    2,
                                    3,
                                    4,
                                    5,
                                    6,
                                    7,
                                    8,
                                    9,
                                    10,
                                    11,
                                    12,
                                    13,
                                    101,
                                    102,
                                    103,
                                    104,
                                    105,
                                    106,
                                    107,
                                    108,
                                    109,
                                    110,
                                    111,
                                    112,
                                    401,
                                    402,
                                    403,
                                    404,
                                    407
                                };
                                if (power < 0)
                                {
                                    powerList.Add(power.Abs());//武器杀
                                }
                                int material = presetItem[506].ParseInt();
                                if (material != 0) // 没有布杀
                                {
                                    powerList.Add(209 + material);//材质杀
                                }
                                power = powerList.Random();
                            }
                            else power = Main.settings.WeaponPower[3] == 4 ? 407 : 401 + Main.settings.WeaponPower[3];
                        }
                        //杀
                        else if (Main.settings.WeaponPower[0] == 2)
                        {
                            if (Main.settings.WeaponPower[2] == 1)//材质杀
                            {
                                int material = presetItem[506].ParseInt();
                                power = material == 0 ? 0 :209 + material; //没有布杀
                            }
                            else power = power.Abs();//武器杀
                        }
                        // 破 辟
                        else
                        {
                            power = (Main.settings.WeaponPower[0] == 0 ? 2 : 101) + Main.settings.WeaponPower[1];
                        }
                        if (power > 0) ChangeNewItemSPower(newItem, presetItemId, power);
                    }
                }
                //防具
                else if (equipTyp == 2)
                {
                    //耐久
                    if (!isFixed)
                    {
                        int minimum = 50;
                        duration = 1 + duration * seed.Next(minimum, 101) / 100;
                    }
                    newItem[902] = duration.ToString();//最大耐久
                    newItem[901] = duration.ToString();//当前耐久
                    if (Main.settings.ArmorAttr != 0 || seed.Next(0, 100) < buffObbs)
                    {
                        int attrTyp = (Main.settings.ArmorAttr == 0) ? seed.Next(0, 6) : (Main.settings.ArmorAttr - 1);
                        newItem[51361 + attrTyp] = (Mathf.Max(int.Parse(presetItem[8]) / 2, 1) * ((buffObbs != 999) ? 5 : 10)).ToString();
                    }
                    if (Main.settings.ArmorPower != 1 && (Main.settings.ArmorPower != 0 || seed.Next(0, 100) < sBuffObbs))
                    {
                        int power;
                        if (Main.settings.ArmorPower == 0)
                        {
                            List<int> list2 = new List<int>
                            {
                                401,
                                402,
                                405,
                                406,
                                407
                            };
                            power = list2.Random();
                        }
                        else
                        {
                            power = Main.settings.ArmorPower <= 3 ? Main.settings.ArmorPower + 399 : Main.settings.ArmorPower + 401;
                        }
                        ChangeNewItemSPower(newItem, presetItemId, power);
                    }
                }
                //饰品
                else
                {
                    //耐久
                    if (!isFixed)
                    {
                        int minimum = 50;
                        duration = 1 + duration * seed.Next(minimum, 101) / 100;
                    }
                    newItem[902] = duration.ToString();//最大耐久
                    newItem[901] = duration.ToString();//当前耐久
                    if (Main.settings.JewelryAttr[0] != 0 || seed.Next(0, 100) < buffObbs)
                    {
                        int attrTyp = Main.settings.JewelryAttr[0] == 0
                            ? seed.Next(0, 100) < 50
                                ? seed.Next(0, 16) + 50501
                                : seed.Next(0, 14) + 50601
                            : Main.settings.JewelryAttr[0] == 1
                                ? Main.settings.JewelryAttr[1] + 50501
                                : Main.settings.JewelryAttr[2] + 50601;
                        newItem[attrTyp] = (Mathf.Max(presetItem[8].ParseInt() / 2, 1) * ((buffObbs != 999) ? 5 : 10)).ToString();
                    }
                    if (Main.settings.JewelryPowerMustBe || seed.Next(0, 100) < sBuffObbs)
                    {
                        ChangeNewItemSPower(newItem, presetItemId, 407);
                    }
                }
            }
            GameData.Items.AddItem(newItemId, newItem);
            __result = newItemId;
            return false;
        }
    }
}