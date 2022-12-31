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
using ColorMC.Core.Http.Skin;
using System.Runtime.CompilerServices;

namespace ColorMC.Gui.UI.Views.Main;

public partial class ItemControl : UserControl
{
    private MainWindow Window;
    private GameSettingObj Obj;
    private LoginObj? Obj1;
    private object Lock = new();
    private Bitmap bitmap;
    public ItemControl()
    {
        InitializeComponent();

        Button_Launch.Click += Button_Launch_Click;
        Button_Edit.Click += Button_Edit_Click;

        Button_Switch.Click += Button_Switch_Click;
        Button_Add.Click += Button_Add_Click;
        Button_Delete.Click += Button_Delete_Click;
        Button_Out.Click += Button_Out_Click;

        var uri = new Uri($"resm:ColorMC.Gui.Resource.Icon.user.png");
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets.Open(uri);

        Image1.Source = bitmap = new Bitmap(asset);
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        
    }

    private void Button_Switch_Click(object? sender, RoutedEventArgs e)
    {
        
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        
    }



    private void Button_Edit_Click(object? sender, RoutedEventArgs e)
    {
        
    }

    private void Button_Launch_Click(object? sender, RoutedEventArgs e)
    {
        
    }

    private void Button_Out_Click(object? sender, RoutedEventArgs e)
    {

    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
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
            
            Button_Delete.IsEnabled = false;

            TextBlock_Name.Text = "没有用户";
        }
        else
        {
            TextBlock_Name.Text = Obj1.UserName;
            LoadHead();
        }
    }

    private async void LoadHead()
    {
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
    }
}
