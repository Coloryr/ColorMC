using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
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
            new FlyoutMenuModel(LanguageUtils.Get("Button.Delete"), true, model.Delete)
        ]).Show(con);
    }
}
