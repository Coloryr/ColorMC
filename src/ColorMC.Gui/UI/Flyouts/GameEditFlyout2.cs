using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout2
{
    private readonly WorldModel Model;

    public GameEditFlyout2(Control con, WorldModel model)
    {
        Model = model;

        _ = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text1"), true, Button2_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text4"), true, Button5_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text3"), !model.World.World.Broken, Button3_Click),
            (App.GetLanguage("GameEditWindow.Flyouts2.Text2"), !model.World.World.Broken, Button4_Click)
        }, con);
    }

    private void Button5_Click()
    {
        App.ShowConfigEdit(Model.World.World);
    }

    private void Button4_Click()
    {
        Model.Delete(Model.World);
    }

    private void Button3_Click()
    {
        Model.Backup(Model);
    }

    private void Button2_Click()
    {
        Model.Export(Model.World);
    }

    private void Button1_Click()
    {
        BaseBinding.OpPath(Model.World.Local);
    }
}
