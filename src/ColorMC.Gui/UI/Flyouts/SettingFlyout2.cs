using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 设置页面
/// 手柄按钮右键菜单
/// </summary>
public class SettingFlyout2
{
    public SettingFlyout2(Control con, SettingModel model, InputButtonModel data)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("SettingWindow.Flyouts.Text2"), true, ()=>
            {
                if(data is InputButtonModel key)
                {
                    model.SetKeyButton(key);
                }
            }),
            new FlyoutMenuObj(App.Lang("SettingWindow.Flyouts.Text3"), true, ()=>
            {
                if(data is InputButtonModel key)
                {
                    model.DeleteInput(key);
                }
            }),
        ], con);
    }
}
