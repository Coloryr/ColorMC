using ColorMC.Core.Game.Auth;

namespace ColorMC.Gui.Objs;

public record LastUser
{
    public string UUID { get; set; }
    public AuthType Type { get; set; }
}

public record ServerCustom
{
    public string IP { get; set; }
    public int Port { get; set; }
    public bool Motd { get; set; }
    public bool JoinServer { get; set; }
    public string MotdColor { get; set; }
    public string MotdBackColor { get; set; }

    public bool LockGame { get; set; }
    public string GameName { get; set; }
    public string UIFile { get; set; }
}

public record WindowsRender
{
    public bool? UseWindowsUIComposition { get; set; }
    public bool? UseWgl { get; set; }
    public bool? UseCompositor { get; set; }
    public bool? UseDeferredRendering { get; set; }
}

public record Render
{
    public WindowsRender Windows { get; set; }
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

    public ServerCustom ServerCustom { get; set; }
    public Render Render { get; set; }

    public string ColorMain { get; set; }
    public string ColorBack { get; set; }
    public string ColorTranBack { get; set; }
    public string ColorFont1 { get; set; }
    public string ColorFont2 { get; set; }
    public bool RGB { get; set; }
    public int RGBS { get; set; }
    public int RGBV { get; set; }

    public bool CornerRadius { get; set; }
    public float Radius { get; set; }

    public string FontName { get; set; }
    public bool FontDefault { get; set; }
}
