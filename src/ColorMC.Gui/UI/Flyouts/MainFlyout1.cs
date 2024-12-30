using System.Linq;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout1
{
    public MainFlyout1(Control con, MainModel model)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text20"), true, model.StartMut),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text22"), false, null)
        ], con);
    }

    public MainFlyout1(Control con, GameGroupModel model)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text20"), true, model.Top.StartMut),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text22"), model.GameList.Any(item=>!item.IsNew), ()=>
            {
                model.Top.StartMut(model);
            })
        ], con);
    }
}
