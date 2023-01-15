using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using Avalonia.Interactivity;
using ColorMC.Gui.UIBinding;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab4Control : UserControl
{
    private SettingWindow Window;
    public Tab4Control()
    {
        InitializeComponent();

        Button_Set.Click += Button_Set_Click;
        Button_Set1.Click += Button_Set1_Click;
        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        ComboBox1.Items = JavaBinding.GetGCTypes();
    }

    private void Button_Set1_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetWindowSettingConfig(new()
        {
            Width = (uint)Input3.Value,
            Height = (uint)Input4.Value,
            FullScreen = CheckBox1.IsChecked == true
        });
        Window.Info2.Show(Localizer.Instance["Tab4Control1.Info1"]);
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
        Window.Info2.Show(Localizer.Instance["Tab4Control1.Info1"]);
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 != null)
        {
            ComboBox1.SelectedIndex = (int)config.Item1.DefaultJvmArg.GC;

            Input1.Value = (uint)config.Item1.DefaultJvmArg.MinMemory;
            Input2.Value = (uint)config.Item1.DefaultJvmArg.MaxMemory;

            TextBox1.Text = config.Item1.DefaultJvmArg.GCArgument;
            TextBox2.Text = config.Item1.DefaultJvmArg.JavaAgent;
            TextBox3.Text = config.Item1.DefaultJvmArg.JvmArgs;
            TextBox4.Text = config.Item1.DefaultJvmArg.GameArgs;

            Input3.Value = config.Item1.Window.Width;
            Input4.Value = config.Item1.Window.Height;
            CheckBox1.IsChecked = config.Item1.Window.FullScreen;
        }
    }
}
