using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ququ
{
    class Program
    {
        const string input_path = @"C:\taiwu\steamapps\common\The Scroll Of Taiwu\Backup\txt\Cricket_Date.txt";
        static List<string> keys;
        static void Main(string[] args)
        {
            //读入数据
            var ququDate = ReadQuquDate(input_path);
            //生成蛐蛐
            var crickets = new List<Dictionary<string, string>>();
            foreach(var ququ in ququDate[0])
            {
                ququ["data96"] = $"Cricket_36_{ququ["data96"]}.png";
                ququ["data99"] = ququ["data99"].Replace(@"\n", "<br/>");
                crickets.Add(ququ);
            }//呆物王
            int ii;
            string ss;
            bool flag;
            foreach (var color in ququDate[1])
            {
                foreach(var part in ququDate[2])
                {
                    var cricket = new Dictionary<string, string>();
                    foreach(var key in keys)
                    {
                        switch (key)
                        {
                            case "data0"://name
                                var colorNames = color[key].Split('|');
                                flag = GetValue(color, 2) >= GetValue(part, 2);
                                ss = flag ? (colorNames[0] + part[key]) : (part[key] + colorNames[1]);
                                break;
                            case "data1"://品级
                                ii = Math.Max(GetValue(color, key), GetValue(part, key));
                                ss = ii.ToString();
                                break;
                            case "data99"://描述
                                ss = part[key];
                                break;
                            case "data96"://图片
                                ss = $"Cricket_{color[key]}_{part[key]}.png";
                                break;
                            case "data8"://进化概率，小数
                                var colorValue = ParseDouble(color[key]);
                                var partValue = ParseDouble(part[key]);
                                ss = (colorValue + partValue).ToString("f2");
                                break;
                            default:
                                ii = GetValue(color, key) + GetValue(part, key);
                                ss = ii.ToString();
                                break;
                        }
                        cricket.Add(key, ss);
                    }
                    crickets.Add(cricket);
                }
            }//生成普通蛐蛐
            var lines = new List<string>();
            keys.Remove("id");
            lines.Add(string.Join(",", keys.ToArray()) + '\n');
            foreach(var cricket in crickets)
            {
                cricket.Remove("id");
                var line = "";
                bool first = true;
                foreach(var key in keys)
                {
                    if (!first) line += ",";
                    else first = false;
                    line += cricket[key];
                }
                line += '\n';
                lines.Add(line);
            }
            //输出
            SaveToFile("AllCrickets.csv", lines);
            /*foreach (var line in lines)
            {
                Console.WriteLine($"{line}");
            }*/
            Console.ReadKey();
        }
        static List<Dictionary<string, string>>[] ReadQuquDate(string path)
        {
            var lines = Read(path);
            keys = new List<string>(lines.Dequeue().Split(','));
            var len = keys.Count;
            for (int i = 0; i < len; i++)
            {
                keys[i] = (i == 0) ? "id" : ("data" + keys[i]);
            }//改key
            var ququs = new List<Dictionary<string, string>>[3];
            for (int i = 0; i < 3; i++)
            {
                ququs[i] = new List<Dictionary<string, string>>();
            }//new
            while (lines.Count > 0)
            {
                var ququ = new Dictionary<string, string>();
                var values = lines.Dequeue().Split(',');
                var vlen = values.Length;
                for (int i = 0; i < len; i++)
                {
                    ququ.Add(keys[i], values[i]);
                }
                var typ = Typ(ququ);
                ququs[typ].Add(ququ);
            }//填充
            return ququs;
        }
        static Queue<string> Read(string path)
        {
            try
            {
                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                var result = new Queue<string>();
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    //Console.WriteLine(s);
                    result.Enqueue(s);
                }
                return result;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        static int Typ(Dictionary<string, string> ququ)
        {
            if (ququ["data4"] == "1")//部件
            {
                return 2;
            }
            if (ququ["data2"] == "9" || ququ["id"] == "0")//呆物王
            {
                return 0;
            }
            return 1;
        }
        static int GetValue(Dictionary<string, string> dict, int key) => ParseInt(dict[$"data{key}"]);
        static int GetValue(Dictionary<string, string> dict, string key) => ParseInt(dict[key]);
        static int ParseInt(string s)
        {
            if(int.TryParse(s, out int result))
            {
                return result;
            }
            throw new Exception($"{s} is not a int");
        }
        static double ParseDouble(string s)
        {
            if (double.TryParse(s, out double result))
            {
                return result;
            }
            throw new Exception($"{s} is not a double");
        }
        static void SaveToFile(string fileName, List<string> massages, string folderName = "")
        {
            string dir = @"F:\3D objects\wiki\";
            if (folderName != "") dir += folderName + "\\";
            if (!System.IO.Directory.Exists(dir))//创建目录
                System.IO.Directory.CreateDirectory(dir);
            FileStream file = new FileStream(dir + fileName, FileMode.Create);
            if (file != null)
            {
                foreach (var str in massages)
                {
                    byte[] data = System.Text.Encoding.Default.GetBytes(str);
                    file.Write(data, 0, data.Length);
                }
                file.Flush();
                file.Close();
            }
            Console.WriteLine("done");
        }
    }
}
