using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab2Control : UserControl
{
    private GameSettingObj Obj;
    private bool load = false;
    public Tab2Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;

        Input1.PropertyChanged += Input1_PropertyChanged;

        ComboBox1.ItemsSource = JavaBinding.GetGCTypes();

        TextBox11.PropertyChanged += TextBox11_TextInput;

        Input1.PropertyChanged += Input1_PropertyChanged1;
        Input2.PropertyChanged += Input1_PropertyChanged1;
        TextBox2.PropertyChanged += TextBox2_PropertyChanged;
        TextBox3.PropertyChanged += TextBox2_PropertyChanged;
        TextBox4.PropertyChanged += TextBox2_PropertyChanged;

        Input3.PropertyChanged += Input3_PropertyChanged;
        Input4.PropertyChanged += Input3_PropertyChanged;

        TextBox5.PropertyChanged += TextBox5_PropertyChanged;
        TextBox6.PropertyChanged += TextBox5_PropertyChanged;

        TextBox7.PropertyChanged += TextBox7_PropertyChanged;
        TextBox8.PropertyChanged += TextBox7_PropertyChanged;
        TextBox9.PropertyChanged += TextBox7_PropertyChanged;
        TextBox10.PropertyChanged += TextBox7_PropertyChanged;

        TextBox12.PropertyChanged += TextBox12_PropertyChanged;

        TextBox14.PropertyChanged += TextBox2_PropertyChanged;
        TextBox15.PropertyChanged += TextBox2_PropertyChanged;

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox3_Click;
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        TextBox15.IsEnabled = CheckBox3.IsChecked == true;

        Save2();
    }

    private void CheckBox2_Click(object? sender, RoutedEventArgs e)
    {
        TextBox14.IsEnabled = CheckBox2.IsChecked == true;

        Save2();
    }

    private void TextBox12_PropertyChanged(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Text")
        {
            Save5();
        }
    }

    private async void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var Window = App.FindRoot(VisualRoot);
        var res = await Window.Info.ShowWait(App.GetLanguage("GameEditWindow.Tab2.Info1"));
        if (res)
        {
            GameBinding.DeleteConfig(Obj);

            Load();
        }
    }

    private void TextBox7_PropertyChanged(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save4();
        }
    }

    private void TextBox5_PropertyChanged(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save3();
        }
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        Save1();
    }

    private void Input3_PropertyChanged(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            Save1();
        }
    }

    private void TextBox2_PropertyChanged(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save2();
        }
    }
    private void Input1_PropertyChanged1(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            Save2();
        }
    }

    private void Input1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Value")
        {
            Save();
        }
    }

    private void Save()
    {
        GameBinding.SetGameJvmMemArg(Obj,
            Input1.Value == null ? null : (uint)Input1.Value,
            Input2.Value == null ? null : (uint)Input2.Value);
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

        var Window = App.FindRoot(VisualRoot);
        Window.Info2.Show(App.GetLanguage("Gui.Info3"));
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);

        var file = await BaseBinding.OpFile(window, FileType.Java);
        if (file != null)
        {
            TextBox11.Text = file;
        }
    }

    private void TextBox11_TextInput(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

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

            Save();
        }
    }

    private void Save4()
    {
        var window = App.FindRoot(VisualRoot);

        if (Funtcions.CheckNotNumber(TextBox8.Text))
        {
            window.Info.Show(App.GetLanguage("Gui.Error7"));
            return;
        }

        GameBinding.SetGameProxy(Obj, new()
        {
            IP = TextBox7.Text,
            Port = ushort.Parse(TextBox8.Text!),
            User = TextBox9.Text,
            Password = TextBox10.Text
        });
    }

    private void Save3()
    {
        if (Funtcions.CheckNotNumber(TextBox6.Text))
        {
            return;
        }

        GameBinding.SetGameServer(Obj, new()
        {
            IP = TextBox5.Text,
            Port = ushort.Parse(TextBox6.Text!)
        });
    }

    private void Save1()
    {
        GameBinding.SetGameWindow(Obj, new()
        {
            Width = (uint)Input3.Value!,
            Height = (uint)Input4.Value!,
            FullScreen = CheckBox1.IsChecked == true
        });
    }

    private void Save2()
    {
        GameBinding.SetGameJvmArg(Obj, new()
        {
            GC = ComboBox1.SelectedIndex == -1 ? null : (GCType)ComboBox1.SelectedIndex,
            JvmArgs = TextBox3.Text,
            GameArgs = TextBox4.Text,
            GCArgument = TextBox1.Text,
            JavaAgent = TextBox2.Text,
            MaxMemory = Input2.Value == null ? null : (uint)Input2.Value,
            MinMemory = Input1.Value == null ? null : (uint)Input1.Value,
            LaunchPre = CheckBox3.IsChecked == true,
            LaunchPreData = TextBox15.Text,
            LaunchPost = CheckBox2.IsChecked == true,
            LaunchPostData = TextBox14.Text,
        });
    }

    private void Save5()
    {
        GameBinding.SetAdvanceJvmArg(Obj, new()
        {
            MainClass = TextBox12.Text,
            ClassPath = TextBox13.Text
        });
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TextBox1.IsEnabled = ComboBox1.SelectedIndex == 4;

        GameBinding.SetJavaLocal(Obj, ComboBox2.SelectedItem as string, TextBox11.Text);
    }

    private void Load()
    {
        load = true;

        var list = new List<string>()
        {
            ""
        };
        list.AddRange(JavaBinding.GetJavaName());

        ComboBox2.ItemsSource = list;

        ComboBox2.SelectedItem = Obj.JvmName ?? "";
        TextBox11.Text = Obj.JvmLocal;

        var config = Obj.JvmArg;
        if (config != null)
        {
            ComboBox1.SelectedIndex = config.GC == null ? -1 : (int)config.GC;

            Input1.Value = config.MinMemory == null ? null : (uint)config.MinMemory;
            Input2.Value = config.MaxMemory == null ? null : (uint)config.MaxMemory;

            TextBox1.Text = config.GCArgument;
            TextBox2.Text = config.JavaAgent;
            TextBox3.Text = config.JvmArgs;
            TextBox4.Text = config.GameArgs;
            TextBox14.Text = config.LaunchPostData;
            TextBox15.Text = config.LaunchPreData;

            CheckBox3.IsChecked = config.LaunchPre;
            CheckBox2.IsChecked = config.LaunchPost;

            TextBox15.IsEnabled = CheckBox3.IsChecked == true;
            TextBox14.IsEnabled = CheckBox2.IsChecked == true;
        }
        else
        {
            ComboBox1.SelectedIndex = -1;
            Input1.Value = null;
            Input2.Value = null;
            TextBox3.Text = null;
            TextBox4.Text = null;
            TextBox14.Text = null;
            TextBox15.Text = null;

            CheckBox3.IsChecked = false;
            CheckBox2.IsChecked = false;

            TextBox15.IsEnabled = false;
            TextBox14.IsEnabled = false;
        }

        var config1 = Obj.Window;
        if (config1 != null)
        {
            Input3.Value = config1.Width;
            Input4.Value = config1.Height;
            CheckBox1.IsChecked = config1.FullScreen;
        }
        else
        {
            Input3.Value = null;
            Input4.Value = null;
            CheckBox1.IsChecked = false;
        }

        var config2 = Obj.StartServer;
        if (config2 != null)
        {
            TextBox5.Text = config2.IP;
            TextBox6.Text = config2.Port.ToString();
        }
        else
        {
            TextBox5.Text = null;
            TextBox6.Text = null;
        }

        var config3 = Obj.ProxyHost;
        if (config3 != null)
        {
            TextBox7.Text = config3.IP;
            TextBox8.Text = config3.Port.ToString();
            TextBox9.Text = config3.User;
            TextBox10.Text = config3.Password;
        }
        else
        {
            TextBox7.Text = null;
            TextBox8.Text = null;
            TextBox9.Text = null;
            TextBox10.Text = null;
        }

        var config4 = Obj.AdvanceJvm;
        if (config4 != null)
        {
            TextBox12.Text = config4.MainClass;
            TextBox13.Text = config4.ClassPath;
        }
        else
        {
            TextBox12.Text = null;
            TextBox13.Text = null;
        }

        load = false;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Title.Content = string.Format(App.GetLanguage("GameEditWindow.Tab2.Text13"), Obj.Name);

        Load();
    }
}
