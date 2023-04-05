using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using System.Globalization;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab3Control : UserControl
{
    private bool load;
    public Tab3Control()
    {
        InitializeComponent();

        ComboBox1.ItemsSource = BaseBinding.GetDownloadSources();

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;

        Input1.PropertyChanged += Input1_PropertyChanged;

        TextBox1.PropertyChanged += TextBox_PropertyChanged;
        TextBox2.PropertyChanged += TextBox_PropertyChanged;
        TextBox3.PropertyChanged += TextBox_PropertyChanged;
        TextBox4.PropertyChanged += TextBox_PropertyChanged;

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox1_Click;
        CheckBox3.Click += CheckBox1_Click;
        CheckBox4.Click += CheckBox1_Click;
        CheckBox5.Click += CheckBox1_Click;
        CheckBox6.Click += CheckBox1_Click;
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpenPicPath();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (ColorMCGui.Update == null)
        {
            window.Info.Show(App.GetLanguage("SettingWindow.Tab3.Error1"));
            return;
        }

        ColorMCGui.Update();
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (ColorMCGui.Update == null || ColorMCGui.Check == null)
        {
            window.Info.Show(App.GetLanguage("SettingWindow.Tab3.Error2"));
            return;
        }
        await window.Info1.Show(App.GetLanguage("SettingWindow.Tab3.Info1"));
        var res = await ColorMCGui.Check();
        window.Info1.Close();
        if (res.Item1 == null)
        {
            window.Info.Show(App.GetLanguage("SettingWindow.Tab3.Error3"));
            return;
        }
        else if (res.Item1 == true)
        {
            var res1 = await window.Info6.ShowWait(App.GetLanguage("SettingWindow.Tab3.Info2"), res.Item2!);
            if (!res1)
            {
                ColorMCGui.Update();
            }
        }
        else
        {
            window.Info.Show(App.GetLanguage("SettingWindow.Tab3.Info3"));
        }
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        if (load)
            return;

        Save();
    }

    private void TextBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save();
        }
    }

    private void Input1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            Save();
        }
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Save();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpenDownloadPath();
    }

    private void Save()
    {
        if (Funtcions.CheckNotNumber(TextBox2.Text))
        {
            return;
        }
        ConfigBinding.SetHttpConfig(new()
        {
            Source = (SourceLocal)ComboBox1.SelectedIndex,
            DownloadThread = (int)Input1.Value,
            ProxyIP = TextBox1.Text,
            ProxyPort = ushort.Parse(TextBox2.Text),
            ProxyUser = TextBox3.Text,
            ProxyPassword = TextBox4.Text,
            LoginProxy = CheckBox1.IsChecked == true,
            DownloadProxy = CheckBox2.IsChecked == true,
            GameProxy = CheckBox3.IsChecked == true,
            CheckFile = CheckBox4.IsChecked == true,
            CheckUpdate = CheckBox5.IsChecked == true,
            AutoDownload = CheckBox6.IsChecked == true
        });
    }

    public void Lock()
    {
        ComboBox1.IsEnabled = false;
        Input1.IsEnabled = false;
        TextBox1.IsEnabled = false;
        TextBox2.IsEnabled = false;
        TextBox3.IsEnabled = false;
        TextBox4.IsEnabled = false;
        CheckBox1.IsEnabled = false;
        CheckBox2.IsEnabled = false;
        CheckBox3.IsEnabled = false;
        CheckBox4.IsEnabled = false;
    }

    public void Unlock()
    {
        ComboBox1.IsEnabled = true;
        Input1.IsEnabled = true;
        TextBox1.IsEnabled = true;
        TextBox2.IsEnabled = true;
        TextBox3.IsEnabled = true;
        TextBox4.IsEnabled = true;
        CheckBox1.IsEnabled = true;
        CheckBox2.IsEnabled = true;
        CheckBox3.IsEnabled = true;
        CheckBox4.IsEnabled = true;
    }

    public void Load()
    {
        load = true;
        if (BaseBinding.IsDownload)
        {
            Lock();
        }
        else
        {
            Unlock();
        }

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
            CheckBox4.IsChecked = config.Item1.Http.CheckFile;
            CheckBox5.IsChecked = config.Item1.Http.CheckUpdate;
            CheckBox6.IsChecked = config.Item1.Http.AutoDownload;
        }
        load = false;
    }
}
