using Avalonia.Controls;
using ColorMC.Core.Objs;
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
            new FlyoutMenuModel(LanguageUtils.Get("UserWindow.Text31"), true, ()=>
            {
                 _obj.Select();
            }),
            new FlyoutMenuModel(LanguageUtils.Get("UserWindow.Text32"), _obj.AuthType is not AuthType.Offline, ()=>
            {
                _obj.Refresh();
            }),
            new FlyoutMenuModel(LanguageUtils.Get("UserWindow.Text33"), _obj.AuthType is not AuthType.Offline
                or AuthType.OAuth, ()=>
                {
                    _obj.Relogin();
                }),
            new FlyoutMenuModel(LanguageUtils.Get("UserWindow.Text34"), true, ()=>
            {
                _obj.Remove();
            }),
            new FlyoutMenuModel(LanguageUtils.Get("UserWindow.Text35"), _obj.AuthType == AuthType.Offline, ()=>
            {
                _obj.Edit();
            })
        ]).Show(con);
    }
}
