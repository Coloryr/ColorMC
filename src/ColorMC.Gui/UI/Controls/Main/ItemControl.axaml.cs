using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class ItemControl : UserControl
{
    private MainWindow Window;
    private LoginObj? Obj1;
    private GameSettingObj? Obj;
    public ItemControl()
    {
        InitializeComponent();

        Button_Launch.Click += Button_Launch_Click;
        Button_Edit.Click += Button_Edit_Click;
        Button_Add1.Click += Button_Add1_Click;

        Button_Switch.Click += Button_Switch_Click;

        Button_Setting.Click += Button_Setting_Click;

        Image1.PointerPressed += Image1_PointerPressed;

        Expander1.ContentTransition = App.CrossFade300;
        Button1.Click += Button1_Click;

        App.SkinLoad += App_SkinLoad;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Window.Button1.IsVisible = true;
        Expander1.IsExpanded = false;
        Button1.IsVisible = false;
    }

    public void Display()
    {
        Window.Button1.IsVisible = false;
        Expander1.IsExpanded = true;
        Button1.IsVisible = true;
    }

    private void App_SkinLoad()
    {
        Image1.Source = UserBinding.HeadBitmap!;

        ProgressBar1.IsVisible = false;
    }

    private void Image1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        App.ShowSkin();
    }

    private void Button_Setting_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowSetting();
    }

    private void Button_Add1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAddGame();
    }

    private void Button_Switch_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowUser();
    }

    private void Button_Edit_Click(object? sender, RoutedEventArgs e)
    {
        if (Obj != null)
        {
            App.ShowGameEdit(Obj);
        }
    }

    private void Button_Launch_Click(object? sender, RoutedEventArgs e)
    {
        Window.Launch(false);
    }

    public void SetGame(GameSettingObj? obj)
    {
        Obj = obj;
        if (obj == null)
        {
            Button_Launch.IsEnabled = false;
            Button_Edit.IsEnabled = false;
        }
        else
        {
            if (BaseBinding.Games.ContainsValue(obj))
            {
                Button_Launch.IsEnabled = false;
            }
            else
            {
                Button_Launch.IsEnabled = true;
            }
            Button_Edit.IsEnabled = true;
        }
    }

    public void SetWindow(MainWindow window)
    {
        Window = window;
    }

    public async void SetUser(LoginObj? obj)
    {
        Obj1 = obj;

        if (Obj1 == null)
        {
            TextBlock_Type.Text = Localizer.Instance["MainWindow.Control.Info1"];
            TextBlock_Name.Text = Localizer.Instance["MainWindow.Control.Info2"];
        }
        else
        {
            TextBlock_Name.Text = Obj1.UserName;
            TextBlock_Type.Text = Obj1.AuthType.GetName();
        }

        ProgressBar1.IsVisible = true;

        await UserBinding.LoadSkin();
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null &&
            config.Item2.ServerCustom?.LockGame == true)
        {
            Button_Add1.IsVisible = false;
        }
        else
        {
            Button_Add1.IsVisible = true;
        }
    }
}
