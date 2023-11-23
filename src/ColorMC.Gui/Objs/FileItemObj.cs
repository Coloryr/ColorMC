using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 文件显示
/// </summary>
public record FileItemObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 介绍
    /// </summary>
    public string Summary { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; }
    /// <summary>
    /// 下载次数
    /// </summary>
    public long DownloadCount { get; set; }
    /// <summary>
    /// 更新日期
    /// </summary>
    public string ModifiedDate { get; set; }
    /// <summary>
    /// 图标网址
    /// </summary>
    public string? Logo { get; set; }
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
    /// 数据
    /// </summary>
    public object Data;
    /// <summary>
    /// 数据
    /// </summary>
    public object? Data1;
}

