using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel : ControlModel, IMutTop
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

    /// <summary>
    /// Motd地址
    /// </summary>
    [ObservableProperty]
    public partial (string, ushort) Server { get; set; }

    /// <summary>
    /// 是否显示服务器信息
    /// </summary>
    [ObservableProperty]
    public partial bool MotdDisplay { get; set; }

    /// <summary>
    /// 是否有游戏错误
    /// </summary>
    [ObservableProperty]
    public partial bool IsGameError { get; set; }

    /// <summary>
    /// 是否显示背景图
    /// </summary>
    [ObservableProperty]
    public partial bool BackDisplay { get; set; }

    /// <summary>
    /// 是否渲染
    /// </summary>
    [ObservableProperty]
    public partial bool Render { get; set; } = true;

    /// <summary>
    /// 是否展示上次启动
    /// </summary>
    [ObservableProperty]
    public partial bool CardLast { get; set; }

    /// <summary>
    /// 是否显示音乐
    /// </summary>
    [ObservableProperty]
    public partial bool CardMusic { get; set; }

    /// <summary>
    /// 是否显示新闻卡片
    /// </summary>
    [ObservableProperty]
    public partial bool CardNews { get; set; }

    /// <summary>
    /// 幸运方块卡片
    /// </summary>
    [ObservableProperty]
    public partial bool CardBlock { get; set; }

    /// <summary>
    /// 是否有启动器更新
    /// </summary>
    [ObservableProperty]
    public partial bool CardUpdate { get; set; }

    /// <summary>
    /// 是否展示联机卡片
    /// </summary>
    [ObservableProperty]
    public partial bool CardOnline { get; set; }

    /// <summary>
    /// 是否有卡片
    /// </summary>
    [ObservableProperty]
    public partial bool HaveCard { get; set; } = true;

    /// <summary>
    /// 音频按钮
    /// </summary>
    [ObservableProperty]
    public partial string AudioIcon { get; set; } = ImageManager.MusicIcons[1];

    /// <summary>
    /// 是否获取新的数据
    /// </summary>
    private bool _isGetNewInfo;

    /// <summary>
    /// 是否正在加载窗口设置
    /// </summary>
    private bool _isLoad;

    public MainModel(WindowModel model) : base(model)
    {
        EventManager.SkinChange += SkinChange;
        EventManager.LastUserChange += EventManager_UserChange;
        ColorMCCore.JavaChange += ColorMCCore_JavaChange;

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
        var list = LangUtils.GetGuide();
        var dialog = new SelectModel(Window.WindowId)
        {
            Text = LangUtils.Get("SettingWindow.Tab7.Text18"),
            Items = [.. list]
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }
        WebBinding.OpenWeb(dialog.Index == 0 ? WebType.Guide1 : WebType.Guide);
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
            Window.Show(LangUtils.Get("MainWindow.Text85"));
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

    /// <summary>
    /// 加载窗口设置
    /// </summary>
    public void Load()
    {
        OneGroup ??= new GameGroupModel(Window, this);

        LoadMotd();
        LoadNews();
        LoadMusic();
        LoadCard();

        _isLoad = true;
        var config = GuiConfigUtils.Config.WindowState;
        GridType = config.MainWindowState;
        if (GridType == ItemsGridType.List)
        {
            GridType = ItemsGridType.Grid;
        }
        _isLoad = false;
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
        var config = GuiConfigUtils.Config;
        if (config?.CheckUpdate == true)
        {
            CheckUpdate();
        }

        LoadGameItem();
        LoadUser();
        LoadBlock();
    }

    /// <summary>
    /// 加载卡片
    /// </summary>
    public void LoadCard()
    {
        var config = GuiConfigUtils.Config.Card;
        CardNews = config.News;
        CardBlock = config.Block;

        if (!config.Online)
        {
            CardOnline = false;
        }
        if (!config.Last)
        {
            CardLast = false;
        }

        if (!CardNews && !CardOnline && !CardLast && !CardUpdate && !CardBlock)
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
