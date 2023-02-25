using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

public enum FileType
{
    ModPack = 0, Mod, World, Shaderpack, Resourcepack, DataPacks
}

public record FileItemDisplayObj
{
    public string Name { get; set; }
    public string Summary { get; set; }
    public string Author { get; set; }
    public long DownloadCount { get; set; }
    public string ModifiedDate { get; set; }
    public string? Logo { get; set; }
    public bool IsDownload { get; set; }

    public FileType FileType;
    public SourceType SourceType;

    /// <summary>
    /// Mod用
    /// </summary>
    public string ID;
    public object Data;
}

public record FileDisplayObj
{
    public string Name { get; set; }
    public long Download { get; set; }
    public string Size { get; set; }
    public string Time { get; set; }
    public bool IsDownload { get; set; }

    public FileType FileType;
    public SourceType SourceType;

    public object Data;
}
