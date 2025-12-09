using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 设置页面
/// 手柄按钮右键菜单
/// </summary>
public static class SettingFlyout2
{
    public static void Show(Control con, SettingModel model, InputButtonModel data)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(LangUtils.Get("SettingWindow.Flyouts.Text2"), true, ()=>
            {
                if(data is InputButtonModel key)
                {
                    model.SetKeyButton(key);
                }
            }),
            new FlyoutMenuModel(LangUtils.Get("SettingWindow.Flyouts.Text3"), true, ()=>
            {
                if(data is InputButtonModel key)
                {
                    model.DeleteInput(key);
                }
            }),
        ]).Show(con);
    }
}
