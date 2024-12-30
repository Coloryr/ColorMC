using System.Linq;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout2
{
    public MainFlyout2(Control con, IMutTop model)
    {
        var list = model.GetMut();
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text21"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), model.MutLaunch),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text2"),
                list.Count != 0, model.MutEdit),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text7"),
                list.Count != 0, model.MutEditGroup),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text11"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), model.MutDelete)
        ], con);
    }

    public MainFlyout2(Control con, GameGroupModel model, IMutTop top)
    {
        var list = top.GetMut();
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text22"),
                model.GameList.Any(item=>!item.IsNew), model.MutAll),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text21"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), top.MutLaunch),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text2"),
                list.Count != 0, top.MutEdit),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text7"),
                list.Count != 0, top.MutEditGroup),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text11"),
                list.Count != 0 && !list.Any(item=>GameManager.IsGameRun(item.Obj)), top.MutDelete)
        ], con);
    }
}
