using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout1
{
    private readonly IEnumerable<JavaDisplayObj> List;
    private readonly Tab5Control Con;
    public SettingFlyout1(Tab5Control con, IList obj)
    {
        Con = con;
        List = obj.Cast<JavaDisplayObj>();

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
        
        Con.Load();
    }
}
