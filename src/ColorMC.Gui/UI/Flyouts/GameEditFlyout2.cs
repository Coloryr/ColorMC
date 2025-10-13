using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 存档右键菜单
/// </summary>
public static class GameEditFlyout2
{
    public static void Show(Control con, WorldModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenPath(model.World);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text11"), CheckHelpers.IsGameVersion120(model.World.Game.Version), ()=>
            {
                model.TopModel.LaunchWorld(model);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text7"), true, ()=>
            {
                model.TopModel.Export(model);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text10"), true, ()=>
            {
                WindowManager.ShowConfigEdit(model.World);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text9"), !model.World.Broken, ()=>
            {
                model.TopModel.BackupWorld(model);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text8"), !model.World.Broken, ()=>
            {
                model.TopModel.DeleteWorld(model);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text14"), !model.World.Broken, ()=>
            {
                GameBinding.OpenWorldSeed(model);
            })
        ]).Show(con);
    }
}
