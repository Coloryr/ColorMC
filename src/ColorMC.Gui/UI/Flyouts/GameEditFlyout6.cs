using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 光影包右键菜单
/// </summary>
public class GameEditFlyout6
{
    private readonly GameEditModel _obj;

    public GameEditFlyout6(Control con, GameEditModel obj)
    {
        _obj = obj;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(_obj.ShaderpackItem!.Local);
            }),
            new FlyoutMenuObj(App.Lang("Button.Delete"), true, ()=>
            {
                _obj.DeleteShaderpack(_obj.ShaderpackItem!);
            })
        ], con);
    }
}