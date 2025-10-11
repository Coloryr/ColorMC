using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 主界面
/// 多选右键菜单
/// </summary>
public static class MainFlyout1
{
    public static void Show(Control con, GameGroupModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text20"), true, model.Top.StartMut),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text22"), model.GameList.Any(item=>!item.IsNew), ()=>
            {
                model.Top.StartMut(model);
            })
        ]).Show(con);
    }
}
