using ColorMC.Core.Objs;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Dns项目
/// </summary>
/// <param name="url"></param>
/// <param name="type"></param>
public partial class DnsItemModel(string url, DnsType type) : ObservableObject
{
    /// <summary>
    /// 地址
    /// </summary>
    public string Url => url;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type => type.GetName();
    /// <summary>
    /// 类型
    /// </summary>
    public DnsType Dns => type;
}
