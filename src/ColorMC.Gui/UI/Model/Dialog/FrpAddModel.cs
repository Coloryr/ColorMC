using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加自定义映射
/// </summary>
public partial class NetFrpAddModel : BaseDialogModel
{
    /// <summary>
    /// 锁定名字
    /// </summary>
    [ObservableProperty]
    public partial bool LockName { get; set; } = true;

    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// 远程服务器地址
    /// </summary>
    [ObservableProperty]
    public partial string Ip { get; set; }

    /// <summary>
    /// 远程服务器用户
    /// </summary>
    [ObservableProperty]
    public partial string User { get; set; }

    /// <summary>
    /// 远程服务器密钥
    /// </summary>
    [ObservableProperty]
    public partial string Key { get; set; }

    /// <summary>
    /// 映射名字
    /// </summary>
    [ObservableProperty]
    public partial string RName { get; set; }

    /// <summary>
    /// 映射端口
    /// </summary>
    [ObservableProperty]
    public partial int? NetPort { get; set; } = 25565;

    /// <summary>
    /// 映射端口
    /// </summary>
    [ObservableProperty]
    public partial int? Port { get; set; } = 7000;

    public NetFrpAddModel(string name) : base(name)
    {

    }

    public NetFrpAddModel(string name, NetFrpSelfItemModel model) : base(name)
    {
        LockName = false;
        Ip = model.Obj.IP;
        Name = model.Obj.Name;
        Key = model.Obj.Key;
        User = model.Obj.User;
        NetPort = model.Obj.NetPort;
        Port = model.Obj.Port;
        RName = model.Obj.RName;
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
