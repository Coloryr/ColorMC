using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : TopModel, IMainTop
{
    private static readonly string[] _icons = 
    [
        "/Resource/Icon/play.svg",
        "/Resource/Icon/pause.svg"
    ];

    public const string SwitchView = "SwitchView";

    public bool IsLaunch;
    public bool IsFirst = true;

    public bool IsPhone { get; } = SystemInfo.Os == OsType.Android;

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
    private string _helloText;

    [ObservableProperty]
    private string _audioIcon = _icons[1];

    [ObservableProperty]
    private int _musicVolume;

    private bool _isNewUpdate;
    private string _updateStr;

    private bool _isGetNewInfo;

    public MainModel(BaseModel model) : base(model)
    {
        ImageManager.SkinChange += SkinChange;
        UserBinding.UserEdit += LoadUser;

        MusicVolume = GuiConfigUtils.Config.ServerCustom.Volume;

        ShowHello();
    }

    partial void OnMusicVolumeChanged(int value)
    {
        BaseBinding.SetVolume(value);
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
        var res = await Model.ShowTextWait(App.Lang("BaseBinding.Info2"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                UpdateUtils.StartUpdate();
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
            AudioIcon = _icons[0];
            Model.Title = "ColorMC";
        }
        else
        {
            BaseBinding.MusicPlay();
            AudioIcon = _icons[1];
            Model.Title = "ColorMC " + App.Lang("MainWindow.Info33");
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
        var list = LanguageBinding.GetGuide();
        var res = await Model.ShowCombo(App.Lang("SettingWindow.Tab7.Info3"), list);
        if (res.Cancel)
        {
            return;
        }
        WebBinding.OpenWeb(res.Index == 0 ? WebType.Guide1 : WebType.Guide);
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
        MotdDisplay = false;
        HelloText = App.Lang("MainWindow.Text20");
        Model.PushBack(NewBack);
        OnPropertyChanged(SwitchView);
    }

    private void ShowHello()
    {
        HelloText = App.Lang("Hello.Text1");
    }

    private void NewBack()
    {
        NewsDisplay = false;
        if (!MinMode)
        {
            SideDisplay = true;
        }
        LoadMotd();
        ShowHello();
        Model.PopBack();
        OnPropertyChanged(SwitchView);
    }

    private void SkinChange()
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
            var data = await UpdateUtils.Check();
            if (!data.Item1)
            {
                return;
            }
            HaveUpdate = true;
            _isNewUpdate = data.Item2 || ColorMCGui.IsAot || ColorMCGui.IsMin;
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
