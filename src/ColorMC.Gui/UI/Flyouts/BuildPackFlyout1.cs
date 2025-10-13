using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.BuildPack;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 导出客户端页面
/// 其他文件选择右键菜单
/// </summary>
public static class BuildPackFlyout1
{
    public static void Show(Control con, BuildPackModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("SettingWindow.Flyouts.Text2"), true, model.DeleteFile),
        ]).Show(con);
    }
}
