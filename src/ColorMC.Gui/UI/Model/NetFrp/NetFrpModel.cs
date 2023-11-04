using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel : MenuModel
{
    public override List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/Setting/item1.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/Setting/item1.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/Setting/item1.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text3") }
    };

    public NetFrpModel(BaseModel model) : base(model)
    {
        
    }

    protected override void Close()
    {
        Remotes.Clear();
        Locals.Clear();
    }
}
