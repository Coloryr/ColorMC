using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout6
{
    private readonly GameEditTab11Model Obj;
    public GameEditFlyout6(Control con, GameEditTab11Model obj)
    {
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("Button.Delete"), true, Button2_Click)
        }, con);
    }

    private void Button1_Click()
    {
        BaseBinding.OpFile(Obj.Item!.Local);
    }

    private void Button2_Click()
    {
        Obj.Delete(Obj.Item!);
    }
}