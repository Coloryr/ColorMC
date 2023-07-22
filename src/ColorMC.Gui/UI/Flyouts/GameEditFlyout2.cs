using Avalonia.Controls;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout2
{
    private readonly WorldModel _model;

    public GameEditFlyout2(Control con, WorldModel model)
    {
        _model = model;

        _ = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text5"), CheckRule.IsGameLaunchVersion120(model.World.World.Game.Version), Button6_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text1"), true, Button2_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text4"), true, Button5_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text3"), !model.World.World.Broken, Button3_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text2"), !model.World.World.Broken, Button4_Click)
        }, con);
    }

    private void Button6_Click()
    {
        _model.Launch(_model.World);
    }

    private void Button5_Click()
    {
        App.ShowConfigEdit(_model.World.World);
    }

    private void Button4_Click()
    {
        _model.Delete(_model.World);
    }

    private void Button3_Click()
    {
        _model.Backup(_model);
    }

    private void Button2_Click()
    {
        _model.Export(_model.World);
    }

    private void Button1_Click()
    {
        BaseBinding.OpPath(_model.World.World);
    }
}
