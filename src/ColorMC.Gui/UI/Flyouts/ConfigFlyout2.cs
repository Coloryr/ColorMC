using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LanguageUtils.Get("Button.Delete"), true, () =>
            {
                model.DeleteItem(item);
            }),
        ]).Show(con);
    }
}
