using ColorMC.Gui.UI.Controls.Hello;

namespace ColorMC.Gui.UI.Windows;

public partial class HelloWindow : SelfBaseWindow
{
    public HelloWindow()
    {
        Main = new HelloControl();
        MainControl.Children.Add(Main.Con);
        SetTitle("MainWindow.Title");
    }
}
