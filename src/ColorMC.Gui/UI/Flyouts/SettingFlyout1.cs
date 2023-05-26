using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout1
{
    private readonly IEnumerable<JavaDisplayObj> List;
    private readonly SettingTab5Model Model;
    public SettingFlyout1(Control con, SettingTab5Model model, IList list)
    {
        Model = model;
        List = list.Cast<JavaDisplayObj>();

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("SettingWindow.Flyouts.Text1"), true, Button1_Click),
        }, con);
    }

    private void Button1_Click()
    {
        foreach (var item in List)
        {
            JavaBinding.RemoveJava(item.Name);
        }

        Model.Load();
    }
}
