namespace ColorMC.Core.Objs.Optifine;

/// <summary>
/// 高清修复信息
/// </summary>
public record OptifineObj
{
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 游戏版本号
    /// </summary>
    public string MCVersion { get; set; }

    /// <summary>
    /// Forge加载器信息
    /// </summary>
    public string Forge { get; set; }
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// 日期
    /// </summary>
    public string Date { get; set; }

    public string Url1;
    public string Url2;
    public SourceLocal Local;
}
