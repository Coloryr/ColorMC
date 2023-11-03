namespace ColorMC.Core.Objs;

/// <summary>
/// 下载项目
/// </summary>
public record DownloadItemObj
{
    /// <summary>
    /// 项目名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 链接
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Local { get; set; }
    public string? MD5 { get; set; }
    public string? SHA1 { get; set; }
    public string? SHA256 { get; set; }
    /// <summary>
    /// 覆盖
    /// </summary>
    public bool Overwrite { get; set; } = false;
    /// <summary>
    /// 使用ColorMC请求头
    /// </summary>
    public bool UseColorMCHead { get; set; } = false;

    /// <summary>
    /// 总大小
    /// </summary>
    public long AllSize { get; set; }
    /// <summary>
    /// 已下载大小
    /// </summary>
    public long NowSize { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public DownloadItemState State { get; set; } = DownloadItemState.Init;
    /// <summary>
    /// 下载后执行
    /// </summary>
    public Action<Stream> Later { get; set; }
    /// <summary>
    /// 错误次数
    /// </summary>
    public int ErrorTime { get; set; }
    /// <summary>
    /// 更新操作
    /// </summary>
    public Action<int> Update { get; set; }
}
