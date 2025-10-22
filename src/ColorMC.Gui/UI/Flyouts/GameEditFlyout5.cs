using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 服务器右键菜单
/// </summary>
public static class GameEditFlyout5
{
    public static void Show(Control con, GameEditModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("Button.Delete"), true, ()=>
            {
                model.DeleteServer(model.ServerItem!);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Flyouts.Text13"), true, ()=>
            {
                var top =TopLevel.GetTopLevel(con);
                if (top == null || model.ServerItem == null)
                {
                    return;
                }
                BaseBinding.CopyTextClipboardAsync(top, model.ServerItem!.IP);
            })
        ]).Show(con);
    }
}
