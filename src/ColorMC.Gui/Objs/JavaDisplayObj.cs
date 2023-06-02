namespace ColorMC.Gui.Objs;

/// <summary>
/// Java显示
/// </summary>
public record JavaDisplayObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 主版本
    /// </summary>
    public string MajorVersion { get; set; }
    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 发行版
    /// </summary>
    public string Arch { get; set; }
}
