using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab2Control : UserControl
{
    private bool load = false;
    public Tab2Control()
    {
        InitializeComponent();

        Button_Delete.Click += Button_Delete_Click;
        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Set.Click += Button_Set_Click;
        Button_Set2.Click += Button_Set2_Click;
        Button_Set4.Click += Button_Set4_Click;
        Button_Set5.Click += Button_Set5_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox3_SelectionChanged;

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox3_Click;
        CheckBox4.Click += CheckBox4_Click;

        ColorPicker1.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker2.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker3.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker4.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker5.ColorChanged += ColorPicker_ColorChanged;

        ComboBox1.Items = BaseBinding.GetWindowTranTypes();
        ComboBox2.Items = BaseBinding.GetLanguages();
        ComboBox3.Items = BaseBinding.GetFontList();

        Input1.PropertyChanged += Input1_PropertyChanged;
    }

    private void Input1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            Save2();
        }
    }

    private void CheckBox4_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox4.IsChecked == true)
        {
            Input1.IsEnabled = true;
        }
        else
        {
            Input1.IsEnabled = false;
        }

        Save2();
    }

    private void Save2()
    {
        ConfigBinding.SetRadius(CheckBox4.IsChecked == true, (float)Input1.Value);
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Save1();
    }

    private void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        if (load)
            return;

        ConfigBinding.SetColor(ColorPicker1.Color.ToString(),
            ColorPicker2.Color.ToString(), ColorPicker3.Color.ToString(),
            ColorPicker4.Color.ToString(), ColorPicker5.Color.ToString());
    }

    private void ComboBox3_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        ConfigBinding.SetFont(ComboBox3.SelectedItem as string, CheckBox3.IsChecked == true);
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        var window = (VisualRoot as SettingWindow)!;
        var type = (LanguageType)ComboBox2.SelectedIndex;
        window.Info1.Show(App.GetLanguage("SettingWindow.Tab2.Info1"));
        LanguageHelper.Change(type);
        window.Info1.Close();
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox3.IsChecked == true)
        {
            ComboBox3.IsEnabled = false;
        }
        else
        {
            ComboBox3.IsEnabled = true;
        }

        ConfigBinding.SetFont(ComboBox3.SelectedItem as string, CheckBox3.IsChecked == true);
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
        var window = (VisualRoot as SettingWindow)!;
        ConfigBinding.ResetColor();
        ColorPicker1.Color = ColorSel.MainColor.ToColor();
        ColorPicker2.Color = ColorSel.BackColor.ToColor();
        ColorPicker3.Color = ColorSel.Back1Color.ToColor();
        ColorPicker4.Color = ColorSel.ButtonFont.ToColor();
        ColorPicker5.Color = ColorSel.FontColor.ToColor();
        window.Info2.Show(App.GetLanguage("SettingWindow.Tab2.Info4"));
    }

    private async void Button_Set2_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;
        if (string.IsNullOrWhiteSpace(TextBox1.Text))
        {
            window.Info.Show(App.GetLanguage("SettingWindow.Tab2.Error1"));
            return;
        }
        window.Info1.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackPic(TextBox1.Text, (int)Slider1.Value);
        window.Info1.Close();

        window.Info2.Show(App.GetLanguage("Info3"));
    }

    private void Save1()
    {
        if (load)
            return;

        var window = (VisualRoot as SettingWindow)!;
        window.Info1.Show(App.GetLanguage("SettingWindow.Tab2.Info5"));
        ConfigBinding.SetBl(CheckBox1.IsChecked == true, ComboBox1.SelectedIndex);
        window.Info1.Close();
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        ComboBox1.IsEnabled = CheckBox1.IsChecked == true;

        Save1();
    }

    public void Load()
    {
        load = true;

        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null)
        {
            Input1.Value = (decimal?)config.Item2.Radius;
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
            CheckBox4.IsChecked = config.Item2.CornerRadius;
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
            if (CheckBox4.IsChecked == true)
            {
                Input1.IsEnabled = true;
            }
            else
            {
                Input1.IsEnabled = false;
            }
        }
        if (config.Item1 != null)
        {
            ComboBox2.SelectedIndex = (int)config.Item1.Language;
        }

        load = false;
    }

    private void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;
        ConfigBinding.SetBackTran((int)Slider2.Value);
        window.Info2.Show(App.GetLanguage("Info3"));
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;
        var file = await BaseBinding.OpFile(window, FileType.Pic);

        if (file != null)
        {
            TextBox1.Text = file;

            Button_Set2_Click(sender, e);
        }
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        TextBox1.Text = "";

        ConfigBinding.DeleteGuiImageConfig();
    }
}
