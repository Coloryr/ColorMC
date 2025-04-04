﻿using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 存档右键菜单
/// </summary>
public class GameEditFlyout2
{
    private readonly WorldModel _model;

    public GameEditFlyout2(Control con, WorldModel model)
    {
        _model = model;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenPath(_model.World);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text11"), CheckHelpers.IsGameVersion120(_model.World.Game.Version), ()=>
            {
                _model.TopModel.LaunchWorld(_model);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text7"), true, ()=>
            {
                _model.TopModel.Export(_model);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text10"), true, ()=>
            {
                WindowManager.ShowConfigEdit(_model.World);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text9"), !_model.World.Broken, ()=>
            {
                _model.TopModel.BackupWorld(_model);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text8"), !_model.World.Broken, ()=>
            {
                _model.TopModel.DeleteWorld(_model);
            })
        ], con);
    }
}
