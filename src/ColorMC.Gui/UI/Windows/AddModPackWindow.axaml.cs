using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public partial class AddModPackWindow : SelfBaseWindow
{
    public AddModPackWindow()
    {
        Main = new AddModPackControl();
        SetTitle("AddModPackWindow.Title");
    }
}
