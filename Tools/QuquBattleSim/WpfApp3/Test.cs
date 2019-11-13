using System.Windows;
using WpfApp3;

static class Test
{
    public static MainWindow Window => (MainWindow)Application.Current.MainWindow;
    public static void Print(object o, bool add = true)
    {
        if (!add) Window.output.Text = "";
        Window.output.Text += o.ToString() + '\n';
    }
    public static bool Weaken => Window.Weaken.IsChecked == true;
}
