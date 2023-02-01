using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.IO;
using Avalonia.Input;

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
        Button_Launch1.Click += Button_Launch1_Click;
        Button_Edit.Click += Button_Edit_Click;
        Button_Add1.Click += Button_Add1_Click;
        Button_Out.Click += Button_Out_Click;

        Button_Switch.Click += Button_Switch_Click;
        Button_Add.Click += Button_Add_Click;

        Button1.Click += Button1_Click;
        Button_Setting.Click += Button_Setting_Click;

        Image1.PointerPressed += Image1_PointerPressed;

        Expander1.ContentTransition = App.CrossFade300;
        Button1.Content = "→";
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

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        if (Expander1.IsExpanded)
        {
            Button1.Content = "←";
        }
        else
        {
            Button1.Content = "→";
        }
        Expander1.IsExpanded = !Expander1.IsExpanded;
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowUser(true);
    }

    private void Button_Switch_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowUser(false);
    }

    private void Button_Launch1_Click(object? sender, RoutedEventArgs e)
    {
        Window.Launch(true);
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

    private void Button_Out_Click(object? sender, RoutedEventArgs e)
    {
        if (Obj != null)
        {
            App.ShowGameEdit(Obj, 5);
        }
    }

    public void SetGame(GameSettingObj? obj)
    {
        Obj = obj;
        if (obj == null)
        {
            Button_Launch.IsEnabled = false;
            Button_Launch1.IsEnabled = false;
            Button_Edit.IsEnabled = false;
            Button_Out.IsEnabled = false;
        }
        else
        {
            if (BaseBinding.Games.ContainsValue(obj))
            {
                Button_Launch.IsEnabled = false;
                Button_Launch1.IsEnabled = false;
            }
            else
            {
                Button_Launch.IsEnabled = true;
                Button_Launch1.IsEnabled = true;
            }
            Button_Edit.IsEnabled = true;
            Button_Out.IsEnabled = true;
        }
    }

    public void SetWindow(MainWindow window)
    {
        Window = window;
    }

    public void SetUser(LoginObj? obj)
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

        LoadHead();
    }

    private async void LoadHead()
    {
        ProgressBar1.IsVisible = true;

        Image1.Source = UserBinding.HeadBitmap!;

        ProgressBar1.IsVisible = false;
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null &&
            config.Item2.ServerCustom?.LockGame == true)
        {
            Button_Add1.IsVisible = false;
            Button_Out.IsVisible = false;
        }
        else
        {
            Button_Add1.IsVisible = true;
            Button_Out.IsVisible = true;
        }
    }
}
