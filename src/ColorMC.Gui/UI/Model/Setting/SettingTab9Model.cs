using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    [ObservableProperty]
    private bool _fastEnable;
    [ObservableProperty]
    private bool _fastModrinth;

    private bool _launchLoad;

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

    /// <summary>
    /// 加载启动器功能设置
    /// </summary>
    public void LoadLaunch()
    {
        _launchLoad = true;

        var config = GuiConfigUtils.Config.LauncherFunction;
        FastEnable = config.FastLaunch;
        FastModrinth = config.FastModrinth;

        _launchLoad = false;
    }
}
