using ColorMC.Gui.UI.Controls.User;

namespace ColorMC.Gui.UI.Windows;

public partial class UserWindow : SelfBaseWindow
{
    public UserWindow()
    {
        Main = new UsersControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("UserWindow.Title");
    }

    public void AddUrl(string url)
    {
        (Main as UsersControl)?.AddUrl(url);
    }

    private new void Closed()
    {
        App.UserWindow = null;
    }
}
