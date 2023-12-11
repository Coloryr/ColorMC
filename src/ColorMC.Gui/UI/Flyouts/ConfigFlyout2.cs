using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.GameConfigEdit;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout2
{
    public ConfigFlyout2(Control con, NbtDialogEditModel model, NbtDataItemModel item)
    {
        _ = new FlyoutsControl(
        [
            (App.Lang("Button.Delete"), true, () =>
            {
                model.DeleteItem(item);
            }),
        ], con);
    }
}
