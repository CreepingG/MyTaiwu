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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var color1 = Color1.Text.ToInt();
            var part1 = Part1.Text.ToInt();
            var color2 = Color2.Text.ToInt();
            var part2 = Part2.Text.ToInt();
            var times = Times.Text.ToInt();
            if (times <= 1)
            {
                var sim = new QuQuBattleSimulation(color1, part1, color2, part2);
                Test.Print(sim.Report());
            }
            else
            {
                var result = new int[2];
                for(int i = 0; i < times; i++)
                {
                    result[new QuQuBattleSimulation(color1, part1, color2, part2).ShowStartBattleState() - 1]++;
                }
                Test.Print($"{DateFile.instance.GetQuquName(color1, part1)} {result[0]} : {DateFile.instance.GetQuquName(color2, part2)} {result[1]}");
            }
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(((TextBox)e.Source).Text, out int i))
            {
                Name1.Content = DateFile.instance.GetQuquName(Color1.Text.ToInt(), Part1.Text.ToInt());
                Name2.Content = DateFile.instance.GetQuquName(Color2.Text.ToInt(), Part2.Text.ToInt());
            }
        }
    }
}
