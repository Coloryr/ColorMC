using ColorMC.Core.Game.Auth;

namespace ColorMC.Gui.Objs;

public record LastUser
{
    public string UUID { get; set; }
    public AuthType Type { get; set; }
}
public record GuiConfigObj
{
    public string Version { get; set; }
    public LastUser LastUser { get; set; }
    public string BackImage { get; set; }
}
