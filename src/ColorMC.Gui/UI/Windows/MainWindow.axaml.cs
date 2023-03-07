using ColorMC.Gui.UI.Controls.Main;

namespace ColorMC.Gui.UI.Windows;

public class MainWindow : SelfBaseWindow
{
    public MainWindow()
    {
        Main = new MainControl();
        MainControl.Children.Add(Main.Con);
        SetTitle("MainWindow.Title");
    }
}
