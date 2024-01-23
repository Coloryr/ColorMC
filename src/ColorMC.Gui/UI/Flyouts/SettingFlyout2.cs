using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout2
{
    public SettingFlyout2(Control con, SettingModel model, InputButtonModel data)
    {
        _ = new FlyoutsControl(
        [
            (App.Lang("SettingWindow.Flyouts.Text2"), true, ()=>
            {
                if(data is InputButtonModel key)
                {
                    model.SetKeyButton(key);
                }
            }),
            (App.Lang("SettingWindow.Flyouts.Text3"), true, ()=>
            {
                if(data is InputButtonModel key)
                {
                    model.DeleteInput(key);
                }
            }),
        ], con);
    }
}
