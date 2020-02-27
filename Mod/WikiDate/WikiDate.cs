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

namespace WikiDate
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            if (go == null)
            {
                go = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(go);
                ip = Printer.Regist<ItemPrinter>(go);
                bp = Printer.Regist<BossPrinter>(go);
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
            bool flag;
            int tmp;
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID(2001-2009)");
            flag = int.TryParse(GUILayout.TextField(PresetActorId.ToString()), out tmp);
            if (flag) PresetActorId = tmp;
            GUILayout.Label("姓名");
            GUILayout.TextField(ActorName);
            GUILayout.Label("难度(0-3)");
            flag = int.TryParse(GUILayout.TextField(Difficulty.ToString()), out tmp);
            if (flag) Difficulty = tmp;
            GUILayout.Label("次序(0-6)");
            flag = int.TryParse(GUILayout.TextField(XXLevel.ToString()), out tmp);
            if (flag) XXLevel = tmp;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            flag = GUILayout.Button("属性");
            if (flag)
            {
                var boss = Boss.Get();
                Print(boss.name);
                Print(boss.allData.ToString());
            }

            flag = GUILayout.Button("开战");
            if (flag) CallBattle();

            flag = GUILayout.Button("找错人了，告辞");
            if (flag) EndBattle();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            flag = GUILayout.Button("剑冢数据");
            if (flag)
            {
                bp.SaveAll();
            }

            flag = GUILayout.Button("物品数据");
            if (flag)
            {
                ip.SaveAll();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            flag = GUILayout.Button("某物");
            if (flag)
            {
                Show(ip.One(PresetActorId)[1]);
            }
            flag = GUILayout.Button("建筑");
            if (flag)
            {

                var instance = SingletonObject.getInstance<DynamicSetSprite>();
                var gsInfoAsset = (GetSpritesInfoAsset)typeof(DynamicSetSprite).GetField("gsInfoAsset", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
                foreach (var kvp in DateFile.instance.basehomePlaceDate)
                {
                    int icon = int.Parse(kvp.Value[98]);
                    Show(icon + "\t" + gsInfoAsset.GetSpriteName("buildingSprites", icon), limit:false);
                }
            }
            flag = GUILayout.Button("图片名");
            if (flag)
            {
                var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
                var gsInfoAsset = (GetSpritesInfoAsset)typeof(DynamicSetSprite)
                    .GetField("gsInfoAsset", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(dynamicSetSprite);
                var commonNameGroup = (Dictionary<string, string[]>)typeof(GetSpritesInfoAsset)
                    .GetField("commonNameGroup", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(gsInfoAsset);
                foreach (var typ in commonNameGroup)
                {
                    Main.Logger.Log($"{typ.Key}:");
                    for (int i = 0; i < typ.Value.Length; i++)
                    {
                        Main.Logger.Log($"\t[{i}] = \"{typ.Value[i]}\"");
                    }
                    Main.Logger.Log("");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.TextArea(showText);
            GUILayout.EndVertical();
        }
        static MethodInfo setItemTypText = typeof(WindowManage).GetMethod("SetItemTypText", BindingFlags.NonPublic | BindingFlags.Instance);
        static string SetItemTypText(int equipId, int actorId = -1)
        {
            return (string)setItemTypText.Invoke(WindowManage.instance, new object[] { equipId, actorId });
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static int XXLevel = 6;
        public static int PresetActorId = 2009;
        public static int Difficulty = 1;
        public static bool Outhome = false;
        public static string ActorName = "";

        public static bool ReadingItem = false;

        public static void Print(string text)
        {
            Main.Logger.Log(text);
        }
        static string showText = "";
        static Queue<string> textQueue = new Queue<string>();
        public static void Show<T>(T obj, bool add = true, bool limit = true)
        {
            string text = obj.ToString();
            if (add && showText != "")
            {
                textQueue.Enqueue(text);
                if (limit && textQueue.Count > 10)
                {
                    textQueue.Dequeue();
                    showText = string.Join("\n", textQueue.ToArray());
                }
                else
                {
                    showText += "\n" + text;
                }
            }
            else
            {
                showText = text;
            }
        }
        public static void Log<T>(T obj)
        {
            Logger.Log(obj.ToString());
        }

        public static void CallBattle()
        {
            DateFile.instance.enemyBorn = Difficulty;
            StartBattle.instance.ShowStartBattleWindow(9001 + XXLevel, 0, 18, new List<int> { PresetActorId });//typ死斗，mapTyp背景
            ActorName = Boss.Get().name;
        }
        public static void EndBattle()
        {
            DateFile.instance.battleStart = false;
            StartBattle.instance.CloseStartBattleWindow();
            StartBattle.instance.startBattleWindow.SetActive(false);
            BattleSystem.instance.battleWindow.SetActive(false);
            //UIMove.instance.ShowGUI();
        }
        //Invoke("StartBattleWindowOpend", 0.25f);

        static GameObject go;
        public static ItemPrinter ip;
        public static BossPrinter bp;
    }

    public static class Util
    {
        public static string Dit() => WindowManage.instance.Dit();
        public static string Cut(int color = 20002) => WindowManage.instance.Cut(color);
        public static string Color(int color, string s) => DateFile.instance.SetColoer(color, s);
        public static string Color(int color, object s) => DateFile.instance.SetColoer(color, s.ToString());
        public static string ColorName(string name) => Color(10002, name);
        public static int ToInt(this string s) => Convert.ToInt32(s);
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

    public class BossPrinter: Printer
    {
        string[] enemyBornText = { "简单", "困难", "极难", "必死" };
        public void SaveOne(int id)
        {
            StartCoroutine(DoSaveOne(id));
        }
        public void SaveAll()
        {
            StartCoroutine(DoSaveAll());
        }
        IEnumerator DoSaveAll()
        {
            gongfas = new List<string[]>();
            yield return null;
            for (int i = 0; i < 9; i++)
            {
                waitFlag = true;
                StartCoroutine(DoSaveOne(2001 + i));
                while (waitFlag) yield return new WaitForSeconds(5);
            }
        }
        IEnumerator DoSaveOne(int presetActorId)
        {
            Main.PresetActorId = presetActorId;
            yield return null;
            for (int i = 0; i < 7; i++)
            {
                Main.XXLevel = i;
                for (int j = 0; j < 4; j++)
                {
                    DateFile.instance.enemyBorn = j;
                    Main.Difficulty = j;
                    Main.CallBattle();
                    var boss = Boss.Get();
                    yield return waitTime;
                    SaveToFile($"相枢化身_1_{boss.name}_1_剑{i + 1}_1_难度{j}.json", boss.allData.ToString(), "相枢化身");
                    yield return waitTime;
                }
            }
            Main.EndBattle();
            waitFlag = false;
        }

        List<string[]> gongfas;
        List<string> AllBossGongFa()
        {
            var result = new List<string>();
            result.Add("<includeonly>{{#switch:{{{1}}}\n");
            foreach (var pair in gongfas)
            {
                string name = pair[0];
                string gongfaTab = pair[1];
                result.Add($"|{name} = {gongfaTab}\n");
            }
            result.Add("|无此相枢化身\n}}</includeonly><noinclude>{{相枢化身功法|以向}}[[分类:Data相枢化身]]</noinclude>");
            return result;
        }
    }

    public class ItemPrinter: Printer
    {
        public void SaveAll()
        {
            StartCoroutine(DoSaveAll());
        }
        IEnumerator DoSaveAll()
        {
            var date = DateFile.instance.presetitemDate;
            int sum = date.Count;
            int cnt = 0;
            foreach (var kvp in date)
            {
                var skip = false;// cnt <= 2640;
                if (!skip)
                {
                    var info = One(kvp.Key);
                    if (info == null) continue;
                    SaveToFile($"物品_1_{info[0]}.wiki", info[1], "物品");
                }
                cnt++;
                Main.ActorName = $"{cnt}/{sum}";
                if (!skip)
                {
                    yield return shortWait;
                }
            }
        }
        public string[] One(int presetItemId)
        {
            Main.ReadingItem = true;
            var presetItem = DateFile.instance.presetitemDate[presetItemId];
            var itemId = DateFile.instance.MakeNewItem(presetItemId, 0, 0, 0, 0);
            var itemDate = GameData.Items.GetItem(itemId);
            //耐久
            var duration = presetItem[902];
            int dur = int.Parse(duration);
            dur = Mathf.Abs(dur);
            dur = Mathf.Max(dur, 1);
            duration = dur.ToString();
            itemDate[902] = duration;
            itemDate[901] = duration;

            var result = new Dictionary<string, string>();
            result["id"] = presetItemId.ToString();
            result["name"] = DateFile.instance.GetItemDate(itemId, 0);
            //上框
            bool isWeapon = int.Parse(DateFile.instance.GetItemDate(itemId, 1)) == 1;
            if (isWeapon)
            {
                //武器伤害GetWeaponDamage（可能随版本而改变）
                result["damage"] = DateFile.instance.SetColoer(20003, $"\n{DateFile.instance.massageDate[8007][1].Split('|')[19]}{(int.Parse(presetItem[602]) / 2 / 10).ToString()}%");
                //攻击距离
                result["distance"] = DateFile.instance.SetColoer(20002, string.Format("\n{0}{1}~{2}", DateFile.instance.massageDate[8007][1].Split('|')[21], ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 502)) / 100f).ToString("f1"), ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 503)) / 100f).ToString("f1")));
            }
            //下框
            ShowItemMassage(itemId, 1, true, -999);
            result["text"] = WindowManage.instance.informationMassage.text;
            if (isWeapon)
            {
                var need = ShowWeaponUseNeed(itemId);
                result["need"] = need.Replace(">20%<", ">-<");
            }
            //页面名称
            string pageName = presetItem[0];
            if (pageName == "血露")
            {
                int grade = int.Parse(presetItem[8]);
                string gradeText = DateFile.instance.massageDate[8001][2].Split('|')[grade - 1].Split(new string[] { "·" }, StringSplitOptions.RemoveEmptyEntries)[1];
                pageName += $"({gradeText})";
            }
            else if (presetItem[4] == "5") //图书
            {
                pageName = Boss.NoNewLine(pageName).Replace("《", "").Replace("》", "");
                if (presetItemId == 5005) //义父的天枢玄机
                {
                    pageName += "(剧情)";
                }
                else if (presetItem[31] == "17")
                {
                    pageName += presetItem[35] == "1" ? "(手抄)" : "(真传)";
                }
            }
            else if (presetItem[5] == "36")//神兵
            {
                var arr = pageName.Split('\n');
                pageName = arr.Last();
            }else if (presetItem[0] == "白鹿角")
            {
                pageName += $"({(presetItemId == 60505 ? "对刺" : "武林大会")})";
            }
            string text = @"{{#invoke:Item|main";
            foreach (var kvp in result)
            {
                text += $"\n|{kvp.Key}={Boss.ReformatColor(kvp.Value).Replace("\n", "<br/>\n")}";
            }
            text += "}}";
            Main.ReadingItem = false;
            return new string[] { pageName, text };
        }

        MethodInfo showItemMassage = typeof(WindowManage).GetMethod("ShowItemMassage", BindingFlags.NonPublic | BindingFlags.Instance);
        void ShowItemMassage(int itemId, int itemTyp, bool setName = true, int showActorId = -1, int shopBookTyp = 0)
        {
            showItemMassage.Invoke(WindowManage.instance, new object[] { itemId, itemTyp, setName, showActorId, shopBookTyp });
        }
        MethodInfo showWeaponUseNeed = typeof(WindowManage).GetMethod("ShowWeaponUseNeed", BindingFlags.NonPublic | BindingFlags.Instance);
        string ShowWeaponUseNeed(int equipId)
        {
            return (string)showWeaponUseNeed.Invoke(WindowManage.instance, new object[] { equipId });
        }
    }
    
    public class Printer : MonoBehaviour
    {
        public static T Regist<T>(GameObject go) where T: Printer
        {
            T p = go.AddComponent<T>();
            UnityEngine.Object.DontDestroyOnLoad(p);
            return p;
        }
        internal bool waitFlag = false;
        internal readonly WaitForSeconds waitTime = new WaitForSeconds(2);
        internal readonly WaitForSeconds shortWait = new WaitForSeconds(0.01f);
        internal const string outputDir = ".\\wiki\\";
        internal void SaveToFile(string fileName, string massage, string folderName = "")
        {
            //if (!fileName.EndsWith(".wiki")) fileName += ".wiki";
            Main.Show(fileName);
            string dir = outputDir;
            if (folderName != "") dir += folderName + "\\";
            if (!System.IO.Directory.Exists(dir))//创建目录
                System.IO.Directory.CreateDirectory(dir);
            FileStream file = new FileStream(dir + fileName, FileMode.Create);
            if (file != null)
            {
                byte[] data = System.Text.Encoding.Default.GetBytes(massage);
                file.Write(data, 0, data.Length);
                file.Flush();
                file.Close();
            }            
        }
    }

    public class Boss
    {
        public readonly int actorId;
        public readonly int presetActorId;
        public readonly string name;
        public Data allData;
        public Boss(int actorId, int presetActorId)
        {
            this.actorId = actorId;
            this.presetActorId = presetActorId;
            _instance = this;
            name = DateFile.instance.GetActorName(actorId);
            allData = new Data();
            allData.Add("id", presetActorId);
            allData.Add("name", name);
            AttrInfo();
            FeatureInfo();
            BaseInfo();
            ValueInfo();
            EquipInfo();
        }
        void Move()
        {
            var part = DateFile.instance.mianPartId;
            var place = DateFile.instance.mianPlaceId;
            DateFile.instance.MoveToPlace(part, place, actorId, false);
        }
        static Boss _instance;
        public static Boss Get()
        {
            if (_instance == null)
                Main.CallBattle();
            return _instance;
        }
        
        static List<string[]>[] GongFas(int presetActorId)
        {
            string[] actorDate = DateFile.instance.presetActorDate[presetActorId][906].Split('|');
            List<string[]>[] gongfaPairs = new List<string[]>[15];
            for (int i = 0; i < gongfaPairs.Length; i++) gongfaPairs[i] = new List<string[]>();
            foreach (var s in actorDate)
            {
                var pair = s.Split('&');
                var gongfaDate = DateFile.instance.gongFaDate[int.Parse(pair[0])];
                pair[0] = gongfaDate[0];
                int typ = int.Parse(gongfaDate[61]) - 101;
                if (typ >= 0) gongfaPairs[typ].Add(pair);
                else gongfaPairs[14].Add(pair);//半血特效
            }
            return gongfaPairs;
        }//获取功法并分类
        static List<string> GongFaTypName
        {
            get
            {
                if (gongFaTypName == null)
                {
                    string[] massage = DateFile.instance.massageDate[301][4].Split('|');
                    List<string> names = new List<string>();
                    bool flag = false;
                    for (int i = 0; i < massage.Length; i++)
                    {
                        if (massage[i] == "内功")
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            names.Add(massage[i]);
                        }
                    }
                    names.Add("半血特性");
                    gongFaTypName = names;
                }
                return gongFaTypName;
            }
        }
        static List<string> gongFaTypName;
        
        static HashSet<int> HitTyps(int itemId)
        {
            var date = DateFile.instance.presetitemDate;
            var presetItem = date[date.ContainsKey(itemId) ? itemId : GameData.Items.GetItemProperty(itemId, 999).ToInt()];
            return new HashSet<int>(presetItem[7].Split('|').Select(s => DateFile.instance.attackTypDate[s.ToInt()][1].ToInt()).Distinct());
        }

        public class Data
        {
            Dictionary<string, string> raw;
            public Data()
            {
                raw = new Dictionary<string, string>();
            }
            public void Add(object key, object value)
            {
                raw[key.ToString()] = value.ToString();
            }
            public override string ToString()
            {
                return JsonConvert.SerializeObject(raw);
            }
        }
        public void AttrInfo()
        {
            var keys = new List<int>(DateFile.instance.presetActorDate[1].Keys);
            foreach (int key in keys)
            {
                string value = DateFile.instance.GetActorDate(this.actorId, key);
                if (value != "" && value != "0") allData.Add(key, value);
            }
            allData.Add("内功发挥", int.Parse(DateFile.instance.GetActorDate(this.actorId, 1105)) + 100);
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.massageDate[1002][1].Split('|')[1 + i];
                allData.Add(name, DateFile.instance.BaseAttr(this.actorId, i, 0));
            }//基础属性
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.massageDate[1002][1].Split('|')[1 + 6 + i];
                allData.Add(name, BattleVaule.instance.GetDeferDefuse(false, this.actorId, false, i, 0));
            }//防御属性
            int moveSpeed = BattleVaule.instance.GetMoveSpeed(false, this.actorId, false);
            int moveRange = BattleVaule.instance.GetMoveRange(false, this.actorId, false);
            allData.Add("移动速度", (moveSpeed / 100d).ToString("f2"));
            allData.Add("移动距离", (moveRange / 100d).ToString("f2"));
            allData.Add("攻击速度", BattleVaule.instance.GetAttackSpeedPower(false, this.actorId, false));
            allData.Add("外伤上限", DateFile.instance.MaxHp(this.actorId));
            allData.Add("内伤上限", DateFile.instance.MaxSp(this.actorId));
            string[] hitName = new string[] { "力道", "精妙", "迅疾", "内息" };
            for (var i = 0; i < 3; i++)
            {
                allData.Add("攻击" + hitName[i], DateFile.instance.AttAttr(this.actorId, i, 0));
            }
            for (var i = 0; i < 3; i++)
            {
                int weapon = DateFile.instance.GetActorDate(actorId, 301 + i).ToInt();
                var hitTyps = HitTyps(weapon);
                var cnt = 0;
                for (var j = 0; j < 4; j++)
                {
                    if (hitTyps.Contains(j + 1))
                    {
                        var name = hitName[j];
                        var value = BattleVaule.instance.GetWeaponHit(false, this.actorId, false, weapon, j + 1); // 因为在战斗准备界面，所以inBattle:false
                        allData.Add($"命中{i + 1}{++cnt}", $"{name}|{value}");
                    }
                    //allData.Add($"{hitName[j]}{i + 1}", BattleVaule.instance.GetWeaponHit(false, actorId, false, weapon, j + 1)); // 因为在战斗准备界面，所以inBattle:false
                }
            }
            int speed1 = BattleVaule.instance.GetMagicSpeed(false, this.actorId, false, 0);
            int speed2 = BattleVaule.instance.GetMagicSpeed(false, this.actorId, false, 1);
            allData.Add("提气速度", (speed1 / 100d).ToString("f2"));
            allData.Add("架势速度", (speed2 / 100d).ToString("f2"));
            for (int i = 0; i < 4; i++)
            {
                string name = DateFile.instance.massageDate[5001][0].Split('|')[i];
                allData.Add(name + "真气", DateFile.instance.GetActorDate(this.actorId, 902 + i, false));
            }//真气
            allData.Add("精纯", DateFile.instance.GetActorDate(this.actorId, 901, false));
            var allQi = DateFile.instance.GetActorAllQi(this.actorId);
            for (int i = 0; i < 5; i++)
            {
                string name = DateFile.instance.massageDate[2004][0].Split('|')[1 + i];
                allData.Add(name + "内力", allQi[i]);
            }//内力
            var qiTyp = DateFile.instance.qiValueStateDate[DateFile.instance.GetActorQiTyp(this.actorId)];
            var qiTypName = qiTyp[0].Split(new string[] { "·" }, StringSplitOptions.None)[0];
            qiTypName = DateFile.instance.SetColoer(int.Parse(qiTyp[98]), qiTypName);
            allData.Add("内力属性", ReformatColor(qiTypName));
            allData.Add("内力属性说明", ReformatColor(qiTyp[99]));
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.poisonDate[i][0];
                string value = DateFile.instance.presetActorDate[presetActorId][41 + i];
                value = (value == "-1") ? "不侵" : DateFile.instance.MaxPoison(this.actorId, i).ToString();
                allData.Add(name + "抵抗", value);
            }//毒
            // 似乎要进入战斗后才计算，就搬一下代码吧
            {
                int enemyDoHealSize = (int.Parse(DateFile.instance.GetActorDate(actorId, 23)) > 0) ? Mathf.Clamp(DateFile.instance.GetActorValue(actorId, 509) / 100, 1, 3) : 0;
                int enemyDoRemovePoisonSize = (int.Parse(DateFile.instance.GetActorDate(actorId, 1110)) > 0) ? Mathf.Clamp(DateFile.instance.GetActorValue(actorId, 510) / 100, 1, 3) : 0;
                if (DateFile.instance.enemyBorn <= 1)
                {
                    enemyDoHealSize = Mathf.Min(enemyDoHealSize, DateFile.instance.enemyBorn);
                    enemyDoRemovePoisonSize = Mathf.Min(enemyDoRemovePoisonSize, DateFile.instance.enemyBorn);
                }
                allData.Add("疗伤次数", enemyDoHealSize);
                allData.Add("驱毒次数", enemyDoRemovePoisonSize);
            }
        }
        public void FeatureInfo()
        {
            int id = 0;
            string[] actorDate = DateFile.instance.presetActorDate[presetActorId][101].Split('|');
            if (actorDate.Length > 1)
            {
                if (!int.TryParse(actorDate[1], out id))
                {
                    id = 0;
                }
            }
            var featureDate = DateFile.instance.actorFeaturesDate[id];
            allData.Add("说明", featureDate[99]);
            allData.Add("特性", featureDate[0]);
        }
        public void BaseInfo()
        {
            int key = actorId;
            int num = DateFile.instance.mianActorId;
            allData.Add("称号", DateFile.instance.GetActorGeneration(key, 999));
            allData.Add("姓名", DateFile.instance.GetActorName(actorId, true));
            allData.Add("性别", DateFile.instance.massageDate[7][0].Split('|')[int.Parse(DateFile.instance.GetActorDate(key, 14, false))]);
            string charm = ((int.Parse(DateFile.instance.GetActorDate(key, 11, false)) <= 14) ? DateFile.instance.massageDate[25][5].Split('|')[0] : ((int.Parse(DateFile.instance.GetActorDate(key, 8, false)) == 1 && int.Parse(DateFile.instance.GetActorDate(key, 305, false)) == 0) ? DateFile.instance.massageDate[25][5].Split('|')[1] : DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(key, 14, false)) - 1].Split('|')[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 15)) / 100, 0, 9)]));
            allData.Add("魅力", ReformatColor(charm));
            string goodness = DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(key)];
            allData.Add("立场", ReformatColor(goodness));
            string fame = DateFile.instance.Color7(DateFile.instance.GetActorFame(key));
            allData.Add("名誉", ReformatColor(fame));
            int num2 = int.Parse(DateFile.instance.GetActorDate(key, 19, false));
            int num3 = int.Parse(DateFile.instance.GetActorDate(key, 20, false));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            int key2 = (num3 < 0) ? (1001 + int.Parse(DateFile.instance.GetActorDate(key, 14, false))) : 1001;
            allData.Add("从属", DateFile.instance.GetGangDate(num2, 0));
            string gangLevel = DateFile.instance.SetColoer((gangValueId == 0) ? 20002 : (20011 - Mathf.Abs(num3)), DateFile.instance.presetGangGroupDateValue[gangValueId][key2]);
            allData.Add("身份", ReformatColor(gangLevel));
            int actorFavor = DateFile.instance.GetActorFavor(false, num, key);
            string favor = ((key == num || actorFavor == -1) ? DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[303][2]) : DateFile.instance.Color5(actorFavor));
            allData.Add("好感", ReformatColor(favor));
            string mood = DateFile.instance.Color2(DateFile.instance.GetActorDate(key, 4, false));
            allData.Add("心情", ReformatColor(mood));
            allData.Add("轮回", DateFile.instance.GetLifeDateList(key, 801).Count);
            for (int i = 0; i < 7; i++)
            {
                string name = DateFile.instance.massageDate[1002][2].Split('|')[i + 1];
                int value = int.Parse(DateFile.instance.GetActorDate(actorId, 2001 + i));
                value = Mathf.Clamp(value, 0, 100);
                allData.Add(name, value);
            }//七元
            allData.Add("剑冢", DateFile.instance.massageDate[519][2].Split('|')[presetActorId - 2001]);
        }
        public void ValueInfo()//技艺武学
        {
            int key = actorId;
            for (int i = 0; i < 16; i++)
            {
                string name = DateFile.instance.massageDate[301][4].Split('|')[4 + i];
                allData.Add(name, DateFile.instance.GetActorValue(key, 501 + i));
            }
            for (int i = 0; i < 14; i++)
            {
                string name = DateFile.instance.massageDate[301][4].Split('|')[4 + 16 + i];
                allData.Add(name, DateFile.instance.GetActorValue(key, 601 + i));
            }
        }
        public void EquipInfo()
        {
            var names = DateFile.instance.massageDate[1001][0].Split('|');
            names[5] = "护甲"; //护体重名
            var Number = new string[3] { "一", "二", "三" };
            for (int i = 0; i < 3; i++)
            {
                names[i + 7] = names[i + 7] + Number[i];
            }//宝物123
            for (int i = 0; i < 12; i++)
            {
                string name = names[i];
                int itemId = DateFile.instance.GetActorDate(actorId, 301 + i).ToInt();
                string itemName = itemId > 0 ? DateFile.instance.GetItemDate(itemId, 0, false) : "无";
                if (i < 3) itemName = NoNewLine(itemName); //去除武器名称中的换行
                allData.Add(name, itemName);
            }
        }

        public static string NoNewLine(string s)
        {
            return s.Replace("\n", "·");

            /*var ss = s.Split('\n');
            return ss[ss.Length - 1];*/
        }
        public static string ReformatColor(string s, bool noColor = false)
        {

            s = Regex.Replace(s, "<color=([^>]+)>", noColor ? "" : "<span style=\"color:${1}\">")
                .Replace("</color>", noColor ? "" : "</span>");
            if(!noColor) s = Regex.Replace(s, @"(#\w{6})FF", "${1}"); //去除最后两位透明度（部分浏览器不支持）
            return s;
        }
        static string Blue (string s) => "<span style=\"color:#8FBAE7>" + s + "</span>";
        static string Red  (string s) => "<span style=\"color:#E4504D>" + s + "</span>";
        static string Green(string s) => "<span style=\"color:#6DB75F>" + s + "</span>";
    }

    [HarmonyPatch(typeof(DateFile), "GetWorldXXLevel")]//改变世界进度
    public static class WikiDate_GetWorldXXLevel_Patch
    {
        static bool Prefix(bool getBaseLevel, ref int __result)
        {
            if (!Main.enabled || getBaseLevel || Main.XXLevel < 0)
            {
                return true;
            }
            __result = Main.XXLevel;
            return false;
        }
    }

    [HarmonyPatch(typeof(DateFile), "MakeXXEnemy")]//捕捉相枢化身
    public static class WikiDate_MakeXXEnemy_Patch
    {
        static void Postfix(int baseActorId, int __result)
        {
            new Boss(__result, baseActorId);
        }
    }

    [HarmonyPatch(typeof(DateFile), "GetWeaponUsePower")]//100发挥
    public static class WikiDate_GetWeaponUsePower_Patch
    {
        static bool Prefix(ref int __result)
        {
            if (!Main.enabled || !Main.ReadingItem) return true;
            __result = 100;
            return false;
        }
    }

    [HarmonyPatch(typeof(WindowManage), "EquipMassage")]//兵器属性
    public static class WikiDate_EquipMassage_Patch
    {
        static bool Prefix(int equipId, ref string __result)
        {
            if (!Main.enabled || !Main.ReadingItem) return true;
            if (GameData.Items.GetItem(equipId) == null) return true;//随机应变三件套
            __result = EquipMassage2(equipId);
            return false;
        }
        static string EquipMassage(int equipId)
        {
            equipId = int.Parse(GameData.Items.GetItemProperty(equipId, 999));
            string str = "";
            int num = 0;
            bool isActor = true;
            str += $"{WindowManage.instance.Dit()}{DateFile.instance.massageDate[151][2].Split('|')[0]}{DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[151][2].Split('|')[int.Parse(DateFile.instance.GetItemDate(equipId, 507))])}\n";
            string text = "";
            string[] array = DateFile.instance.GetItemDate(equipId, 7).Split('|');
            for (int i = 0; i < array.Length; i++)
            {
                text += DateFile.instance.SetColoer(int.Parse(DateFile.instance.attackTypDate[int.Parse(array[i])][99]), DateFile.instance.attackTypDate[int.Parse(array[i])][0]);
                if (i < array.Length - 1)
                {
                    text += WindowManage.instance.Cut(20002);
                }
            }
            str += $"{WindowManage.instance.Dit()}{DateFile.instance.massageDate[8007][1].Split('|')[14]}{text}\n";
            str += $"{WindowManage.instance.Dit()}{DateFile.instance.massageDate[8007][1].Split('|')[33]}{DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split('|')[24])}{DateFile.instance.SetColoer(20005, DateFile.instance.presetitemDate[equipId][701])}{WindowManage.instance.Cut(20002)}{DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split('|')[25])}{DateFile.instance.SetColoer(20005, DateFile.instance.presetitemDate[equipId][702])}{WindowManage.instance.Cut(20002)}{DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split('|')[26])}{DateFile.instance.SetColoer(20005, DateFile.instance.presetitemDate[equipId][703])}\n";
            str += $"{WindowManage.instance.Dit()}{DateFile.instance.massageDate[8007][1].Split('|')[32]}{DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split('|')[22])}{DateFile.instance.SetColoer(20006, DateFile.instance.presetitemDate[equipId][604])}{WindowManage.instance.Cut(20002)}{DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split('|')[23])}{DateFile.instance.SetColoer(20006, DateFile.instance.presetitemDate[equipId][605])}\n";
            str += string.Format("{0}{1}{2}{3}\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split('|')[34], DateFile.instance.SetColoer(20007, int.Parse(DateFile.instance.GetItemDate(equipId, 10)) + "%"), DateFile.instance.SetColoer(20004, $" +{DateFile.instance.GetItemDate(equipId, 17)}%{DateFile.instance.massageDate[8007][1].Split('|')[35]}"));

            //str += $"{WindowManage.instance.Dit()}追击/变招系数：{DateFile.instance.SetColoer(20009, DateFile.instance.presetitemDate[equipId][14])}\n";
            //str = $"<equip>{str}</equip>";
            return str;
        }
        static string EquipMassage2(int equipId)
        {
            var presetItem = DateFile.instance.presetitemDate[int.Parse(GameData.Items.GetItemProperty(equipId, 999))];
            string str = "";
            int actorIsInBattle = 0;
            bool isActor = true;
            str += $"{Util.Dit()}{DateFile.instance.massageDate[151][2].Split('|')[0]}{Util.Color(20003, DateFile.instance.massageDate[151][2].Split('|')[presetItem[507].ToInt()])}\n";
            string[] array = DateFile.instance.massageDate[8007][1].Split('|');
            HashSet<int> 命中类型 = new HashSet<int>();
            string text = "";
            string[] 招式 = presetItem[7].Split('|');
            for (int i = 0; i < 招式.Length; i++)
            {
                int key = int.Parse(招式[i]);
                Dictionary<int, string> dictionary = DateFile.instance.attackTypDate[key];
                int coloer = int.Parse(dictionary[99]);
                string 招式名 = dictionary[0];
                int item = int.Parse(dictionary[1]);
                text += Util.Color(coloer, 招式名);
                if (i < 招式.Length - 1)
                {
                    text += Util.Cut(20002);
                }
                命中类型.Add(item);
            }
            str += $"{Util.Dit()}{array[14]}{text}\n";
            StringBuilder stringBuilder = new StringBuilder(Util.Dit() + array[33]);
            bool 已有命中 = false;
            for (int i = 0; i < 4; i++)
            {
                //int weaponHit = BattleVaule.instance.GetWeaponHit(isActor, showEquipActorId, actorIsInBattle != 0, equipId, i+1);
                string weaponHit = presetItem[701 + i];
                if (命中类型.Contains(i + 1))
                {
                    if (已有命中)
                    {
                        stringBuilder.Append(Util.Cut(20002));
                    }
                    stringBuilder.Append(Util.Color(20003, i < 3 ? array[24 + i] : StringCenter.Get("UI_ACTOR_ATTR_DIR")));
                    stringBuilder.Append(Util.Color(20005, weaponHit));
                    已有命中 = true;
                }
            }
            stringBuilder.Append("\n");
            str += stringBuilder.ToString();
            string 破体 = presetItem[604];
            string 破气 = presetItem[605];
            //int num2 = (actorIsInBattle != 0 && !ActorMenu.Exists) ? BattleVaule.instance.GetDestroy(isActor, showEquipActorId, BattleSystem.instance.ActorId(!isActor), equipId, 0, 0) : BattleVaule.instance.GetWeaponDestroy(isActor, showEquipActorId, equipId, 0);
            //int num3 = (actorIsInBattle != 0 && !ActorMenu.Exists) ? BattleVaule.instance.GetDestroy(isActor, showEquipActorId, BattleSystem.instance.ActorId(!isActor), equipId, 0, 1) : BattleVaule.instance.GetWeaponDestroy(isActor, showEquipActorId, equipId, 1);
            str += $"{Util.Dit()}{array[32]}{Util.Color(20003, array[22])}{Util.Color(20006, 破体)}{Util.Cut(20002)}{Util.Color(20003, array[23])}{Util.Color(20006, 破气)}\n";
            //int num4 = int.Parse(DateFile.instance.GetItemDate(equipId, 10));
            int 内伤比 = presetItem[10].ToInt();
            //int num5 = BattleVaule.instance.SetAttackMagic(showEquipActorId, equipId);
            //int num6 = num5 - num4;
            int 内功传导 = presetItem[17].ToInt();
            return str + $"{Util.Dit()}{array[34]}{Util.Color(20007, $"{内伤比}%")}（{Util.Color(10003, "内功发挥")}{Util.Color(20003, "转化")} {Util.Color(20004, 内功传导)}）\n";

        }
    }

    [HarmonyPatch(typeof(DateFile), "MakeNewItem")]//无词条
    public static class WikiDate_MakeNewItem_Patch
    {
        static void Prefix(ref int buffObbs, ref int sBuffObbs)
        {
            if (!Main.enabled || !Main.ReadingItem) return;
            buffObbs = 0;
            sBuffObbs = 0;
        }
    }

    [HarmonyPatch(typeof(WindowManage), "ShowBookMassage")]//无法查看此书籍的内容
    public static class WikiDate_ShowBookMassage_Patch
    {
        static bool Prefix(WindowManage __instance, ref string __result)
        {
            if (!Main.enabled || !Main.ReadingItem) return true;
            __result = $"{__instance.SetMassageTitle(8007, 0, 12)}{__instance.Dit()}{DateFile.instance.massageDate[8006][4]}\n\n";
            return false;
        }
    }


}