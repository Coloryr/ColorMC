using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Flyouts;

public class ServerPackFlyout1
{
    public ServerPackFlyout1(Control con, ServerPackModel model, ServerPackConfigModel obj)
    {
        _ = new FlyoutsControl(
        [
            (App.Lang("Button.Delete"), true, ()=>
            {
                model.DeleteFile(obj);
            }),
        ], con);
    }
}
