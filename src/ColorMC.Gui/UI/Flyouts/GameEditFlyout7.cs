using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 结构文件右键菜单
/// </summary>
public static class GameEditFlyout7
{
    public static void Show(Control con, GameEditModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(model.SchematicItem!.Local);
            }),
            new FlyoutMenuModel(App.Lang("Button.Delete"), true, model.DeleteSchematic)
        ]).Show(con);
    }
}