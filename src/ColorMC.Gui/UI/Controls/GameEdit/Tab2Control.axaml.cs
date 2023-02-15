using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab2Control : UserControl
{
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private bool load = false;
    public Tab2Control()
    {
        InitializeComponent();

        Button_Set.Click += Button_Set_Click;
        Button_Set1.Click += Button_Set1_Click;
        Button_Set2.Click += Button_Set2_Click;
        Button_Set3.Click += Button_Set3_Click;
        Button_Set4.Click += Button_Set4_Click;
        Button_Set5.Click += Button_Set5_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;

        Input1.PropertyChanged += Input1_PropertyChanged;

        ComboBox1.Items = JavaBinding.GetGCTypes();

        TextBox11.PropertyChanged += TextBox11_TextInput;
    }

    private void Input1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            GameBinding.SetGameJvmMemArg(Obj, (uint)Input1.Value, (uint)Input2.Value);
            Window.Info2.Show(Localizer.Instance["Info3"]);
        }
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        if (ComboBox2.SelectedIndex == 0)
        {
            GameBinding.SetJavaLocal(Obj, null, TextBox11.Text);
        }
        else
        {
            GameBinding.SetJavaLocal(Obj, ComboBox2.SelectedItem as string, TextBox11.Text);
        }

        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private async void Button_Set5_Click(object? sender, RoutedEventArgs e)
    {
        var file = await BaseBinding.OpFile(Window, Localizer.Instance["SettingWindow.Tab5.Info2"],
            SystemInfo.Os == OsType.Windows ? "*.exe" : "", Localizer.Instance["SettingWindow.Tab5.Info2"]);
        if (file.Any())
        {
            TextBox11.Text = file[0].GetPath();
        }
    }

    private void Button_Set4_Click(object? sender, RoutedEventArgs e)
    {
        GameBinding.SetJavaLocal(Obj, ComboBox2.SelectedItem as string, TextBox11.Text);
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void TextBox11_TextInput(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var property = e.Property.Name;
        if (property == "Text")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (string.IsNullOrWhiteSpace(TextBox11.Text))
                {
                    ComboBox2.IsEnabled = true;
                }
                else
                {
                    ComboBox2.IsEnabled = false;
                }
            });
        }
    }

    private void Button_Set3_Click(object? sender, RoutedEventArgs e)
    {
        if (UIUtils.CheckNotNumber(TextBox8.Text))
        {
            Window.Info.Show(Localizer.Instance["Error7"]);
            return;
        }

        GameBinding.SetGameProxy(Obj, new()
        {
            IP = TextBox7.Text,
            Port = ushort.Parse(TextBox8.Text!),
            User = TextBox9.Text,
            Password = TextBox10.Text
        });
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button_Set2_Click(object? sender, RoutedEventArgs e)
    {
        if (UIUtils.CheckNotNumber(TextBox6.Text))
        {
            Window.Info.Show(Localizer.Instance["Error7"]);
            return;
        }

        GameBinding.SetGameServer(Obj, new()
        {
            IP = TextBox5.Text,
            Port = ushort.Parse(TextBox6.Text!)
        });
        Window.Info2.Show(Localizer.Instance["Info3"]);
    }

    private void Button_Set1_Click(object? sender, RoutedEventArgs e)
    {
        GameBinding.SetGameWindow(Obj, new()
        {
            Width = (uint)Input3.Value!,
            Height = (uint)Input4.Value!,
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
        GameBinding.SetGameJvmArg(Obj, new()
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

    private void Load()
    {
        load = true;

        var list = new List<string>()
        {
            ""
        };
        list.AddRange(JavaBinding.GetJavaName());

        ComboBox2.Items = list;

        ComboBox2.SelectedItem = Obj.JvmName ?? "";
        TextBox11.Text = Obj.JvmLocal;

        var config = Obj.JvmArg;
        if (config != null)
        {
            ComboBox1.SelectedIndex = (int)config.GC;

            Input1.Value = config.MinMemory;
            Input2.Value = config.MaxMemory;

            TextBox1.Text = config.GCArgument;
            TextBox2.Text = config.JavaAgent;
            TextBox3.Text = config.JvmArgs;
            TextBox4.Text = config.GameArgs;
        }

        var config1 = Obj.Window;
        if (config1 != null)
        {
            Input3.Value = config1.Width;
            Input4.Value = config1.Height;
            CheckBox1.IsChecked = config1.FullScreen;
        }

        var config2 = Obj.StartServer;
        if (config2 != null)
        {
            TextBox5.Text = config2.IP;
            TextBox6.Text = config2.Port.ToString();
        }

        var config3 = Obj.ProxyHost;
        if (config3 != null)
        {
            TextBox7.Text = config3.IP;
            TextBox8.Text = config3.Port.ToString();
            TextBox9.Text = config3.User;
            TextBox10.Text = config3.Password;
        }

        load = false;
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;

        Title.Content = string.Format(Localizer.Instance["GameEditWindow.Tab2.Text13"], obj.Name);
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}
