using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 下载项目显示
/// </summary>
public partial class DownloadDisplayModel : ObservableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string name;
    /// <summary>
    /// 总大小
    /// </summary>
    [ObservableProperty]
    private string allSize;
    /// <summary>
    /// 已下载大小
    /// </summary>
    [ObservableProperty]
    private string nowSize;
    /// <summary>
    /// 当前状态
    /// </summary>
    [ObservableProperty]
    private string state;
    /// <summary>
    /// 错误次数
    /// </summary
    [ObservableProperty]
    private int errorTime;

    /// <summary>
    /// 1秒下载的大小
    /// </summary>
    public long Last;
}
