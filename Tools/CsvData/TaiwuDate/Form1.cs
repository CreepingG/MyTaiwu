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
    public partial class Form1 : Form
    {
        string Path;
        string Content = "";

        public Form1()//初始化
        {
            InitializeComponent();
        }

        string ReadTxtAsCsv(string path)
        {
            try
            {
                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                var lines = new List<string> { };
                string tmp;
                while (true)
                {
                    tmp = sr.ReadLine();
                    if (tmp == null) break;
                    lines.Add(tmp);
                }
                lines[0] = lines[0].Split(',').Select(s => s == "#" ? "id" : "data" + s).Aggregate((s1, s2) => s1 + ',' + s2);
                return lines.Aggregate((s1, s2) => s1 + '\n' + s2);
            }
            catch(IOException e)
            {
                textBox1.Text = e.ToString();
                return "";
            }
        }

        void Read(string path)
        {
            Path = path;
            Content = ReadTxtAsCsv(path);
            var bytes = Encoding.UTF8.GetBytes(Content);
            bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
            Content = Encoding.Unicode.GetString(bytes);
            Print(Content, false);
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) // 运行拖进来的是文件（而不是文本、图片之类的元素）
                e.Effect = DragDropEffects.All; // 会改变拖动时的指针样式
            else e.Effect = DragDropEffects.None;
        }
        
        private void Output(object sender, EventArgs e)
        {
            SaveToFile(Path.Replace(".txt", ".csv"), Content);
        }

        void SaveToFile(string fileName, string massage, string folderName = "")
        {
            if (folderName != "")
            {
                if (!System.IO.Directory.Exists(folderName))//创建目录
                    System.IO.Directory.CreateDirectory(folderName);
                fileName = folderName + @"\" + fileName;
            }
            StreamWriter sw = new StreamWriter(fileName, false, Encoding.Unicode);
            if (sw != null)
            {
                sw.Write(massage);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
