using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WpfApp3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void FilterInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        Task Work(int color1, int part1, int color2, int part2, bool weaken, int times)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < times; i++)
                {
                    var result = new QuQuBattleSimulation(color1, part1, color2, part2, weaken).ShowStartBattleState();
                    var winner = result.win ? 0 : 1;
                    lock(aggregate)
                    {
                        aggregate[winner][0]++;
                        aggregate[winner][(int)result.status]++;
                    }
                }
            });
        }

        List<int[]> aggregate;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var color1 = Color1.Text.ToInt();
            var part1 = Part1.Text.ToInt();
            var color2 = Color2.Text.ToInt();
            var part2 = Part2.Text.ToInt();
            var times = Times.Text.ToInt();
            var weaken = Weaken.IsChecked == true;
            if (times <= 1)
            {
                var sim = new QuQuBattleSimulation(color1, part1, color2, part2, weaken);
                Print(sim.Report());
            }
            else
            {
                BTN.Content = "执行中...";
                BTN.IsEnabled = false;
                aggregate = new List<int[]> { new int[5], new int[5] };
                var taskCount = Environment.ProcessorCount; //4
                var tasks = new Task[taskCount]; 
                for (int i=0; i < taskCount; i++)
                {
                    tasks[i] = Work(color1, part1, color2, part2, weaken, times / taskCount);
                }
                await Task.WhenAll(tasks);
                var messages = new string[]
                {
                    $"{DateFile.instance.GetQuquName(color1, part1)} {ReasonAnalysis(aggregate[0])}",
                    $"{DateFile.instance.GetQuquName(color2, part2)} {ReasonAnalysis(aggregate[1])}",
                    ""
                };
                foreach (var message in messages)
                {
                    Print(message);
                }
                BTN.Content = "开始执行";
                BTN.IsEnabled = true;
            }
        }
        private string ReasonAnalysis(int[] data)
        {
            if (data[0] == 0)
            {
                return "0";
            }
            var s = "";
            for(int i = 1; i < 5; i++)
            {
                if (data[i] > 0)
                {
                    if (s != "") s += " ";
                    s += $"{string.Format("{0:F1}", data[i] * 100.0 / data[0])}%{GetEnumName((QuquBattler.BattlerStatus)i)}";
                }
            }
            return $"{data[0]}({s})";
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(((TextBox)e.Source).Text, out int i))
            {
                Name1.Content = DateFile.instance.GetQuquName(Color1.Text.ToInt(), Part1.Text.ToInt());
                Name2.Content = DateFile.instance.GetQuquName(Color2.Text.ToInt(), Part2.Text.ToInt());
            }
        }

        public void Print(object o, bool add = true)
        {
            if (!add) Output.Text = "";
            Output.Text += o.ToString() + '\n';
        }
        public static string GetEnumName(object en) => Enum.GetName(en.GetType(), en);
    }

    
}
