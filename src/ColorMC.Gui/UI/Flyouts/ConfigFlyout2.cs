using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameConfigEdit;
using ColorMC.Gui.UI.Model.Items;
using System;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout2
{
    private readonly GameConfigEditModel _model;
    private readonly NbtDataItemModel _item;

    public ConfigFlyout2(Control con, GameConfigEditModel model, NbtDataItemModel item)
    {
        _model = model;
        _item = item;

        _ = new FlyoutsControl(new (string, bool, Action)[]
        {
            (App.Lang("Button.Delete"), true, Button1_Click),
        }, con);
    }

    public void Button1_Click()
    {
        _model.DeleteItem(_item);
    }
}
