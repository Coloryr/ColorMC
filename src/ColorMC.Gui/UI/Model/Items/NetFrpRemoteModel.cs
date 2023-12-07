using ColorMC.Core.Objs.Frp;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpRemoteModel(SakuraFrpChannelObj obj)
{
    public string Name => obj.name;
    public int ID => obj.id;
    public bool Use => obj.online == false && obj.type == "tcp";
    public string Type => obj.type;
    public int Remote => obj.remote;
}
