using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LangUtils.Get("MainWindow.Text62"),
                model.GameList.Any(item=>!item.IsNew), model.MutAll),
            new FlyoutMenuModel(LangUtils.Get("MainWindow.Text61"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), top.MutLaunch),
            new FlyoutMenuModel(LangUtils.Get("MainWindow.Text43"),
                list.Count != 0, top.MutEdit),
            new FlyoutMenuModel(LangUtils.Get("MainWindow.Text47"),
                list.Count != 0, top.MutEditGroup),
            new FlyoutMenuModel(LangUtils.Get("MainWindow.Text51"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), top.MutDelete)
        ]).Show(con);
    }
}
