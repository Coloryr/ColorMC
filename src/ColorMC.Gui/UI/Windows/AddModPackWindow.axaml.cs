using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public partial class AddModPackWindow : SelfBaseWindow
{
    public AddModPackWindow()
    {
        var con = new AddModPackControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
