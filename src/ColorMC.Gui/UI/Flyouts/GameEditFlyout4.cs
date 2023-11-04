using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout4
{
    private readonly ScreenshotModel _model;
    public GameEditFlyout4(Control con, ScreenshotModel model)
    {
        _model = model;

        var fy = new FlyoutsControl(new()
        {
            (App.Lang("Button.OpFile"), true, Button1_Click),
            (App.Lang("GameEditWindow.Tab9.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        _model.Top.DeleteScreenshot(_model);
    }

    private void Button1_Click()
    {
        PathBinding.OpFile(_model.Screenshot);
    }
}
