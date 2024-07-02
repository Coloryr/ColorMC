using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;

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
