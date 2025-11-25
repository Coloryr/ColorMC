using ColorMC.Core.GuiHandle;
using ColorMC.Core.Objs;
using SharpCompress.Archives.Zip;

namespace ColorMC.Core.Worker;

public abstract class ModPackWork
{
    protected readonly ZipArchive Zip;
    protected readonly IOverGameGui? Gui;
    protected readonly IModPackGui? Packgui;

    protected Loaders Loader = Loaders.Normal;
    protected string LoaderVersion = "";
    protected string GameVersion;
    protected GameSettingObj? Game;
    protected List<FileItemObj>? Downloads;

    public ModPackWork(Stream st, IOverGameGui? gui, IModPackGui? packgui)
    {
        Gui = gui;
        Packgui = packgui;
        Zip = ZipArchive.Open(st);
    }

    public ModPackWork(string file, IOverGameGui? gui, IModPackGui? packgui)
    {
        Gui = gui;
        Packgui = packgui;
        Zip = ZipArchive.Open(file);
    }
}
