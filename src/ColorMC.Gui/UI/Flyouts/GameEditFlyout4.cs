using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 截图右键菜单
/// </summary>
public static class GameEditFlyout4
{
    public static void Show(Control con, ScreenshotModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(model.Screenshot);
            }),
            new FlyoutMenuModel(App.Lang("GameEditWindow.Tab9.Text1"), true, ()=>
            {
                model.TopModel.DeleteScreenshot(model);
            })
        ]).Show(con);
    }
}
