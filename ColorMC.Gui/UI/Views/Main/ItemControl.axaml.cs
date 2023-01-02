using Avalonia.Controls;
using ColorMC.Core.Objs;
using System;
using Avalonia.Interactivity;
using System.IO;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UIBinding;
using ColorMC.Core.Objs.Login;
using Avalonia.Platform;
using Avalonia;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Avalonia.Animation;
using ColorMC.Core.Utils;
using Avalonia.Input;
using ColorMC.Core.Http;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Views.Main;

public partial class ItemControl : UserControl
{
    private MainWindow Window;
    private GameSettingObj? Obj = null;
    private LoginObj? Obj1;
    private object Lock = new();
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

        var uri = new Uri($"resm:ColorMC.Gui.Resource.Icon.user.png");
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets.Open(uri);

        Image1.Source = bitmap = new Bitmap(asset);
        Expander1.ContentTransition = new CrossFade(TimeSpan.FromMilliseconds(300));
        Button1.Content = ">";
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
        App.ShowUser(1);
    }

    private void Button_Switch_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowUser(0);
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
            TextBlock_Type.Text = "空账户";
            TextBlock_Name.Text = "没有用户";
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

        file = await HeadImageUtils.MakeHeadImage(file);
        if (file == null)
        {
            Image1.Source = bitmap;
            return;
        }

        Image1.Source = new Bitmap(file);
        ProgressBar1.IsVisible = false;
    }
}
