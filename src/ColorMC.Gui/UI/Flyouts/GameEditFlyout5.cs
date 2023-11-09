using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout5
{
    private readonly Control _con;
    private readonly GameEditModel _model;
    public GameEditFlyout5(Control con, GameEditModel model)
    {
        _con = con;
        _model = model;

        _ = new FlyoutsControl(new (string, bool, Action)[]
        {
            (App.Lang("Button.Delete"), true, Button1_Click),
            (App.Lang("GameEditWindow.Flyouts5.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        GameBinding.CopyServer(_model.ServerItem!);
    }

    private void Button1_Click()
    {
        _model.DeleteServer(_model.ServerItem!);
    }
}
