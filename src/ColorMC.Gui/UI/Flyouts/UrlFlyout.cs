using Avalonia.Controls;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UrlFlyout
{
    public UrlFlyout(Control con, string? url, string? url1)
    {
        _ = new FlyoutsControl(
        [
            (App.Lang("AddWindow.Control.Text3"), url != null, ()=>
            {
                BaseBinding.OpUrl(url);
            }),
            (App.Lang("AddWindow.Control.Text4"), url1 != null, ()=>
            {
                BaseBinding.OpUrl(url1);
            })
        ], con);
    }
}
