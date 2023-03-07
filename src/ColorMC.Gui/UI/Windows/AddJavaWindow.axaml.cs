using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public class AddJavaWindow : SelfBaseWindow
{
    public AddJavaWindow()
    {
        Main = new AddJavaControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("AddJavaWindow.Title");
    }

    private new void Closed()
    {
        App.AddJavaWindow = null;
    }
}
