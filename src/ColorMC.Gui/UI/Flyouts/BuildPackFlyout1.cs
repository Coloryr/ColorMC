using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.BuildPack;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 导出客户端页面
/// 其他文件选择右键菜单
/// </summary>
public class BuildPackFlyout1
{
    public BuildPackFlyout1(Control con, BuildPackModel model)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("SettingWindow.Flyouts.Text2"), true, model.DeleteFile),
        ], con);
    }
}
