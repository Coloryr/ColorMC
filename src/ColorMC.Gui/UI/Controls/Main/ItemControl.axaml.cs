using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Threading;
using Tmds.DBus.Protocol;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class ItemControl : UserControl
{
    private LoginObj? Obj1;
    private GameSettingObj? Obj;
    private bool islaunch;
    private bool isplay = true;
    private bool isopen = true;
    public ItemControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button4.Click += Button4_Click;
        Button3.Click += Button3_Click;
        Button5.Click += Button5_Click;
        Button6.Click += Button6_Click;
        Button7.Click += Button7_Click;
        Button8.Click += Button8_Click;
        Button9.Click += Button3_Click;
        Button10.Click += Button4_Click;
        Button11.Click += Button5_Click;
        Button12.Click += Button6_Click;
        Button13.Click += Button7_Click;

        Image1.PointerPressed += Image1_PointerPressed;
        Image1.PointerEntered += Image1_PointerEntered;
        Image1.PointerExited += Image1_PointerExited;

        App.SkinLoad += App_SkinLoad;
    }

    private void Image1_PointerExited(object? sender, PointerEventArgs e)
    {
        Border2.IsVisible = false;
    }

    private void Image1_PointerEntered(object? sender, PointerEventArgs e)
    {
        Border2.IsVisible = true;
    }

    private void Button8_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowUser();
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

    private void Button7_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowSetting(SettingType.Normal);
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAddGame();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowSkin();
    }

    private void Button6_Click(object? sender, RoutedEventArgs e)
    {
        if (Obj != null)
        {
            App.ShowGameEdit(Obj);
        }
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as MainControl)?.Launch(false);
    }

    public void SetLaunch(bool launch)
    {
        islaunch = launch;
    }

    public void SetGame(GameSettingObj? obj)
    {
        Obj = obj;
        if (obj == null)
        {
            Button5.IsEnabled = false;
            Button6.IsEnabled = false;
            Button11.IsEnabled = false;
            Button12.IsEnabled = false;
        }
        else
        {
            if (BaseBinding.IsGameRun(obj) || islaunch)
            {
                Button11.IsEnabled = false;
                Button5.IsEnabled = false;
            }
            else
            {
                Button11.IsEnabled = true;
                Button5.IsEnabled = true;
            }
            Button6.IsEnabled = true;
            Button12.IsEnabled = true;
        }
    }

    public void UpdateLaunch()
    {
        SetGame(Obj);
    }

    public async void SetUser(LoginObj? obj)
    {
        Obj1 = obj;

        if (Obj1 == null)
        {
            TextBlock2.Text = App.GetLanguage("MainWindow.Info35");
            TextBlock1.Text = App.GetLanguage("MainWindow.Info36");
        }
        else
        {
            TextBlock1.Text = Obj1.UserName;
            TextBlock2.Text = Obj1.AuthType.GetName();
        }

        ProgressBar1.IsVisible = true;

        await UserBinding.LoadSkin();
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2?.ServerCustom?.LockGame == true)
        {
            Button10.IsVisible = false;
            Button4.IsVisible = false;
        }
        else
        {
            Button10.IsVisible = true;
            Button4.IsVisible = true;
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
