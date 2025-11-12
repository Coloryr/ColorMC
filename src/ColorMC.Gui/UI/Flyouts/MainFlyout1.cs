using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text60"), true, model.Top.StartMut),
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text62"), model.GameList.Any(item=>!item.IsNew), ()=>
            {
                model.Top.StartMut(model);
            })
        ]).Show(con);
    }
}
