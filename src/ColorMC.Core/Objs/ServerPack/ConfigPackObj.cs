namespace ColorMC.Core.Objs.ServerPack;

/// <summary>
/// 配置项目
/// </summary>
public record ConfigPackObj
{
    /// <summary>
    /// 分组
    /// </summary>
    public string Group { get; set; }
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha256 { get; set; }
    /// <summary>
    /// 是否为压缩包打包
    /// </summary>
    public bool IsZip { get; set; }
    /// <summary>
    /// 是否为路径
    /// </summary>
    public bool IsDir { get; set; }
    /// <summary>
    /// 下载地址
    /// </summary>
    public string Url { get; set; }
}
