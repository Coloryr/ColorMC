namespace ColorMC.Core.Objs;

/// <summary>
/// 版本储存
/// </summary>
public record VersionStrObj
{
    /// <summary>
    /// 修正后的版本号
    /// </summary>
    public Version Version;
    /// <summary>
    /// 原始版本号
    /// </summary>
    public string VersionStr;
}

