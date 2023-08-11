using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab3Model : BaseModel
{
    private bool _load;

    public List<string> SourceList { get; init; } = LanguageBinding.GetDownloadSources();

    [ObservableProperty]
    private int _source;
    [ObservableProperty]
    private int _thread;

    [ObservableProperty]
    private string _iP;
    [ObservableProperty]
    private ushort _port;
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

    public SettingTab3Model(IUserControl con) : base(con)
    {

    }

    partial void OnServerKeyChanged(string value)
    {
        if (_load)
            return;

        ConfigBinding.SetServerKey(value);
    }

    partial void OnCheckFileChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckUpdateChanged(bool value)
    {
        SetCheck();
    }

    partial void OnAutoDownloadChanged(bool value)
    {
        SetCheck();
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

    partial void OnThreadChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetDownloadThread(value);
    }

    partial void OnSourceChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetDownloadSource((SourceLocal)value);
    }

    [RelayCommand]
    public async Task GameCloudConnect()
    {
        await GameCloudUtils.StartConnect();
        ServerInfo = GameCloudUtils.Info;
    }

    [RelayCommand]
    public void OpenDownload()
    {
        PathBinding.OpPath(PathType.DownloadPath);
    }

    [RelayCommand]
    public void OpenPic()
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
        Progress(App.GetLanguage("SettingWindow.Tab3.Info1"));
        var res = await UpdateChecker.CheckOne();
        ProgressClose();
        if (res.Item1 == null)
        {
            Show(App.GetLanguage("Gui.Error21"));
            return;
        }
        else if (res.Item1 == true)
        {
            var res1 = await TextInfo(App.GetLanguage("SettingWindow.Tab3.Info2"), res.Item2!);
            if (!res1)
            {
                UpdateChecker.StartUpdate();
            }
        }
        else
        {
            Show(App.GetLanguage("SettingWindow.Tab3.Info3"));
        }
    }

    [RelayCommand]
    public void SetProxy()
    {
        ConfigBinding.SetDownloadProxy(IP, Port, User, Password);
    }

    public void Load()
    {
        _load = true;

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
        _load = false;
    }

    private void SetCheck()
    {
        if (_load)
            return;

        ConfigBinding.SetDownloadCheck(CheckFile, AutoDownload, CheckUpdate);
    }

    private void SetProxyEnable()
    {
        if (_load)
            return;

        ConfigBinding.SetDownloadProxyEnable(LoginProxy, DownloadProxy, GameProxy);
    }

    public override void Close()
    {
        
    }
}
