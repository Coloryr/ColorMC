using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.NetFrp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加自定义映射
/// </summary>
public partial class NetFrpAddModel : ObservableObject
{
    /// <summary>
    /// 锁定名字
    /// </summary>
    [ObservableProperty]
    private bool _lockName = true;

    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 远程服务器地址
    /// </summary>
    [ObservableProperty]
    private string _ip;
    /// <summary>
    /// 远程服务器用户
    /// </summary>
    [ObservableProperty]
    private string _user;
    /// <summary>
    /// 远程服务器密钥
    /// </summary>
    [ObservableProperty]
    private string _key;
    /// <summary>
    /// 映射名字
    /// </summary>
    [ObservableProperty]
    private string _rName;

    /// <summary>
    /// 映射端口
    /// </summary>
    [ObservableProperty]
    private int? _netPort = 25565;
    /// <summary>
    /// 映射端口
    /// </summary>
    [ObservableProperty]
    private int? _port = 7000;

    public NetFrpAddModel()
    {

    }

    public NetFrpAddModel(NetFrpSelfItemModel model)
    {
        _lockName = false;
        Ip = model.Obj.IP;
        Name = model.Obj.Name;
        Key = model.Obj.Key;
        User = model.Obj.User;
        NetPort = model.Obj.NetPort;
        Port = model.Obj.Port;
        RName = model.Obj.RName;
    }

    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(NetFrpModel.NameCon1, true);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(NetFrpModel.NameCon1, false);
    }

    public FrpSelfObj Build()
    {
        return new()
        {
            IP = Ip,
            Name = Name,
            Key = Key,
            NetPort = NetPort ?? 25565,
            User = User,
            Port = Port ?? 7000,
            RName = RName
        };
    }
}
