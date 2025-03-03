using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public const string NameNetSetting = "NetworkSetting";

    private bool _httpLoad = true;

    public string[] SourceList { get; init; } = LanguageBinding.GetDownloadSources();
    public string[] DnsList { get; init; } = LanguageBinding.GetDns();

    public ObservableCollection<DnsItemModel> Dns { get; init; } = [];

    [ObservableProperty]
    private SourceLocal _source;
    [ObservableProperty]
    private int? _thread = 5;
    [ObservableProperty]
    private DnsType _dnsType;

    [ObservableProperty]
    private DnsItemModel _dnsItem;

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
    [ObservableProperty]
    private bool _dnsEnable;
    [ObservableProperty]
    private bool _dnsProxy;

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
        SetCheckUpdate();
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

        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        ConfigBinding.SetDownloadThread(value ?? 5);
    }

    partial void OnDnsEnableChanged(bool value)
    {
        SetDns();
    }

    partial void OnDnsProxyChanged(bool value)
    {
        SetDns();
    }

    partial void OnDnsTypeChanged(DnsType value)
    {
        SetDns();
    }

    partial void OnSourceChanged(SourceLocal value)
    {
        if (_httpLoad)
            return;

        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        ConfigBinding.SetDownloadSource(value);
    }

    [RelayCommand]
    public async Task GameCloudConnect()
    {
        await GameCloudUtils.StartConnect();
        ServerInfo = ColorMCCloudAPI.Info;
        if (ColorMCCloudAPI.Connect)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Info4"));
        }
        else
        {
            Model.Show(App.Lang("SettingWindow.Tab3.Error1"));
        }
    }

    [RelayCommand]
    public void StartUpdate()
    {
        UpdateUtils.StartUpdate();
    }

    [RelayCommand]
    public async Task StartCheck()
    {
        Model.Progress(App.Lang("SettingWindow.Tab3.Info1"));
        var res = await UpdateUtils.CheckOne();
        Model.ProgressClose();
        if (res.Item1 == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab3.Error2"));
            return;
        }
        else if (res.Item1 == true)
        {
            var res1 = await Model.TextAsync(App.Lang("SettingWindow.Tab3.Info2"), res.Item2!);
            if (res1)
            {
                UpdateUtils.StartUpdate();
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
        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        ConfigBinding.SetDownloadProxy(IP, Port ?? 1080, User, Password);
    }

    [RelayCommand]
    public async Task AddDnsItem()
    {
        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        var model = new AddDnsModel();
        var res = await DialogHost.Show(model, NameNetSetting);
        if (res is not true)
        {
            return;
        }

        var url = model.Url;

        if (model.IsDns)
        {
            if (!IPAddress.TryParse(url, out _))
            {
                Model.Show(App.Lang("SettingWindow.Tab3.Error4"));
                return;
            }

            if (Dns.Any(item => item.Dns == DnsType.DnsOver && item.Url == url))
            {
                Model.Show(App.Lang("SettingWindow.Tab3.Error6"));
                return;
            }

            Dns.Add(new(url, DnsType.DnsOver));
            ConfigBinding.AddDns(url, DnsType.DnsOver);
        }

        if (model.IsHttps)
        {
            if (!url.StartsWith("https://"))
            {
                Model.Show(App.Lang("SettingWindow.Tab3.Error4"));
                return;
            }

            if (Dns.Any(item => item.Dns == DnsType.DnsOverHttps && item.Url == url))
            {
                Model.Show(App.Lang("SettingWindow.Tab3.Error6"));
                return;
            }

            Dns.Add(new(url, DnsType.DnsOverHttps));
            ConfigBinding.AddDns(url, DnsType.DnsOverHttps);
        }

        Model.Notify(App.Lang("UserWindow.Info12"));
    }

    public void DeleteDns(DnsItemModel model)
    {
        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        Dns.Remove(model);
        ConfigBinding.RemoveDns(model.Url, model.Dns);
    }

    public async void TestGameCloudConnect()
    {
        await GameCloudUtils.StartConnect();
        ServerInfo = ColorMCCloudAPI.Info;
        if (ColorMCCloudAPI.Connect)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Info4"));
        }
    }

    public void LoadHttpSetting()
    {
        _httpLoad = true;

        var config = ConfigUtils.Config;
        if (config is { } con)
        {
            Source = con.Http.Source;

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
            DnsEnable = con.Dns.Enable;
            DnsProxy = con.Dns.HttpProxy;
            DnsType = con.Dns.DnsType;

            Dns.Clear();

            foreach (var item in con.Dns.Dns)
            {
                Dns.Add(new(item, DnsType.DnsOver));
            }
            foreach (var item in con.Dns.Https)
            {
                Dns.Add(new(item, DnsType.DnsOverHttps));
            }
        }
        var config1 = GuiConfigUtils.Config;
        if (config1 is { } con1)
        {
            ServerKey = con1.ServerKey;
            ServerInfo = ColorMCCloudAPI.Info;
            CheckUpdate = con1.CheckUpdate;
        }
        _httpLoad = false;
    }

    private void SetCheckUpdate()
    {
        if (_httpLoad)
            return;

        ConfigBinding.SetUpdateCheck(CheckUpdate);
    }

    private void SetDownloadCheck()
    {
        if (_httpLoad)
            return;

        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        ConfigBinding.SetDownloadCheck(CheckFile, AutoDownload);
    }

    private void SetProxyEnable()
    {
        if (_httpLoad)
            return;

        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        ConfigBinding.SetDownloadProxyEnable(LoginProxy, DownloadProxy, GameProxy);
    }

    private void SetDns()
    {
        if (_httpLoad)
            return;

        if (BaseBinding.IsDownload)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Error3"));
            return;
        }

        ConfigBinding.SetDns(DnsEnable, DnsType, DnsProxy);
    }
}
