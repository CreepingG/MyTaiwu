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

        /// <summary>记录task中的对战结果</summary>
        int[][] aggregate;
        /// <summary>剩余对战次数</summary>
        int countdown;
        /// <summary>是否反击衰减</summary>
        bool weaken;

        private void FilterInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        /// <summary>让两种指定蛐蛐交战</summary>
        Task MakeSpecifiedBattle(int color1, int part1, int color2, int part2)
        {
            return Task.Run(() =>
            {
                var i = 0;
                while (true)
                {
                    i = Interlocked.Decrement(ref countdown);
                    if (i < 0) break;

                    var result = new QuQuBattleSimulation(color1, part1, color2, part2, weaken).ShowStartBattleState();
                    var winner = result.win ? 0 : 1;

                    Interlocked.Increment(ref aggregate[winner][0]);
                    Interlocked.Increment(ref aggregate[winner][(int)result.status]);
                }
            });
        }

        /// <summary>为一种指定蛐蛐随机选择对手交战</summary>
        Task MakeRandomBattle(int color, int part, int[][] enemys)
        {
            return Task.Run(() =>
            {
                var i = 0;
                while (true)
                {
                    i = Interlocked.Decrement(ref countdown);
                    if (i < 0) break;

                    var enemy = enemys[Random.Range(0, enemys.Length)];
                    var result = new QuQuBattleSimulation(color, part, enemy[0], enemy[1], weaken).ShowStartBattleState();

                    Interlocked.Increment(ref aggregate[0][result.win ? 0 : 1]);
                }
            });
        }
        
        private async void Button_Click_Pair(object sender, RoutedEventArgs e)
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
                countdown = times;
                for (int i=0; i < taskCount; i++)
                {
                    tasks[i] = MakeSpecifiedBattle(color1, part1, color2, part2);
                }
                await Task.WhenAll(tasks);
                var messages = new string[]
                {
                    $"{QuquSystem.instance.GetName(color1, part1)} {ReasonAnalysis(aggregate[0])}",
                    $"{QuquSystem.instance.GetName(color2, part2)} {ReasonAnalysis(aggregate[1])}",
                    ""
                };
                var w = new Writer("logs.txt");
                foreach (var message in messages)
                {
                    Print(message);
                    w.WriteLine(message);
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
        
        private async void Button_Click_All(object sender, RoutedEventArgs e)
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

            var w = new Writer("logs.txt");
            var cnt = 0;
            for (var mainLevel = 1; mainLevel <= 9; mainLevel++)
            {
                foreach (var mainQuqu in QuquSystem.instance.GetAllTypes(mainLevel))
                {
                    var rate = "";
                    for (var enemyLevel = 1; enemyLevel <= 9; enemyLevel++)
                    {
                        countdown = times;
                        aggregate = new int[][] { new int[2] };
                        for (int i = 0; i < taskCount; i++)
                        {
                            tasks[i] = MakeRandomBattle(mainQuqu[0], mainQuqu[1], QuquSystem.instance.GetAllTypes(enemyLevel));
                        }
                        await Task.WhenAll(tasks);
                        var win0 = aggregate[0][0];
                        var win1 = aggregate[0][1];
                        rate += $"{win0 * 10000 / (win0 + win1)}|";
                        if (enemyLevel == 1) Print($"{++cnt} {QuquSystem.instance.GetName(mainQuqu[0], mainQuqu[1])}", false);
                        Print($"{enemyLevel}: {win0} / {win0 + win1}"); //百分比，保留一位小数，向下取整
                    }
                    var line = $"{QuquSystem.instance.GetName(mainQuqu[0], mainQuqu[1])},{mainQuqu[0]},{mainQuqu[1]},{rate}";//csv格式输出
                    //Print(line);
                    w.WriteLine(line);
                }
            }
            BTN.Content = "开始执行";
            BTN.IsEnabled = true;
            return;
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(((TextBox)e.Source).Text, out int i))
            {
                Name1.Content = QuquSystem.instance.GetName(Color1.Text.ToInt(), Part1.Text.ToInt());
                Name2.Content = QuquSystem.instance.GetName(Color2.Text.ToInt(), Part2.Text.ToInt());
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
