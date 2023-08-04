using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout3
{
    private readonly ResourcepackDisplayObj _obj;
    private readonly ResourcePackModel _model;

    public GameEditFlyout3(Control con, ResourcePackModel model, ResourcepackDisplayObj obj)
    {
        _obj = obj;
        _model = model;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts3.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        _model.Delete(_obj);
    }

    private void Button1_Click()
    {
        PathBinding.OpFile(_obj.Local);
    }
}
