using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

public static class BlockFlyout
{
    public static void Show(Control con, BlockItemModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("BlockBackpackFlyout.Text1"), true, model.Use)
        ]).Show(con);
    }
}
