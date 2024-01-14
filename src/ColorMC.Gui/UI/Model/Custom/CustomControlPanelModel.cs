using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Custom;

public partial class CustomControlPanelModel : TopModel
{
    private readonly IMainTop _top;

    [ObservableProperty]
    private GameItemModel _game;

    public bool IsLaunch { get; set; }

    [ObservableProperty]
    private string _userName = "";
    [ObservableProperty]
    private string _userType = "";
    [ObservableProperty]
    private Bitmap _head = App.LoadIcon;
    [ObservableProperty]
    private (string, ushort) _server;

    public CustomControlPanelModel(CustomControl con, BaseModel model, GameSettingObj obj) : base(model)
    {
        _top = con;
        _game = new(model, con, obj);

        App.UserEdit += App_UserEdit;
        App.SkinLoad += App_SkinLoad;
    }

    [RelayCommand]
    public void Launch()
    {
        _top.Launch(Game);
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
        {
            return;
        }

        IsLaunch = true;
        Model.Progress(App.Lang("MainWindow.Info3"));
        var item = Game;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        Model.Notify(App.Lang(string.Format(App.Lang("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(Model, game, wait: GuiConfigUtils.Config.CloseBeforeLaunch);
        Model.Title1 = null;
        item.IsLoad = false;
        Model.ProgressClose();
        if (res.Item1 == false)
        {
            Model.Show(res.Item2!);
        }
        else
        {
            Model.Notify(App.Lang("MainWindow.Info2"));

            item.IsLaunch = true;

            Model.Progress(App.Lang("MainWindow.Info26"));
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
            UserType = App.Lang("MainWindow.Info35");
            UserName = App.Lang("MainWindow.Info36");
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

    protected override void Close()
    {

    }
}
