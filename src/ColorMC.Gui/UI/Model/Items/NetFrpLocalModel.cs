using ColorMC.Gui.UI.Model.NetFrp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 映射本地项目
/// </summary>
/// <param name="top"></param>
/// <param name="motd"></param>
/// <param name="port"></param>
public partial class NetFrpLocalModel(NetFrpModel top, string motd, string port) : SelectItemModel
{
    /// <summary>
    /// 服务器内容
    /// </summary>
    public string Motd => motd;
    /// <summary>
    /// 端口
    /// </summary>
    public string Port => port;

    /// <summary>
    /// 是否启动
    /// </summary>
    [ObservableProperty]
    private bool _isStart;

    /// <summary>
    /// 启动映射
    /// </summary>
    [RelayCommand]
    public void Start()
    {
        top.StartThisFrp(this);
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void Select()
    {
        top.SelectLocal(this);
    }
}
