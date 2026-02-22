using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Tab12.Text16"), model.SchematicItem!.Obj.BlockCount > 0, ()=>
            {
                model.DisplayBlocks();
            }),
            new FlyoutMenuModel(LangUtils.Get("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(model.SchematicItem!.Local);
            }),
            new FlyoutMenuModel(LangUtils.Get("Button.Delete"), true, model.DeleteSchematic)
        ]).Show(con);
    }
}