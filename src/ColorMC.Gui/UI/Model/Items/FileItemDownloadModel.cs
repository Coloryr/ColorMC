using ColorMC.Core.Objs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 下载标记
/// </summary>
public partial class FileItemDownloadModel : ObservableObject
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public required FileType Type;
    /// <summary>
    /// 下载源
    /// </summary>
    public required SourceType Source;
    /// <summary>
    /// 项目ID
    /// </summary>
    public required string PID;

    public string Name { get; init; }

    /// <summary>
    /// 当前信息
    /// </summary>
    [ObservableProperty]
    private string? _info;
    /// <summary>
    /// 当前信息
    /// </summary>
    [ObservableProperty]
    private string? _subInfo;
    /// <summary>
    /// 进度
    /// </summary>
    [ObservableProperty]
    private double _now;
    /// <summary>
    /// 子进度
    /// </summary>
    [ObservableProperty]
    private double _nowSub;
    /// <summary>
    /// 是否显示子进度条
    /// </summary>
    [ObservableProperty]
    private bool _showSub;
}