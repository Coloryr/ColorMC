using Avalonia.Controls;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class UrlFlyout
{
    private string _url;

    public UrlFlyout(Control con, string url)
    {
        _url = url;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("AddWindow.Control.Text3"), true, Button1_Click)
        }, con);
    }

    private void Button1_Click()
    {
        BaseBinding.OpUrl(_url);
    }
}
