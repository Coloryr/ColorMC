using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Core.Helpers;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Custom;

public partial class CustomControlPanelModel : ObservableObject
{
    private readonly CustomControl _con;

    [ObservableProperty]
    private GameItemModel _game;

    public bool IsLaunch = false;

    [ObservableProperty]
    private string _userName = "";
    [ObservableProperty]
    private string _userType = "";
    [ObservableProperty]
    private Bitmap _head = App.LoadIcon;
    [ObservableProperty]
    private (string, ushort) _server;

    public CustomControlPanelModel(CustomControl con, GameSettingObj obj)
    {
        _con = con;

        _game = new(con, con, obj);

        App.UserEdit += App_UserEdit;
        App.SkinLoad += App_SkinLoad;
    }

    [RelayCommand]
    public void Launch()
    {
        _con.Launch(Game);
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
        if (IsLaunch || obj.IsLaunch)
            return;

        var window = _con.Window;
        IsLaunch = true;
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
        IsLaunch = false;
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
}
