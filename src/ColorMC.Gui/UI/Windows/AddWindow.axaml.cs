using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public class AddWindow : SelfBaseWindow
{
    public AddWindow()
    {
        Main = new AddControl();
        SetTitle("AddWindow.Title");
    }
}
