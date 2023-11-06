using ColorMC.Gui.Objs;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel : MenuModel
{
    public override List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/NetFrp/item1.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/NetFrp/item2.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/NetFrp/item3.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text3") }
    };

    public NetFrpModel(BaseModel model) : base(model)
    {

    }

    protected override void Close()
    {
        _client.Stop();

        Remotes.Clear();
        Locals.Clear();
    }
}
