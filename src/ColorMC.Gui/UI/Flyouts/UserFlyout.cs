using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UserFlyout
{
    private UserDisplayObj Obj;
    private UsersControl Con;
    public UserFlyout(UsersControl con, UserDisplayObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("UserWindow.Flyouts.Text1"), true, Button1_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text2"), obj.AuthType != AuthType.Offline, Button2_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text3"), obj.AuthType != AuthType.Offline
                && obj.AuthType != AuthType.OAuth, Button4_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text4"), true, Button3_Click)
        }, con);
    }

    private void Button4_Click()
    {
        Con.ReLogin(Obj);
    }

    private void Button3_Click()
    {
        UserBinding.Remove(Obj.UUID, Obj.AuthType);
        Con.Load();
    }

    private void Button2_Click()
    {
        Con.Refresh(Obj);
    }

    private void Button1_Click()
    {
        UserBinding.SetLastUser(Obj.UUID, Obj.AuthType);
        Con.Load();
    }
}
