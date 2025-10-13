using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 主界面
/// 多选后右键菜单
/// </summary>
public static class MainFlyout2
{
    public static void Show(Control con, GameGroupModel model, IMutTop top)
    {
        var list = top.GetMut();
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text22"),
                model.GameList.Any(item=>!item.IsNew), model.MutAll),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text21"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), top.MutLaunch),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text2"),
                list.Count != 0, top.MutEdit),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text7"),
                list.Count != 0, top.MutEditGroup),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text11"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), top.MutDelete)
        ]).Show(con);
    }
}
