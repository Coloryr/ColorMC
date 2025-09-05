using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    [ObservableProperty]
    private bool _fastEnable;

    private bool _launchLoad;

    partial void OnFastEnableChanged(bool value)
    {
        if (_launchLoad)
        {
            return;
        }

        ConfigBinding.SetLauncherFunction(value);

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
                ToolUtils.RegisterFastLaunch();
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
                ToolUtils.DeleteFastLaunch();
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

        _launchLoad = false;
    }
}
