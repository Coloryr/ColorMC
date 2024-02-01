using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    private bool _httpLoad = true;

    public string[] SourceList { get; init; } = LanguageBinding.GetDownloadSources();

    [ObservableProperty]
    private int _source;
    [ObservableProperty]
    private int? _thread = 5;

    [ObservableProperty]
    private string _iP;
    [ObservableProperty]
    private ushort? _port = 1080;
    [ObservableProperty]
    private string _user;
    [ObservableProperty]
    private string _password;
    [ObservableProperty]
    private string _serverKey;
    [ObservableProperty]
    private string _serverInfo;

    [ObservableProperty]
    private bool _isDownload;
    [ObservableProperty]
    private bool _loginProxy;
    [ObservableProperty]
    private bool _downloadProxy;
    [ObservableProperty]
    private bool _gameProxy;
    [ObservableProperty]
    private bool _checkFile;
    [ObservableProperty]
    private bool _checkUpdate;
    [ObservableProperty]
    private bool _autoDownload;

    partial void OnServerKeyChanged(string value)
    {
        if (_httpLoad)
            return;

        ConfigBinding.SetServerKey(value);
    }

    partial void OnCheckFileChanged(bool value)
    {
        SetDownloadCheck();
    }

    partial void OnCheckUpdateChanged(bool value)
    {
        SetDownloadCheck();
    }

    partial void OnAutoDownloadChanged(bool value)
    {
        SetDownloadCheck();
    }

    partial void OnDownloadProxyChanged(bool value)
    {
        SetProxyEnable();
    }

    partial void OnLoginProxyChanged(bool value)
    {
        SetProxyEnable();
    }

    partial void OnGameProxyChanged(bool value)
    {
        SetProxyEnable();
    }

    partial void OnThreadChanged(int? value)
    {
        if (_httpLoad)
            return;

        ConfigBinding.SetDownloadThread(value ?? 5);
    }

    partial void OnSourceChanged(int value)
    {
        if (_httpLoad)
            return;

        ConfigBinding.SetDownloadSource((SourceLocal)value);
    }

    [RelayCommand]
    public async Task GameCloudConnect()
    {
        await GameCloudUtils.StartConnect();
        ServerInfo = GameCloudUtils.Info;
        if (GameCloudUtils.Connect)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Info4"));
        }
        else
        {
            Model.Show(App.Lang("SettingWindow.Tab3.Error1"));
        }
    }

    [RelayCommand]
    public void OpenDownloadPath()
    {
        PathBinding.OpPath(PathType.DownloadPath);
    }

    [RelayCommand]
    public void OpenPicPath()
    {
        PathBinding.OpPath(PathType.PicPath);
    }

    [RelayCommand]
    public void StartUpdate()
    {
        UpdateChecker.StartUpdate();
    }

    [RelayCommand]
    public async Task StartCheck()
    {
        Model.Progress(App.Lang("SettingWindow.Tab3.Info1"));
        var res = await UpdateChecker.CheckOne();
        Model.ProgressClose();
        if (res.Item1 == null)
        {
            Model.Show(App.Lang("Gui.Error21"));
            return;
        }
        else if (res.Item1 == true)
        {
            var res1 = await Model.ShowTextWait(App.Lang("SettingWindow.Tab3.Info2"), res.Item2!);
            if (res1)
            {
                UpdateChecker.StartUpdate();
            }
        }
        else
        {
            Model.Show(App.Lang("SettingWindow.Tab3.Info3"));
        }
    }

    [RelayCommand]
    public void SetProxy()
    {
        ConfigBinding.SetDownloadProxy(IP, Port ?? 1080, User, Password);
    }

    public async void TestGameCloudConnect()
    {
        await GameCloudUtils.StartConnect();
        ServerInfo = GameCloudUtils.Info;
        if (GameCloudUtils.Connect)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Info4"));
        }
    }

    public void LoadHttpSetting()
    {
        _httpLoad = true;

        IsDownload = BaseBinding.IsDownload;

        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 is { } con)
        {
            Source = (int)con.Http.Source;

            Thread = con.Http.DownloadThread;

            IP = con.Http.ProxyIP;
            Port = con.Http.ProxyPort;
            User = con.Http.ProxyUser;
            Password = con.Http.ProxyPassword;

            LoginProxy = con.Http.LoginProxy;
            DownloadProxy = con.Http.DownloadProxy;
            GameProxy = con.Http.GameProxy;
            CheckFile = con.Http.CheckFile;
            AutoDownload = con.Http.AutoDownload;
            CheckUpdate = con.Http.CheckUpdate;
        }
        if (config.Item2 is { } con1)
        {
            ServerKey = con1.ServerKey;
            ServerInfo = GameCloudUtils.Info;
        }
        _httpLoad = false;
    }

    private void SetDownloadCheck()
    {
        if (_httpLoad)
            return;

        ConfigBinding.SetDownloadCheck(CheckFile, AutoDownload, CheckUpdate);
    }

    private void SetProxyEnable()
    {
        if (_httpLoad)
            return;

        ConfigBinding.SetDownloadProxyEnable(LoginProxy, DownloadProxy, GameProxy);
    }
}
