using ColorMC.Core.Objs;

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

    public bool ServerPack { get; set; }
    public string ServerUrl { get; set; }

    public bool PlayMusic { get; set; }
    public string Music { get; set; }
    public int Volume { get; set; }
    public bool SlowVolume { get; set; }
}

public record WindowsRender
{
    public bool? UseWindowsUIComposition { get; set; }
    public bool? UseWgl { get; set; }
    public bool? UseCompositor { get; set; }
    public bool? UseDeferredRendering { get; set; }
}

public record X11Render
{
    public bool? UseEGL { get; set; }
    public bool? UseGpu { get; set; }
    public bool? OverlayPopups { get; set; }
    public bool? UseDeferredRendering { get; set; }
    public bool? UseCompositor { get; set; }
}

public record Render
{
    public WindowsRender Windows { get; set; }
    public X11Render X11 { get; set; }
}

/// <summary>
/// Gui配置文件
/// </summary>
public record GuiConfigObj
{
    public string Version { get; set; }
    public LastUser LastUser { get; set; }
    public string BackImage { get; set; }
    public int BackEffect { get; set; }
    public int BackTran { get; set; }
    public int BackLimitValue { get; set; }
    public bool BackLimit { get; set; }
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

    public string FontName { get; set; }
    public bool FontDefault { get; set; }

    public bool CloseBeforeLaunch { get; set; }

    public bool WindowMode { get; set; }
}
