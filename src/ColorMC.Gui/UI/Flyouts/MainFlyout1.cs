using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout1
{
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
