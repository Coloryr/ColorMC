using ColorMC.Gui.UI.Controls.Skin;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : SelfBaseWindow
{
    public SkinWindow()
    {
        var con = new SkinControl();
        Main = con;
        MainControl.Children.Add(con);
        Main.Update();
    }
}


