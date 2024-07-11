using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

public class LockLoginFlyout
{
    public LockLoginFlyout(Control con, LockLoginModel model)
    {
        _ = new FlyoutsControl(
        [
            (App.Lang("Button.Delete"), true, model.Delete)
        ], con);
    }
}
