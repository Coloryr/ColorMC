using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Setting;

public class FontDisplay
{
    public string FontName { get; init; }
    public FontFamily FontFamily { get; init; }

    public override string ToString()
    {
        return FontName;
    }
}

public partial class Tab2Control : UserControl
{
    private bool load = false;
    private ObservableCollection<FontDisplay> Fonts = new();

    public Tab2Control()
    {
        InitializeComponent();

        Button_Delete.Click += Button_Delete_Click;
        Button_Use.Click += Button_Use_Click;
        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Set.Click += Button_Set_Click;
        Button_Set2.Click += Button_Set2_Click;
        Button_Set3.Click += Button_Set3_Click;
        Button_Set4.Click += Button_Set4_Click;
        Button_Set5.Click += Button_Set5_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;

        ListBox1.SelectionChanged += ListBox1_SelectionChanged;

        RadioButton1.IsCheckedChanged += RadioButton1_IsCheckedChanged;
        RadioButton2.IsCheckedChanged += RadioButton2_IsCheckedChanged;
        RadioButton3.IsCheckedChanged += RadioButton3_IsCheckedChanged;

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox3_Click;
        CheckBox5.Click += CheckBox5_Click;
        CheckBox7.Click += CheckBox7_Click;

        ColorPicker1.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker2.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker3.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker4.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker5.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker6.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker7.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker8.ColorChanged += ColorPicker_ColorChanged;
        ColorPicker9.ColorChanged += ColorPicker_ColorChanged;

        ComboBox1.ItemsSource = BaseBinding.GetWindowTranTypes();
        ComboBox2.ItemsSource = BaseBinding.GetLanguages();

        if (SystemInfo.Os == OsType.Linux)
        {
            CheckBox5.IsEnabled = false;
        }

        BaseBinding.GetFontList().ForEach(item =>
        {
            Fonts.Add(new()
            {
                FontName = item.Name,
                FontFamily = item
            });
        });

        ListBox1.ItemsSource = Fonts;

        Slider5.PropertyChanged += Slider5_PropertyChanged;
    }

    private void RadioButton3_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (load)
            return;

        if (RadioButton3.IsChecked == true)
        {
            ConfigBinding.SetColorType(ColorType.Dark);
        }
    }

    private void RadioButton2_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (load)
            return;

        if (RadioButton2.IsChecked == true)
        {
            ConfigBinding.SetColorType(ColorType.Light);
        }
    }

    private void RadioButton1_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (load)
            return;

        if (RadioButton1.IsChecked == true)
        {
            ConfigBinding.SetColorType(ColorType.Auto);
        }
    }


    private void ListBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        if (ListBox1.SelectedItem is not FontDisplay item)
            return;

        DropDownButton1.Flyout?.Hide();
        DropDownButton1.Content = item;

        ConfigBinding.SetFont(item.FontName, CheckBox3.IsChecked == true);
    }

    private void Button_Use_Click(object? sender, RoutedEventArgs e)
    {
        Button_Set2_Click(sender, e);
    }

    private async void CheckBox7_Click(object? sender, RoutedEventArgs e)
    {
        Slider5.IsEnabled = Button_Set3.IsEnabled = CheckBox7.IsChecked == true;
        if (CheckBox7.IsChecked == true)
        {
            var window = App.FindRoot(VisualRoot);
            window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
            await ConfigBinding.SetBackLimit(CheckBox7.IsChecked == true, (int)Slider5.Value);
            window.ProgressInfo.Close();
        }
    }

    private async void Button_Set3_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackLimit(CheckBox7.IsChecked == true, (int)Slider5.Value);
        window.ProgressInfo.Close();

        window.NotifyInfo.Show(App.GetLanguage("Gui.Info3"));
    }

    private void Slider5_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            Label1.Content = $"{Slider5.Value:0}%";
        }
    }

    private void CheckBox5_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetWindowMode(CheckBox5.IsChecked == true);
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
            ColorPicker4.Color.ToString(), ColorPicker5.Color.ToString(),
            ColorPicker6.Color.ToString(), ColorPicker7.Color.ToString(),
            ColorPicker8.Color.ToString(), ColorPicker9.Color.ToString());
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        var window = App.FindRoot(VisualRoot);
        var type = (LanguageType)ComboBox2.SelectedIndex;
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info1"));
        ConfigBinding.SetLanguage(type);
        window.ProgressInfo.Close();
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox3.IsChecked == true)
        {
            DropDownButton1.IsEnabled = false;
        }
        else
        {
            DropDownButton1.IsEnabled = true;
        }

        ConfigBinding.SetFont((DropDownButton1.Content as FontDisplay)?.FontName,
            CheckBox3.IsChecked == true);
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
        var window = App.FindRoot(VisualRoot);
        load = true;
        ConfigBinding.ResetColor();
        ColorPicker1.Color = ColorSel.MainColor.ToColor();
        ColorPicker2.Color = Color.Parse(ColorSel.BackLigthColorStr);
        ColorPicker3.Color = Color.Parse(ColorSel.Back1LigthColorStr);
        ColorPicker4.Color = Color.Parse(ColorSel.ButtonLightFontStr);
        ColorPicker5.Color = Color.Parse(ColorSel.FontLigthColorStr);
        ColorPicker6.Color = Color.Parse(ColorSel.BackDarkColorStr);
        ColorPicker7.Color = Color.Parse(ColorSel.Back1DarkColorStr);
        ColorPicker8.Color = Color.Parse(ColorSel.ButtonDarkFontStr);
        ColorPicker9.Color = Color.Parse(ColorSel.FontDarkColorStr);
        load = false;
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info4"));
    }

    private async void Button_Set2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (string.IsNullOrWhiteSpace(TextBox1.Text))
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab2.Error1"));
            return;
        }
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackPic(TextBox1.Text, (int)Slider1.Value);
        window.ProgressInfo.Close();

        window.NotifyInfo.Show(App.GetLanguage("Gui.Info3"));
    }

    private void Save1()
    {
        if (load)
            return;

        var window = App.FindRoot(VisualRoot);
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info5"));
        ConfigBinding.SetWindowTran(CheckBox1.IsChecked == true, ComboBox1.SelectedIndex);
        window.ProgressInfo.Close();
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
            TextBox1.Text = config.Item2.BackImage;
            Slider1.Value = config.Item2.BackEffect;
            Slider2.Value = config.Item2.BackTran;
            Slider3.Value = config.Item2.RGBS;
            Slider4.Value = config.Item2.RGBV;
            Slider5.Value = config.Item2.BackLimitValue;
            CheckBox1.IsChecked = config.Item2.WindowTran;
            ComboBox1.SelectedIndex = config.Item2.WindowTranType;

            var item = Fonts.FirstOrDefault(a => a.FontName == config.Item2.FontName);
            ListBox1.SelectedItem = item;
            DropDownButton1.Content = item;

            switch (config.Item2.ColorType)
            {
                case ColorType.Auto:
                    RadioButton1.IsChecked = true;
                    break;
                case ColorType.Light:
                    RadioButton2.IsChecked = true;
                    break;
                case ColorType.Dark:
                    RadioButton3.IsChecked = true;
                    break;
            }
            ColorPicker1.Color = Color.Parse(config.Item2.ColorMain);
            ColorPicker2.Color = Color.Parse(config.Item2.ColorLight.ColorBack);
            ColorPicker3.Color = Color.Parse(config.Item2.ColorLight.ColorTranBack);
            ColorPicker4.Color = Color.Parse(config.Item2.ColorLight.ColorFont1);
            ColorPicker5.Color = Color.Parse(config.Item2.ColorLight.ColorFont2);
            ColorPicker6.Color = Color.Parse(config.Item2.ColorDark.ColorBack);
            ColorPicker7.Color = Color.Parse(config.Item2.ColorDark.ColorTranBack);
            ColorPicker8.Color = Color.Parse(config.Item2.ColorDark.ColorFont1);
            ColorPicker9.Color = Color.Parse(config.Item2.ColorDark.ColorFont2);
            CheckBox2.IsChecked = config.Item2.RGB;
            CheckBox3.IsChecked = config.Item2.FontDefault;
            CheckBox5.IsChecked = config.Item2.WindowMode;
            CheckBox7.IsChecked = config.Item2.BackLimit;

            ComboBox1.IsEnabled = config.Item2.WindowTran;
            DropDownButton1.IsEnabled = !(CheckBox3.IsChecked == true);
            Slider3.IsEnabled = Slider4.IsEnabled = config.Item2.RGB;
            Slider5.IsEnabled = Button_Set3.IsEnabled = CheckBox7.IsChecked == true;
        }
        if (config.Item1 != null)
        {
            ComboBox2.SelectedIndex = (int)config.Item1.Language;
        }

        load = false;
    }

    private void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        ConfigBinding.SetBackTran((int)Slider2.Value);
        window.NotifyInfo.Show(App.GetLanguage("Gui.Info3"));
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
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
