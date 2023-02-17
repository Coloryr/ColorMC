using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab2Control : UserControl
{
    private SettingWindow Window;
    public Tab2Control()
    {
        InitializeComponent();

        Button_Delete.Click += Button_Delete_Click;
        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Set.Click += Button_Set_Click;
        Button_Set1.Click += Button_Set1_Click;
        Button_Set2.Click += Button_Set2_Click;
        Button_Set3.Click += Button_Set3_Click;
        Button_Set4.Click += Button_Set4_Click;
        Button_Set5.Click += Button_Set5_Click;
        Button_Change.Click += Button_Change_Click;
        Button_Change1.Click += Button_Change1_Click;

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox3_Click;

        ComboBox1.Items = BaseBinding.GetWindowTranTypes();
        ComboBox2.Items = BaseBinding.GetLanguages();
        ComboBox3.Items = BaseBinding.GetFontList();
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox3.IsChecked == true)
        {
            ComboBox3.IsEnabled = false;
            ConfigBinding.SetFont(ComboBox3.SelectedItem as string, true);
        }
        else
        {
            ComboBox3.IsEnabled = true;
        }
    }

    private void Button_Change1_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetFont(ComboBox3.SelectedItem as string, CheckBox3.IsChecked == true);
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button_Set5_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetRgb((int)Slider3.Value, (int)Slider4.Value);
    }

    private void CheckBox2_Click(object? sender, RoutedEventArgs e)
    {
        var enable = CheckBox2.IsChecked == true;
        if (enable)
        {
            Slider3.IsEnabled = Slider4.IsEnabled = true;
        }
        else
        {
            Slider3.IsEnabled = Slider4.IsEnabled = false;
        }

        ConfigBinding.SetRgb(enable);
    }

    private void Button_Set4_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetColor("#FF5ABED6", "#FFF4F4F5", "#88FFFFFF", "#FFFFFFFF", "#FF000000");
        ColorPicker1.Color = ColorSel.MainColor.ToColor();
        ColorPicker2.Color = ColorSel.BackColor.ToColor();
        ColorPicker3.Color = ColorSel.Back1Color.ToColor();
        ColorPicker4.Color = ColorSel.ButtonFont.ToColor();
        ColorPicker5.Color = ColorSel.FontColor.ToColor();
        Window.Info2.Show(Localizer.Instance["SettingWindow.Tab2.Info4"]);
    }

    private void Button_Set3_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetColor(ColorPicker1.Color.ToString(),
            ColorPicker2.Color.ToString(), ColorPicker3.Color.ToString(),
            ColorPicker4.Color.ToString(), ColorPicker5.Color.ToString());
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private async void Button_Set2_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextBox1.Text))
        {
            Window.Info.Show(Localizer.Instance["SettingWindow.Tab2.Error1"]);
            return;
        }
        Window.Info1.Show(Localizer.Instance["SettingWindow.Tab2.Info2"]);
        await ConfigBinding.SetBackPic(TextBox1.Text, (int)Slider1.Value);
        Window.Info1.Close();

        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button_Set1_Click(object? sender, RoutedEventArgs e)
    {
        Window.Info1.Show(Localizer.Instance["SettingWindow.Tab2.Info5"]);
        ConfigBinding.SetBl(CheckBox1.IsChecked == true, ComboBox1.SelectedIndex);
        Window.Info1.Close();

        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button_Change_Click(object? sender, RoutedEventArgs e)
    {
        var type = (LanguageType)ComboBox2.SelectedIndex;
        Window.Info1.Show(Localizer.Instance["SettingWindow.Tab2.Info1"]);
        LanguageHelper.Change(type);
        Window.Info1.Close();
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        ComboBox1.IsEnabled = CheckBox1.IsChecked == true;
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null)
        {
            TextBox1.Text = config.Item2.BackImage;
            Slider1.Value = config.Item2.BackEffect;
            Slider2.Value = config.Item2.BackTran;
            Slider3.Value = config.Item2.RGBS;
            Slider4.Value = config.Item2.RGBV;
            CheckBox1.IsChecked = config.Item2.WindowTran;
            ComboBox1.SelectedIndex = config.Item2.WindowTranType;
            ComboBox3.SelectedItem = config.Item2.FontName;
            ColorPicker1.Color = ColorSel.MainColor.ToColor();
            ColorPicker2.Color = ColorSel.BackColor.ToColor();
            ColorPicker3.Color = ColorSel.Back1Color.ToColor();
            ColorPicker4.Color = ColorSel.ButtonFont.ToColor();
            ColorPicker5.Color = ColorSel.FontColor.ToColor();
            CheckBox2.IsChecked = config.Item2.RGB;
            CheckBox3.IsChecked = config.Item2.FontDefault;
            if (config.Item2.WindowTran)
            {
                ComboBox1.IsEnabled = true;
            }
            else
            {
                ComboBox1.IsEnabled = false;
            }
            if (config.Item2.RGB)
            {
                Slider3.IsEnabled = Slider4.IsEnabled = true;
            }
            else
            {
                Slider3.IsEnabled = Slider4.IsEnabled = false;
            }
            if (CheckBox3.IsChecked == true)
            {
                ComboBox3.IsEnabled = false;
            }
            else
            {
                ComboBox3.IsEnabled = true;
            }
        }
        if (config.Item1 != null)
        {
            ComboBox2.SelectedIndex = (int)config.Item1.Language;
        }
    }

    private void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetBackTran((int)Slider2.Value);
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var file = await Window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = Localizer.Instance["SettingWindow.Tab2.Info3"],
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new FilePickerFileType(Localizer.Instance["SettingWindow.Tab2.Info6"])
                {
                    Patterns = new List<string>()
                    {
                        "*.png",
                        "*.jpg",
                        "*.bmp"
                    }
                }
            }
        });

        if (file?.Any() == true)
        {
            var item = file[0];
            TextBox1.Text = item.GetPath();

            Button_Set2_Click(sender, e);
        }
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        TextBox1.Text = "";

        ConfigBinding.DeleteGuiImageConfig();
    }
}
