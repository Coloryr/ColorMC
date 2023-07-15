using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Custom;

public partial class CustomWindowModel : ObservableObject
{
    private CustomControl Con;

    private GameSettingObj Obj;

    [ObservableProperty]
    private GameItemModel game;

    public bool launch = false;

    [ObservableProperty]
    private string userName = "";
    [ObservableProperty]
    private string userType = "";
    [ObservableProperty]
    private Bitmap head = App.LoadIcon;
    [ObservableProperty]
    private (string, ushort) server;

    public CustomWindowModel(CustomControl con, GameSettingObj obj)
    {
        Con = con;
        Obj = obj;

        Game = new(con, con, obj);

        App.UserEdit += App_UserEdit;
        App.SkinLoad += App_SkinLoad;
    }

    [RelayCommand]
    public void Launch()
    {
        Con.Launch(Game);
    }

    [RelayCommand]
    public void Setting()
    {
        App.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public void User()
    {
        App.ShowUser();
    }

    [RelayCommand]
    public void Skin()
    {
        App.ShowSkin();
    }

    [RelayCommand]
    public void OpUrl(object? value)
    {
        BaseBinding.OpUrl(value?.ToString());
    }

    public async void Launch(GameItemModel obj)
    {
        if (launch || obj.IsLaunch)
            return;

        var window = Con.Window;
        launch = true;
        window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info3"));
        var item = Game;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        window.NotifyInfo.Show(App.GetLanguage(string.Format(App.GetLanguage("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(window, game);
        window.Head.Title1 = null;
        item.IsLoad = false;
        await window.ProgressInfo.CloseAsync();
        if (res.Item1 == false)
        {
            window.OkInfo.Show(res.Item2!);
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("MainWindow.Info2"));

            item.IsLaunch = true;

            window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info26"));
        }
        launch = false;
    }

    public void App_SkinLoad()
    {
        Head = UserBinding.HeadBitmap!;
    }

    public async void App_UserEdit()
    {
        var user = UserBinding.GetLastUser();

        if (user == null)
        {
            UserType = App.GetLanguage("MainWindow.Info35");
            UserName = App.GetLanguage("MainWindow.Info36");
        }
        else
        {
            UserType = user.AuthType.GetName();
            UserName = user.UserName;
        }

        await UserBinding.LoadSkin();
    }

    public void MotdLoad()
    {
        var config = ConfigBinding.GetAllConfig();
        Server = (config.Item2.ServerCustom.IP, config.Item2.ServerCustom.Port);
    }

    private void HeadImg_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
        {
            App.ShowSkin();
        }
    }
}

public partial class CustomControl : UserControl, IUserControl, IMainTop
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title { get; set; }

    private CustomWindowModel Model;

    public CustomControl()
    {
        InitializeComponent();
    }

    public void Closed()
    {
        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameDownload = null;

        App.CustomWindow = null;

        if (App.MainWindow == null)
        {
            App.Close();
        }
    }

    public void Load(string local)
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 == null)
        {
            return;
        }

        Grid1.Children.Clear();

        var obj = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
        if (obj == null)
        {
            Grid1.Children.Add(new Label()
            {
                Content = App.GetLanguage("MainWindow.Info18"),
                Foreground = Brushes.Black,
                Background = Brush.Parse("#EEEEEE"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return;
        }

        var ui1 = AvaloniaRuntimeXamlLoader.Parse<CustomPanelControl>(File.ReadAllText(local));

        Model = new CustomWindowModel(this, obj);

        ui1.DataContext = Model;

        Title = ui1.Title;
        Window.SetTitle(Title);

        Grid1.Children.Add(ui1);

        Model.App_UserEdit();
        Model.MotdLoad();

        Task.Run(() => BaseBinding.ServerPackCheck(obj));
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (Model.launch)
        {
            var res = await windows.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info34"));
            if (res)
            {
                return false;
            }
            return true;
        }

        if (BaseBinding.IsGameRuning())
        {
            App.Hide();
            return true;
        }

        return false;
    }

    public void Launch(GameItemModel obj)
    {
        Model.Launch(obj);
    }

    public void Select(GameItemModel? model)
    {
        
    }

    public void EditGroup(GameItemModel model)
    {

    }
}
