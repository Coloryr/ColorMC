using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout4
{
    private readonly ScreenshotModel Model;
    public GameEditFlyout4(Control con, ScreenshotModel model)
    {
        Model = model;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Tab9.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        Model.Delete();
    }

    private void Button1_Click()
    {
        BaseBinding.OpFile(Model.Screenshot.Local);
    }
}
