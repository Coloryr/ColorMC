using Avalonia.Controls;
using ColorMC.Gui.UI.Model.ConfigEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout2
{
    private readonly ConfigEditModel Model;
    private readonly DataItem Item;

    public ConfigFlyout2(Control con, ConfigEditModel model, DataItem item)
    {
        Model = model;
        Item = item;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.Delete"), true, Button1_Click),
        }, con);
    }

    public void Button1_Click()
    {
        Model.DeleteItem(Item);
    }
}
