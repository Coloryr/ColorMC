using Avalonia.Controls;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UrlFlyout
{
    private string? _url;
    private string? _url1;

    public UrlFlyout(Control con, string? url, string? url1)
    {
        _url = url;
        _url1 = url1;

        var fy = new FlyoutsControl(new()
        {
            (App.Lang("AddWindow.Control.Text3"), _url != null, Button1_Click),
            (App.Lang("AddWindow.Control.Text4"), _url1 != null, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        BaseBinding.OpUrl(_url1);
    }

    private void Button1_Click()
    {
        BaseBinding.OpUrl(_url);
    }
}
