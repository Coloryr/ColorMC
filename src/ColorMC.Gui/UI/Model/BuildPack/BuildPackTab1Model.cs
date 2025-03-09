using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.BuildPack;

public partial class BuildPackModel
{
    public ObservableCollection<JavaSelectModel> Javas { get; init; } = [];

    [ObservableProperty]
    private bool _uiSetting;
    [ObservableProperty]
    private bool _uiBg;
    [ObservableProperty]
    private bool _uiColor;
    [ObservableProperty]
    private bool _uiLive2d;
    [ObservableProperty]
    private bool _launchSetting;
    [ObservableProperty]
    private bool _launchCheck;
    [ObservableProperty]
    private bool _launchJvm;
    [ObservableProperty]
    private bool _launchGame;
    [ObservableProperty]
    private bool _launchPrRun;
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
    [ObservableProperty]
    private bool _java;
    [ObservableProperty]
    private bool _packLaunch;
    [ObservableProperty]
    private bool _packUpdate;

    partial void OnUiSettingChanged(bool value)
    {
        if (value)
        {
            UiBg = UiColor = UiLive2d = true;
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

    partial void OnUiLive2dChanged(bool value)
    {
        UISwitch();
    }

    partial void OnLaunchSettingChanged(bool value)
    {
        if (value)
        {
            LaunchCheck = LaunchJvm = LaunchGame = LaunchPrRun = true;
        }
    }

    partial void OnLaunchCheckChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnLaunchJvmChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnLaunchGameChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnLaunchPrRunChanged(bool value)
    {
        LaunchSwitch();
    }

    partial void OnServerSettingChanged(bool value)
    {
        if (value)
        {
            ServerOpt = ServerLock = ServerMusic = ServerUi = true;
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

    private void UISwitch()
    {
        UiSetting = UiBg || UiColor || UiLive2d;
    }

    private void LaunchSwitch()
    {
        LaunchSetting = LaunchCheck || LaunchJvm || LaunchGame || LaunchPrRun;
    }

    private void ServerSwitch()
    {
        ServerSetting = ServerOpt || ServerLock || ServerMusic || ServerUi;
    }
}
