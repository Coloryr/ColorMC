using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using Avalonia.Interactivity;
using ColorMC.Core.Net;
using Avalonia.Input;
using System.Text.RegularExpressions;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab3Control : UserControl
{
    private static Regex re = new("[^0-9]+");
    private SettingWindow Window;
    public Tab3Control()
    {
        InitializeComponent();

        ComboBox1.Items = OtherBinding.GetDownloadSources();
        Button_Set.Click += Button_Set_Click;
    }

    private void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        if (re.IsMatch(TextBox2.Text) || re.IsMatch(Input1.Text))
        {
            Window.Info.Show("输入信息有误");
            return;   
        }
        ConfigBinding.SetHttpConfig(new()
        {
            Source = (SourceLocal)ComboBox1.SelectedIndex,
            DownloadThread = int.Parse(Input1.Text),
            ProxyIP = TextBox1.Text,
            ProxyPort = ushort.Parse(TextBox2.Text),
            ProxyUser = TextBox3.Text,
            ProxyPassword = TextBox4.Text,
            LoginProxy = CheckBox1.IsChecked == true,
            DownloadProxy = CheckBox2.IsChecked == true,
            GameProxy = CheckBox3.IsChecked == true
        });
        Window.Info2.Show("设置完成");
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 != null)
        {
            ComboBox1.SelectedIndex = (int)config.Item1.Http.Source;

            Input1.Value = config.Item1.Http.DownloadThread;

            TextBox1.Text = config.Item1.Http.ProxyIP;
            TextBox2.Text = config.Item1.Http.ProxyPort.ToString();
            TextBox3.Text = config.Item1.Http.ProxyUser;
            TextBox4.Text = config.Item1.Http.ProxyPassword;

            CheckBox1.IsChecked = config.Item1.Http.LoginProxy;
            CheckBox2.IsChecked = config.Item1.Http.DownloadProxy;
            CheckBox3.IsChecked = config.Item1.Http.GameProxy;
        }
    }
}
