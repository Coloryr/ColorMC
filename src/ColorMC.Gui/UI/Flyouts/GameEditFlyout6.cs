﻿using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout6
{
    private readonly GameEditModel _obj;
    public GameEditFlyout6(Control con, GameEditModel obj)
    {
        _obj = obj;

        _ = new FlyoutsControl(
        [
            (App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(_obj.ShaderpackItem!.Local);
            }),
            (App.Lang("Button.Delete"), true, ()=>
            {
                _obj.DeleteShaderpack(_obj.ShaderpackItem!);
            })
        ], con);
    }
}