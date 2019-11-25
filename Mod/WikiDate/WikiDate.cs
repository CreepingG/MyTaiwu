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

            if (go == null || printer == null)
            {
                go = new GameObject();
                printer = go.AddComponent<Printer>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                UnityEngine.Object.DontDestroyOnLoad(printer);
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
                printer.SaveAll();
            }

            flag = GUILayout.Button("物品数据");
            if (flag)
            {
                printer.SaveItems();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            flag = GUILayout.Button("某物");
            if (flag)
            {
                Show(Printer.ItemInfo(PresetActorId)[1]);
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
        public static Printer printer;
    }
    
    public class Printer : MonoBehaviour
    {
        static readonly string[] enemyBornText = { "简单", "困难", "极难", "必死" };
        List<string[]> gongfas;
        readonly WaitForSeconds waitTime = new WaitForSeconds(2);
        readonly WaitForSeconds shortWait = new WaitForSeconds(0.01f);

        public bool waitFlag = false;
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
            for(int i = 0; i < 9; i++)
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
                    SaveToFile($"Data_2_相枢化身_1_{boss.name}_1_剑{i + 1}_1_难度{j}.json.wiki", boss.allData.ToString());
                    yield return waitTime;
                }
            }
            Main.EndBattle();
            waitFlag = false;
        }
        
        void EndBattle() { Main.EndBattle(); }
        public const string outputDir = ".\\wiki\\";
        static void SaveToFile(string fileName, string massage, string folderName = "")
        {
            if (!fileName.EndsWith(".wiki")) fileName += ".wiki";
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

        public void SaveItems()
        {
            StartCoroutine(DoSaveItems());
        }
        IEnumerator DoSaveItems()
        {
            var date = DateFile.instance.presetitemDate;
            int sum = date.Count;
            int cnt = 0;
            foreach (var kvp in date)
            {
                var skip = false;// cnt <= 2640;
                if (!skip)
                {
                    var info = ItemInfo(kvp.Key);
                    if (info == null) continue;
                    SaveToFile($"物品_1_{info[0]}", info[1], "物品");
                }
                cnt++;
                Main.ActorName = $"{cnt}/{sum}";
                if (!skip)
                {
                    yield return shortWait;
                }
            }
        }
        public static string[] ItemInfo(int presetItemId)
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
            }
            string text = @"{{#invoke:Item|main";
            foreach (var kvp in result)
            {
                text += $"\n|{kvp.Key}={Boss.Color(kvp.Value).Replace("\n", "<br/>\n")}";
            }
            text += "}}";
            Main.ReadingItem = false;
            return new string[] { pageName, text };
        }
        
        static MethodInfo showItemMassage = typeof(WindowManage).GetMethod("ShowItemMassage", BindingFlags.NonPublic | BindingFlags.Instance);
        static void ShowItemMassage(int itemId, int itemTyp, bool setName = true, int showActorId = -1, int shopBookTyp = 0)
        {
            showItemMassage.Invoke(WindowManage.instance, new object[] { itemId, itemTyp, setName, showActorId, shopBookTyp });
        }
        static MethodInfo showWeaponUseNeed = typeof(WindowManage).GetMethod("ShowWeaponUseNeed", BindingFlags.NonPublic | BindingFlags.Instance);
        static string ShowWeaponUseNeed(int equipId)
        {
            return (string)showWeaponUseNeed.Invoke(WindowManage.instance, new object[] { equipId });
        }
    }

    public class Boss
    {
        public readonly int actorId;
        public readonly int presetActorId;
        public readonly string name;
        public Data allData;
        /*public Boss(int presetActorId, int index, int gameLevel = 1, bool outHome = false)
        {
            this.presetActorId = presetActorId;
            Main.XXLevel = index;
            DateFile.instance.enemyBorn = gameLevel;
            actorId = DateFile.instance.MakeXXEnemy(presetActorId, index, outHome);
            _instance = this;
        }
        public Boss() : this(Main.PresetActorId, Main.XXLevel, Main.Difficulty, Main.Outhome) { }*/
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


        public string TabedGongFaInfo()
        {
            var l = GongFas(presetActorId);
            var ss = BoxedLine(l);
            return RichTab(ss);
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
        static string[] BoxedLine(List<string[]>[] allGongfa)
        {
            var result = new string[allGongfa.Length];
            for (int j = 0; j < allGongfa.Length; j++)
            {
                result[j] = "";
                foreach (var pair in allGongfa[j])
                {
                    result[j] += Box(pair[0], pair[1] == "1");
                }
            }
            return result;
        }//将每类功法组成一个字符串
        static string RichTab(string[] ss)
        {
            string result = "{{RichTab\n";
            result += "|" + GongFaTypName[14] + "\n";
            result += "|" + ss[14] + "\n";
            for (int i = 0; i < 14; i++)
            {
                if (ss[i].Length > 0)
                {
                    result += "|" + GongFaTypName[i] + "\n";
                    result += "|" + ss[i] + "\n";
                }
                /*else
                {
                    result += "|\n";
                    result += "|{{暂无功法||" + GongFaTypName[i] + "}}\n";
                }*/
            }
            result += "}}";
            return result;
        }//合并总字符串
        static string Box(string s, bool ni) => "{{功法Box|" + s + (ni ? "|1" : "") + "}}";

        public string ActorGongFasInfo()
        {
            var result = "";
            var gongfas = GongFas(presetActorId);
            for(int i = 0; i < 15; i++)
            {
                result += $"|Data{GongFaTypName[i]}=";
                foreach(var pair in gongfas[i])
                {
                    result += $"{pair[0]},{pair[1]};";
                }
                result += "\n";
            }
            return result;
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
                string value = DateFile.instance.GetActorDate(actorId, key);
                allData.Add(key, value);
            }
            allData.Add("内功发挥", int.Parse(DateFile.instance.GetActorDate(actorId, 1105)) + 100);
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.massageDate[1002][1].Split('|')[1 + i];
                allData.Add(name, DateFile.instance.BaseAttr(actorId, i, 0));
            }//基础属性
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.massageDate[1002][1].Split('|')[1 + 6 + i];
                allData.Add(name, BattleVaule.instance.GetDeferDefuse(false, actorId, false, i, 0));
            }//防御属性
            int moveSpeed = BattleVaule.instance.GetMoveSpeed(false, actorId, false);
            int moveRange = BattleVaule.instance.GetMoveRange(false, actorId, false);
            allData.Add("移动速度", (moveSpeed / 100d).ToString("f2"));
            allData.Add("移动距离", (moveRange / 100d).ToString("f2"));
            allData.Add("攻击速度", BattleVaule.instance.GetAttackSpeedPower(false, actorId, false));
            allData.Add("外伤上限", DateFile.instance.MaxHp(actorId));
            allData.Add("内伤上限", DateFile.instance.MaxSp(actorId));
            allData.Add("攻击力道", DateFile.instance.AttAttr(actorId, 0, 0));
            allData.Add("攻击精妙", DateFile.instance.AttAttr(actorId, 1, 0));
            allData.Add("攻击迅疾", DateFile.instance.AttAttr(actorId, 2, 0));
            int speed1 = BattleVaule.instance.GetMagicSpeed(false, actorId, false, 0);
            int speed2 = BattleVaule.instance.GetMagicSpeed(false, actorId, false, 1);
            allData.Add("提气速度", (speed1 / 100d).ToString("f2"));
            allData.Add("架势速度", (speed2 / 100d).ToString("f2"));
            for (int i = 0; i < 4; i++)
            {
                string name = DateFile.instance.massageDate[5001][0].Split('|')[i];
                allData.Add(name + "真气", DateFile.instance.GetActorDate(actorId, 902 + i, false));
            }//真气
            allData.Add("精纯", DateFile.instance.GetActorDate(actorId, 901, false));
            var allQi = DateFile.instance.GetActorAllQi(actorId);
            for (int i = 0; i < 5; i++)
            {
                string name = DateFile.instance.massageDate[2004][0].Split('|')[1 + i];
                allData.Add(name + "内力", allQi[i]);
            }//内力
            var qiTyp = DateFile.instance.qiValueStateDate[DateFile.instance.GetActorQiTyp(actorId)];
            var qiTypName = qiTyp[0].Split(new string[] { "·" }, StringSplitOptions.None)[0];
            qiTypName = DateFile.instance.SetColoer(int.Parse(qiTyp[98]), qiTypName);
            allData.Add("内力属性", Color(qiTypName));
            allData.Add("内力属性说明", Color(qiTyp[99]));
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.poisonDate[i][0];
                string value = DateFile.instance.presetActorDate[presetActorId][41 + i];
                value = (value == "-1") ? "不侵" : DateFile.instance.MaxPoison(actorId, i).ToString();
                allData.Add(name + "抵抗", value);
            }//毒
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
            allData.Add("魅力", Color(charm));
            string goodness = DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(key)];
            allData.Add("立场", Color(goodness));
            string fame = DateFile.instance.Color7(DateFile.instance.GetActorFame(key));
            allData.Add("名誉", Color(fame));
            int num2 = int.Parse(DateFile.instance.GetActorDate(key, 19, false));
            int num3 = int.Parse(DateFile.instance.GetActorDate(key, 20, false));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            int key2 = (num3 < 0) ? (1001 + int.Parse(DateFile.instance.GetActorDate(key, 14, false))) : 1001;
            allData.Add("从属", DateFile.instance.GetGangDate(num2, 0));
            string gangLevel = DateFile.instance.SetColoer((gangValueId == 0) ? 20002 : (20011 - Mathf.Abs(num3)), DateFile.instance.presetGangGroupDateValue[gangValueId][key2]);
            allData.Add("身份", Color(gangLevel));
            int actorFavor = DateFile.instance.GetActorFavor(isEnemy: false, num, key);
            string favor = ((key == num || actorFavor == -1) ? DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[303][2]) : DateFile.instance.Color5(actorFavor));
            allData.Add("好感", Color(favor));
            string mood = DateFile.instance.Color2(DateFile.instance.GetActorDate(key, 4, false));
            allData.Add("心情", Color(mood));
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
                int itemId = int.Parse(DateFile.instance.presetActorDate[presetActorId][301 + i]);
                string itemName = itemId > 0 ? DateFile.instance.presetitemDate[itemId][0] : "无";
                if (i < 3) itemName = NoNewLine(itemName);
                allData.Add(name, itemName);
            }
        }

        public static string NoNewLine(string s)
        {
            return s.Replace("\n", "·");

            /*var ss = s.Split('\n');
            return ss[ss.Length - 1];*/
        }
        public static string Color(string s, bool nocolor = false)
        {

            s = Regex.Replace(s, "<color=([^>]*?)>", nocolor ? "" : "<span style=\"color:${1}\">")
                .Replace("</color>", nocolor ? "" : "</span>");
            if(!nocolor) s = Regex.Replace(s, @"(#\w{6})FF", "${1}");
            return s;
        }
        static string Color2(string s) => Regex.Replace(s, @"C_200\d\d", "").Replace("C_D", "");
        static string Blue (string s) => "<span style=\"color:#8FBAE7>" + s + "</span>";
        static string Red  (string s) => "<span style=\"color:#E4504D>" + s + "</span>";
        static string Green(string s) => "<span style=\"color:#6DB75F>" + s + "</span>";
        //static string Gray (string s) => "<span style=\"color:#8FBAE7>" + s + "</span>";
        /*static string GOR<T>(T a, T b) where T:IEqualityComparer
        {
            if (a.Equals(b)) return a.ToString();
            if (a > b) return Green(s);
            return Red(s);
        }
        static string GOR(double a, double b) => GOR(a.ToString(), Compare(a, b));*/

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
            __result = EquipMassage(equipId);
            return false;
        }
        static string EquipMassage(int equipId)
        {
            equipId = int.Parse(GameData.Items.GetItemProperty(equipId, 999));
            string str = "";
            int num = 0;
            bool isActor = num == 0 || num == 1;
            if (ActorMenu.instance.actorMenu.activeInHierarchy)
            {
                isActor = !ActorMenu.instance.isEnemy;
            }
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
    }

    /*[HarmonyPatch(typeof(WindowManage), "SetItemTypText")]//兵器属性
    public static class WikiDate_SetItemTypText_Patch
    {
        static bool Prefix(int itemId)
        {
            Main.Show(itemId);
            Main.Show(((float)int.Parse(DateFile.instance.GetItemDate(itemId, 601)) / 100f));
            Main.Show(((float)int.Parse(DateFile.instance.GetItemDate(itemId, 603)) / 100f));
            DateFile.instance.GetItem(DateFile.instance.mianActorId, itemId, 1,false);
            return true;
        }
    }*/

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