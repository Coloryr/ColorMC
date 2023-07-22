using Avalonia.Controls;
using ColorMC.Gui.UI.Model.ConfigEdit;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout2
{
    private readonly ConfigEditModel _model;
    private readonly NbtDataItem _item;

    public ConfigFlyout2(Control con, ConfigEditModel model, NbtDataItem item)
    {
        _model = model;
        _item = item;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
        }, con);
    }

    public void Button1_Click()
    {
        _model.DeleteItem(_item);
    }
}
