using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using Avalonia;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab4Control : UserControl
{
    private SettingWindow Window;
    private bool load = false;
    public Tab4Control()
    {
        InitializeComponent();

        Button_Set.Click += Button_Set_Click;
        Button_Set1.Click += Button_Set1_Click;
        Button_Set2.Click += Button_Set2_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        Input1.PropertyChanged += Input_PropertyChanged;
        Input2.PropertyChanged += Input_PropertyChanged;

        ComboBox1.Items = JavaBinding.GetGCTypes();
    }

    private void Input_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            ConfigBinding.SetJvmArgMemConfig((uint)Input1.Value, (uint)Input2.Value);
        }
    }

    private void Button_Set2_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetGameCheckConfig(new()
        {
            CheckCore = CheckBox2.IsChecked == true,
            CheckAssets = CheckBox3.IsChecked == true,
            CheckLib = CheckBox4.IsChecked == true,
            CheckMod = CheckBox5.IsChecked == true,
        });

        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button_Set1_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetWindowSettingConfig(new()
        {
            Width = (uint)Input3.Value,
            Height = (uint)Input4.Value,
            FullScreen = CheckBox1.IsChecked == true
        });
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TextBox1.IsEnabled = ComboBox1.SelectedIndex == 4;
    }

    private void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetJvmArgConfig(new()
        {
            GC = (GCType)ComboBox1.SelectedIndex,
            JvmArgs = TextBox3.Text,
            GameArgs = TextBox4.Text,
            GCArgument = TextBox1.Text,
            JavaAgent = TextBox2.Text,
            MaxMemory = (uint)Input2.Value,
            MinMemory = (uint)Input1.Value
        });
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    {
        load = true;
        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 != null)
        {
            ComboBox1.SelectedIndex = (int)config.Item1.DefaultJvmArg.GC;

            Input1.Value = (uint)config.Item1.DefaultJvmArg.MinMemory;
            Input2.Value = (uint)config.Item1.DefaultJvmArg.MaxMemory;
            Input3.Value = config.Item1.Window.Width;
            Input4.Value = config.Item1.Window.Height;

            TextBox1.Text = config.Item1.DefaultJvmArg.GCArgument;
            TextBox2.Text = config.Item1.DefaultJvmArg.JavaAgent;
            TextBox3.Text = config.Item1.DefaultJvmArg.JvmArgs;
            TextBox4.Text = config.Item1.DefaultJvmArg.GameArgs;

            CheckBox1.IsChecked = config.Item1.Window.FullScreen;
            CheckBox2.IsChecked = config.Item1.GameCheck.CheckCore;
            CheckBox3.IsChecked = config.Item1.GameCheck.CheckAssets;
            CheckBox4.IsChecked = config.Item1.GameCheck.CheckLib;
            CheckBox5.IsChecked = config.Item1.GameCheck.CheckMod;
        }

        load = false;
    }
}
