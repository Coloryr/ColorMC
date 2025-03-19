using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 结构文件右键菜单
/// </summary>
public class GameEditFlyout7
{
    private readonly GameEditModel _model;

    public GameEditFlyout7(Control con, GameEditModel model)
    {
        _model = model;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(_model.SchematicItem!.Local);
            }),
            new FlyoutMenuObj(App.Lang("Button.Delete"), true, ()=>
            {
                _model.DeleteSchematic(_model.SchematicItem!);
            })
        ], con);
    }
}