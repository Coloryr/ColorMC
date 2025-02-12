using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 映射远程项目
/// </summary>
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

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 端口ID
    /// </summary>
    public int ID { get; }
    /// <summary>
    /// 是否被占用
    /// </summary>
    public bool Use { get; }
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; }
    /// <summary>
    /// 远程地址
    /// </summary>
    public string Remote { get; }
    /// <summary>
    /// 映射类型
    /// </summary>
    public FrpType FrpType;
    /// <summary>
    /// API KEY
    /// </summary>
    public string Key;
}
