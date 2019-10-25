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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        Dictionary<int, JObject> ById;
        Dictionary<string, JObject> ByName;
        JObject[] All;
        string Path = @"C:\taiwu\steamapps\common\The Scroll Of Taiwu\Backup\2.6.7\Item_Date.txt";
        string OutputDir = @"D:\3D objects\太吾绘卷\wiki\上传\";
        int Length = 0;

        public Form1()//初始化
        {
            InitializeComponent();
        }
        
        string ItemName(JObject jo)
        {
            string name = (string)jo["data0"];
            int id = (int?)jo["id"] ?? 0;
            name = name.Replace("\n", "·").Replace("《", "").Replace("》", "");
            if (name == "血露")
            {
                int level = (int?)jo["data8"] ?? 0;
                string gradeText = "下·九品|中·八品|上·七品|奇·六品|秘·五品|极·四品|超·三品|绝·二品|神·一品".Split('|')[level - 1].Split(new string[] { "·" }, StringSplitOptions.RemoveEmptyEntries)[1];
                name += $"({gradeText})";
            }
            else if ((int?)jo["data4"] == 5) //图书
            {
                if (id == 5005) //义父的天枢玄机
                {
                    name += "(剧情)";
                }
                else
                {
                    if ((int?)jo["data31"] == 17) name += (int?)jo["data35"] == 1 ? "(手抄)" : "(真传)";
                }
            }
            else if ((int?)jo["data5"] == 36)//神兵
            {
                var arr = name.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                name = arr.Last();
            }
            return name;
        }

        string[] ReadCsvLines(string path)
        {
            try
            {
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
        void Lines2JObject(string[] lines)
        {
            var firstLine = lines[0].Split(',');
            var keys = new string[firstLine.Length];
            int idCol = 0;
            for(int col = 0; col < firstLine.Length; col++)
            {
                var rawKey = firstLine[col];
                if (rawKey == "#")
                {
                    keys[col] = "id";
                    idCol = col;
                }
                else
                {
                    keys[col] = "data" + rawKey;
                }
            }
            All = new JObject[lines.Length - 1];
            ById = new Dictionary<int, JObject>();
            ByName = new Dictionary<string, JObject>();
            for (int row = 1; row < lines.Length; row++)
            {
                JObject jo = new JObject();
                var line = lines[row].Split(',');
                var id = int.Parse(line[idCol]);
                for(int col = 0; col < line.Length; col++)
                {
                    var value = line[col];
                    var key = keys[col];
                    if (int.TryParse(value, out var intValue))
                    {
                        if (intValue != 0) jo.Add(key, intValue);
                    }
                    else if (double.TryParse(value, out var floatValue))
                    {
                        if (floatValue != 0.0) jo.Add(key, floatValue);
                    }
                    else
                    {
                        if (value != "Null") jo.Add(key, value);
                    }
                }
                string name = Path.Contains("Item_Date") ? ItemName(jo) : (string)jo["data0"];
                jo.Add("pageName", name);
                jo.Add("version", VERSION.Text);
                All[row - 1] = jo;
                ById[id] = jo;
                ByName[name] = jo;
            }
            return;
        }

        void Read(string path)
        {
            Path = path;
            Lines2JObject(ReadCsvLines(path));
            Length = All.Length;
            Print($"共{Length}项\r\n",false);
            Print(All[0].ToString(), true);
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
            if (int.TryParse(text, out int id))
            {
                if (ById.ContainsKey(id))
                    Print(ById[id].ToString(), false);
                else
                    Print("no found", false);
            }
            else
            {
                if (ByName.ContainsKey(text))
                    Print(ByName[text].ToString(), false);
                else
                    Print("no found", false);
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
            Read(path);
        }
        private void DragEnter1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All; //重要代码：表明是所有类型的数据，比如文件路径
            else e.Effect = DragDropEffects.None;
        }

        string ParseFileName(string name) => name.Replace(":", "_2_").Replace("/", "_1_");

        private void Output(object sender, EventArgs e)
        {
            OutputBtn.Enabled = false;
            int cnt = 0;
            foreach (JObject jo in All)
            {
                int id = (int?)jo["id"]??0;
                string fileName = ParseFileName($"Item/{id}.json");
                string content = jo.ToString();
                OutputBtn.Text = $"{++cnt} / {Length}\r\n{jo["pagename"]}\r\n{fileName}";
                SaveToFile(fileName, content);
            }
            OutputBtn.Enabled = true;
        }

        void SaveToFile(string fileName, string massage, string folderName = "json")
        {
            string dir = OutputDir;
            if (folderName != "") dir += folderName + @"\";
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
}
