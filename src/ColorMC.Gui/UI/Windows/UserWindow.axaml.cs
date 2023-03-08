using ColorMC.Gui.UI.Controls.User;

namespace ColorMC.Gui.UI.Windows;

public partial class UserWindow : SelfBaseWindow
{
    public UserWindow()
    {
        var con = new UsersControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
