using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public class AddJavaWindow : SelfBaseWindow
{
    public AddJavaWindow()
    {
        var con = new AddJavaControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
