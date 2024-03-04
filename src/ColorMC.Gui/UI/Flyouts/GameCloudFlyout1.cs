using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameCloudFlyout1
{
    private readonly WorldCloudModel _model;

    public GameCloudFlyout1(Control con, WorldCloudModel model)
    {
        _model = model;

        _ = new FlyoutsControl(
        [
            (App.Lang("Button.OpFile"), model.HaveLocal, () =>
            {
                PathBinding.OpPath(_model.World);
            }),
            (App.Lang("GameCloudWindow.Flyouts.Text1"), model.HaveLocal, _model.Upload),
            (App.Lang("GameCloudWindow.Flyouts.Text2"), model.HaveCloud, _model.Download),
            (App.Lang("GameCloudWindow.Flyouts.Text3"), model.HaveCloud, _model.DeleteCloud),
        ], con);
    }
}
