using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout3
{
    private readonly ResourcepackDisplayObj Obj;
    private readonly Tab8Control Con;

    public GameEditFlyout3(Tab8Control con, ResourcepackDisplayObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts3.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        Con.Delete(Obj);
    }

    private void Button1_Click()
    {
        BaseBinding.OpFile(Obj.Local);
    }
}
