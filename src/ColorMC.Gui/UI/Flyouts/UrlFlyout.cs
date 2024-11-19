using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UrlFlyout
{
    public UrlFlyout(Control con, string? url, string? url1)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("NetFrpWindow.Tab1.Text14"), url != null, ()=>
            {
                BaseBinding.OpUrl(url);
            }),
            new FlyoutMenuObj(App.Lang("AddWindow.Text13"), url1 != null, ()=>
            {
                BaseBinding.OpUrl(url1);
            })
        ], con);
    }
}
