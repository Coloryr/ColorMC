using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpRemoteModel : SelectItemModel
{
    public NetFrpRemoteModel(string key, SakuraFrpChannelObj obj)
    {
        Key = key;
        Name = obj.name;
        ID = obj.id;
        Use = obj.online == false && obj.type == "tcp";
        Type = obj.type;
        Remote = obj.remote.ToString();
        FrpType = FrpType.SakuraFrp;
    }

    public NetFrpRemoteModel(string key, OpenFrpChannelObj.Data data, OpenFrpChannelObj.Proxie obj)
    {
        Key = key;
        Name = data.name + obj.name;
        ID = obj.id;
        Use = obj.type == "tcp";
        Type = obj.type;
        Remote = obj.remote;
        FrpType = FrpType.OpenFrp;
    }

    public string Name { get; }
    public int ID { get; }
    public bool Use { get; }
    public string Type { get; }
    public string Remote { get; }

    public FrpType FrpType;
    public string Key;
}
