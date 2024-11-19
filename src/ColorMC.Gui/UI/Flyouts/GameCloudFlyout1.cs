using Avalonia.Controls;
using ColorMC.Gui.Objs;
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
            new FlyoutMenuObj(App.Lang("Button.OpFile"), model.HaveLocal, () =>
            {
                PathBinding.OpenPath(_model.World);
            }),
            new FlyoutMenuObj(App.Lang("GameCloudWindow.Flyouts.Text1"),
                model.HaveLocal, _model.Upload),
            new FlyoutMenuObj(App.Lang("GameCloudWindow.Flyouts.Text2"),
                model.HaveCloud, _model.Download),
            new FlyoutMenuObj(App.Lang("GameCloudWindow.Flyouts.Text3"),
                model.HaveCloud,_model.DeleteCloud),
        ], con);
    }
}
