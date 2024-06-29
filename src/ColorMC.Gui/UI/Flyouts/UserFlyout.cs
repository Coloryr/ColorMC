using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.User;

namespace ColorMC.Gui.UI.Flyouts;

public class UserFlyout
{
    private readonly UserDisplayModel _obj;
    public UserFlyout(Control con, UserDisplayModel model)
    {
        _obj = model;

        _ = new FlyoutsControl(
        [
            (App.Lang("UserWindow.Flyouts.Text1"), true, ()=>
            {
                 _obj.Select();
            }),
            (App.Lang("UserWindow.Flyouts.Text2"), _obj.AuthType != AuthType.Offline, ()=>
            {
                _obj.Refresh();
            }),
            (App.Lang("UserWindow.Flyouts.Text3"), _obj.AuthType != AuthType.Offline
                && _obj.AuthType != AuthType.OAuth, ()=>
                {
                    _obj.ReLogin();
                }),
            (App.Lang("UserWindow.Flyouts.Text4"), true, ()=>
            {
                _obj.Remove();
            }),
            (App.Lang("UserWindow.Flyouts.Text5"), _obj.AuthType == AuthType.Offline, ()=>
            {
                _obj.Edit();
            })
        ], con);
    }
}
