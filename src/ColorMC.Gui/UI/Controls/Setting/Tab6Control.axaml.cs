using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab6Control : UserControl
{
    private bool load;
    private List<string> uuids = new();

    public Tab6Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button8.Click += Button8_Click;
        Button9.Click += Button9_Click;
        Button10.Click += Button10_Click;

        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button6.Click += Button6_Click;
        Button7.Click += Button7_Click;

        CheckBox3.Click += CheckBox3_Click;
        CheckBox4.Click += CheckBox3_Click;
        CheckBox5.Click += CheckBox3_Click;
        CheckBox6.Click += CheckBox3_Click;

        TextBox1.PropertyChanged += TextBox_PropertyChanged;
        TextBox2.PropertyChanged += TextBox_PropertyChanged;
        TextBox3.PropertyChanged += TextBox_PropertyChanged;
        TextBox4.PropertyChanged += TextBox_PropertyChanged;
        TextBox5.PropertyChanged += TextBox_PropertyChanged;

        TextBox4.LostFocus += TextBox4_LostFocus;

        CheckBox1.Click += CheckBox_Click;
        CheckBox2.Click += CheckBox_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        Slider1.PropertyChanged += Slider1_PropertyChanged;
    }

    private async void Button10_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.OpFile(window, FileType.Music);
        if (file == null)
        {
            return;
        }

        TextBox5.Text = file;
    }

    private void Button9_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.MusicPlay();
    }

    private void Button8_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.MusicPause();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.MusicStart();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.MusicStop();
    }

    private void Slider1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            Label1.Content = $"{Slider1.Value:0}%";

            if (!load)
            {
                Save();
            }

            BaseBinding.SetVolume((int)Slider1.Value);
        }
    }

    private void TextBox4_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextBox4.Text))
            return;

        if (TextBox4.Text.EndsWith("/"))
            return;

        TextBox4.Text += "/";
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Save();
    }

    private void CheckBox_Click(object? sender, RoutedEventArgs e)
    {
        Save();

        App.MainWindow?.MotdLoad();
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

    private void Button7_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (string.IsNullOrWhiteSpace(TextBox3.Text))
        {
            window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
            return;
        }
        var file = TextBox3.Text;
        if (!File.Exists(file))
        {
            file = BaseBinding.GetRunDir() + TextBox3.Text;
            if (!File.Exists(file))
            {
                window.OkInfo.Show(App.GetLanguage("Gui.Error9"));
                return;
            }
        }
        try
        {
            var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
            if (obj == null)
            {
                window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab6.Error1"));
                return;
            }

            App.ShowCustom(obj);
        }
        catch (Exception ex)
        {
            var data = App.GetLanguage("SettingWindow.Tab6.Error2");
            Logs.Error(data, ex);
            App.ShowError(data, ex);
        }
    }

    private async void Button6_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var str = await BaseBinding.SaveFile(window, FileType.UI, null);
        if (str == null)
            return;

        if (str == false)
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab6.Error3"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab6.Info4"));
        return;
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        TextBox3.Text = null;

        Save();
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await BaseBinding.OpFile(window, FileType.UI);
        if (res != null)
        {
            TextBox3.Text = res;
        }
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        Switch();

        Save();
    }

    private void Switch()
    {
        if (CheckBox3.IsChecked == true)
        {
            ComboBox1.IsEnabled = true;
            CheckBox4.IsEnabled = true;
            TextBox4.IsEnabled = true;
        }
        else
        {
            ComboBox1.IsEnabled = false;
            CheckBox4.IsEnabled = false;
            TextBox4.IsEnabled = false;
        }

        Button1.IsEnabled = Button8.IsEnabled = Button9.IsEnabled = Button2.IsEnabled =
        Button10.IsEnabled = CheckBox6.IsEnabled = TextBox5.IsEnabled = Slider1.IsEnabled =
            CheckBox5.IsChecked == true;
    }

    private void Save()
    {
        var obj = new ServerCustom()
        {
            IP = TextBox1.Text,
            Port = Funtcions.CheckNotNumber(TextBox2.Text) ? 0 : int.Parse(TextBox2.Text!),
            Motd = CheckBox1.IsChecked == true,
            JoinServer = CheckBox2.IsChecked == true,
            MotdColor = ColorPicker1.Color.ToString(),
            MotdBackColor = ColorPicker2.Color.ToString(),
            ServerPack = CheckBox4.IsChecked == true,
            ServerUrl = TextBox4.Text,
            LockGame = CheckBox3.IsChecked == true,
            UIFile = TextBox3.Text,
            PlayMusic = CheckBox5.IsChecked == true,
            Music = TextBox5.Text,
            Volume = (int)Slider1.Value,
            SlowVolume = CheckBox6.IsChecked == true,
        };

        if (ComboBox1.SelectedIndex == -1)
        {
            obj.GameName = null;
        }
        else
        {
            obj.GameName = uuids[ComboBox1.SelectedIndex];
        }

        ConfigBinding.SetServerCustom(obj);
    }

    public void Load()
    {
        load = true;

        var list = from item in GameBinding.GetGames() select (item.UUID, item.Name);
        var list1 = new List<string>();

        uuids.Clear();
        foreach (var item in list)
        {
            list1.Add(item.Name);
            uuids.Add(item.UUID);
        }

        ComboBox1.ItemsSource = list1;

        var config = ConfigBinding.GetAllConfig().Item2?.ServerCustom;

        if (config != null)
        {
            TextBox1.Text = config.IP;
            TextBox2.Text = config.Port.ToString();
            TextBox3.Text = config.UIFile;
            TextBox4.Text = config.ServerUrl;
            TextBox5.Text = config.Music;

            CheckBox1.IsChecked = config.Motd;
            CheckBox2.IsChecked = config.JoinServer;
            CheckBox3.IsChecked = config.LockGame;
            CheckBox4.IsChecked = config.ServerPack;
            CheckBox5.IsChecked = config.PlayMusic;
            CheckBox6.IsChecked = config.SlowVolume;

            ColorPicker1.Color = ColorSel.MotdColor.ToColor();
            ColorPicker2.Color = ColorSel.MotdBackColor.ToColor();

            ComboBox1.SelectedIndex = uuids.IndexOf(config.GameName);

            Slider1.Value = config.Volume;

            if (config.Volume == 0)
            {
                Label1.Content = "0%";
            }

            Switch();
        }

        load = false;
    }
}
