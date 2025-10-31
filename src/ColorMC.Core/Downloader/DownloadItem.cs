using ColorMC.Core.Objs;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载项目
/// </summary>
internal record DownloadItem
{
    /// <summary>
    /// 下载任务
    /// </summary>
    public DownloadTask Task;
    /// <summary>
    /// 下载项目
    /// </summary>
    public FileItemObj File;
}
