using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public class AddGameWindow : SelfBaseWindow
{
    public AddGameWindow()
    {
        Main = new AddGameControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("AddGameWindow.Title");
    }

    public new void Closed()
    {
        App.AddGameWindow = null;
    }
}
