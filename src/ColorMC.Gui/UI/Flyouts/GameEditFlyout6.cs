using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout6
{
    private readonly GameEditModel _obj;
    public GameEditFlyout6(Control con, GameEditModel obj)
    {
        _obj = obj;

        _ = new FlyoutsControl(new (string, bool, Action)[]
        {
            (App.Lang("Button.OpFile"), true, Button1_Click),
            (App.Lang("Button.Delete"), true, Button2_Click)
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