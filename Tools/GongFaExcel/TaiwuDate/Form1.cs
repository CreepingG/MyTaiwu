using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
//todo：将运功效果中的固定值和发挥效率合并
namespace TaiwuDate
{
    struct Field
    {
        public int type;//0-不变 1-100倍 2-百分比
        public string name;
        public Field(string name, int type = 0)
        {
            this.name = name;
            this.type = type;
        }
    }
    public partial class Form1 : Form
    {
        static bool Origin = false;
        static Dictionary<string, Dictionary<int, string>> GongFaDate = new Dictionary<string, Dictionary<int, string>> { };
        static Dictionary<int, string> GongFaFPowerDate = new Dictionary<int, string> { };
        static Dictionary<int, Field> 运功字段 = new Dictionary<int, Field>
        {
            {50041,new Field("全毒素抵抗",1) },
            {50032,new Field("外伤上限",1) },
            {50033,new Field("内伤上限",1) },
            {50022,new Field("御守效率",2) },
            {50023,new Field("疗伤效率",2) },
            {51110,new Field("驱毒效率",2) },
            {51101,new Field("提气速度") },
            {51102,new Field("架势速度") },
            {51103,new Field("提气消耗",2) },
            {51104,new Field("架势消耗",2) },
            {51105,new Field("内功发挥",2) },
            {51106,new Field("施展速度",2) },
            {51107,new Field("攻击速度",2) },
            {51108,new Field("攻击切换",2) },
            {51109,new Field("移动速度") },
            {51111,new Field("移动距离") },
            {50081,new Field("护体",1) },
            {50082,new Field("御气",1) },
            {50083,new Field("内息",1) },
            {51367,new Field("护体发挥",2) },
            {51368,new Field("御气发挥",2) },
            {51369,new Field("内息发挥",2) },
            {50084,new Field("卸力",1) },
            {50085,new Field("拆招",1) },
            {50086,new Field("闪避",1) },
            {51370,new Field("卸力发挥",2) },
            {51371,new Field("拆招发挥",2) },
            {51372,new Field("闪避发挥",2) },
            {50071,new Field("力道",1) },
            {50072,new Field("精妙",1) },
            {50073,new Field("迅疾",1) },
            {50092,new Field("力道发挥",2) },
            {50093,new Field("精妙发挥",2) },
            {50094,new Field("迅疾发挥",2) },
            {50095,new Field("造成外伤",2) },
            {50096,new Field("造成内伤",2) },
            {50097,new Field("破体强度",2) },
            {50098,new Field("破气强度",2) },
        };
        static Dictionary<int, Field> 通用字段 = new Dictionary<int, Field>{
            { -1, new Field("id") },
            { 0, new Field("功法名") },
            { 2, new Field("品级") },
            { 61, new Field("类型") },
            { 103, new Field("正练特效") },
            { 104, new Field("逆练特效") },
            { 6, new Field("运功位") },
            { 7, new Field("占格") },
            { 3, new Field("门派") },
            { 4, new Field("功法属性") },
            { 710, new Field("发挥需求 ") },
            { 711, new Field("发挥上限") },
            { 8, new Field("提气比例") },
            { 9, new Field("架势提气消耗") },
            { 40, new Field("消耗移动力") },
            { 10, new Field("施展时间") },
            { 32, new Field("固定移动速度") },
            { 33, new Field("百分比移动速度") },
            { 34, new Field("固定移动距离") },
            { 35, new Field("百分比移动距离") },
            { 36, new Field("获得移动力") },
        };
        static Dictionary<int, Field> 内功专属字段 = new Dictionary<int, Field>
        {
            { 921, new Field("摧破格") },
            { 922, new Field("轻灵格") },
            { 923, new Field("护体格") },
            { 924, new Field("奇窍格") },
            { 701, new Field("金刚内力") },
            { 702, new Field("紫霞内力") },
            { 703, new Field("玄阴内力") },
            { 704, new Field("纯阳内力") },
            { 705, new Field("归元内力") },
        };
        static Dictionary<int, Field> 摧破专属字段 = new Dictionary<int, Field>
        {

            { 31, new Field("攻击范围") },
            { 21, new Field("胸背") },
            { 22, new Field("腰腹") },
            { 23, new Field("头颈") },
            { 24, new Field("左臂") },
            { 25, new Field("右臂") },
            { 26, new Field("左腿") },
            { 27, new Field("右腿") },
            { 28, new Field("心神") },
            { 29, new Field("毒质") },
            { 30, new Field("全身") },
            { 601, new Field("力道") },
            { 602, new Field("精妙") },
            { 603, new Field("迅疾") },
            { 611, new Field("力道占比") },
            { 612, new Field("精妙占比") },
            { 613, new Field("迅疾占比") },
            { 614, new Field("破体") },
            { 615, new Field("破气") },
            { 604, new Field("基础伤害") },
            { 81, new Field("烈毒") },
            { 82, new Field("郁毒") },
            { 83, new Field("寒毒") },
            { 84, new Field("赤毒") },
            { 85, new Field("腐毒") },
            { 86, new Field("幻毒") },
            { 37, new Field("封穴") },
            { 91, new Field("破绽") },
        };
        static Dictionary<int, Field> 护体专属字段 = new Dictionary<int, Field>
        {
            { 41, new Field("护体") },
            { 42, new Field("御气") },
            { 43, new Field("护体%") },
            { 44, new Field("御气%") },
            { 45, new Field("卸力") },
            { 46, new Field("闪避") },
            { 47, new Field("拆招") },
            { 48, new Field("卸力%") },
            { 49, new Field("闪避%") },
            { 50, new Field("拆招%") },
            { 51, new Field("反击倍率") },
            { 52, new Field("反震外伤") },
            { 53, new Field("反震内伤") },
            { 614, new Field("破体") },
            { 615, new Field("破气") },
            { 54, new Field("持续时间") },
            { 55, new Field("反击/反震范围") },
        };
        static string[] PreparationName = { "内伤比例", "提气", "架势" };
        static Dictionary<int, Field> 新增字段 = new Dictionary<int, Field>
        {
            { -100,new Field("万用格") },
            {-10,new Field(PreparationName[0]) },
            {-11,new Field(PreparationName[1]) },
            {-12,new Field(PreparationName[2]) },
            {-2,new Field("使用消耗") },
            {-3,new Field("施展效果") },
            {-4,new Field("发挥需求") },
            {-5,new Field("运功效果") },
            {-201,new Field("使用时移动速度") },
            {-202,new Field("使用时移动距离") },
            {-203,new Field("持续时间") },
            {-211,new Field("招式需求") },
            {-212,new Field("部位系数") },
            {-213,new Field("破体倍率") },
            {-214,new Field("破气倍率") },
            {-215,new Field("重伤程度") },
            {-216,new Field("破防效率") },
            {-101,new Field("内功属性") },
            {-102,new Field("功法属性") },
            {-103,new Field("功法命中") },
            {-104,new Field("参考伤害") },
        };
        static Dictionary<int, string> ActorDateName = new Dictionary<int, string>
        {
            {61,"膂力"},
            {62,"体质"},
            {63,"灵敏"},
            {64,"根骨"},
            {65,"悟性"},
            {66,"定力"},
            {71,"膂力发挥"},
            {72,"体质发挥"},
            {73,"灵敏发挥"},
            {74,"根骨发挥"},
            {75,"悟性发挥"},
            {76,"定力发挥"},
            {501,"音律"},
            {502,"弈棋"},
            {503,"诗书"},
            {504,"绘画"},
            {505,"术数"},
            {506,"品鉴"},
            {507,"锻造"},
            {508,"制木"},
            {509,"医术"},
            {510,"毒术"},
            {511,"织锦"},
            {512,"巧匠"},
            {513,"道法"},
            {514,"佛学"},
            {515,"厨艺"},
            {516,"杂学"},
            {601,"内功"},
            {602,"身法"},
            {603,"绝技"},
            {604,"拳掌"},
            {605,"指法"},
            {606,"腿法"},
            {607,"暗器"},
            {608,"剑法"},
            {609,"刀法"},
            {610,"长兵"},
            {611,"奇门"},
            {612,"软兵"},
            {613,"御射"},
            {614,"乐器"},
            {0,"无" },
        };
        static string[] 功法属性 = new string[6] { "混元", "金刚", "紫霞", "玄阴", "纯阳", "归元" };
        static string[] 门派 =new string[16]
        {
            "无门无派",
            "少林派",
            "峨眉派",
            "百花谷",
            "武当派",
            "元山派",
            "狮相门",
            "然山派",
            "璇女派",
            "铸剑山庄",
            "空桑派",
            "无量金刚宗",
            "五仙教",
            "界青门",
            "伏龙坛",
            "血犼教",
        };
        static string[] 品级 = new string[10]
        {
            "",
            "下·九品",
            "中·八品",
            "上·七品",
            "奇·六品",
            "秘·五品",
            "极·四品",
            "超·三品",
            "绝·二品",
            "神·一品",
        };
        static string path_GongFaDate = "GongFa_Date.txt";
        static string path_GongFaOtherFPowerDate = "GongFaOtherFPower_Date.txt";
        static string path_GongFaFPowerDate = "GongFaFPower_Date.txt";
        static string Endl = "\r\n";
        static List<int> BaseList = new List<int> { 0, 2, 3, 4, 7, 711 };
        static List<int> RequireList = new List<int> { };
        static List<int> BonusList = new List<int> { };
        static List<int> List0 = new List<int> { 103, 104, -100 };
        static List<int> List0_ = new List<int> { 103, 104, -101 };
        static List<int> List1 = new List<int> { 61, -10, -11, -12,};
        static List<int> List1_ = new List<int> { 61, 31, -2, - 103, -102, -104, 91, 37 };
        static List<int> List2 = new List<int> { 40, 36, 10, 32, 33, 34, 35, 103, 104 };
        static List<int> List2_ = new List<int> { -2, -3, 103, 104 };
        static List<int> List3 = new List<int> { -11, -12, 10, 103, 104 };
        static List<int> List3_ = new List<int> { -2, -3, 103, 104 };
        static List<int> List4 = new List<int> { 103, 104 };
        static List<int>[] DifferentList = { List0, List1, List2, List3, List4 };
        static List<int>[] DifferentList_ = { List0_, List1_, List2_, List3_, List4 };
        static string[] FileName = { "内功","摧破","轻灵","护体","奇窍" };
        static string[] 真气 = { "摧破", "轻灵", "护体", "奇窍" };
        static Color[] Colors = { Color.Red, Color.Green, Color.Orange, Color.Purple, Color.Gray };
        static Dictionary<int, int> 固定概率 = new Dictionary<int, int> { };
        static string[] NumberChar = {"零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        static string[] AttackTypName = new string[17] { "掷", "弹", "御", "劈", "刺", "撩", "崩", "点", "拿", "音", "缠", "咒", "机", "药", "毒", "无", "扫", };
        static int[] 伤势 = new int[10]{ 6, 6, 10, 4, 4, 4, 4, 4, 4, 8 };
        static Dictionary<int, string> 功法类型 = new Dictionary<int, string>
        {
            { 0,"其他" },
            { 101,"内功" },
            { 102,"身法" },
            { 103,"绝技" },
            { 104,"拳法" },
            { 105,"指法" },
            { 106,"腿法" },
            { 107,"暗器" },
            { 108,"剑法" },
            { 109,"刀法" },
            { 110,"长兵" },
            { 111,"奇门" },
            { 112,"软兵" },
            { 113,"御射" },
            { 114,"魔音" },
        };

        public Form1()//初始化
        {
            InitializeComponent();
            for(int i = 1; i < 8; i++)//发挥需求
            {
                int keyId = -400 - i;
                int valueId = -410 - i;
                新增字段.Add(keyId, new Field("需求属性" + i.ToString()));
                新增字段.Add(valueId, new Field("需求数值" + i.ToString()));
                RequireList.Add(keyId);
                RequireList.Add(valueId);
            }
            for (int i = 1; i < 4; i++)//发挥需求
            {
                int keyId = -500 - i;
                int valueId = -510 - i;
                新增字段.Add(keyId, new Field("式" + i.ToString()));
                新增字段.Add(valueId, new Field("数" + i.ToString()));
                List1.Add(keyId);
                List1.Add(valueId);
            }
            foreach (var kvp in 运功字段)
            {
                通用字段.Add(kvp.Key, kvp.Value);
                BonusList.Add(kvp.Key);
            }
            foreach(var kvp in 新增字段) 通用字段.Add(kvp.Key, kvp.Value);
            foreach (var kvp in 内功专属字段)
            {
                List0.Add(kvp.Key);
                通用字段.Add(kvp.Key, kvp.Value);
            }
            foreach (var kvp in 摧破专属字段)
            {
                List1.Add(kvp.Key);
                通用字段.Add(kvp.Key, kvp.Value);
            }
            foreach (var kvp in 护体专属字段)
            {
                List3.Add(kvp.Key);
                if(!通用字段.ContainsKey(kvp.Key)) 通用字段.Add(kvp.Key, kvp.Value);
            }
            Analysis_GongFaFPower(Read(path_GongFaFPowerDate));
            Analysis_GongFaOtherFPower(Read(path_GongFaOtherFPowerDate));
            Analysis_GongFaDate(Read(path_GongFaDate));
            textBox2.Select();
        }
        
        string[] Read(string path)
        {
            try
            {
                path = @"C:\taiwu\steamapps\common\The Scroll Of Taiwu\Backup\txt\" + path;
                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                var lines = new List<string> { };
                string s;
                while (true)
                {
                    s = sr.ReadLine();
                    if (s == null) break;
                    lines.Add(s);
                }
                return lines.ToArray();
            }
            catch(IOException e)
            {
                textBox1.Text = e.ToString();
                return new string[] { };
            }
        }
        List<Dictionary<int, string>> ToDict(string[] arr)
        {
            var firstLine = arr[0].Split(',');
            var keys = new int[firstLine.Length];
            for(int i = 1; i < firstLine.Length; i++)
            {
                keys[i] = int.Parse(firstLine[i]);
            }
            var result = new List<Dictionary<int, string>>();
            for(int i = 1; i < arr.Length; i++)
            {
                var line = arr[i].Split(',');
                var id = int.Parse(line[0]);
                var record = new Dictionary<int, string>();
                for(int j = 1; j < line.Length; j++)
                {
                    var value = line[j];
                    var key = keys[j];
                    record.Add(key, value);
                }
                result.Add(record);
            }
            return result;
        }
        
        class GongFa
        {
            public Dictionary<int, string> data;
            public string name;
            readonly int _leveled;
            T Leveled<T>(int key, int div = 1)
            {
                double value = Convert.ToDouble(data[key]) * _leveled / div;
                data[key] = value.ToString();
                return (T)Convert.ChangeType(value, typeof(T));
            }//品级系数
            int RequireValue(double f) => Convert.ToInt32(f * 150.0 / 5) * 5;//发挥需求
            double ResistPenetrate(double f)
            {
                double brk = (f * 6 + 250) + 100; // 50摧破真气
                double resist = 1000 + 100; // 1000护体
                if (brk > resist)
                {
                    double n = 0.4f;
                    return Math.Min(3f, 1f + (brk - resist) / resist * n);
                }
                else
                {
                    double n = 1f;
                    return Math.Max(0.25f, 1f + (brk - resist) / resist * n);
                }

            }//计算破防
            int Crit(int percent)
            {
                if (percent >= 100) return 3 * percent;
                if (percent < 50) return 1 * percent;
                return 2 * percent;
            }//重伤
            public static string Merge<T>(T v0, T v1, int times = 1)
            {
                double d0 = Convert.ToDouble(v0);
                double d1 = Convert.ToDouble(v1);
                string s0 = $"+{v0}%";
                string s1 = times >= 100 ? string.Format("+{0:F0}", d1) : string.Format("+{0:F2}", d1 / times);
                if (d0 != 0)
                    return d1 != 0 ? s0 + "※" + s1 : s0;
                else
                    return d1 != 0 ? s1 : null;
            }
            public static string RepeatedString(string s, int times)
            {
                string result = "";
                for (int i = 0; i < times; i++)
                {
                    result += s;
                }
                return result;
            }

            public GongFa(Dictionary<int, string> d)
            {
                data = d;
                name = d[0];
                int 运功位 = int.Parse(d[6]);
                //发挥需求
                string[] requirements = d[710].Split(new char[] { '|' });
                int cnt = 0;
                string s3 = "";
                foreach (string requirement in requirements)
                {
                    string[] a = requirement.Split(new char[] { '&' });
                    if (a.Length < 2) continue;
                    int s1 = int.Parse(a[0]);
                    string key = a[0];
                    if (ActorDateName.ContainsKey(s1)) key = ActorDateName[s1];
                    string value = RequireValue(double.Parse(a[1])).ToString();
                    cnt++;
                    d.Add(-400 - cnt, key);
                    d.Add(-410 - cnt, value);
                    s3 += (cnt <= 1 ? "" : Endl) + key + "：" + value;
                }
                d.Add(-4, s3);
                //施展时间
                if (运功位 == 2 || 运功位 == 3)
                {
                    int i1 = int.Parse(d[10]);
                    int time = Convert.ToInt32(20 + 0.25 * i1);
                    d[10] = time.ToString();
                }
                else if (运功位 == 1)
                {
                    int i1 = int.Parse(d[10]);
                    int time = Convert.ToInt32(40 + 0.25 * i1);
                    d[10] = time.ToString();
                }
                int level = int.Parse(d[2]);
                _leveled = level > 7 ? 100 : 80;
                d[2] = 品级[level];
                d[4] = 功法属性[int.Parse(d[4])];
                int gangId = int.Parse(d[3]);
                if (gangId <= 15) d[3] = 门派[gangId];
                else d[3] = "其他";
                int typ = int.Parse(d[61]);
                if (功法类型.ContainsKey(typ)) d[61] = 功法类型[typ];
                int 内伤比 = int.Parse(d[8]);
                int 总消耗 = int.Parse(d[9]);
                string 提气 = (内伤比 * 总消耗 / 100).ToString();
                string 架势 = ((100 - 内伤比) * 总消耗 / 100).ToString();
                d.Add(-11, 提气);
                d.Add(-12, 架势);
                if (运功位 == 1) d.Add(-10, d[8]);
                int 发挥上限 = int.Parse(d[711]) * 10 + 100;
                d[711] = 发挥上限.ToString();
                //正逆练特效
                int 正练id = int.Parse(d[103]);
                if (GongFaFPowerDate.ContainsKey(正练id))
                {
                    string text = GongFaFPowerDate[正练id];
                    if (text.Contains("一定机率") && 固定概率.ContainsKey(正练id))
                    {
                        text = text.Replace("一定机率", 固定概率[正练id].ToString() + "%机率");
                    }
                    d[103] = text;
                }
                int 逆练id = int.Parse(d[104]);
                if (GongFaFPowerDate.ContainsKey(逆练id))
                {
                    string text = GongFaFPowerDate[逆练id];
                    if (text.Contains("一定机率") && 固定概率.ContainsKey(逆练id))
                    {
                        text = text.Replace("一定机率", 固定概率[逆练id].ToString() + "%机率");
                    }
                    d[104] = text;
                }
                //内功
                if (运功位 == 0 || 运功位 == -1)
                {
                    int 万用格 = 0;
                    for (int j = 901; j <= 910; j++)
                    {
                        if (d.ContainsKey(j))
                            万用格 += int.Parse(d[j]);
                    }
                    d.Add(-100, 万用格.ToString());
                    //内功属性
                    string s = "";
                    int i1 = int.Parse(d[921]);
                    s += "摧破：" + RepeatedString("◇", i1) + Endl;
                    int i2 = int.Parse(d[922]);
                    s += "轻灵：" + RepeatedString("◇", i2) + Endl;
                    int i3 = int.Parse(d[923]);
                    s += "护体：" + RepeatedString("◇", i3) + Endl;
                    int i4 = int.Parse(d[924]);
                    s += "奇窍：" + RepeatedString("◇", i4) + Endl;
                    int i5 = 万用格;
                    s += "万用：" + RepeatedString("◇", i5) + Endl;
                    double 内力 = 0;
                    for (int j = 701; j <= 705; j++)
                    {
                        内力 = double.Parse(d[j]);
                        if (内力 > 0)
                        {
                            break;
                        }
                    }
                    string 内力属性 = d[4];
                    if (内力属性 == "混元")
                    {
                        内力属性 = "全属性";
                        内力 *= 2;
                    }
                    s += $"内力增加：{内力属性}内力+{(int)(内力 * 100)}";
                    d.Add(-101, s);
                }
                //摧破
                if (运功位 == 1)
                {
                    int 攻击距离 = int.Parse(d[31]);
                    d[31] = string.Format("{0:F2}", 攻击距离 / 100f);

                    string 招式需求 = "";
                    for (int j = 1; j <= 3; j++)
                    {
                        string[] pair = d[j + 10].Split('|');
                        if (pair.Length > 1)
                        {
                            string 招式名 = AttackTypName[int.Parse(pair[0])];
                            string n1 = pair[1];
                            if (n1 != "0")
                            {
                                d.Add(-500 - j, 招式名);
                                d.Add(-510 - j, n1);
                                if (招式需求 != "") 招式需求 += "※";
                                招式需求 += 招式名 + "×" + n1;
                            }
                        }
                    }
                    if (招式需求 != "") d.Add(-211, 招式需求);
                    string 功法命中 = "";
                    double 功法力道 = Leveled<double>(601);
                    double 功法精妙 = Leveled<double>(602);
                    double 功法迅疾 = Leveled<double>(603);
                    int 力道占比 = int.Parse(d[611]);
                    int 精妙占比 = int.Parse(d[612]);
                    int 迅疾占比 = int.Parse(d[613]);
                    功法命中 += "迅疾：" + (迅疾占比 == 0 ? "无法闪避" : RepeatedString("◇", 迅疾占比 / 10) + " " + 功法迅疾.ToString()) + Endl;
                    功法命中 += "精妙：" + (精妙占比 == 0 ? "无法拆招" : RepeatedString("◇", 精妙占比 / 10) + " " + 功法精妙.ToString()) + Endl;
                    功法命中 += "力道：" + (力道占比 == 0 ? "无法卸力" : RepeatedString("◇", 力道占比 / 10) + " " + 功法力道.ToString());
                    d.Add(-103, 功法命中);
                    int 重伤系数 = Crit(力道占比) + Crit(精妙占比) + Crit(迅疾占比) + Crit(力道占比 + 精妙占比 + 迅疾占比);
                    d.Add(-215, 重伤系数.ToString());

                    const double 威力发挥 = 3f;
                    const double 造成伤害 = 3f;
                    string 武学效果 = "";
                    double 基础伤害 = Leveled<double>(604) / 10;
                    double 破体 = Leveled<double>(614);
                    double 破气 = Leveled<double>(615);
                    武学效果 += $"伤害：{基础伤害}%{Endl}";
                    武学效果 += $"穿透：破体{破体}※破气{破气}{Endl}";
                    double 破体倍率 = ResistPenetrate(破体 * 威力发挥);
                    double 破气倍率 = ResistPenetrate(破气 * 威力发挥);
                    double 破防倍率 = ResistPenetrate((破体 + 破气) * 威力发挥);
                    d.Add(-213, 破体倍率.ToString());
                    d.Add(-214, 破气倍率.ToString());
                    d.Add(-216, 破防倍率.ToString());
                    武学效果 += "部位：";
                    int 总系数 = 0;
                    int 总权重 = 0;
                    for (int j = 21; j <= 30; j++)
                    {
                        if (d[j] != "0")
                        {
                            int weight = int.Parse(d[j]);
                            总权重 += weight;
                            总系数 += weight * 伤势[j - 21];
                            武学效果 += 通用字段[j].name + d[j] + " ";
                        }
                    }
                    double 部位期望 = (double)总系数 / 总权重;
                    d.Add(-212, 部位期望.ToString());
                    int damage = (int)(基础伤害 * 威力发挥 * 破防倍率 * 部位期望 * 重伤系数 * 造成伤害 / 1000);
                    d.Add(-104, damage.ToString());

                    //毒
                    for (int j = 81; j <= 86; j++)
                    {
                        if (d[j] != "0")
                        {
                            武学效果 += Endl;
                            武学效果 += 通用字段[j].name + "：" + Leveled<string>(j);
                        }
                    }
                    d.Add(-102, 武学效果);
                }
                //轻功施展效果
                if (运功位 == 2)
                {
                    string usingEffect = "";
                    //移动速度
                    string speed = Merge(Leveled<double>(33), Leveled<double>(32, 100));
                    if (speed != null)
                    {
                        d.Add(-201, speed);
                        usingEffect += "移动速度：" + speed + Endl;
                    }
                    //移动距离
                    string distance = Merge(Leveled<double>(35), Leveled<double>(34, 100));
                    if (distance != null)
                    {
                        d.Add(-202, distance);
                        usingEffect += "移动距离：" + distance + Endl;
                    }
                    //持续时间
                    string duration = d[36] + "×" + "120";
                    d.Add(-203, duration);
                    usingEffect += "持续时间：" + duration;
                    d.Add(-3, usingEffect);
                }
                //绝技施展效果
                if (运功位 == 3)
                {
                    string 护体 = Merge(Leveled<double>(43), Leveled<double>(41), 100);
                    if (护体 != null) 护体 = "护体：" + 护体;
                    string 御气 = Merge(Leveled<double>(44), Leveled<double>(42), 100);
                    if (御气 != null) 御气 = "御气：" + 御气;
                    string 卸力 = Merge(Leveled<double>(48), Leveled<double>(45), 100);
                    if (卸力 != null) 卸力 = "卸力：" + 卸力;
                    string 拆招 = Merge(Leveled<double>(49), Leveled<double>(46), 100);
                    if (拆招 != null) 拆招 = "拆招：" + 拆招;
                    string 闪避 = Merge(Leveled<double>(50), Leveled<double>(47), 100);
                    if (闪避 != null) 闪避 = "闪避：" + 闪避;
                    int 持续 = int.Parse(d[54]) / 10;
                    string duration = 持续 > 0 ? "持续时间：" + 持续.ToString() + ".0" : null;
                    int 距离 = int.Parse(d[55]);
                    string range = 距离 > 0 ? string.Format("反击/反震范围:2.00~{0:F2}", 距离 / 100.0) : null;
                    double 反击威力 = Leveled<double>(51);
                    string 反击 = 反击威力 > 0 ? "化解普攻时以" + 反击威力.ToString() + "%" + "威力反击" : null;
                    double 反外比例 = Leveled<double>(52);
                    string 反外 = 反外比例 > 0 ? "反震所受外伤的" + 反外比例.ToString() + "%" : null;
                    double 反内比例 = Leveled<double>(53);
                    string 反内 = 反内比例 > 0 ? "反震所受内伤的" + 反内比例.ToString() + "%" : null;
                    double 反震破体 = Leveled<double>(614);
                    double 反震破气 = Leveled<double>(615);
                    string 反震穿透 = (反震破体 > 0 || 反震破气 > 0) ? $"破体{反震破体} 破气{反震破气}" : null;
                    string usingEffect = "";
                    int cnt1 = 0;
                    foreach (string line in new string[] { 护体, 御气, 卸力, 拆招, 闪避, duration, range, 反击, 反外, 反内, 反震穿透 })
                    {
                        if (line == null) continue;
                        if (cnt1++ != 0) usingEffect += Endl;
                        usingEffect += line;
                    }
                    d.Add(-3, usingEffect);
                }
                //使用消耗
                if (运功位 == 1 || 运功位 == 2 || 运功位 == 3)
                {
                    string movePower = (d[40] != "0") ? ("移动消耗：" + d[40]) : null;
                    string attackTyp = d.ContainsKey(-211) ? ("招式消耗：" + d[-211]) : null;
                    string needPreparation = 总消耗 != 0 ? ("架势" + 架势 + "%" + Endl + "提气" + 提气 + "%") : null;
                    string needTime = (d[10] != "0") ? (通用字段[10].name + "：" + d[10]) : null;
                    string consumption = "";
                    int cnt1 = 0;
                    foreach (string line in new string[] { movePower, attackTyp, needPreparation, needTime })
                    {
                        if (line == null) continue;
                        if (cnt1++ != 0) consumption += Endl;
                        consumption += line;
                    }
                    d.Add(-2, consumption);
                }
                //运功效果
                string s4 = "";
                bool first = true;
                foreach (var kvp in 运功字段)
                {
                    int id = kvp.Key;
                    string name = kvp.Value.name;
                    int printType = kvp.Value.type;
                    double value = Leveled<double>(id, printType == 0 ? 100: 1);
                    if (value != 0.0)
                    {
                        string s_value = printType == 2 ? $"{value}%" :
                            (printType == 0 ? string.Format("{0:F2}", value) : string.Format("{0:F0}", value));
                        s4 += (first ? "" : Endl) + name + (value < 0 ? "" : "+") + s_value;
                        first = false;
                    }
                }
                d.Add(-5, s4);
            }
        }
        void Analysis_GongFaDate(string[] strArray)
        {
            var all = ToDict(strArray);
            foreach(var dict in all)
            {
                var gongfa = new GongFa(dict);
                GongFaDate.Add(gongfa.name, gongfa.data);
            }
        }

        public static string Quotation(string s) => "「" + s + "」";
        public static string Clean(string s) => Regex.Replace(s, @"C_\d{5}(.*?)C_D", @"$1");
        void Analysis_GongFaFPower(string[] strArray)
        {
            char[] separator = new char[] { ',' };
            List<int> fieldId = strArray[0].Split(separator).Select(s => (s == "#" ? -1 : int.Parse(s))).ToList();
            int length = strArray.Length;
            for (int i = 1;i<length;i++)
            {
                string[] records = strArray[i].Split(separator);
                int key = int.Parse(records[0]);
                string field = "";
                int len = records.Length;
                int cnt = 0;
                for(int j = 3; j < len; j++)
                {
                    int id = fieldId[j];
                    string text = records[j];
                    if (text.Contains('&'))
                    {
                        if (text != "0&0")
                        {
                            string[] pair = text.Split('&');
                            int value1 = int.Parse(pair[0]);
                            int value2 = int.Parse(pair[1]);
                            if (id == 3)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += Quotation(功法属性[value1]) + "类功法的威力+" + value2.ToString() + "%";
                            }
                            else if (id == 4)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += Quotation(功法属性[value1]) + "类功法的提气与架势消耗-" + value2.ToString() + "%";
                            }
                            else if (id == 5)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "当运用者的内力属性为"+ Quotation(功法属性[value1]) + "时，" + Quotation(功法属性[value2]) + "类功法对运用者造成的伤害-30%";
                            }
                            else if (id == 6)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "运用者的" + Quotation(功法属性[value1]) + "类功法可以对" + Quotation(功法属性[value2]) + "内力造成额外30%的伤害";
                            }
                            else if (id == 7)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "将运用者的" + Quotation(功法属性[value1]) + "内力化为" + Quotation(功法属性[value2]) + "内力";
                            }
                            else if (id == 10)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += Quotation(功法属性[value1]) + "类功法的发挥效率上限+" + value2.ToString() + "%";
                            }
                            else if (id == 11)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += Quotation(功法属性[value1]) + "类功法的使用需求-" + value2.ToString() + "%";
                            }/*
                            else if (id == 101)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "运用者的" + ZhenqiName[value1] + "真气的效果+" + value2.ToString() + "%";
                            }
                            else if (id == 102)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "运用者除" + ZhenqiName[value1] + "以外真气的效果-" + value2.ToString() + "%";
                            }*/

                        }
                        
                    }
                    else
                    {
                        int value = int.Parse(text);
                        if (value != -1)
                        {
                            if (value != 0)
                            {
                                if (ActorDateName.ContainsKey(id))
                                {
                                    if (cnt++ != 0) field += Endl;
                                    string attr = ActorDateName[id];
                                    field += attr + "+" + text;
                                    if (attr.Contains("发挥")) field += "%";
                                }
                                else if (id == 1)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "所有" + Quotation(门派[value]) + "的功法的发挥效率上限+20%";
                                }
                                else if (id == 2)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "所有" + Quotation(门派[value]) + "的功法的威力+10%";
                                }
                                else if (id == 111)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者每有一个名誉高于75的前世，所有功法的威力提高" + value.ToString() + "%，最多50%";
                                }
                                else if (id == 112)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者每有一个名誉低于-75的前世，所有功法的威力提高" + value.ToString() + "%，最多50%";
                                }
                                else if (id == 121)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者身上被封闭的穴道对运用者造成的影响-" + value.ToString() + "%";
                                }
                                else if (id == 131)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者身上的破绽对运用者造成的影响-" + value.ToString() + "%";
                                }
                                else if (id == 122)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者身上的每个部位最多只会被封闭" + value.ToString() + "个穴道";
                                }
                                else if (id == 132)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者身上的每个部位最多只会出现" + value.ToString() + "个破绽";
                                }
                                else if (id == 141)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者在战斗中治疗伤势的速度与效果+" + value.ToString() + "%";
                                }
                                else if (id == 142)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "敌人在战斗中治疗伤势的速度与效果-" + value.ToString() + "%";
                                }
                                else if (id == 151)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "当运用者与敌人的处世立场相同时，运用者所有功法的威力+" + value.ToString() + "%";
                                }
                                else if (id == 152)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "当运用者与敌人的处世立场不同时，运用者所有功法的威力+" + value.ToString() + "%";
                                }
                                else if (id == 161)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "如果运用者为童子之身，运用者所有功法的威力+" + value.ToString() + "%";
                                }
                                else if (id == 162)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "如果运用者为童女之身，运用者所有功法的威力+" + value.ToString() + "%";
                                }
                                else if (id == 171)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "所有武器的发挥效率上限+" + value.ToString() + "%";
                                }
                                else if (id == 172)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "所有武器的使用需求-" + value.ToString() + "%";
                                }
                                else if (id == 181)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者当前的真气总量比运用者初始的真气总量每多3点，运用者所有功法的威力+1%，最多30%";
                                }
                                else if (id == 182)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者当前的真气总量比运用者初始的真气总量每少3点，运用者所有功法的威力+1%，最多30%";
                                }
                                else if (id == 191)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者的每有一级正面特性，，运用者所有功法的威力+" + value.ToString() + "%，最多30%";
                                }
                                else if (id == 192)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者的每有一级负面特性，，运用者所有功法的威力+" + value.ToString() + "%，最多30%";
                                }
                                else if (id == 201)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者的心情越愉悦，运用者所有功法的威力就越高，乐极时为" + (value * 3).ToString() + "%";
                                }
                                else if (id == 202)
                                {
                                    if (cnt++ != 0) field += Endl;
                                    field += "运用者的心情越痛苦，运用者所有功法的威力就越高，悲极时为" + (value * 3).ToString() + "%";
                                }
                            }
                            if (id == 8)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "将运用者的" + Quotation(功法属性[value]) + "类功法的逆练效果转变为正练效果";
                            }
                            else if (id == 9)
                            {
                                if (cnt++ != 0) field += Endl;
                                field += "将运用者的" + Quotation(功法属性[value]) + "类功法的正练效果转变为逆练效果";
                            }
                        }

                        
                    }
                    
                }
                if (cnt == 0)
                {
                    GongFaFPowerDate.Add(key, Clean(records[1]));
                }
                else
                {
                    GongFaFPowerDate.Add(key, field);
                }
                
            }
             
        }
        void Analysis_GongFaOtherFPower(string[] strArray)
        {
            char[] separator = new char[] { ',' };
            foreach (string s in strArray)
            {
                string[] vs = s.Split(separator);
                if (vs[0] == "#") continue;
                int key = int.Parse(vs[0]);
                string value = Clean(vs[11]);
                int probability = int.Parse(vs[5]);
                if (!GongFaFPowerDate.ContainsKey(key))
                {
                    GongFaFPowerDate.Add(key, value);
                    if (probability > 0 && probability < 100)
                    {
                        固定概率.Add(key, probability);
                    }
                }
                else Print("Repeated Power ID: " + key);
            }
        }


        private void button3_Click(object sender, EventArgs e)//输出
        {
            List<int> l = new List<int> { };
            foreach (int i in checkedListBox1.CheckedIndices)
            {
                l.Add(i);
            }
            Write(l);

        }
        void Write(List<int> list)
        {
            button3.Text = "Writing...";
            button3.Enabled = false;
            Excel.Application application = new Excel.Application();
            Excel.Workbooks workbooks = application.Workbooks;
            application.AlertBeforeOverwriting = false;//为啥还是跳窗口？
            foreach(int i in list)
            {
                Excel._Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                Write_Workbook(workbook, i);
            }
            application.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(application);
            application = null;
            GC.Collect();
            //System.Diagnostics.Process.Start(filePath);
            button3.Text = "输出";
            button3.Enabled = true;
        }
        void Write_Workbook(Excel._Workbook workbook, int index)
        {
            workbook.Sheets.Add(Type.Missing, Type.Missing, 1, Type.Missing);
            //第一页
            Origin = false;
            Excel._Worksheet worksheet = workbook.Sheets[1];
            worksheet.Name = "浏览用";
            Write_Worksheet(worksheet, index);
            //第二页
            Origin = true;
            worksheet = workbook.Sheets[2];
            worksheet.Name = "计算用";
            Write_Worksheet(worksheet, index);
            //
            Origin = checkBox1.Checked;
            string filePath = @"D:\桌面\" + FileName[index] +".xlsx";
            workbook.SaveAs(filePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            
        }
        void Write_Worksheet(Excel._Worksheet worksheet, int index)
        {
            int i = 1;
            List<int> printList = PrintList(index);
            foreach (int id in printList)
            {
                worksheet.Cells[i][1] = 通用字段[id].name;
                Excel.Range range = worksheet.Columns[i, Type.Missing];
                if (id == 0)//功法名
                {
                    range.ColumnWidth = 8;
                    range.WrapText = true;
                }
                else if (id == 2) range.ColumnWidth = 7;
                else if (id == 3) range.ColumnWidth = 10;
                else if (id == 4) range.ColumnWidth = 8;
                else if (id == 7) range.ColumnWidth = 4;
                else if (id == 711) range.ColumnWidth = 8;
                else if (id == -101) range.ColumnWidth = 24;//内功属性
                else if (id == -2) range.ColumnWidth = (index == 1 ? 26 : 14);//使用消耗
                else if (id == -3) range.ColumnWidth = (index == 2 ? 21 : 24);//施展效果
                else if (id == 103 || id == 104)//特效
                {
                    range.ColumnWidth = 20;
                    range.WrapText = true;
                }
                else if (id == -4) range.ColumnWidth = 9;//发挥需求
                else if (id == -5) range.ColumnWidth = 14;//运功效果
                else if (id == -102) range.ColumnWidth = 32;//功法命中
                else if (id == -103) range.ColumnWidth = 30;//功法命中
                else if (id == -104) range.ColumnWidth = 8;//参考伤害
                else if (id == 61) range.ColumnWidth = 4;//功法类型
                i++;
            }
            int j = 2;
            foreach (var kvp in GongFaDate)
            {
                var d = kvp.Value;
                i = 1;
                if (d[6] == index.ToString() || (index == 0 && d[6] == "-1"))
                {
                    foreach (int id in printList)
                    {
                        if (d.ContainsKey(id))
                        {
                            worksheet.Cells[i][j] = d[id];
                            if (index == 0 && id == -101)
                            {
                                
                                int lineCnt = 0;
                                string text = ((Excel.Range)worksheet.Cells[i][j]).Characters.Text;
                                int cnt = text.Length;
                                var cell = ((Excel.Range)worksheet.Cells[i][j]);
                                for (int k = 1; k <= cnt; k++)
                                {
                                    string s = text.Substring(k-1,1);
                                    if (s == "◇")
                                    {
                                        cell.Characters[k, 1].Font.Color = Colors[lineCnt];
                                    }
                                    else if (s == "\n")
                                    {
                                        lineCnt += 1;
                                        if (lineCnt >= 5) break;
                                    }
                                }
                            };
                        }
                        i++;
                    }
                    j++;
                    //break;
                }
            }
        }
        List<int> PrintList(int type)
        {
            List<int> list = new List<int> { };
            if (Origin)
            {
                list.AddRange(BaseList);
                list.AddRange(DifferentList[type]);
                list.AddRange(RequireList);
                list.AddRange(BonusList);
            }
            else
            {
                list.AddRange(BaseList);
                list.AddRange(DifferentList_[type]);
                list.Add(-4);
                list.Add(-5);
            }
            return list;
        }

        private void button2_Click(object sender, EventArgs e)//kill
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("excel");
            foreach (System.Diagnostics.Process pro in procs)
            {
                pro.Kill();    //没有更好的方法,只有杀掉进程
            }
            //Close();
        }

        private void button1_Click(object sender, EventArgs e)//search
        {
            Search(textBox2.Text);
        }
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) Search(textBox2.Text);
        }
        void Search(string text)
        {
            if (GongFaDate.ContainsKey(text))
            {
                Print("",false);
                var d = GongFaDate[text];
                int type = int.Parse(d[6]);
                foreach (var kvp in GongFaDate[text])
                {
                    int fieldId = kvp.Key;
                    if (Origin)
                    {
                        string fieldName = 通用字段.ContainsKey(fieldId) ? 通用字段[fieldId].name : fieldId.ToString();
                        Print(fieldName + ":" + kvp.Value + Endl);
                    }
                    else
                    {
                        if (通用字段.ContainsKey(fieldId))
                        {
                            Print(通用字段[fieldId].name + ":" + kvp.Value + Endl);
                        }
                    }
                }
            }
            else
            {
                List<string> names = new List<string> { };
                foreach (var kvp in GongFaDate)
                {
                    string name = kvp.Key;
                    if (name.Contains(text))
                    {
                        names.Add(name);
                    }
                }
                int length = names.Count;
                switch (length)
                {
                    case 0:
                        Print("NotFound",false);
                        break;
                    case 1:
                        Search(names[0]);
                        break;
                    default:
                        textBox1.Text = "";
                        foreach(string name in names)
                        {
                            Print(name + Endl);
                        }
                        break;

                }
            }
        }
        void Print(string info, bool add = true)
        {
            if (add) textBox1.Text += info;
            else textBox1.Text = info;
        }

        private void DragDrop1(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }
        private void DragEnter1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All; //重要代码：表明是所有类型的数据，比如文件路径
            else e.Effect = DragDropEffects.None;
        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Origin = !Origin;
        }

        private void form1BindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }
    }
}
