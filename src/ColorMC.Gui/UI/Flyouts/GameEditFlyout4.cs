using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 截图右键菜单
/// </summary>
public class GameEditFlyout4
{
    private readonly ScreenshotModel _model;

    public GameEditFlyout4(Control con, ScreenshotModel model)
    {
        _model = model;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(_model.Screenshot);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Tab9.Text1"), true, ()=>
            {
                _model.TopModel.DeleteScreenshot(_model);
            })
        ], con);
    }
}
