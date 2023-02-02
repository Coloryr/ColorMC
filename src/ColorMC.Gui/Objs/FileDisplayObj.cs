using ColorMC.Core.Objs.CurseForge;

namespace ColorMC.Gui.Objs;

public record FileDisplayObj
{
    public string Name { get; set; }
    public long Download { get; set; }
    public string Size { get; set; }
    public string Time { get; set; }
    public bool IsDownload { get; set; }

    public CurseForgeObj.Data.LatestFiles File;
}
