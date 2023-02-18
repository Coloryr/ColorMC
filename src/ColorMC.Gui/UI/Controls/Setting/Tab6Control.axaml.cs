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
    private SettingWindow Window;

    public Tab6Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;
        Button6.Click += Button6_Click;
        Button7.Click += Button7_Click;

        CheckBox3.Click += CheckBox3_Click;
    }

    private void Button7_Click(object? sender, RoutedEventArgs e)
    {
        var file = TextBox3.Text;
        if (string.IsNullOrWhiteSpace(file))
        {
            Window.Info.Show(App.GetLanguage("Error8"));
            return;
        }
        if (!File.Exists(TextBox3.Text))
        {
            Window.Info.Show(App.GetLanguage("Error9"));
            return;
        }
        try
        {
            var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
            if (obj == null)
            {
                Window.Info.Show(App.GetLanguage("SettingWindow.Tab6.Error1"));
                return;
            }

            App.ShowCustom(obj);
        }
        catch (Exception ex)
        {
            CoreMain.OnError?.Invoke(App.GetLanguage("SettingWindow.Tab6.Error2"), ex, false);
        }
    }

    private async void Button6_Click(object? sender, RoutedEventArgs e)
    {
        var str = await BaseBinding.OpSave(Window, App.GetLanguage("SettingWindow.Tab6.Info1"), ".json", "ui.json");
        if (str == null)
            return;

        var file = str.GetPath();

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        File.WriteAllBytes(file, BaseBinding.GetUIJson());
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        var file = TextBox3.Text;
        if (string.IsNullOrWhiteSpace(file))
        {
            Window.Info.Show(App.GetLanguage("Error7"));
            return;
        }

        if (!File.Exists(file))
        {
            Window.Info.Show(App.GetLanguage("Error9"));
            return;
        }

        ConfigBinding.SetUIFile(file);

        Window.Info2.Show(App.GetLanguage("Info3"));
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        TextBox3.Text = null;
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var res = await BaseBinding.OpFile(Window, App.GetLanguage("SettingWindow.Tab6.Info2"),
            "*.json", App.GetLanguage("SettingWindow.Tab6.Info3"));
        if (res.Any())
        {
            TextBox3.Text = res[0].GetPath();
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetServerCustom(CheckBox3.IsChecked == true,
            ComboBox1.SelectedItem as string);

        Window.Info2.Show(App.GetLanguage("Info3"));
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
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
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

        Window.Info2.Show(App.GetLanguage("Info3"));
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    {
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
    }
}
