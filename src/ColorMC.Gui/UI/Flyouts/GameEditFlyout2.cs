using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout2
{
    private readonly WorldDisplayObj Obj;
    private readonly Tab5Control Con;

    public GameEditFlyout2(Tab5Control con, WorldDisplayObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text1"), true, Button2_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text4"), true, Button5_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text3"), true, Button3_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text2"), true, Button4_Click)
        }, con);
    }

    private void Button5_Click()
    {
        App.ShowConfigEdit(Obj.World);
    }

    private void Button4_Click()
    {
        Con.Backup(Obj);
    }

    private void Button3_Click()
    {
        Con.Delete(Obj);
    }

    private void Button2_Click()
    {
        Con.Export(Obj);
    }

    private void Button1_Click()
    {
        BaseBinding.OpPath(Obj.Local);
    }
}
