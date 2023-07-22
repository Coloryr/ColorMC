using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Flyouts;

public class ServerPackFlyout1
{
    private readonly ServerPackConfigDisplayObj _obj;
    private readonly ServerPackTab4Model _model;
    public ServerPackFlyout1(Control con, ServerPackTab4Model model, ServerPackConfigDisplayObj obj)
    {
        _model = model;
        _obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
        }, con);
    }

    private void Button1_Click()
    {
        _model.Delete(_obj);
    }
}
