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
using System.Threading;

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

        Task MakeRepeatBattle(int color1, int part1, int color2, int part2, int times)
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
        Task MakeRandBattle(int color, int part, int[][] enemys, int times)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    lock (count)
                    {
                        if (count[0] < times) count[0]++;
                        else break;
                    }
                    var enemy = enemys[Random.Range(0, enemys.Length)];
                    var result = new QuQuBattleSimulation(color, part, enemy[0], enemy[1], weaken).ShowStartBattleState();
                    Interlocked.Increment(ref count[result.win ? 1 : 2]); //线程安全的+1
                }
            });
        }

        int[][] aggregate;
        
        int[] count;

        bool weaken;
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var color1 = Color1.Text.ToInt();
            var part1 = Part1.Text.ToInt();
            var color2 = Color2.Text.ToInt();
            var part2 = Part2.Text.ToInt();
            var times = Times.Text.ToInt();
            weaken = Weaken.IsChecked == true;
            if (times <= 1)
            {
                var sim = new QuQuBattleSimulation(color1, part1, color2, part2, weaken)
                {
                    showDetail = true
                };
                Print(sim.Report());
            }
            else
            {
                BTN.Content = "执行中...";
                BTN.IsEnabled = false;
                aggregate = new int[][] { new int[5], new int[5] };
                var taskCount = Environment.ProcessorCount; //4
                var tasks = new Task[taskCount]; 
                for (int i=0; i < taskCount; i++)
                {
                    tasks[i] = MakeRepeatBattle(color1, part1, color2, part2, times / taskCount);
                }
                await Task.WhenAll(tasks);
                var messages = new string[]
                {
                    $"{DateFile.instance.GetQuquName(color1, part1)} {ReasonAnalysis(aggregate[0])}",
                    $"{DateFile.instance.GetQuquName(color2, part2)} {ReasonAnalysis(aggregate[1])}",
                    ""
                };
                var w = new Writer("test.txt");
                foreach (var message in messages)
                {
                    Print(message);
                    w.WriteLine(message);
                }
                w.Done();
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
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var times = Times.Text.ToInt();
            if (times <= 1)
            {
                times = 10000;
            }
            weaken = Weaken.IsChecked == true;

            BTN.Content = "执行中...";
            BTN.IsEnabled = false;
            var taskCount = Environment.ProcessorCount; //4
            var tasks = new Task[taskCount];

            var w = new Writer("test.txt");
            var cnt = 0;
            for (var mainLevel = 1; mainLevel <= 9; mainLevel++)
            {
                foreach (var mainQuqu in QuquData.instance.GetAll(mainLevel))
                {
                    var rate = "";
                    for (var enemyLevel = 1; enemyLevel <= 9; enemyLevel++)
                    {
                        count = new int[3];
                        for (int i = 0; i < taskCount; i++)
                        {
                            tasks[i] = MakeRandBattle(mainQuqu[0], mainQuqu[1], QuquData.instance.GetAll(enemyLevel), times);
                        }
                        await Task.WhenAll(tasks);
                        rate += $"{count[1] * 10000 / (count[1] + count[2])}|";
                        if (enemyLevel == 1) Print($"{++cnt} {DateFile.instance.GetQuquName(mainQuqu[0], mainQuqu[1])}", false);
                        Print($"{enemyLevel}: {count[1]} / {count[1] + count[2]}"); //百分比，保留一位小数，向下取整
                    }
                    var line = $"{DateFile.instance.GetQuquName(mainQuqu[0], mainQuqu[1])},{mainQuqu[0]},{mainQuqu[1]},{rate}";//csv格式输出
                    //Print(line);
                    w.WriteLine(line);
                }
            }
            w.Done();
            BTN.Content = "开始执行";
            BTN.IsEnabled = true;
            return;
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
            Output.ScrollToEnd();
        }

        public static string GetEnumName(object en) => Enum.GetName(en.GetType(), en);
    }

    
}
