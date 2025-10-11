using System.Collections;
using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 启动器设置
/// Java右键菜单
/// </summary>
public static class SettingFlyout1
{
    public static void Show(Control con, SettingModel model, IList list)
    {
        var java = list.Cast<JavaDisplayModel>();

        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("SettingWindow.Flyouts.Text1"), true, ()=>
            {
                foreach (var item in java)
                {
                    JavaBinding.RemoveJava(item.Name);
                }

                model.LoadJava();
            }),
        ]).Show(con);
    }
}
