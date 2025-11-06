using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 设置界面
/// DNS右键菜单
/// </summary>
public static class SettingFlyout3
{
    public static void Show(Control con, SettingModel model, DnsItemModel data)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(LanguageUtils.Get("Button.Delete"), true, ()=>
            {
                 model.DeleteDns(data);
            }),
        ]).Show(con);
    }
}
