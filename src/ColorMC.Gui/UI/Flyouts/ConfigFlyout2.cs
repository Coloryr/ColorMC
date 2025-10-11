using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 配置文件编辑页面
/// Nbt标签列表右键菜单
/// </summary>
public static class ConfigFlyout2
{
    public static void Show(Control con, NbtDialogEditModel model, NbtDataItemModel item)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("Button.Delete"), true, () =>
            {
                model.DeleteItem(item);
            }),
        ]).Show(con);
    }
}
