using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public class AddWindow : SelfBaseWindow
{
    public AddWindow()
    {
        var con = new AddControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
