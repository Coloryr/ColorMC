using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;

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
        Button_Change.Click += Button_Change_Click;

        CheckBox1.Click += CheckBox1_Click;

        ComboBox1.Items = OtherBinding.GetWindowTranTypes();
        ComboBox2.Items = OtherBinding.GetLanguages();
    }

    private void Button_Change_Click(object? sender, RoutedEventArgs e)
    {
        var type = (LanguageType)ComboBox2.SelectedIndex;
        Window.Info1.Show(Localizer.Instance["Tab2Control1.Info1"]);
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
            CheckBox1.IsChecked = config.Item2.WindowTran;
            ComboBox1.SelectedIndex = config.Item2.WindowTranType;
            ComboBox2.SelectedIndex = (int)config.Item1.Language;
            if (config.Item2.WindowTran)
            {
                ComboBox1.IsEnabled = true;
            }
            else
            {
                ComboBox1.IsEnabled = false;
            }
        }
    }

    private async void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        string file = TextBox1.Text;

        Window.Info1.Show(Localizer.Instance["Tab2Control1.Info2"]);
        await ConfigBinding.SetGuiConfig(new()
        {
            BackImage = file,
            BackEffect = (int)Slider1.Value,
            BackTran = (int)Slider2.Value,
            WindowTran = CheckBox1.IsChecked == true,
            WindowTranType = ComboBox1.SelectedIndex
        });
        Window.Info1.Close();

        Window.Info2.Show(Localizer.Instance["Tab2Control1.Info4"]);
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = Localizer.Instance["Tab2Control1.Info5"],
            AllowMultiple = false,
            Filters = new()
            {
                new FileDialogFilter()
                {
                    Extensions = new()
                    {
                        "png",
                        "jpg",
                        "bmp"
                    }
                }
            }
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            TextBox1.Text = item;
        }
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        TextBox1.Text = "";

        ConfigBinding.DeleteGuiImageConfig();
    }
}
