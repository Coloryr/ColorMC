namespace ColorMC.Core.Objs;

/// <summary>
/// 一段日志
/// </summary>
public record GameLogItemObj
{
    /// <summary>
    /// 系统日志
    /// </summary>
    public GameSystemLog LogType { get; init; }
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime TimeSpan { get; init; }
    /// <summary>
    /// 内容
    /// </summary>
    public string Log { get; set; }
    /// <summary>
    /// 所在的线程
    /// </summary>
    public string Thread { get; init; }
    /// <summary>
    /// 时间
    /// </summary>
    public string Time { get; init; }
    /// <summary>
    /// 所在的分组
    /// </summary>
    public string Category { get; init; }
    /// <summary>
    /// 日志等级
    /// </summary>
    public LogLevel Level { get; init; }
}

