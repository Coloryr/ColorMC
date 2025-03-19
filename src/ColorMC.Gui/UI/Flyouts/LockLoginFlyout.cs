using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 客户端定制
/// 账户锁定右键菜单
/// </summary>
public class LockLoginFlyout
{
    public LockLoginFlyout(Control con, LockLoginModel model)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.Delete"), true, model.Delete)
        ], con);
    }
}
