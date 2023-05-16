using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 文件显示
/// </summary>
public record FileItemDisplayObj
{
    public string Name;
    public string Summary;
    public string Author;
    public long DownloadCount;
    public string ModifiedDate;
    public string? Logo;
    public bool IsDownload;

    public FileType FileType;
    public SourceType SourceType;

    /// <summary>
    /// Mod用
    /// </summary>
    public string ID;
    public object Data;
}

/// <summary>
/// 项目显示
/// </summary>
public record FileDisplayObj
{
    public string Name { get; set; }
    public long Download { get; set; }
    public string Size { get; set; }
    public string Time { get; set; }
    public bool IsDownload { get; set; }

    public FileType FileType;
    public SourceType SourceType;

    /// <summary>
    /// Mod用
    /// </summary>
    public string ID;
    public string ID1;


    public object Data;
}
