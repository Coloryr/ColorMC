using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 资源包右键菜单
/// </summary>
public static class GameEditFlyout3
{
    public static void Show(Control con, ResourcePackModel model)
    {
        var obj = model.Obj;

        new FlyoutsControl(
        [
            new FlyoutMenuModel(LangUtils.Get("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(obj.Local);
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text12"), true, model.DeleteResource)
        ]).Show(con);
    }
}
