namespace ColorMC.Gui.Objs;

/// <summary>
/// Java下载项目
/// </summary>
public record JavaDownloadDisplayObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 系统
    /// </summary>
    public string Os { get; set; }
    /// <summary>
    /// 发现版
    /// </summary>
    public string Arch { get; set; }
    /// <summary>
    /// 主版本
    /// </summary>
    public string MainVersion { get; set; }
    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 大小
    /// </summary>
    public string Size { get; set; }

    /// <summary>
    /// 网址
    /// </summary>
    public string Url;
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha256;
    /// <summary>
    /// 文件名
    /// </summary>
    public string File;
}
