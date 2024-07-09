using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : TopModel, IMainTop
{
    public const string SwitchView = "SwitchView";

    public bool IsLaunch;
    public bool IsFirst = true;

    public bool IsPhone { get; } = SystemInfo.Os == OsType.Android;

    private static readonly string[] _side =
    [
        "/Resource/Icon/left.svg",
        "/Resource/Icon/menu.svg"
    ];

    private readonly Semaphore _semaphore = new(0, 2);
    private readonly Dictionary<string, GameItemModel> Launchs = [];

    private bool _isplay = true;
    private bool _isCancel;

    [ObservableProperty]
    private (string, ushort) _server;

    [ObservableProperty]
    private bool _motdDisplay;
    [ObservableProperty]
    private bool _isGameError;

    [ObservableProperty]
    private bool _sideDisplay = true;
    [ObservableProperty]
    private bool _musicDisplay;
    [ObservableProperty]
    private bool _newsDisplay;
    [ObservableProperty]
    private bool _backDisplay;

    [ObservableProperty]
    private bool _render = true;

    [ObservableProperty]
    private bool _haveUpdate;

    [ObservableProperty]
    private bool _topSide = true;
    [ObservableProperty]
    private bool _topSide1 = true;
    [ObservableProperty]
    private bool _topSide2 = false;
    [ObservableProperty]
    private string _sidePath = _side[0];

    [ObservableProperty]
    private string _helloText;

    [ObservableProperty]
    private float _musicVolume;

    private bool _isNewUpdate;
    private string _updateStr;

    private bool _isGetNewInfo;

    public MainModel(BaseModel model) : base(model)
    {
        App.SkinLoad += App_SkinLoad;
        App.UserEdit += LoadUser;

        ShowHello();
    }

    partial void OnTopSideChanged(bool value)
    {
        if (value)
        {
            SidePath = _side[1];
        }
        else
        {
            SidePath = _side[0];
        }
    }

    [RelayCommand]
    public void OpenSide()
    {
        if (TopSide)
        {
            TopSide1 = true;
            TopSide = false;
        }
        else
        {
            TopSide1 = false;
            TopSide = true;
        }
    }

    [RelayCommand]
    public async Task NewInfo()
    {
        if (_isGetNewInfo)
        {
            return;
        }
        _isGetNewInfo = true;

        var data = await WebBinding.GetNewLog();
        if (data == null)
        {
            Model.Show(App.Lang("MainWindow.Error1"));
        }
        else
        {
            Model.ShowText(App.Lang("MainWindow.Info40"), data);
        }

        _isGetNewInfo = false;
    }

    [RelayCommand]
    public async Task Upgrade()
    {
        var res = await Model.ShowTextWait(App.Lang("Text.Update"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                UpdateChecker.StartUpdate();
            }
        }
    }

    [RelayCommand]
    public void ShowCount()
    {
        WindowManager.ShowCount();
    }

    [RelayCommand]
    public void MusicPause()
    {
        if (_isplay)
        {
            BaseBinding.MusicPause();

            Model.Title = App.Lang("Name");
        }
        else
        {
            BaseBinding.MusicPlay();

            Model.Title = App.Lang("Name") + " " + App.Lang("MainWindow.Info33");
        }

        _isplay = !_isplay;
    }

    [RelayCommand]
    public void ShowSkin()
    {
        WindowManager.ShowSkin();
    }

    [RelayCommand]
    public void ShowUser()
    {
        WindowManager.ShowUser();
    }

    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public async Task OpenGuide()
    {
        var res = await Model.ShowWait(App.Lang("SettingWindow.Tab7.Info3"));
        WebBinding.OpenWeb(res ? WebType.Guide1 : WebType.Guide);
    }

    [RelayCommand]
    public void OpenNetFrp()
    {
        if (UserBinding.HaveOnline())
        {
            WindowManager.ShowNetFrp();
        }
        else
        {
            Model.Show(App.Lang("MainWindow.Error6"));
        }
    }

    [RelayCommand]
    public void OpenNews()
    {
        NewsDisplay = true;
        SideDisplay = false;
        HelloText = App.Lang("MainWindow.Text20");
        Model.PushBack(NewBack);
        OnPropertyChanged(SwitchView);
    }

    private void ShowHello()
    {
        HelloText = App.Lang("MainWindow.Hello.Text1");
    }

    private void NewBack()
    {
        NewsDisplay = false;
        SideDisplay = true;
        ShowHello();
        Model.PopBack();
        OnPropertyChanged(SwitchView);
    }

    private void App_SkinLoad()
    {
        Head = ImageManager.HeadBitmap!;

        IsHeadLoad = false;
    }

    public void LoadMotd()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (config != null && config?.Motd == true &&
            !string.IsNullOrWhiteSpace(config?.IP))
        {
            MotdDisplay = true;

            Server = (config.IP, config.Port);
        }
        else
        {
            MotdDisplay = false;
        }
    }

    public async void LoadDone()
    {
        LoadGameItem();
        LoadUser();

        LoadMotd();
        _ = LoadNews();

        BaseBinding.LoadMusic();

        var config = GuiConfigUtils.Config;
        var config1 = ConfigUtils.Config;
        if (config.Live2D?.LowFps == true)
        {
            LowFps = true;
        }
        if (config1.Http?.CheckUpdate == true)
        {
            var data = await UpdateChecker.Check();
            if (!data.Item1)
            {
                return;
            }
            HaveUpdate = true;
            _isNewUpdate = data.Item2 || ColorMCGui.IsAot;
            _updateStr = data.Item3!;
        }
    }

    public override void Close()
    {
        GroupList.Clear();
        foreach (var item in GameGroups)
        {
            item.Close();
        }
        GameGroups.Clear();
        Launchs.Clear();
    }
}
