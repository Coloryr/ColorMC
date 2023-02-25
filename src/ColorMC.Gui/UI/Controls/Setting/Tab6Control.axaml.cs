using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab6Control : UserControl
{
    private bool load;

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
        var window = (VisualRoot as IBaseWindow)!;
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
            ColorMCCore.OnError?.Invoke(App.GetLanguage("SettingWindow.Tab6.Error2"), ex, false);
        }
    }

    private async void Button6_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;
        var str = await BaseBinding.OpSave(window, App.GetLanguage("SettingWindow.Tab6.Info1"), ".json", "ui.json");
        if (str == null)
            return;

        var file = str.GetPath();

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        File.WriteAllBytes(file, BaseBinding.GetUIJson());
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
        var window = (VisualRoot as SettingWindow)!;
        var res = await BaseBinding.OpFile(window,
            App.GetLanguage("SettingWindow.Tab6.Info2"),
            new string[] { "*.json" },
            App.GetLanguage("SettingWindow.Tab6.Info3"));
        if (res.Any())
        {
            TextBox3.Text = res[0].GetPath();
        }
    }

    private void Save1()
    {
        ConfigBinding.SetServerCustom(CheckBox3.IsChecked == true,
            ComboBox1.SelectedItem as string);
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

        ComboBox1.Items = from item in GameBinding.GetGames() select item.Name;

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

            ComboBox1.SelectedItem = config.GameName;

            Switch();
        }

        load = false;
    }
}
