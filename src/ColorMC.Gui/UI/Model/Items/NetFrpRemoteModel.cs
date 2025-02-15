using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.UI.Model.NetFrp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 映射远程项目
/// </summary>
public partial class NetFrpRemoteModel : SelectItemModel
{
    private readonly NetFrpModel _model;
    private readonly bool _isSakura;

    public NetFrpRemoteModel(NetFrpModel model, string key, SakuraFrpChannelObj obj)
    {
        Key = key;
        Name = obj.name;
        ID = obj.id;
        Remote = obj.remote.ToString();
        FrpType = FrpType.SakuraFrp;
        _model = model;
        _isSakura = true;
    }

    public NetFrpRemoteModel(NetFrpModel model, string key, OpenFrpChannelObj.Data data, OpenFrpChannelObj.Proxie obj)
    {
        Key = key;
        Name = data.name + obj.name;
        ID = obj.id;
        Remote = obj.remote;
        FrpType = FrpType.OpenFrp;
        _model = model;
        _isSakura = false;
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

    public void Select()
    {
        if (_isSakura)
        {
            _model.SelectSakura(this);
        }
        else
        {
            _model.SelectOpenFrp(this);
        }
    }
}
