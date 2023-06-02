using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 文件显示
/// </summary>
public record FileItemDisplayObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name;
    /// <summary>
    /// 介绍
    /// </summary>
    public string Summary;
    /// <summary>
    /// 作者
    /// </summary>
    public string Author;
    /// <summary>
    /// 下载次数
    /// </summary>
    public long DownloadCount;
    /// <summary>
    /// 更新日期
    /// </summary>
    public string ModifiedDate;
    /// <summary>
    /// 图标网址
    /// </summary>
    public string? Logo;
    /// <summary>
    /// 是否已经下载
    /// </summary>
    public bool IsDownload;

    /// <summary>
    /// 文件类型
    /// </summary>
    public FileType FileType;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType SourceType;

    /// <summary>
    /// Mod用
    /// </summary>
    public string ID;
    /// <summary>
    /// 数据
    /// </summary>
    public object Data;
}

/// <summary>
/// 项目显示
/// </summary>
public record FileDisplayObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 下载次数
    /// </summary>
    public long Download { get; set; }
    /// <summary>
    /// 大小
    /// </summary>
    public string Size { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public string Time { get; set; }
    /// <summary>
    /// 是否已经下载
    /// </summary>
    public bool IsDownload { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public FileType FileType;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType SourceType;

    /// <summary>
    /// Mod用
    /// </summary>
    public string ID;
    /// <summary>
    /// Mod用
    /// </summary>
    public string ID1;

    /// <summary>
    /// 数据
    /// </summary>
    public object Data;
}
