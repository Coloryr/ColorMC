using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 多个模组下载任务
/// </summary>
public record FileItemDownloadTask
{
    /// <summary>
    /// 下载的模组项目
    /// </summary>
    public required DownloadModArg? Modsave;
    /// <summary>
    /// 下载源
    /// </summary>
    public required SourceType Source;

    /// <summary>
    /// 下载结果
    /// </summary>
    public bool TaskRes;
    /// <summary>
    /// 是否结束
    /// </summary>
    public bool IsEnd;
}
