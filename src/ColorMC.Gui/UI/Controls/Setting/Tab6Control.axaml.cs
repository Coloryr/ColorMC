using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using Avalonia.Interactivity;
using System.Collections.Generic;
using ColorMC.Gui.Utils.LaunchSetting;
using ColorMC.Gui.Objs;
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

        CheckBox3.Click += CheckBox3_Click;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetServerCustom(CheckBox3.IsChecked == true, 
            ComboBox1.SelectedItem as string);

        Window.Info2.Show(Localizer.Instance["SettingWindow.Tab2.Info8"]);
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
            Port = UIUtils.CheckNotNumber(TextBox2.Text) ?  0 : int.Parse(TextBox2.Text!),
            Motd = CheckBox1.IsChecked == true,
            JoinServer = CheckBox2.IsChecked == true,
            MotdColor = ColorPicker1.Color.ToString(),
            MotdBackColor = ColorPicker2.Color.ToString()
        });

        Window.Info2.Show(Localizer.Instance["SettingWindow.Tab2.Info8"]);
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    {
        ComboBox1.Items = from item in GameBinding.GetGames() select item.Name;

        var config = ConfigBinding.GetAllConfig().Item2.ServerCustom;

        if (config != null)
        {
            TextBox1.Text = config.IP;
            TextBox2.Text = config.Port.ToString();

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
