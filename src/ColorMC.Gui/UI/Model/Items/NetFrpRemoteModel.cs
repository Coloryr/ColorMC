using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.UI.Model.NetFrp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 映射远程项目
/// </summary>
public partial class NetFrpRemoteModel : SelectItemModel
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// 端口ID
    /// </summary>
    public int ID { get; init; }
    /// <summary>
    /// 远程地址
    /// </summary>
    public string Remote { get; init; }
    /// <summary>
    /// 映射类型
    /// </summary>
    public FrpType FrpType { get; init; }
    /// <summary>
    /// API KEY
    /// </summary>
    public string Key { get; init; }

    /// <summary>
    /// 顶层
    /// </summary>
    private readonly NetFrpModel _model;

    public NetFrpRemoteModel(NetFrpModel model, string key, SakuraFrpChannelObj obj)
    {
        Key = key;
        Name = obj.name;
        ID = obj.id;
        Remote = obj.remote.ToString();
        FrpType = FrpType.SakuraFrp;
        _model = model;
    }

    public NetFrpRemoteModel(NetFrpModel model, string key, OpenFrpChannelObj.OpenFrpChannelData data, OpenFrpChannelObj.ProxieObj obj)
    {
        Key = key;
        Name = data.name + obj.name;
        ID = obj.id;
        Remote = obj.remote;
        FrpType = FrpType.OpenFrp;
        _model = model;
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void Select()
    {
        if (FrpType == FrpType.SakuraFrp)
        {
            _model.SelectSakura(this);
        }
        else
        {
            _model.SelectOpenFrp(this);
        }
    }
}
