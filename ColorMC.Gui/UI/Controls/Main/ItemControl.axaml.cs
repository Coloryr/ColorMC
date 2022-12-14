using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Language;
using ColorMC.Gui.UI.Windows;
using System;
using System.IO;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class ItemControl : UserControl
{
    private MainWindow Window;
    private LoginObj? Obj1;
    private Bitmap bitmap;
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

        var uri = new Uri($"resm:ColorMC.Gui.Resource.Pic.user.png");
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets.Open(uri);

        Image1.Source = bitmap = new Bitmap(asset);
        Expander1.ContentTransition = new CrossFade(TimeSpan.FromMilliseconds(300));
        Button1.Content = ">";
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
            Button1.Content = "<";
        }
        else
        {
            Button1.Content = ">";
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

    }

    private void Button_Launch_Click(object? sender, RoutedEventArgs e)
    {
        Window.Launch(false);
    }

    private void Button_Out_Click(object? sender, RoutedEventArgs e)
    {

    }

    public void SetGame(GameSettingObj? obj)
    {
        if (obj == null)
        {
            Button_Launch.IsEnabled = false;
            Button_Launch1.IsEnabled = false;
            Button_Edit.IsEnabled = false;
            Button_Out.IsEnabled = false;
        }
        else
        {
            Button_Launch.IsEnabled = true;
            Button_Launch1.IsEnabled = true;
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
            Image1.Source = bitmap;
            TextBlock_Type.Text = Localizer.Instance["ItemControl.Info1"];
            TextBlock_Name.Text = Localizer.Instance["ItemControl.Info2"];
        }
        else
        {
            TextBlock_Name.Text = Obj1.UserName;
            TextBlock_Type.Text = Obj1.AuthType.GetName();
            LoadHead();
        }
    }

    private async void LoadHead()
    {
        ProgressBar1.IsVisible = true;
        if (Obj1 == null)
        {
            Image1.Source = bitmap;
            return;
        }

        var file = await GetSkin.DownloadSkin(Obj1);
        if (file == null)
        {
            Image1.Source = bitmap;
            return;
        }

        var data = await ImageUtils.MakeHeadImage(file);
        if (file == null)
        {
            Image1.Source = bitmap;
            return;
        }

        data.Seek(0, SeekOrigin.Begin);
        Image1.Source = new Bitmap(data);
        data.Close();
        ProgressBar1.IsVisible = false;
    }
}
