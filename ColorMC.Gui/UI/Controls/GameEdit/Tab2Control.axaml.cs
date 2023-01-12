using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.Language;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using Newtonsoft.Json.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab2Control : UserControl
{
    private GameEditWindow Window;
    private GameSettingObj Obj;
    public Tab2Control()
    {
        InitializeComponent();

        Button_Set.Click += Button_Set_Click;
        Button_Set1.Click += Button_Set1_Click;
        Button_Set2.Click += Button_Set2_Click;
        Button_Set3.Click += Button_Set3_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        ComboBox1.Items = JavaBinding.GetGCTypes();
    }

    private void Button_Set3_Click(object? sender, RoutedEventArgs e)
    {
        if (UIUtils.CheckNumb(TextBox8.Text))
        {
            Window.Info.Show(Localizer.Instance["Tab3Control1.Error1"]);
            return;
        }

        GameBinding.SetGameProxy(Obj, new()
        {
            IP = TextBox7.Text,
            Port = ushort.Parse(TextBox8.Text),
            User = TextBox9.Text,
            Password = TextBox10.Text
        });
        Window.Info2.Show(Localizer.Instance["Tab4Control1.Info1"]);
    }

    private void Button_Set2_Click(object? sender, RoutedEventArgs e)
    {
        if (UIUtils.CheckNumb(TextBox6.Text))
        {
            Window.Info.Show(Localizer.Instance["Tab3Control1.Error1"]);
            return;
        }

        GameBinding.SetGameServer(Obj, new()
        {
            IP = TextBox5.Text,
            Port = ushort.Parse(TextBox6.Text)
        });
        Window.Info2.Show(Localizer.Instance["Tab4Control1.Info1"]);
    }

    private void Button_Set1_Click(object? sender, RoutedEventArgs e)
    {
        GameBinding.SetGameWindow(Obj, new()
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
        Window.Info2.Show(Localizer.Instance["Tab4Control1.Info1"]);
    }

    private void Load()
    {
        var config = Obj.JvmArg;
        if (config != null)
        {
            ComboBox1.SelectedIndex = (int)config.GC;

            Input1.Value = (uint)config.MinMemory;
            Input2.Value = (uint)config.MaxMemory;

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
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;

        Title.Content = string.Format(Localizer.Instance["Tab2Control2.Text1"], obj.Name);
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}
