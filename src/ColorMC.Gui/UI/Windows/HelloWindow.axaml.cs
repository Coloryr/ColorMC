using ColorMC.Gui.UI.Controls.Hello;

namespace ColorMC.Gui.UI.Windows;

public partial class HelloWindow : SelfBaseWindow
{
    public HelloWindow()
    {
        var con = new HelloControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
