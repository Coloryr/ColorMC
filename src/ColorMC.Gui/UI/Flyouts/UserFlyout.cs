using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.User;

namespace ColorMC.Gui.UI.Flyouts;

public class UserFlyout
{
    private readonly UserDisplayObj _obj;
    private readonly UsersControlModel _model;
    public UserFlyout(Control con, UsersControlModel model)
    {
        _model = model;
        _obj = model.Item!;

        _ = new FlyoutsControl(
        [
            (App.Lang("UserWindow.Flyouts.Text1"), true, ()=>
            {
                 _model.Select(_obj);
            }),
            (App.Lang("UserWindow.Flyouts.Text2"), _obj.AuthType != AuthType.Offline, ()=>
            {
                _model.Refresh(_obj);
            }),
            (App.Lang("UserWindow.Flyouts.Text3"), _obj.AuthType != AuthType.Offline
                && _obj.AuthType != AuthType.OAuth, ()=>
                {
                    _model.ReLogin(_obj);
                }),
            (App.Lang("UserWindow.Flyouts.Text4"), true, ()=>
            {
                _model.Remove(_obj);
            }),
            (App.Lang("UserWindow.Flyouts.Text5"), _obj.AuthType == AuthType.Offline, ()=>
            {
                _model.Edit(_obj);
            })
        ], con);
    }
}
