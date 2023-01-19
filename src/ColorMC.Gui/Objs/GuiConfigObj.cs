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
    public int BackEffect { get; set; }
    public int BackTran { get; set; }
    public bool WindowTran { get; set; }
    public int WindowTranType { get; set; }

    public string ColorMain { get; set; }
    public string ColorBack { get; set; }
    public string ColorTranBack { get; set; }
    public string ColorFont1 { get; set; }
    public string ColorFont2 { get; set; }
    public bool RGB { get; set; }
    public int RGBS { get; set; }
    public int RGBV { get; set; }
}
