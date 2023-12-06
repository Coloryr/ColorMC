using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout2
{
    private readonly WorldModel _model;

    public GameEditFlyout2(Control con, WorldModel model)
    {
        _model = model;

        _ = new FlyoutsControl(
        [
            (App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpPath(_model.World);
            }),
            (App.Lang("GameEditWindow.Flyouts2.Text5"), CheckHelpers.IsGameVersion120(_model.World.Game.Version), ()=>
            {
                _model.Top.LaunchWorld(_model);
            }),
            (App.Lang("GameEditWindow.Flyouts2.Text1"), true, ()=>
            {
                _model.Top.Export(_model);
            }),
            (App.Lang("GameEditWindow.Flyouts2.Text4"), true, ()=>
            {
                App.ShowConfigEdit(_model.World);
            }),
            (App.Lang("GameEditWindow.Flyouts2.Text3"), !_model.World.Broken, ()=>
            {
                _model.Top.BackupWorld(_model);
            }),
            (App.Lang("GameEditWindow.Flyouts2.Text2"), !_model.World.Broken, ()=>
            {
                _model.Top.DeleteWorld(_model);
            })
        ], con);
    }
}
