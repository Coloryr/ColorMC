using Avalonia.Controls;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UrlFlyout
{
    private string Url;

    public UrlFlyout(Control con, string url)
    {
        Url = url;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("AddWindow.Control.Text3"), true, Button1_Click)
        }, con);
    }

    private void Button1_Click()
    {
        BaseBinding.OpUrl(Url);
    }
}
