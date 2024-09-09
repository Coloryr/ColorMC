using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

public record DownloadModArg
{
    public DownloadItemObj Item;
    public ModInfoObj Info;
}

public record DownloadModItemArg : DownloadModArg
{
    public string Local;
}
