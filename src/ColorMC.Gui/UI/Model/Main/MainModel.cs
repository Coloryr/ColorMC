using System.Threading.Tasks;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel : TopModel, IMutTop
{
    public const string SwitchView = "SwitchView";

    /// <summary>
    /// 是否在启动状态
    /// </summary>
    public bool IsLaunch;
    /// <summary>
    /// 是否为首次启动
    /// </summary>
    public bool IsFirst = true;

#if Phone
    /// <summary>
    /// 是否为手机模式
    /// </summary>
    public bool IsPhone => true;
#else
    /// <summary>
    /// 是否为手机模式
    /// </summary>
    public bool IsPhone => false;
#endif
    /// <summary>
    /// Motd地址
    /// </summary>
    [ObservableProperty]
    private (string, ushort) _server;

    /// <summary>
    /// 是否显示服务器信息
    /// </summary>
    [ObservableProperty]
    private bool _motdDisplay;
    /// <summary>
    /// 是否有游戏错误
    /// </summary>
    [ObservableProperty]
    private bool _isGameError;
    /// <summary>
    /// 是否显示侧边栏
    /// </summary>
    [ObservableProperty]
    private bool _sideDisplay = true;
    /// <summary>
    /// 是否显示音乐
    /// </summary>
    [ObservableProperty]
    private bool _musicDisplay;
    /// <summary>
    /// 是否显示背景图
    /// </summary>
    [ObservableProperty]
    private bool _backDisplay;
    /// <summary>
    /// 是否显示玩家
    /// </summary>
    [ObservableProperty]
    private bool _playerDisplay = true;
    /// <summary>
    /// 是否显示项目列表
    /// </summary>
    [ObservableProperty]
    private bool _menuDisplay = true;
    /// <summary>
    /// 是否渲染
    /// </summary>
    [ObservableProperty]
    private bool _render = true;
    /// <summary>
    /// 是否显示新闻卡片
    /// </summary>
    [ObservableProperty]
    private bool _cardNews;
    /// <summary>
    /// 是否有卡片
    /// </summary>
    [ObservableProperty]
    private bool _haveCard = true;
    /// <summary>
    /// 欢迎消息
    /// </summary>
    [ObservableProperty]
    private string _helloText;
    /// <summary>
    /// 音频按钮
    /// </summary>
    [ObservableProperty]
    private string _audioIcon = ImageManager.MusicIcons[1];
    /// <summary>
    /// 是否获取新的数据
    /// </summary>
    private bool _isGetNewInfo;

    private int _helloClick;

    public MainModel(BaseModel model) : base(model)
    {
        ImageManager.SkinChange += SkinChange;
        UserBinding.UserEdit += LoadUser;

        MusicVolume = GuiConfigUtils.Config.ServerCustom.Volume;
    }

    /// <summary>
    /// 打开收藏
    /// </summary>
    [RelayCommand]
    public void OpenCollect()
    {
        WindowManager.ShowCollect();
    }
    /// <summary>
    /// 打开统计
    /// </summary>
    [RelayCommand]
    public void ShowCount()
    {
        WindowManager.ShowCount();
    }
    /// <summary>
    /// 显示皮肤
    /// </summary>
    [RelayCommand]
    public void ShowSkin()
    {
        WindowManager.ShowSkin();
    }
    /// <summary>
    /// 显示用户列表
    /// </summary>
    [RelayCommand]
    public void ShowUser()
    {
        WindowManager.ShowUser();
    }
    /// <summary>
    /// 显示启动器设置
    /// </summary>
    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }
    /// <summary>
    /// 打开指南手册
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task OpenGuide()
    {
        var list = LanguageBinding.GetGuide();
        var res = await Model.Combo(App.Lang("SettingWindow.Tab7.Info3"), list);
        if (res.Cancel)
        {
            return;
        }
        WebBinding.OpenWeb(res.Index == 0 ? WebType.Guide1 : WebType.Guide);
    }
    /// <summary>
    /// 打开映射大厅
    /// </summary>
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
    /// <summary>
    /// 打开新闻列表
    /// </summary>
    [RelayCommand]
    public void OpenNews()
    {
        WindowManager.ShowNews();
    }

    protected override void MinModeChange()
    {
        foreach (var item in GameGroups)
        {
            item.SetMinMode(MinMode);
        }
    }

    /// <summary>
    /// 加载服务器motd
    /// </summary>
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

    /// <summary>
    /// 加载信息
    /// </summary>
    public void LoadDone()
    {
        _isload = true;

        var config = GuiConfigUtils.Config;
        SimpleMode = config.Simple;
        if (config.Live2D?.LowFps == true)
        {
            LowFps = true;
        }
        if (config?.CheckUpdate == true)
        {
            CheckUpdate();
        }

        LoadGameItem();
        LoadUser();
        LoadMotd();
        LoadNews();
        LoadMusic();
        LoadCard();

        _isload = false;
    }

    /// <summary>
    /// 加载卡片
    /// </summary>
    private void LoadCard()
    {
        var config = GuiConfigUtils.Config.Card;
        CardNews = config.News;
        if (!config.Online)
        {
            IsOnlineMode = false;
        }
        if (!config.Last)
        {
            HaveLast = false;
        }

        if (!CardNews && !IsOnlineMode && !HaveLast && !HaveUpdate)
        {
            HaveCard = false;
        }
        else
        {
            HaveCard = true;
        }
    }

    public override void Close()
    {
        foreach (var item in GameGroups)
        {
            item.Close();
        }
        GameGroups.Clear();
        Launchs.Clear();
    }
}
