using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout7
{
    private readonly GameEditModel _model;
    public GameEditFlyout7(Control con, GameEditModel model)
    {
        _model = model;

        var fy = new FlyoutsControl(new()
        {
            (App.Lang("Button.OpFile"), true, Button1_Click),
            (App.Lang("Button.Delete"), true, Button2_Click)
        }, con);
    }

    private void Button1_Click()
    {
        PathBinding.OpFile(_model.SchematicItem!.Local);
    }

    private void Button2_Click()
    {
        _model.DeleteSchematic(_model.SchematicItem!);
    }
}