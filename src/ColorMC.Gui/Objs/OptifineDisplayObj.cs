using ColorMC.Core.Objs.Optifine;

namespace ColorMC.Gui.Objs;

public record OptifineDisplayObj
{
    public string MC { get; set; }
    public string Version { get; set; }
    public string Date { get; set; }
    public string Forge { get; set; }

    public OptifineObj Data;
}
