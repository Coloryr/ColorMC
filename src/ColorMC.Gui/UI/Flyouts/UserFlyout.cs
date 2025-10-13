using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 用户界面
/// 账户右键菜单
/// </summary>
public class UserFlyout
{
    private readonly UserDisplayModel _obj;
    public UserFlyout(Control con, UserDisplayModel model)
    {
        _obj = model;

        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("UserWindow.Flyouts.Text1"), true, ()=>
            {
                 _obj.Select();
            }),
            new FlyoutMenuModel(App.Lang("UserWindow.Flyouts.Text2"), _obj.AuthType is not AuthType.Offline, ()=>
            {
                _obj.Refresh();
            }),
            new FlyoutMenuModel(App.Lang("UserWindow.Flyouts.Text3"), _obj.AuthType is not AuthType.Offline
                or AuthType.OAuth, ()=>
                {
                    _obj.Relogin();
                }),
            new FlyoutMenuModel(App.Lang("UserWindow.Flyouts.Text4"), true, ()=>
            {
                _obj.Remove();
            }),
            new FlyoutMenuModel(App.Lang("UserWindow.Flyouts.Text5"), _obj.AuthType == AuthType.Offline, ()=>
            {
                _obj.Edit();
            })
        ]).Show(con);
    }
}
