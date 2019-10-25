using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityModManagerNet;

namespace EasyRefine
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int breakGrade = 1;
        public KeyCode hotKey = KeyCode.C;

        public int equipGrade = 9;
        public int materialGrade = 1;
        public bool advancedBreak = true;
        public bool useWarehouse = true;
        public bool clickLock = true;
        public bool showProduct = true;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string labelText = "";
        public static string[] gradeName = new string[9]
        {
            "<color=#8E8E8EFF>下·九品</color>",
            "<color=#FBFBFBFF>中·八品</color>",
            "<color=#6DB75FFF>上·七品</color>",
            "<color=#8FBAE7FF>奇·六品</color>",
            "<color=#63CED0FF>秘·五品</color>",
            "<color=#AE5AC8FF>极·四品</color>",
            "<color=#E3C66DFF>超·三品</color>",
            "<color=#F28234FF>绝·二品</color>",
            "<color=#E4504DFF>神·一品</color>"
        };
        static Looper looper = null;
        public static int WatchingActor = -1;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            if (looper == null)
            {
                looper = (new GameObject()).AddComponent<Looper>();
                UnityEngine.Object.DontDestroyOnLoad(looper);
            }

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
            bool flag = GUILayout.Button("一键拆解低级装备[快捷键Ctrl+C]");
            if (flag) labelText = EasyRefine_BatchBreak.DoBreak();
            GUILayout.Label(labelText);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("不高于", GUILayout.Width(50));
            settings.breakGrade = GUILayout.SelectionGrid(settings.breakGrade - 1, gradeName, 9) + 1;
            GUILayout.Label("的装备", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.advancedBreak = GUILayout.Toggle(settings.advancedBreak, "拆解高级装备时，消耗低级精制材料，提升拆解产物的品级（对一键拆解无效）", new GUILayoutOption[0]);
            settings.useWarehouse = GUILayout.Toggle(settings.useWarehouse, "使用仓库材料", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("不高于", GUILayout.Width(50));
            settings.materialGrade = GUILayout.SelectionGrid(settings.materialGrade - 1, gradeName, 9) + 1;
            GUILayout.Label("的材料", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("不低于", GUILayout.Width(50));
            settings.equipGrade = GUILayout.SelectionGrid(settings.equipGrade - 1, gradeName, 9) + 1;
            GUILayout.Label("的装备", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.clickLock = GUILayout.Toggle(settings.clickLock, "左键单击锁定装备/材料（如果无效就试试ALT+单击）", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showProduct = GUILayout.Toggle(settings.showProduct, "物品信息显示拆解产物及其精制效果", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    public static class Func
    {
        public static string GetItemName(int itemId)
        {
            int grade = int.Parse(DateFile.instance.GetItemDate(itemId, 8, false));
            string name = DateFile.instance.GetItemDate(itemId, 0, false);
            name = DateFile.instance.SetColoer(20001 + grade, name);
            return name;
        }
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
    }

    public static class MyModDate
    {
        static readonly string Name = "EasyRefine";
        static bool GetDict(out Dictionary<string,string> dict)
        {
            var modDate = DateFile.instance.modDate;
            if (modDate.ContainsKey(Name))
            {
                dict = modDate[Name];
                return true;
            }
            else
            {
                modDate.Add(Name, new Dictionary<string, string> { });
                dict = modDate[Name];
                return false;
            }
        }
        public static bool Switch(int id)
        {
            bool haveDict = GetDict(out var dict);
            string key = id.ToString();
            bool haveKey = dict.ContainsKey(key);
            if (haveKey)
            {
                dict.Remove(key);
                return true;
            }
            else
            {
                dict.Add(key, "1");
                return false;
            }
        }
        public static bool Locked(int id)
        {
            if(!GetDict(out var dict))
            {
                return false;
            }
            else
            {
                return (dict.ContainsKey(id.ToString()));
            }
        }
    }

    public class Looper : MonoBehaviour
    {
        void LateUpdate()
        {
            CheckKey();
        }
        static void CheckKey()
        {
            if (!Main.enabled) return;
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (ctrl && Input.GetKeyDown(Main.settings.hotKey)) Func.ShowTips(EasyRefine_BatchBreak.DoBreak());
        }
    }

    //一键拆分低级装备
    public static class EasyRefine_BatchBreak
    {
        public static string DoBreak()
        {
            int mainActorId = DateFile.instance.mianActorId;
            if (DateFile.instance == null || mainActorId <= 0) return "存档未载入";
            if (DateFile.instance.showTrunChange) return "换季中";
            int sum = 0;
            Dictionary<int, int> newItemCnt = new Dictionary<int, int> { };
            foreach (int[] itemId in DateFile.instance.actorItemsDate[mainActorId].Keys
                .Select(id => new int[2] { id, GetProduct(id)}) //[原物品，拆解产物]
                .Where(pair => pair[1]>0) //有拆解产物
                .ToList()) //将动态序列变成静态序列
            {
                int itemNumber = 1; //装备不可叠加，数量必为1
                sum += itemNumber;
                Main.Logger.Log($"{Func.GetItemName(itemId[0])}>>>{Func.GetItemName(itemId[1])}");
                DateFile.instance.LoseItem(mainActorId, itemId[0], itemNumber, true);
                if (newItemCnt.ContainsKey(itemId[1])) newItemCnt[itemId[1]] += itemNumber;
                else newItemCnt.Add(itemId[1], itemNumber);
            }
            if (sum == 0) return "没有符合条件的装备";

            DateFile.instance.GetItem(mainActorId, 
                newItemCnt.Select(kvp => new int[] { kvp.Key, kvp.Value }).ToList(), //物品id及数量
                true, //创建新物品
                0); //显示获得物品
            if (Main.WatchingActor == mainActorId) //正打开着背包
                ActorMenu.instance.UpdateItems(mainActorId, ActorMenu.instance.itemTyp); //刷新背包物品
            AudioManager.instance.PlaySE("SE_8"); //播放拆解音效
            return $"共拆解了{sum}件装备";
        }
        static int GetProduct(int itemId) //拆解产物
        {
            if (MyModDate.Locked(itemId)) return -99;
            if (!IsBreakableEquip(itemId)) return -2;
            int level = int.Parse(DateFile.instance.GetItemDate(itemId, 8));
            int require = Main.settings.breakGrade;
            if (level > require) return -1;
            int newItemId = int.Parse(DateFile.instance.GetItemDate(itemId, 52));
            return newItemId;
        }
        public static bool IsBreakableEquip(int itemId)
        {
            int typeInBag = int.Parse(DateFile.instance.GetItemDate(itemId, 4));//是装备
            bool breakable = DateFile.instance.GetItemDate(itemId, 50) == "1";//能通过拆解获得物品
            return typeInBag == 4 && breakable;
        }
    }

    //拆解升级
    public static class EasyRefine_AdvancedBreak
    {
        const int YorN_ID = -78435;
        const int WarehouseId = -999;
        
        static List<ItemInfo> BagUsage = null;
        static List<ItemInfo> WarehouseUsage = null;
        static int OldGetId = 0;
        static int NewGetId = 0;
        static int EquipId = 0;
        static int RequireNumber = 0;
        static int HaveNumber = 0;
        static int NeedResourceTyp = 0;
        static int NeedResourceSize = 0;
        static int NeedMoney = 0;
        static int HaveResourceSize = 0;
        static int HaveMoney = 0;

        public class ItemInfo
        {
            public readonly int itemId;
            public int cnt;
            public readonly int actorId;
            public ItemInfo(int itemId, int cnt, int actorId)
            {
                this.itemId = itemId;
                this.cnt = cnt;
                this.actorId = actorId;
            }
        }

        [HarmonyPatch(typeof(DropObject), "OnDrop")]
        public static class EasyRefine_OnDrop_Patch
        {
            static bool Prefix(DropObject __instance, ref UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (!Main.enabled) return true;
                if (!Main.settings.advancedBreak) return true;
                if (__instance.containerImage == null || BattleSystem.instance.battleWindow.activeSelf) return true;
                var method = typeof(DropObject).GetMethod("GetDropDes", BindingFlags.NonPublic | BindingFlags.Instance);
                List<Image> dropDes = (List<Image>)method.Invoke(__instance, new object[] { eventData });
                //List<Image> dropDes = __instance.GetDropDes(eventData);
                if (dropDes == null || !dropDes.Contains(__instance.dropDesImage)) return true;
                int actorId = DateFile.instance.MianActorID();
                //__instance.containerImage.color = __instance.normalColor;
                int dropTyp = __instance.dropObjectTyp;
                if (dropTyp != 10) return true;
                if (ActorMenu.instance.isEnemy) return true;
                int itemId = int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[1]);
                int matchResult = MatchEquip(itemId);
                if(matchResult == -101)
                {
                    __instance.OnPointerExit(eventData);
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "拆解升级", $"精制材料不足({HaveNumber}/{RequireNumber})，无法提升产物品级", false, false);
                    return false;
                }
                if (matchResult == -102)
                {
                    string lackText = "";
                    if (HaveResourceSize < NeedResourceSize)
                    {
                        lackText += DateFile.instance.massageDate[7018][0].Split('|')[NeedResourceTyp] +
                            $"{HaveResourceSize}/{NeedResourceSize}" +
                            DateFile.instance.massageDate[7018][0].Split('|')[7];
                    }
                    if (HaveMoney < NeedMoney)
                    {
                        if (lackText != "") lackText += "、";
                        lackText += DateFile.instance.massageDate[7018][0].Split('|')[5] +//C_20008银钱
                            $"{HaveMoney}/{NeedMoney}" +
                            DateFile.instance.massageDate[7018][0].Split('|')[7];//C_D
                    }
                    __instance.OnPointerExit(eventData);
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "拆解升级", $"资源不足({lackText})，无法提升产物品级", false, false);
                    return false;
                }
                if (matchResult <= 0)
                {
                    Main.Logger.Log($"matchResult={matchResult}");
                    return true;
                }
                EquipId = itemId;
                string text = "";
                if (BagUsage.Count > 0) text += $"消耗背包物品：{ItemsText(BagUsage)}\n";
                if (Main.settings.useWarehouse && WarehouseUsage.Count > 0) text += $"消耗仓库物品：{ItemsText(WarehouseUsage)}\n";
                text += "消耗资源：" +
                    $"{DateFile.instance.massageDate[7018][0].Split('|')[NeedResourceTyp]}{NeedResourceSize}{DateFile.instance.massageDate[7018][0].Split('|')[7]}"
                    + "、" 
                    + $"{DateFile.instance.massageDate[7018][0].Split('|')[5]}{NeedMoney}{DateFile.instance.massageDate[7018][0].Split('|')[7]}"
                    + "\n";
                text += $"将产物从{Func.GetItemName(OldGetId)}提升为{Func.GetItemName(NewGetId)}\n";
                text += $"<color=#4B4B4BFF>可用精制材料数量：{HaveNumber}</color>";
                WindowManage.instance.WindowSwitch(false, null);
                DropUpdate.instance.updateId = __instance.dropObjectTyp;
                __instance.OnPointerExit(eventData);
                YesOrNoWindow.instance.SetYesOrNoWindow(YorN_ID, "拆解升级", text, false, true);
                return false;
            }
            static int MatchEquip(int itemId)
            {
                int typInBag = int.Parse(DateFile.instance.GetItemDate(itemId, 4));
                if (typInBag != 4) return -1;//不是装备
                if (int.Parse(DateFile.instance.GetItemDate(itemId, 50)) == 0) return -2;//无法拆解得到物品
                if (int.Parse(DateFile.instance.GetItemDate(itemId, 53)) == 0) return -3;//不能精制
                int require = 10 - int.Parse(DateFile.instance.GetItemDate(itemId, 996));
                if (require <= 0) return -4;//满精制
                int grade = int.Parse(DateFile.instance.GetItemDate(itemId, 8));
                if (grade < Main.settings.equipGrade) return -5;//品阶不达标
                int enough = Enough(itemId, require);
                if (enough <= 0) return enough;
                OldGetId = int.Parse(DateFile.instance.GetItemDate(itemId, 52));
                if (OldGetId > 0)
                {
                    var presetId = int.Parse(GameData.Items.GetItemProperty(itemId, 999));
                    NewGetId = int.Parse(DateFile.instance.presetitemDate[presetId][52]) + 2;
                }
                return OldGetId;
            }
            static int Enough(int itemId, int require = 10)
            {
                int actorId = DateFile.instance.mianActorId;

                var available = AvailableItem(actorId);
                if (Main.settings.useWarehouse)
                {
                    available = available.Concat(AvailableItem(WarehouseId));
                }
                HaveNumber = available.Sum(vt => vt.cnt);
                RequireNumber = require;
                if (HaveNumber < RequireNumber)
                {
                    return -101;
                }

                var usage = TakeByOrder(available, require);
                NeedResourceSize = usage.Sum(info => int.Parse(DateFile.instance.GetItemDate(info.itemId, 45)) * info.cnt);
                NeedMoney = usage.Sum(info => int.Parse(DateFile.instance.GetItemDate(info.itemId, 46)) * info.cnt);
                BagUsage = usage.Where(info => info.actorId == actorId).ToList();
                WarehouseUsage = usage.Where(info => info.actorId == WarehouseId).ToList();

                NeedResourceTyp = int.Parse(DateFile.instance.GetItemDate(itemId, 44, true)) - 1;
                HaveResourceSize = int.Parse(DateFile.instance.GetActorDate(actorId, 401 + NeedResourceTyp));
                HaveMoney = int.Parse(DateFile.instance.GetActorDate(actorId, 406));
                return (HaveResourceSize >= NeedResourceSize && HaveMoney >= NeedMoney) ? 1 : -102;
            }
            static IEnumerable<ItemInfo> AvailableItem(int actorId)
            {
                return DateFile.instance.actorItemsDate[actorId]
                    .Where(kvp => !MyModDate.Locked(kvp.Key) //未上锁
                        && IsMaterial(kvp.Key) //是精制材料
                        && int.Parse(DateFile.instance.GetItemDate(kvp.Key, 8, false)) <= Main.settings.materialGrade) //品级符合要求
                        .AsEnumerable()
                        .Select(kvp => new ItemInfo(kvp.Key, kvp.Value, actorId ));
            }
            public static bool IsMaterial(int itemId)
            {
                int reTyp = int.Parse(DateFile.instance.GetItemDate(itemId, 51, false));//再加工材料类型
                return (reTyp >= 101 && reTyp <= 108);
            }
            static List<ItemInfo> TakeByOrder(IEnumerable<ItemInfo> all, int require)
            {
                var take = new List<ItemInfo>();
                foreach (var tuple in all.OrderBy(tuple => GetOrder(tuple.itemId)))//按品级、重量排序
                {
                    int itemNumber = tuple.cnt;
                    if (require > itemNumber)
                    {
                        require -= itemNumber;
                        take.Add(tuple);
                    }
                    else
                    {
                        tuple.cnt = require;
                        take.Add(tuple);
                        break;
                    }
                }
                return take;
            }
            static Dictionary<int, int> sortCache = new Dictionary<int, int>();
            static int GetOrder(int itemId)
            {
                if (!sortCache.ContainsKey(itemId))
                {
                    var level = int.Parse(DateFile.instance.presetitemDate[itemId][8]);
                    var weight = int.Parse(DateFile.instance.presetitemDate[itemId][501]);
                    var value = level * 10000 - weight;
                    sortCache.Add(itemId, value);
                    return value;
                }
                return sortCache[itemId];
            }

            static string ItemsText(List<ItemInfo> items) => items.Select(info => $"{Func.GetItemName(info.itemId)}×{info.cnt}").Join(null, "，");
        }

        [HarmonyPatch(typeof(OnClick), "Index")]
        public static class EasyRefine_Index_Patch
        {
            static bool Prefix(OnClick __instance)
            {
                if (Main.enabled && __instance.ID == YorN_ID)
                {
                    int actorId = DateFile.instance.mianActorId;
                    DateFile.instance.LoseItem(actorId, EquipId, 1, true, false);
                    foreach(var info in BagUsage)
                    {
                        DateFile.instance.LoseItem(actorId, info.itemId, info.cnt, false, false);
                    }
                    foreach (var info in WarehouseUsage)
                    {
                        DateFile.instance.LoseItem(WarehouseId, info.itemId, info.cnt, false, false);
                    }
                    UIDate.instance.ChangeResource(actorId, NeedResourceTyp, -NeedResourceSize, false);
                    UIDate.instance.ChangeResource(actorId, 5, -NeedMoney, false);
                    DateFile.instance.GetItem(actorId, NewGetId, 1, false, 0, 0);
                    ActorMenu.instance.UpdateItems(ActorMenu.instance.actorId, ActorMenu.instance.itemTyp);
                    ActorMenu.instance.UpdateActorResource(ActorMenu.instance.actorId);
                    AudioManager.instance.PlaySE("SE_8");
                    return true;
                }
                return true;
            }
        }

    }

    //切换锁定
    public class EasyRefine_Lock
    {
        //点击事件
        class ClickAction : MonoBehaviour, IPointerClickHandler
        {
            int _itemid;
            public void OnPointerClick(PointerEventData eventData)
            {
                bool locked = MyModDate.Switch(_itemid);
                string itemName = DateFile.instance.GetItemDate(_itemid, 0, false);
                Main.Logger.Log((locked ? "解锁" : "锁定") + $" {itemName}");
                //刷新物品说明
                var methodInfo = typeof(WindowManage).GetMethod("ShowItemMassage", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo.Invoke(WindowManage.instance, new object[] { _itemid, 0, true, -1, 0 });
            }
            public void SetParam(int itemid)
            {
                _itemid = itemid;
            }
        }
        //预先设置点击事件
        [HarmonyPatch(typeof(SetItem), "SetActorMenuItemIcon")]
        public static class EasyRefine_SetActorMenuItemIcon_Patch
        {
            static void Postfix(SetItem __instance, int itemId)
            {
                if (!Main.enabled) return;
                AddClickAction(__instance.gameObject, itemId);
            }
        }
        [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
        public static class EasyRefine_SetActorEquipIcon_Patch
        {
            static void Postfix(SetItem __instance, int itemId)
            {
                if (!Main.enabled) return;
                AddClickAction(__instance.gameObject, itemId);
            }
        }
        static void AddClickAction(GameObject icon, int itemId)
        {
            if (!Main.settings.clickLock) return;
            if (!Match(itemId)) return;
            //添加相应处理Component,注入参数
            var clickActions = icon.GetComponents<ClickAction>();
            if (clickActions.Length >= 1)//避免重复添加
            {
                clickActions[0].SetParam(itemId);
                /*foreach(var clickAction in clickActions)
                {
                    UnityEngine.Object.Destroy(clickAction);
                }*/
            }
            else
            {
                var actionstub = icon.AddComponent<ClickAction>();
                actionstub.SetParam(itemId);
            }
        }
        static bool Match(int itemId)
        {
            return EasyRefine_AdvancedBreak.EasyRefine_OnDrop_Patch.IsMaterial(itemId)
                || EasyRefine_BatchBreak.IsBreakableEquip(itemId);
        }
    }
    
    //修改显示信息
    [HarmonyPatch(typeof(WindowManage), "ShowItemMassage")]
    public static class EasyRefine_ShowItemMassage_Patch
    {
        static void Postfix(WindowManage __instance, int itemId, bool setName)
        {
            if (!Main.enabled) return;
            if (setName && Main.settings.clickLock && MyModDate.Locked(itemId))
            {
                __instance.informationName.text = DateFile.instance.SetColoer(10004, "已锁定[简化精制]") + "\n" 
                    + __instance.informationName.text;
            }
            if(Main.settings.showProduct)
            {
                RefineMaterial rm;
                string newText;
                bool isRM = RefineMaterial.TryGet(itemId, out rm);
                if (isRM)
                {
                    string title = DateFile.instance.SetColoer(10002, "【精制效果】");
                    newText = $"{title}\n";
                    newText += $"·{rm.onWeapon} || {rm.onArmor} || {rm.onJewelry}\n";
                }
                else
                {
                    int productId = int.Parse(DateFile.instance.GetItemDate(itemId, 52, false));
                    bool haveProduct = productId > 0 && DateFile.instance.GetItemDate(itemId, 50, false) == "1";
                    if (!haveProduct) return;

                    productId += 2;//满精制
                    string title = DateFile.instance.SetColoer(10002, "【满精制拆解产物】");
                    string name = Func.GetItemName(productId);
                    newText = $"{title}\n·{name}\n";
                    bool hasEffect = RefineMaterial.TryGet(productId, out rm);
                    if (hasEffect)
                    {
                        newText += $"·{rm.onWeapon} || {rm.onArmor} || {rm.onJewelry}\n";
                    }
                }
                __instance.informationMassage.text += newText + "\n";
                typeof(WindowManage).GetField("baseWeaponMassage", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(__instance, __instance.informationMassage.text);
            }
        }
    }

    struct RefineMaterial
    {
        public static bool TryGet(int id, out RefineMaterial refineMaterial)
        {
            int typ = int.Parse(DateFile.instance.GetItemDate(id, 51, false));
            if (typ >= 101 && typ <= 108)
            {
                int grade = int.Parse(DateFile.instance.GetItemDate(id, 8, false));
                refineMaterial = new RefineMaterial(typ, grade);
                return true;
            }
            refineMaterial = new RefineMaterial();
            return false;
        }
        public string onWeapon;
        public string onArmor;
        public string onJewelry;
        RefineMaterial(int typ, int grade = 2)
        {
            onWeapon = GetEffect(typ, grade, 0);
            onArmor = GetEffect(typ, grade, 1);
            onJewelry = GetEffect(typ, grade, 2);
        }
        static string GetEffect(int refineTyp, int grade, int equipTyp)
        {
            int id = refineTyp + equipTyp * 100;
            var changeEquipDate = DateFile.instance.changeEquipDate[id];
            string effectName = changeEquipDate[1];
            float power = int.Parse(changeEquipDate[3]);
            power = Mathf.Abs(power);
            power *= grade;
            power /= int.Parse(changeEquipDate[5]);
            return effectName + power.ToString();
        }
    }

    [HarmonyPatch(typeof(GEvent), "OnEvent")]//记录游戏状态
    public static class WindTest_OnEvent_Patch
    {
        static void Postfix(Enum _em)
        {
            switch (_em)
            {
                case eEvents.ActorMenuClosed:
                    Main.WatchingActor = -1;
                    break;
                case eEvents.ActorMenuOpened:
                    Main.WatchingActor = ActorMenu.instance.actorId;
                    break;
            }
        }
    }
}