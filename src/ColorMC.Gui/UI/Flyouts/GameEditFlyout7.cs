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

        _ = new FlyoutsControl(
        [
            (App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpFile(_model.SchematicItem!.Local);
            }),
            (App.Lang("Button.Delete"), true, ()=>
            {
                _model.DeleteSchematic(_model.SchematicItem!);
            })
        ], con);
    }
}