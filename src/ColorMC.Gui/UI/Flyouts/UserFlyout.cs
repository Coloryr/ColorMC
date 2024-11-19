using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

public class UserFlyout
{
    private readonly UserDisplayModel _obj;
    public UserFlyout(Control con, UserDisplayModel model)
    {
        _obj = model;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("UserWindow.Flyouts.Text1"), true, ()=>
            {
                 _obj.Select();
            }),
            new FlyoutMenuObj(App.Lang("UserWindow.Flyouts.Text2"), _obj.AuthType is not AuthType.Offline, ()=>
            {
                _obj.Refresh();
            }),
            new FlyoutMenuObj(App.Lang("UserWindow.Flyouts.Text3"), _obj.AuthType is not AuthType.Offline
                or AuthType.OAuth, ()=>
                {
                    _obj.ReLogin();
                }),
            new FlyoutMenuObj(App.Lang("UserWindow.Flyouts.Text4"), true, ()=>
            {
                _obj.Remove();
            }),
            new FlyoutMenuObj(App.Lang("UserWindow.Flyouts.Text5"), _obj.AuthType == AuthType.Offline, ()=>
            {
                _obj.Edit();
            })
        ], con);
    }
}
