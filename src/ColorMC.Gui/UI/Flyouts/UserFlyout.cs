using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UserFlyout
{
    private readonly UserDisplayObj Obj;
    private readonly UsersModel Model;
    public UserFlyout(Control con, UsersModel model)
    {
        Model = model;
        Obj = model.Item!;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("UserWindow.Flyouts.Text1"), true, Button1_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text2"), Obj.AuthType != AuthType.Offline, Button2_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text3"), Obj.AuthType != AuthType.Offline
                && Obj.AuthType != AuthType.OAuth, Button4_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text4"), true, Button3_Click)
        }, con);
    }

    private void Button4_Click()
    {
        Model.ReLogin(Obj);
    }

    private void Button3_Click()
    {
        Model.Remove(Obj);
    }

    private void Button2_Click()
    {
        Model.Refresh(Obj);
    }

    private void Button1_Click()
    {
        Model.Select(Obj);
    }
}
