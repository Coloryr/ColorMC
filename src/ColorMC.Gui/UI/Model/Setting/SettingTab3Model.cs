using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置页面
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// Dns列表
    /// </summary>
    public ObservableCollection<DnsItemModel> Dns { get; init; } = [];

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _httpLoad = true;

    /// <summary>
    /// 下载源列表
    /// </summary>
    public string[] SourceList { get; init; } = LangUtils.GetDownloadSources();
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

        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
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

    partial void OnSourceChanged(SourceLocal value)
    {
        if (_httpLoad)
            return;

        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
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
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text36"));
        }
        else
        {
            Window.Show(LangUtils.Get("SettingWindow.Tab3.Text37"));
        }
    }
    /// <summary>
    /// 开始更新
    /// </summary>
    [RelayCommand]
    public void StartUpdate()
    {
        UpdateUtils.StartUpdate(Window);
    }
    /// <summary>
    /// 开始检测更新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task StartCheck()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("SettingWindow.Tab3.Text33"));
        var res = await UpdateUtils.CheckNowVersion();
        Window.CloseDialog(dialog);
        if (!res.IsOk)
        {
            Window.Show(LangUtils.Get("SettingWindow.Tab3.Text38"));
            return;
        }
        else if (res.HaveUpdate)
        {
            var dialog1 = new LongTextModel(Window.WindowId)
            {
                Text1 = string.Format(LangUtils.Get("SettingWindow.Tab3.Text34"), res.Version),
                Text2 = res.Text ?? ""
            };
            var res1 = await Window.ShowDialogWait(dialog1);
            if (res1 is true)
            {
                UpdateUtils.StartUpdate(Window);
            }
        }
        else
        {
            Window.Show(LangUtils.Get("SettingWindow.Tab3.Text35"));
        }
    }
    /// <summary>
    /// 设置代理
    /// </summary>
    [RelayCommand]
    public void SetProxy()
    {
        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
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
        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
            return;
        }

        var model = new InputModel(Window.WindowId)
        {
            Watermark1 = LangUtils.Get("SettingWindow.Tab3.Text30")
        };
        var res = await Window.ShowDialogWait(model);
        if (res is not true)
        {
            return;
        }

        var url = model.Text1;

        if (!url.StartsWith("https://"))
        {
            Window.Show(LangUtils.Get("SettingWindow.Tab3.Text40"));
            return;
        }

        if (Dns.Any(item => item.Url == url))
        {
            Window.Show(LangUtils.Get("SettingWindow.Tab3.Text41"));
            return;
        }

        Dns.Add(new DnsItemModel(url));
        ConfigBinding.AddDns(url);

        Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text32"));
    }

    /// <summary>
    /// 删除Dns项目
    /// </summary>
    /// <param name="model"></param>
    public void DeleteDns(DnsItemModel model)
    {
        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
            return;
        }

        Dns.Remove(model);
        ConfigBinding.RemoveDns(model.Url);
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
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text36"));
        }
    }

    /// <summary>
    /// 加载联网设置
    /// </summary>
    public void LoadHttpSetting()
    {
        _httpLoad = true;

        var config = ConfigLoad.Config;
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

            Dns.Clear();

            foreach (var item in con.Dns.Https)
            {
                Dns.Add(new DnsItemModel(item));
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

        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
            return;
        }

        ConfigBinding.SetDownloadCheck(CheckFile, AutoDownload);
    }

    private void SetProxyEnable()
    {
        if (_httpLoad)
            return;

        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
            return;
        }

        ConfigBinding.SetDownloadProxyEnable(LoginProxy, DownloadProxy, GameProxy);
    }

    private void SetDns()
    {
        if (_httpLoad)
            return;

        if (DownloadManager.State)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text39"));
            return;
        }

        ConfigBinding.SetDns(DnsEnable, DnsProxy);
    }
}
