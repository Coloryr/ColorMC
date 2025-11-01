using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 方块选中右键
/// </summary>
public static class BlockFlyout
{
    public static void Show(Control con, BlockItemModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(LanguageUtils.Get("BlockBackpackFlyout.Text1"), true, model.Use)
        ]).Show(con);
    }
}
