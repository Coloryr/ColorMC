using System;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

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
            (App.Lang("Button.Delete"), true, ()=>
            {
                _model.DeleteServer(_model.ServerItem!);
            }),
            (App.Lang("GameEditWindow.Flyouts.Text13"), true, ()=>
            {
                var top =TopLevel.GetTopLevel(con);
                if (top == null)
                {
                    return;
                }
                GameBinding.CopyServer(top, _model.ServerItem!);
            })
        }, con);
    }
}
