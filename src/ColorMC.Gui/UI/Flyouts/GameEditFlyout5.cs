using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout5
{
    private readonly Control Con;
    private readonly GameEditTab10Model Model;
    public GameEditFlyout5(Control con, GameEditTab10Model model)
    {
        Con = con;
        Model = model;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts5.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        GameBinding.CopyServer(TopLevel.GetTopLevel(Con), Model.Item!);
    }

    private void Button1_Click()
    {
        Model.Delete(Model.Item!);
    }
}
