using Avalonia.Controls;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout5
{
    private readonly ServerInfoObj Obj;
    private readonly Tab10Control Con;
    public GameEditFlyout5(Tab10Control con, ServerInfoObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts5.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        GameBinding.CopyServer(TopLevel.GetTopLevel(Con), Obj);
    }

    private void Button1_Click()
    {
        Con.Delete(Obj);
    }
}
