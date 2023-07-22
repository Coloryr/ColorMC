using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.User;

namespace ColorMC.Gui.UI.Flyouts;

public class UserFlyout
{
    private readonly UserDisplayObj _obj;
    private readonly UsersModel _model;
    public UserFlyout(Control con, UsersModel model)
    {
        _model = model;
        _obj = model.Item!;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("UserWindow.Flyouts.Text1"), true, Button1_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text2"), _obj.AuthType != AuthType.Offline, Button2_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text3"), _obj.AuthType != AuthType.Offline
                && _obj.AuthType != AuthType.OAuth, Button4_Click),
            (App.GetLanguage("UserWindow.Flyouts.Text4"), true, Button3_Click)
        }, con);
    }

    private void Button4_Click()
    {
        _model.ReLogin(_obj);
    }

    private void Button3_Click()
    {
        _model.Remove(_obj);
    }

    private void Button2_Click()
    {
        _model.Refresh(_obj);
    }

    private void Button1_Click()
    {
        _model.Select(_obj);
    }
}
