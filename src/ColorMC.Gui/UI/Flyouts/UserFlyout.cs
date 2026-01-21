using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LangUtils.Get("UserWindow.Text31"), true, ()=>
            {
                 _obj.Select();
            }),
            new FlyoutMenuModel(LangUtils.Get("UserWindow.Text32"), _obj.CanRefresh, ()=>
            {
                _obj.Refresh();
            }),
            new FlyoutMenuModel(LangUtils.Get("UserWindow.Text33"), _obj.CanRelogin, ()=>
            {
                _obj.Relogin();
            }),
            new FlyoutMenuModel(LangUtils.Get("UserWindow.Text34"), true, ()=>
            {
                _obj.Remove();
            }),
            new FlyoutMenuModel(LangUtils.Get("UserWindow.Text35"), _obj.CanEdit, ()=>
            {
                _obj.Edit();
            })
        ]).Show(con);
    }
}
