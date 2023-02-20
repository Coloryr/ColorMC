using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab4Control : UserControl
{
    private bool load = false;
    public Tab4Control()
    {
        InitializeComponent();

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox2_Click;
        CheckBox4.Click += CheckBox2_Click;
        CheckBox5.Click += CheckBox2_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        TextBox1.PropertyChanged += TextBox1_PropertyChanged;
        TextBox2.PropertyChanged += TextBox1_PropertyChanged;
        TextBox3.PropertyChanged += TextBox1_PropertyChanged;
        TextBox4.PropertyChanged += TextBox1_PropertyChanged;

        Input1.PropertyChanged += Input_PropertyChanged;
        Input2.PropertyChanged += Input_PropertyChanged;

        Input3.PropertyChanged += Input3_PropertyChanged;
        Input4.PropertyChanged += Input3_PropertyChanged;

        ComboBox1.Items = JavaBinding.GetGCTypes();
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        Save1();
    }

    private void Input3_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            Save1();
        }
    }

    private void TextBox1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save();
        }
    }

    private void CheckBox2_Click(object? sender, RoutedEventArgs e)
    {
        ConfigBinding.SetGameCheckConfig(new()
        {
            CheckCore = CheckBox2.IsChecked == true,
            CheckAssets = CheckBox3.IsChecked == true,
            CheckLib = CheckBox4.IsChecked == true,
            CheckMod = CheckBox5.IsChecked == true,
        });
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

    private void Save1()
    {
        ConfigBinding.SetWindowSettingConfig(new()
        {
            Width = (uint)Input3.Value,
            Height = (uint)Input4.Value,
            FullScreen = CheckBox1.IsChecked == true
        });
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TextBox1.IsEnabled = ComboBox1.SelectedIndex == 4;

        if (load)
            return;

        Save();
    }

    private void Save()
    {
        ConfigBinding.SetJvmArgConfig(new()
        {
            GC = (GCType)ComboBox1.SelectedIndex,
            JvmArgs = TextBox3.Text,
            GameArgs = TextBox4.Text,
            GCArgument = TextBox1.Text,
            JavaAgent = TextBox2.Text
        });
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
