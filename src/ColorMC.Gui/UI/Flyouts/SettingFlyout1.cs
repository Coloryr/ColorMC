using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout1
{
    private JavaDisplayObj Obj;
    private Tab5Control Con;
    public SettingFlyout1(Tab5Control con, JavaDisplayObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("SettingWindow.Flyouts.Text1"), true, Button1_Click),
        }, con);
    }

    private void Button1_Click()
    {
        JavaBinding.RemoveJava(Obj.Name);
        Con.Load();
    }
}
