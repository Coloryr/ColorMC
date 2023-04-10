using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Server;

namespace ColorMC.Gui.UI.Flyouts;

public class ServerPackFlyout1
{
    private readonly ServerPackConfigDisplayObj Obj;
    private readonly Tab4Control Con;
    public ServerPackFlyout1(Tab4Control con, ServerPackConfigDisplayObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
        }, con);
    }

    private void Button1_Click()
    {
        Con.Delete(Obj);
    }
}
