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

namespace FightBoss
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
            /*GUILayout.Label("姓名");
            GUILayout.TextField(ActorName);*/
            GUILayout.Label("难度(0-3)");
            flag = int.TryParse(GUILayout.TextField(Difficulty.ToString()), out tmp);
            if (flag) Difficulty = tmp;
            GUILayout.Label("次序(0-6)");
            flag = int.TryParse(GUILayout.TextField(XXLevel.ToString()), out tmp);
            if (flag) XXLevel = tmp;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            flag = GUILayout.Button("开战");
            if (flag) CallBattle();

            flag = GUILayout.Button("找错人了，告辞");
            if (flag) EndBattle();
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

        public static void Print(string text)
        {
            Main.Logger.Log(text);
        }
        static string showText = "";
        static Queue<string> textQueue = new Queue<string>();
        public static void Show<T>(T item, bool add = true)
        {
            string text = item.ToString();
            if (add && showText != "")
            {
                textQueue.Enqueue(text);
                if (textQueue.Count > 10)
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
        public static void Log<T>(T item)
        {
            Logger.Log(item.ToString());
        }

        public static void CallBattle()
        {
            if (Difficulty >= 0 && Difficulty <= 3)
                DateFile.instance.enemyBorn = Difficulty;
            StartBattle.instance.ShowStartBattleWindow(9001 + DateFile.instance.GetWorldXXLevel(), 0, 18, new List<int> { PresetActorId });//typ死斗，mapTyp背景
            ActorName = Boss.Get().name;
        }
        public static void EndBattle()
        {
            DateFile.instance.battleStart = false;
            StartBattle.instance.CloseStartBattleWindow();
            StartBattle.instance.startBattleWindow.SetActive(false);
            BattleSystem.instance.battleWindow.SetActive(false);
            UIMove.instance.ShowGUI();
        }
        //Invoke("StartBattleWindowOpend", 0.25f);
        
    }

    public class Boss
    {
        public int actorId;
        public int presetActorId;
        public string name;
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

        public string AttrInfo()
        {
            string result = "";
            var keys = new List<int>(DateFile.instance.presetActorDate[1].Keys);
            foreach (int key in keys)
            {
                string value = DateFile.instance.GetActorDate(actorId, key).Replace('|', ';');
                result += $"|Data{key}={value}\n";
            }
            result += $"|Data内功发挥={int.Parse(DateFile.instance.GetActorDate(actorId, 1105)) + 100}\n";
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.massageDate[1002][1].Split('|')[1 + i];
                result += $"|Data{name}={DateFile.instance.BaseAttr(actorId, i, 0)}\n";
            }//基础属性
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.massageDate[1002][1].Split('|')[1 + 6 + i];
                result += $"|Data{name}={BattleVaule.instance.GetDeferDefuse(false, actorId, false, i, 0)}\n";
            }//防御属性
            int moveSpeed = BattleVaule.instance.GetMoveSpeed(false, actorId, false);
            int moveRange = BattleVaule.instance.GetMoveRange(false, actorId, false);
            result += $"|Data移动速度={(moveSpeed / 100d).ToString("f2")}\n";
            result += $"|Data移动距离={(moveRange / 100d).ToString("f2")}\n";
            result += $"|Data攻击速度={BattleVaule.instance.GetAttackSpeedPower(false, actorId, false)}\n";
            result += $"|Data外伤上限={ActorMenu.instance.MaxHp(actorId)}\n";
            result += $"|Data内伤上限={ActorMenu.instance.MaxSp(actorId)}\n";
            result += $"|Data攻击力道={DateFile.instance.AttAttr(actorId, 0, 0)}\n";
            result += $"|Data攻击精妙={DateFile.instance.AttAttr(actorId, 1, 0)}\n";
            result += $"|Data攻击迅疾={DateFile.instance.AttAttr(actorId, 2, 0)}\n";
            int speed1 = BattleVaule.instance.GetMagicSpeed(false, actorId, false, 0);
            int speed2 = BattleVaule.instance.GetMagicSpeed(false, actorId, false, 1);
            result += $"|Data提气速度={(speed1 / 100d).ToString("f2")}\n";
            result += $"|Data架势速度={(speed2 / 100d).ToString("f2")}\n";
            for (int i = 0; i < 4; i++)
            {
                string name = DateFile.instance.massageDate[5001][0].Split('|')[i];
                result += $"|Data{name}真气={DateFile.instance.GetActorDate(actorId, 902 + i, false)}\n";
            }//真气
            result += $"|Data精纯={DateFile.instance.GetActorDate(actorId, 901, false)}\n";
            var allQi = DateFile.instance.GetActorAllQi(actorId);
            for (int i = 0; i < 5; i++)
            {
                string name = DateFile.instance.massageDate[2004][0].Split('|')[1 + i];
                result += $"|Data{name}内力={allQi[i]}\n";
            }//内力
            var qiTyp = DateFile.instance.qiValueStateDate[DateFile.instance.GetActorQiTyp(actorId)];
            var qiTypName = qiTyp[0].Split(new string[] { "·" }, StringSplitOptions.None)[0];
            qiTypName = DateFile.instance.SetColoer(int.Parse(qiTyp[98]), qiTypName);
            result += $"|Data内力属性={Color(qiTypName)}\n";
            result += $"|Data内力属性说明={Color(qiTyp[99])}\n";
            for (int i = 0; i < 6; i++)
            {
                string name = DateFile.instance.poisonDate[i][0];
                string value = DateFile.instance.presetActorDate[presetActorId][41 + i];
                value = (value == "-1") ? "不侵" : ActorMenu.instance.MaxPoison(actorId, i).ToString();
                result += $"|Data{name}抵抗={value}\n";
            }//毒
            return result;
        }
        public string FeatureInfo()
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
            string result = $"|Data说明={featureDate[99]}\n";
            result += $"|Data特性={featureDate[0]}\n";
            return result;
        }
        public string BaseInfo()
        {
            int key = actorId;
            int num = DateFile.instance.mianActorId;
            string result = "";
            result += $"|Data称号={DateFile.instance.GetActorGeneration(key, 999)}\n";
            result += $"|Data姓名={DateFile.instance.GetActorName(actorId, true)}\n";
            result += $"|Data性别={DateFile.instance.massageDate[7][0].Split('|')[DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 14, addValue: false))]}\n";
            string charm = ((DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 11, addValue: false)) <= 14) ? DateFile.instance.massageDate[25][5].Split('|')[0] : ((DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 8, addValue: false)) == 1 && DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 305, addValue: false)) == 0) ? DateFile.instance.massageDate[25][5].Split('|')[1] : DateFile.instance.massageDate[25][DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 14, addValue: false)) - 1].Split('|')[Mathf.Clamp(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 15)) / 100, 0, 9)]));
            result += $"|Data魅力={Color(charm)}\n";
            string goodness = DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(key)];
            result += $"|Data立场={Color(goodness)}\n";
            string fame = ActorMenu.instance.Color7(DateFile.instance.GetActorFame(key));
            result += $"|Data名誉={Color(fame)}\n";
            int num2 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 19, addValue: false));
            int num3 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 20, addValue: false));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            int key2 = (num3 < 0) ? (1001 + DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 14, addValue: false))) : 1001;
            result += $"|Data从属={DateFile.instance.GetGangDate(num2, 0)}\n";
            string gangLevel = DateFile.instance.SetColoer((gangValueId == 0) ? 20002 : (20011 - Mathf.Abs(num3)), DateFile.instance.presetGangGroupDateValue[gangValueId][key2]);
            result += $"|Data身份={Color(gangLevel)}\n";
            int actorFavor = DateFile.instance.GetActorFavor(isEnemy: false, num, key);
            string favor = ((key == num || actorFavor == -1) ? DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[303][2]) : ActorMenu.instance.Color5(actorFavor));
            result += $"|Data好感={Color(favor)}\n";
            string mood = ActorMenu.instance.Color2(DateFile.instance.GetActorDate(key, 4, addValue: false));
            result += $"|Data心情={Color(mood)}\n";
            result += $"|Data轮回={DateFile.instance.GetLifeDateList(key, 801).Count.ToString()}\n";
            for (int i = 0; i < 7; i++)
            {
                string name = DateFile.instance.massageDate[1002][2].Split('|')[i + 1];
                int value = int.Parse(DateFile.instance.GetActorDate(actorId, 2001 + i));
                value = Mathf.Clamp(value, 0, 100);
                result += $"|Data{name}={value}\n";
            }//七元
            result += $"|Data剑冢={DateFile.instance.massageDate[519][2].Split('|')[presetActorId - 2001]}\n";
            return result;
        }
        public string ValueInfo()
        {
            int key = actorId;
            string result = "";
            for (int i = 0; i < 16; i++)
            {
                string name = DateFile.instance.massageDate[301][4].Split('|')[4 + i];
                result += $"|Data{name}={DateFile.instance.GetActorValue(key, 501 + i)}\n";
            }
            for (int i = 0; i < 14; i++)
            {
                string name = DateFile.instance.massageDate[301][4].Split('|')[4 + 16 + i];
                result += $"|Data{name}={DateFile.instance.GetActorValue(key, 601 + i)}\n";
            }
            return result;
        }//随剑冢进度改变
        public string EquipInfo()
        {
            string result = "";
            for (int i = 0; i < 12; i++)
            {
                string name = DateFile.instance.massageDate[1001][0].Split('|')[i];
                int itemId = int.Parse(DateFile.instance.presetActorDate[presetActorId][301 + i]);
                string itemName = itemId > 0 ? DateFile.instance.presetitemDate[itemId][0] : "无";
                if (i < 3) itemName = NoNewLine(itemName);
                result += $"|Data{name}={itemName}\n";
            }
            return result;
        }

        public static string NoNewLine(string s)
        {
            return s.Replace("\n", "·");

            var ss = s.Split('\n');
            return ss[ss.Length - 1];
        }
        public static string Color(string s)
        {

            s = Regex.Replace(s, "<color=([^>]*?)>", "<span style=\"color:${1}\">")
                .Replace("</color>", "</span>");
            s = Regex.Replace(s, @"(#\w{6})FF", "${1}");
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
    public static class FightBoss_GetWorldXXLevel_Patch
    {
        static bool Prefix(bool getBaseLevel, ref int __result)
        {
            if (!Main.enabled || getBaseLevel || Main.XXLevel < 0 || Main.XXLevel > 6)
            {
                return true;
            }
            __result = Main.XXLevel;
            return false;
        }
    }

    [HarmonyPatch(typeof(DateFile), "MakeXXEnemy")]//捕捉相枢化身
    public static class FightBoss_MakeXXEnemy_Patch
    {
        static void Postfix(int baseActorId, int __result)
        {
            new Boss(__result, baseActorId);
        }
    }

}