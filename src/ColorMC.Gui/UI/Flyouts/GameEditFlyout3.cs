using Avalonia.Controls;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout3
{
    private readonly ResourcepackObj _obj;
    private readonly GameEditTab8Model _top;

    public GameEditFlyout3(Control con, ResourcePackModel model)
    {
        _top = model.Top;
        _obj = model.Pack;
        
        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts3.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        _top.Delete(_obj);
    }

    private void Button1_Click()
    {
        PathBinding.OpFile(_obj.Local);
    }
}
