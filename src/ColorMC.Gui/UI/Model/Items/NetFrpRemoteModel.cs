using ColorMC.Core.Objs.Frp;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpRemoteModel : ObservableObject
{
    public string Name { get; init; }
    public int ID { get; init; }
    public bool Use { get; init; }
    public string Type { get; init; }
    public int Remote { get; init; }

    public NetFrpRemoteModel(SakuraFrpChannelObj obj)
    {
        Name = obj.name;
        ID = obj.id;
        Type = obj.type;
        Remote = obj.remote;

        Use = obj.online == false && obj.type == "tcp";
    }
}
