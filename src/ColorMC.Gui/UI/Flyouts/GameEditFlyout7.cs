using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout7
{
    private readonly GameEditTab12Model Model;
    public GameEditFlyout7(Control con, GameEditTab12Model model)
    {
        Model = model;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("Button.Delete"), true, Button2_Click)
        }, con);
    }

    private void Button1_Click()
    {
        BaseBinding.OpFile(Model.Item!.Local);
    }

    private void Button2_Click()
    {
        Model.Delete(Model.Item!);
    }
}