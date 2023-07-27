using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameConfigEdit;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout2
{
    private readonly GameConfigEditModel _model;
    private readonly NbtDataItem _item;

    public ConfigFlyout2(Control con, GameConfigEditModel model, NbtDataItem item)
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
