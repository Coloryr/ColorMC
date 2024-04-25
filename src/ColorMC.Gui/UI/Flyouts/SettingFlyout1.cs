using System.Collections;
using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout1
{
    public SettingFlyout1(Control con, SettingModel model, IList list)
    {
        var java = list.Cast<JavaDisplayObj>();

        _ = new FlyoutsControl(
        [
            (App.Lang("SettingWindow.Flyouts.Text1"), true, ()=>
            {
                foreach (var item in java)
                {
                    JavaBinding.RemoveJava(item.Name);
                }

                model.LoadJava();
            }),
        ], con);
    }
}
