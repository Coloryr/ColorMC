using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
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

        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button6.Click += Button6_Click;
        Button7.Click += Button7_Click;

        CheckBox3.Click += CheckBox3_Click;

        TextBox1.PropertyChanged += TextBox_PropertyChanged;
        TextBox2.PropertyChanged += TextBox_PropertyChanged;

        TextBox3.PropertyChanged += TextBox3_PropertyChanged;

        CheckBox1.Click += CheckBox_Click;
        CheckBox2.Click += CheckBox_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
    }

    private void TextBox3_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {

        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save2();
        }
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Save1();
    }

    private void CheckBox_Click(object? sender, RoutedEventArgs e)
    {
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

    private void Button7_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = TextBox3.Text;
        if (string.IsNullOrWhiteSpace(file))
        {
            window.Info.Show(App.GetLanguage("Error8"));
            return;
        }
        if (!File.Exists(TextBox3.Text))
        {
            window.Info.Show(App.GetLanguage("Error9"));
            return;
        }
        try
        {
            var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
            if (obj == null)
            {
                window.Info.Show(App.GetLanguage("SettingWindow.Tab6.Error1"));
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
            window.Info.Show(App.GetLanguage("SettingWindow.Tab6.Error3"));
            return;
        }

        window.Info2.Show(App.GetLanguage("SettingWindow.Tab6.Info4"));
        return;
    }

    private void Save2()
    {
        ConfigBinding.SetUIFile(TextBox3.Text);
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        TextBox3.Text = null;

        Save2();
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

    private void Save1()
    {
        if (load)
            return;

        if (ComboBox1.SelectedIndex == -1)
        {
            ConfigBinding.SetServerCustom(CheckBox3.IsChecked == true, null);
            return;
        }

        ConfigBinding.SetServerCustom(CheckBox3.IsChecked == true,
            uuids[ComboBox1.SelectedIndex]);
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        Switch();
    }

    private void Switch()
    {
        if (CheckBox3.IsChecked == true)
        {
            ComboBox1.IsEnabled = true;
        }
        else
        {
            ComboBox1.IsEnabled = false;
        }

        Save1();
    }

    private void Save()
    {
        ConfigBinding.SetServerCustom(new ServerCustom()
        {
            IP = TextBox1.Text!,
            Port = UIUtils.CheckNotNumber(TextBox2.Text) ? 0 : int.Parse(TextBox2.Text!),
            Motd = CheckBox1.IsChecked == true,
            JoinServer = CheckBox2.IsChecked == true,
            MotdColor = ColorPicker1.Color.ToString(),
            MotdBackColor = ColorPicker2.Color.ToString()
        });
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

        ComboBox1.Items = list1;

        var config = ConfigBinding.GetAllConfig().Item2?.ServerCustom;

        if (config != null)
        {
            TextBox1.Text = config.IP;
            TextBox2.Text = config.Port.ToString();
            TextBox3.Text = config.UIFile;

            CheckBox1.IsChecked = config.Motd;
            CheckBox2.IsChecked = config.JoinServer;
            CheckBox3.IsChecked = config.LockGame;

            ColorPicker1.Color = ColorSel.MotdColor.ToColor();
            ColorPicker2.Color = ColorSel.MotdBackColor.ToColor();

            ComboBox1.SelectedIndex = uuids.IndexOf(config.GameName);

            Switch();
        }

        load = false;
    }
}
