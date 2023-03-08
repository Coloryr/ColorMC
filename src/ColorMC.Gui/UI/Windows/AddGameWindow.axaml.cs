using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public class AddGameWindow : SelfBaseWindow
{
    public AddGameWindow()
    {
        var con = new AddGameControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
