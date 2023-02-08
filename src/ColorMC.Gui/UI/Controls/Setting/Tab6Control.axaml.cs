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
            Window.Info.Show(Localizer.Instance["Error8"]);
            return;
        }
        if (!File.Exists(TextBox3.Text))
        {
            Window.Info.Show(Localizer.Instance["Error9"]);
            return;
        }
        try
        {
            var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
            if (obj == null)
            {
                Window.Info.Show(Localizer.Instance["SettingWindow.Tab6.Error1"]);
                return;
            }

            App.ShowCustom(obj);
        }
        catch (Exception ex)
        {
            CoreMain.OnError?.Invoke(Localizer.Instance["SettingWindow.Tab6.Error2"], ex, false);
        }
    }

    private async void Button6_Click(object? sender, RoutedEventArgs e)
    {
        var str = await BaseBinding.OpSave(Window, Localizer.Instance["SettingWindow.Tab6.Info1"], ".json", "ui.json");
        if (!string.IsNullOrWhiteSpace(str))
        {
            if (File.Exists(str))
            {
                File.Delete(str);
            }

            File.WriteAllBytes(str, BaseBinding.GetUIJson());
        }
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        var file = TextBox3.Text;
        if (string.IsNullOrWhiteSpace(file))
        {
            Window.Info.Show(Localizer.Instance["Error7"]);
            return;
        }

        if (File.Exists(file))
        {
            Window.Info.Show(Localizer.Instance["Error9"]);
            return;
        }

        ConfigBinding.SetUIFile(file);

        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        TextBox3.Text = null;
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog open = new()
        {
            Title = Localizer.Instance["SettingWindow.Tab6.Info2"],
            AllowMultiple = false,
            Filters = new()
            {
                new()
                {
                    Name = Localizer.Instance["SettingWindow.Tab6.Info3"],
                    Extensions = new()
                    {
                        "json"
                    }
                }
            }
        };

        var res = await open.ShowAsync(Window);
        if (res?.Length > 0)
        {
            var file = res[0];
            TextBox3.Text = file;
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetServerCustom(CheckBox3.IsChecked == true,
            ComboBox1.SelectedItem as string);

        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
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
            IP = TextBox1.Text,
            Port = UIUtils.CheckNotNumber(TextBox2.Text) ? 0 : int.Parse(TextBox2.Text!),
            Motd = CheckBox1.IsChecked == true,
            JoinServer = CheckBox2.IsChecked == true,
            MotdColor = ColorPicker1.Color.ToString(),
            MotdBackColor = ColorPicker2.Color.ToString()
        });

        Window.Info2.Show(Localizer.Instance["Info3"]);
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

            ColorPicker1.Color = Colors.MotdColor.ToColor();
            ColorPicker2.Color = Colors.MotdBackColor.ToColor();

            ComboBox1.SelectedItem = config.GameName;

            CheckBox3_Click(null, null);
        }
    }
}
