using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class ItemControl : UserControl
{
    private LoginObj? Obj1;
    private GameSettingObj? Obj;
    private bool Launch;
    private bool isplay = true;
    private bool isopen = true;
    public ItemControl()
    {
        InitializeComponent();

        Button_Launch.Click += Button_Launch_Click;
        Button_Edit.Click += Button_Edit_Click;
        Button_Add1.Click += Button_Add1_Click;
        Button_Switch.Click += Button_Switch_Click;
        Button_Setting.Click += Button_Setting_Click;

        Button_Launch1.Click += Button_Launch_Click;
        Button_Edit1.Click += Button_Edit_Click;
        Button_Add11.Click += Button_Add1_Click;
        Button_Switch1.Click += Button_Switch_Click;
        Button_Setting1.Click += Button_Setting_Click;

        Image1.PointerPressed += Image1_PointerPressed;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        App.SkinLoad += App_SkinLoad;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (isplay)
        {
            BaseBinding.MusicPause();

            window.SetTitle(App.GetLanguage("MainWindow.Title"));
        }
        else
        {
            BaseBinding.MusicPlay();

            window.SetTitle(App.GetLanguage("MainWindow.Title") + " " + App.GetLanguage("MainWindow.Info33"));
        }

        isplay = !isplay;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        if (isopen)
        {
            App.CrossFade100.Start(Grid1, null, CancellationToken.None);
            Button1.Content = "←";
            isopen = false;
            Grid2.IsVisible = true;
        }
        else
        {
            App.CrossFade100.Start(null, Grid1, CancellationToken.None);
            Button1.Content = "→";
            isopen = true;
            Grid2.IsVisible = false;
        }
    }

    private void App_SkinLoad()
    {
        Image1.Source = UserBinding.HeadBitmap!;

        ProgressBar1.IsVisible = false;
    }

    private void Image1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            App.ShowUser();
        }
    }

    private void Button_Setting_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowSetting(SettingType.Normal);
    }

    private void Button_Add1_Click(object? sender, RoutedEventArgs e)
    {
        if (BaseBinding.IsDownload)
        {
            var window = App.FindRoot(VisualRoot);
            window.Info.Show(App.GetLanguage("MainWindow.Control.Info3"));
            return;
        }
        App.ShowAddGame();
    }

    private void Button_Switch_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowSkin();
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
        var window = App.FindRoot(VisualRoot);
        (window.Con as MainControl)?.Launch(false);
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
            if (BaseBinding.IsGameRun(obj) || Launch)
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

    public void SetLaunch(bool launch)
    {
        Launch = launch;
    }

    public async void SetUser(LoginObj? obj)
    {
        Obj1 = obj;

        if (Obj1 == null)
        {
            TextBlock_Type.Text = App.GetLanguage("MainWindow.Control.Info1");
            TextBlock_Name.Text = App.GetLanguage("MainWindow.Control.Info2");
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
        if (config.Item2?.ServerCustom?.LockGame == true)
        {
            Button_Add1.IsVisible = false;
        }
        else
        {
            Button_Add1.IsVisible = true;
        }

        if (config.Item2?.ServerCustom?.PlayMusic == true)
        {
            var window = App.FindRoot(VisualRoot);
            window.SetTitle(App.GetLanguage("MainWindow.Title") + " " + App.GetLanguage("MainWindow.Info33"));
            Button2.IsVisible = true;
        }
        else
        {
            Button2.IsVisible = false;
        }
    }
}
