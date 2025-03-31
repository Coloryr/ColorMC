using System.Collections.ObjectModel;
using System.IO;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.BuildPack;

/// <summary>
/// 导出客户端配置
/// </summary>
public partial class BuildPackModel
{
    /// <summary>
    /// 内置Java列表
    /// </summary>
    public ObservableCollection<JavaSelectModel> Javas { get; init; } = [];

    /// <summary>
    /// 是否导出界面设置
    /// </summary>
    [ObservableProperty]
    private bool _uiSetting;
    [ObservableProperty]
    private bool _uiBg;
    [ObservableProperty]
    private bool _uiColor;
    [ObservableProperty]
    private bool _uiOther;
    /// <summary>
    /// 是否导出启动设置
    /// </summary>
    [ObservableProperty]
    private bool _launchSetting;
    [ObservableProperty]
    private bool _launchCheck;
    [ObservableProperty]
    private bool _launchArg;
    [ObservableProperty]
    private bool _launchWindow;
    /// <summary>
    /// 是否导出服务器设置
    /// </summary>
    [ObservableProperty]
    private bool _serverSetting;
    [ObservableProperty]
    private bool _serverOpt;
    [ObservableProperty]
    private bool _serverLock;
    [ObservableProperty]
    private bool _serverUi;
    [ObservableProperty]
    private bool _serverMusic;
    /// <summary>
    /// 是否导出内置Java
    /// </summary>
    [ObservableProperty]
    private bool _java;
    /// <summary>
    /// 是否导出启动器
    /// </summary>
    [ObservableProperty]
    private bool _packLaunch;
    [ObservableProperty]
    private bool _packUpdate;

    /// <summary>
    /// 是否可以选择背景界面
    /// </summary>
    [ObservableProperty]
    private bool _canBg;
    /// <summary>
    /// 是否可以选择自定义UI
    /// </summary>
    [ObservableProperty]
    private bool _canUi;
    /// <summary>
    /// 是否可以选择背景音乐
    /// </summary>
    [ObservableProperty]
    private bool _canMusic;
    /// <summary>
    /// 是否可以打包启动器
    /// </summary>
    [ObservableProperty]
    private bool _canPack;
    /// <summary>
    /// 是否可以打包启动器升级
    /// </summary>
    [ObservableProperty]
    private bool _canUpdate;

    /// <summary>
    /// 是否在切换状态
    /// </summary>
    private bool _isItem;

    //设置项修改
    partial void OnUiSettingChanged(bool value)
    {
        if (_isItem)
        {
            return;
        }
        if (value)
        {
            if (CanBg)
            {
                UiBg = true;
            }
            UiOther = UiColor = true;
        }
        else
        {
            UiBg = UiOther = UiColor = false;
        }
    }

    partial void OnUiBgChanged(bool value)
    {
        UISwitch();
    }

    partial void OnUiColorChanged(bool value)
    {
        UISwitch();
    }

    partial void OnUiOtherChanged(bool value)
    {
        UISwitch();
    }

    partial void OnLaunchSettingChanged(bool value)
    {
        if (_isItem)
        {
            return;
        }
        LaunchCheck = LaunchArg = LaunchWindow = value;
    }

    partial void OnLaunchCheckChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnLaunchArgChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnLaunchWindowChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnServerSettingChanged(bool value)
    {
        if (_isItem)
        {
            return;
        }
        if (value)
        {
            if (CanMusic)
            {
                ServerMusic = true;
            }
            if (CanUi)
            {
                ServerUi = true;
            }
            ServerOpt = ServerLock = true;
        }
        else
        {
            ServerMusic = ServerUi = ServerOpt = ServerLock = false;
        }
    }

    partial void OnServerOptChanged(bool value)
    {
        ServerSwitch();
    }

    partial void OnServerLockChanged(bool value)
    {
        ServerSwitch();
    }

    partial void OnServerMusicChanged(bool value)
    {
        ServerSwitch();
    }

    partial void OnServerUiChanged(bool value)
    {
        ServerSwitch();
    }

    /// <summary>
    /// 加载设置项目
    /// </summary>
    private void LoadSetting()
    {
        var conf = GuiConfigUtils.Config;
        CanBg = File.Exists(conf.BackImage);
        CanPack = PackLaunch = SystemInfo.Os is OsType.Windows;
        CanUpdate = PackUpdate = File.Exists(LauncherUpgrade.LocalPath[0]);
        CanUi = File.Exists(Path.Combine(ColorMCGui.BaseDir, GuiNames.NameCustomUIFile));
        CanMusic = File.Exists(conf.ServerCustom.Music);

        Javas.Clear();
        foreach (var item in JavaBinding.GetJavas())
        {
            if (!item.Path.StartsWith(JvmPath.JavaDir))
            {
                continue;
            }
            Javas.Add(new JavaSelectModel(item));
        }
    }

    //总开关切换
    private void UISwitch()
    {
        _isItem = true;
        UiSetting = UiBg || UiColor || UiOther;
        _isItem = false;
    }

    private void LaunchSwitch()
    {
        _isItem = true;
        LaunchSetting = LaunchCheck || LaunchArg || LaunchWindow;
        _isItem = false;
    }

    private void ServerSwitch()
    {
        _isItem = true;
        ServerSetting = ServerOpt || ServerLock || ServerMusic || ServerUi;
        _isItem = false;
    }
}
