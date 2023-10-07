namespace ColorMC.Core.Objs;

/// <summary>
/// Java信息
/// </summary>
public record JavaInfo
{
    public string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 主版本号
    /// </summary>
    public int MajorVersion { get; set; }
    /// <summary>
    /// Java类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 进制
    /// </summary>
    public ArchEnum Arch { get; set; }
}
