using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 下载项目显示
/// </summary>
public partial class DownloadItemDisplayModel : ObservableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 总大小
    /// </summary>
    [ObservableProperty]
    private string _allSize;
    /// <summary>
    /// 已下载大小
    /// </summary>
    [ObservableProperty]
    private string _nowSize;
    /// <summary>
    /// 当前状态
    /// </summary>
    [ObservableProperty]
    private string _state;
    /// <summary>
    /// 错误次数
    /// </summary
    [ObservableProperty]
    private int _errorTime;

    /// <summary>
    /// 1秒下载的大小
    /// </summary>
    public long Last;
}
