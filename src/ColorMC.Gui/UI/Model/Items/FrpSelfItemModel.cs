using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.NetFrp;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 自定义Frp项目
/// </summary>
/// <param name="model"></param>
/// <param name="obj"></param>
public partial class NetFrpSelfItemModel(NetFrpModel model, FrpSelfObj obj) : SelectItemModel
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => obj.Name;
    /// <summary>
    /// 远程服务器地址
    /// </summary>
    public string IP => obj.IP + ":" + obj.Port;
    /// <summary>
    /// 映射端口
    /// </summary>
    public int NetPort => obj.NetPort;
    /// <summary>
    /// 密钥
    /// </summary>
    public string Key => string.IsNullOrWhiteSpace(obj.Key) ? "" : "********";

    /// <summary>
    /// 映射信息
    /// </summary>
    public FrpSelfObj Obj => obj;

    /// <summary>
    /// 重载信息
    /// </summary>
    public void Reload()
    {
        OnPropertyChanged(nameof(IP));
        OnPropertyChanged(nameof(NetPort));
        OnPropertyChanged(nameof(Key));
    }

    /// <summary>
    /// 编辑映射
    /// </summary>
    [RelayCommand]
    public void Edit()
    {
        model.Edit(this);
    }

    /// <summary>
    /// 删除映射
    /// </summary>
    [RelayCommand]
    public void Delete()
    {
        model.Delete(this);
    }

    /// <summary>
    /// 选择映射
    /// </summary>
    public void Select()
    {
        model.Select(this);
    }
}
