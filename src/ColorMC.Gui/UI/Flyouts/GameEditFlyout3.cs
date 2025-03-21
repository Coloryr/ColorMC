using Avalonia.Controls;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 资源包右键菜单
/// </summary>
public class GameEditFlyout3
{
    public GameEditFlyout3(Control con, ResourcePackModel model)
    {
        var obj = model.Obj;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(obj.Local);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text12"), true, ()=>
            {
                model.DeleteResource();
            })
        ], con);
    }
}
