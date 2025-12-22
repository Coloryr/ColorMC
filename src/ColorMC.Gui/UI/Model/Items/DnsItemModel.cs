using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Dns项目
/// </summary>
/// <param name="url"></param>
public partial class DnsItemModel(string url) : ObservableObject
{
    /// <summary>
    /// 地址
    /// </summary>
    public string Url => url;
}
