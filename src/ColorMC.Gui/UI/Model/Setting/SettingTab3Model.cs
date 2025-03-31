using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置页面
/// </summary>
public partial class SettingModel
{
    public const string NameNetSetting = "NetworkSetting";

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _httpLoad = true;

    /// <summary>
    /// 下载源列表
    /// </summary>
    public string[] SourceList { get; init; } = LanguageBinding.GetDownloadSources();
    /// <summary>
    /// Dns类型列表
    /// </summary>
    public string[] DnsList { get; init; } = LanguageBinding.GetDns();
    /// <summary>
    /// Dns列表
    /// </summary>
    public ObservableCollection<DnsItemModel> Dns { get; init; } = [];

    /// <summary>
    /// 下载源
    /// </summary>
    [ObservableProperty]
    private SourceLocal _source;
    /// <summary>
    /// 下载线程
    /// </summary>
    [ObservableProperty]
    private int? _thread = 5;
    /// <summary>
    /// Dns类型
    /// </summary>
    [ObservableProperty]
    private DnsType _dnsType;
    /// <summary>
    /// 选中的Dns项目
    /// </summary>
    [ObservableProperty]
    private DnsItemModel _dnsItem;

    /// <summary>
    /// 代理地址
    /// </summary>
    [ObservableProperty]
    private string _iP;
    /// <summary>
    /// 代理端口
    /// </summary>
    [ObservableProperty]
    private ushort? _port = 1080;
    /// <summary>
    /// 代理用户
    /// </summary>
    [ObservableProperty]
    private string _user;
    /// <summary>
    /// 代理密码
    /// </summary>
    [ObservableProperty]
    private string _password;
    /// <summary>
    /// 云同步密钥
    /// </summary>
    [ObservableProperty]
    private string _serverKey;
    /// <summary>
    /// 云同步信息
    /// </summary>
    [ObservableProperty]
    private string _serverInfo;

    /// <summary>
    /// 是否代理登录过程
    /// </summary>
    [ObservableProperty]
    private bool _loginProxy;
    /// <summary>
    /// 是否代理下载过程
    /// </summary>
    [ObservableProperty]
    private bool _downloadProxy;
    /// <summary>
    /// 是否代理游戏
    /// </summary>
    [ObservableProperty]
    private bool _gameProxy;
    /// <summary>
    /// 是否检测下载文件完整性
    /// </summary>
    [ObservableProperty]
    private bool _checkFile;
    /// <summary>
    /// 是否检测启动器更新
    /// </summary>
    [ObservableProperty]
    private bool _checkUpdate;
    /// <summary>
    /// 是否自动更新启动器
    /// </summary>
    [ObservableProperty]
    private bool _autoDownload;
    /// <summary>
    /// 是否启用自定义DNS
    /// </summary>
    [ObservableProperty]
    private bool _dnsEnable;
    /// <summary>
    /// 代理是否启用自定义DNS
    /// </summary>
    [ObservableProperty]
    private bool _dnsProxy;

    //设置修改
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

    /// <summary>
    /// 链接云同步服务器
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// 开始更新
    /// </summary>
    [RelayCommand]
    public void StartUpdate()
    {
        LauncherUpgrade.StartUpdate();
    }
    /// <summary>
    /// 开始检测更新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task StartCheck()
    {
        Model.Progress(App.Lang("SettingWindow.Tab3.Info1"));
        var res = await LauncherUpgrade.CheckOne();
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
                LauncherUpgrade.StartUpdate();
            }
        }
        else
        {
            Model.Show(App.Lang("SettingWindow.Tab3.Info3"));
        }
    }
    /// <summary>
    /// 设置代理
    /// </summary>
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
    /// <summary>
    /// 添加DNS项目
    /// </summary>
    /// <returns></returns>
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
        else if (model.IsHttps)
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

    /// <summary>
    /// 删除Dns项目
    /// </summary>
    /// <param name="model"></param>
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

    /// <summary>
    /// 测试云同步服务器
    /// </summary>
    public async void TestGameCloudConnect()
    {
        await GameCloudUtils.StartConnect();
        ServerInfo = ColorMCCloudAPI.Info;
        if (ColorMCCloudAPI.Connect)
        {
            Model.Notify(App.Lang("SettingWindow.Tab3.Info4"));
        }
    }

    /// <summary>
    /// 加载联网设置
    /// </summary>
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

    //配置保存
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
