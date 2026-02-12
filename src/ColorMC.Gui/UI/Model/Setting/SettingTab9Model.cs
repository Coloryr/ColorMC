using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    /// <summary>
    /// 启用URI支持
    /// </summary>
    [ObservableProperty]
    private bool _fastEnable;
    /// <summary>
    /// 启用Modrinth复写
    /// </summary>
    [ObservableProperty]
    private bool _fastModrinth;
    /// <summary>
    /// 是否检测用户是否占用
    /// </summary>
    [ObservableProperty]
    private bool _checkUser;
    /// <summary>
    /// 是否检测加载器是否启用
    /// </summary>
    [ObservableProperty]
    private bool _checkLoader;
    /// <summary>
    /// 是否检测内存分配
    /// </summary>
    [ObservableProperty]
    private bool _checkMemory;
    /// <summary>
    /// 是否启动后关闭启动器
    /// </summary>
    [ObservableProperty]
    private bool _closeBefore;
    /// <summary>
    /// 是否启用安全Log4j
    /// </summary>
    [ObservableProperty]
    private bool _safeLog4j;
    /// <summary>
    /// 是否管理员启动
    /// </summary>
    [ObservableProperty]
    private bool _adminLaunch;
    /// <summary>
    /// 是否游戏管理员启动
    /// </summary>
    [ObservableProperty]
    private bool _gameAdminLaunch;

    private bool _launchLoad;

    //配置修改
    partial void OnGameAdminLaunchChanged(bool value)
    {
        if (_serverLoad)
        {
            return;
        }

        SetAdmin();
    }

    partial void OnAdminLaunchChanged(bool value)
    {
        if (_serverLoad)
        {
            return;
        }

        SetAdmin();
    }

    partial void OnSafeLog4jChanged(bool value)
    {
        if (_launchLoad)
        {
            return;
        }

        ConfigBinding.SetSafeLog4j(value);
    }

    partial void OnCloseBeforeChanged(bool value)
    {
        if (_launchLoad)
        {
            return;
        }

        ConfigBinding.SetLaunchCloseConfig(value);
    }

    partial void OnCheckUserChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckLoaderChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckMemoryChanged(bool value)
    {
        SetCheck();
    }

    partial void OnFastModrinthChanged(bool value)
    {
        if (_launchLoad)
        {
            return;
        }

        ConfigBinding.SetLauncherFunction(FastEnable, value);

        if (value)
        {
            if (!ProcessUtils.IsRunAsAdmin())
            {
                try
                {
                    ProcessUtils.LaunchAdmin(["--register", "--remap_modrinth"]);
                }
                catch
                {

                }
            }
            else
            {
                HookUtils.RegisterFastLaunch(true);
            }
        }
    }

    partial void OnFastEnableChanged(bool value)
    {
        if (_launchLoad)
        {
            return;
        }

        ConfigBinding.SetLauncherFunction(value, FastModrinth);

        if (value)
        {
            if (!ProcessUtils.IsRunAsAdmin())
            {
                try
                {
                    ProcessUtils.LaunchAdmin(["--register"]);
                }
                catch
                {

                }
            }
            else
            {
                HookUtils.RegisterFastLaunch(false);
                if (FastModrinth)
                {
                    HookUtils.RegisterFastLaunch(true);
                }
            }
        }
        else
        {
            if (!ProcessUtils.IsRunAsAdmin())
            {
                try
                {
                    ProcessUtils.LaunchAdmin(["--register"]);
                }
                catch
                {

                }
            }
            else
            {
                HookUtils.DeleteFastLaunch();
            }
        }
    }

    //保存配置
    private void SetCheck()
    {
        if (_launchLoad)
        {
            return;
        }

        ConfigBinding.SetCheck(CheckUser, CheckLoader, CheckMemory);
    }

    private void SetAdmin()
    {
        ConfigBinding.SetAdmin(AdminLaunch, GameAdminLaunch);
    }

    /// <summary>
    /// 加载启动器功能设置
    /// </summary>
    public void LoadLaunch()
    {
        _launchLoad = true;

        var config = GuiConfigUtils.Config.LauncherFunction;
        if (config is { } con)
        {
            FastEnable = con.FastLaunch;
            FastModrinth = con.FastModrinth;
            AdminLaunch = config.AdminLaunch;
            GameAdminLaunch = config.GameAdminLaunch;
        }

        var config1 = GuiConfigUtils.Config;
        if (config1 is { } con1)
        {
            CloseBefore = con1.CloseBeforeLaunch;

            CheckUser = con1.LaunchCheck.CheckUser;
            CheckLoader = con1.LaunchCheck.CheckLoader;
            CheckMemory = con1.LaunchCheck.CheckMemory;
        }

        _launchLoad = false;
    }
}
