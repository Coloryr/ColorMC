using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 光影包右键菜单
/// </summary>
public static class GameEditFlyout6
{
    public static void Show(Control con, GameEditModel obj)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(obj.ShaderpackItem!.Local);
            }),
            new FlyoutMenuObj(App.Lang("Button.Delete"), true, obj.DeleteShaderpack)
        ]).Show(con);
    }
}