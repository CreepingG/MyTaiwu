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

namespace TaiwuDate
{
    public partial class RootForm : Form
    {
        Dictionary<int, JObject> ById;
        Dictionary<string, JObject> ByName;
        JObject[] All;
        string SourcePath;
        int Length = 0;
        string ConfigFile = "path.txt";
        bool Inited = false;

        public RootForm()//初始化
        {
            InitializeComponent();
            if (System.IO.File.Exists(ConfigFile))
            {
                StreamReader sr = new StreamReader(ConfigFile, Encoding.Default);
                var s = sr.ReadLine();
                if (!string.IsNullOrEmpty(s))
                {
                    OutputPath.Text = s;
                }
                sr.Dispose();
            }
            Inited = true;
        }
        
        string ItemName(JObject jo)
        {
            string name = (string)jo["0"];
            int id = (int?)jo["id"] ?? 0;
            name = name.Replace("\n", "·").Replace("《", "").Replace("》", "");
            if (name == "血露")
            {
                int level = (int?)jo["8"] ?? 0;
                string gradeText = "下·九品|中·八品|上·七品|奇·六品|秘·五品|极·四品|超·三品|绝·二品|神·一品".Split('|')[level - 1].Split(new string[] { "·" }, StringSplitOptions.RemoveEmptyEntries)[1];
                name += $"({gradeText})";
            }
            else if ((int?)jo["4"] == 5) //图书
            {
                if (id == 5005) //义父的天枢玄机
                {
                    name += "(剧情)";
                }
                else
                {
                    if ((int?)jo["31"] == 17) name += (int?)jo["35"] == 1 ? "(手抄)" : "(真传)";
                }
            }
            else if ((int?)jo["5"] == 36)//神兵
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
                    keys[col] = rawKey;
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
                for (int col = 0; col < line.Length; col++)
                {
                    var value = line[col];
                    var key = keys[col];
                    if (SourcePath.Contains("GongFa_Date") && (key.ToInt() ?? 0) / 100 == 2) continue; //功法的[200+]用于记录临时数据
                    if (int.TryParse(value, out var intValue))
                    {
                        if (intValue != 0 || col == 0) jo.Add(key, intValue); //id例外
                    }
                    else if (double.TryParse(value, out var floatValue))
                    {
                        if (floatValue != .0) jo.Add(key, floatValue);
                    }
                    else
                    {
                        if (value != "Null" && value != "") jo.Add(key, value);
                    }
                }
                string name = SourcePath.Contains("Item_Date") ? ItemName(jo) : (string)jo["0"];
                if (SourcePath.Contains("Item_Date"))
                {
                    jo.Add("pageName", name);
                }
                All[row - 1] = jo;
                ById[id] = jo;
                if (!string.IsNullOrEmpty(name)) ByName[name] = jo;
            }
            return;
        }

        void Read(string path)
        {
            SourcePath = path;
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
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Read(path);
        }
        private void DragEnter1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) // 允许拖进来的是文件（而不是文本、图片之类的元素）
                e.Effect = DragDropEffects.All; // 会改变拖动时的指针样式
            else e.Effect = DragDropEffects.None;
        }

        string ParseFileName(string name) => name.Replace(":", "_2_").Replace("/", "_1_");

        private void Output(object sender, EventArgs e)
        {
            OutputBtn.Enabled = false;
            int cnt = 0;
            Print("正在输出...", false);
            foreach (JObject jo in All)
            {
                int id = (int?)jo["id"]??0;
                string fileName = ParseFileName($"{Path.GetFileNameWithoutExtension(SourcePath).Split('_')[0]}/{id}.json");
                string content = jo.ToString();
                if (!SaveToFile(fileName, content))
                {
                    Print("输出异常中断", false);
                    goto end;
                }
                cnt++;
            }
            Print("输出完成:" + cnt.ToString(), false);
            end:
            OutputBtn.Enabled = true;
        }

        bool SaveToFile(string fileName, string massage, bool usePath = true)
        {
            if (usePath)
            {
                var path = OutputPath.Text;
                try
                {
                    if (!System.IO.Directory.Exists(path))//创建目录
                        System.IO.Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "异常", MessageBoxButtons.OK);
                    return false;
                }

                if (!path.EndsWith("\\")) path += "\\";
                fileName = path + fileName;
            }
            FileStream file = new FileStream(fileName, FileMode.Create); //输出到exe所在目录
            if (file != null)
            {
                byte[] data = Encoding.Default.GetBytes(massage);
                file.Write(data, 0, data.Length);
                file.Flush();
                file.Close();
            }
            return true;
        }

        private void ChooseFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = OutputPath.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                OutputPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void OutputPath_TextChanged(object sender, EventArgs e)
        {
            if (!Inited) return;
            SaveToFile(ConfigFile, OutputPath.Text, false);
        }
    }

    public static class Util
    {
        public static int? ToInt(this string s)
        {
            if(int.TryParse(s, out int n)){
                return n;
            }
            return null;
        }
    }
}
