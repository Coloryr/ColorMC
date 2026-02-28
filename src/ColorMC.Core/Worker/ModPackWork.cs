using ColorMC.Core.GuiHandle;
using ColorMC.Core.Objs;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Writers.Zip;

namespace ColorMC.Core.Worker;

public abstract class ModPackWork
{
    protected readonly IWritableArchive<ZipWriterOptions> Zip;
    protected readonly IOverGameGui? Gui;
    protected readonly IAddGui? Packgui;

    protected Loaders Loader = Loaders.Normal;
    protected string LoaderVersion = "";
    protected string GameVersion;
    protected GameSettingObj? Game;
    protected List<FileItemObj>? Downloads;

    protected CancellationToken Token;

    public ModPackWork(IWritableArchive<ZipWriterOptions> zip, IOverGameGui? gui, IAddGui? packgui, CancellationToken token)
    {
        Gui = gui;
        Packgui = packgui;
        Zip = zip;
        Token = token;
    }

    public ModPackWork(string file, IOverGameGui? gui, IAddGui? packgui, CancellationToken token)
    {
        Gui = gui;
        Packgui = packgui;
        Zip = ZipArchive.OpenArchive(file);
        Token = token;
    }
}
