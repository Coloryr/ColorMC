using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 客户端定制
/// 账户锁定右键菜单
/// </summary>
public static class LockLoginFlyout
{
    public static void Show(Control con, LockLoginModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("Button.Delete"), true, model.Delete)
        ]).Show(con);
    }
}
