using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Setting;

namespace ColorMC.Gui.UI.Windows;

public partial class SettingWindow : SelfBaseWindow
{
    public SettingWindow()
    {
        var con = new SettingControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
