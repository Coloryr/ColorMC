using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Flyouts;

public class ServerPackFlyout1
{
    private readonly ServerPackConfigDisplayObj Obj;
    private readonly ServerPackTab4Model Model;
    public ServerPackFlyout1(Control con, ServerPackTab4Model model, ServerPackConfigDisplayObj obj)
    {
        Model = model;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
        }, con);
    }

    private void Button1_Click()
    {
        Model.Delete(Obj);
    }
}
