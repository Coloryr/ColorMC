using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout6
{
    private readonly GameEditModel _obj;
    public GameEditFlyout6(Control con, GameEditModel obj)
    {
        _obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("Button.Delete"), true, Button2_Click)
        }, con);
    }

    private void Button1_Click()
    {
        PathBinding.OpFile(_obj.ShaderpackItem!.Local);
    }

    private void Button2_Click()
    {
        _obj.DeleteShaderpack(_obj.ShaderpackItem!);
    }
}