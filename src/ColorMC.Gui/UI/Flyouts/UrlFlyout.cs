using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 下载项目
/// 右键菜单
/// </summary>
public static class UrlFlyout
{
    public static void Show(Control con, string? url, string? url1)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("NetFrpWindow.Tab1.Text14"), url != null, ()=>
            {
                BaseBinding.OpenUrl(url);
            }),
            new FlyoutMenuObj(App.Lang("AddWindow.Text13"), url1 != null, ()=>
            {
                BaseBinding.OpenUrl(url1);
            })
        ]).Show(con);
    }
}
