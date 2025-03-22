using System;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

public record DownloadModArg
{
    public DownloadItemObj Item;
    public ModInfoObj Info;
    public ModObj? Old;
}

public record ProcessUpdateArg
{
    public Action<string>? Update;
}