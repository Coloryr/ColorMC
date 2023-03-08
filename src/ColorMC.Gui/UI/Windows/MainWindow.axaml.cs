using ColorMC.Gui.UI.Controls.Main;

namespace ColorMC.Gui.UI.Windows;

public class MainWindow : SelfBaseWindow
{
    public MainWindow()
    {
        var con = new MainControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
