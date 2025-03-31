using System.Collections.Generic;
using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

public record UiBackConfigObj
{
    public bool EnableBG { get; set; }
    public string? BackImage { get; set; }
    public int BackEffect { get; set; }
    public int BackTran { get; set; }
    public bool BackLimit { get; set; }
    public int BackLimitValue { get; set; }
}

public record UiColorConfigObj
{
    public ColorType ColorType { get; set; }
    public string ColorMain { get; set; }
    public bool RGB { get; set; }
    public int RGBS { get; set; }
    public int RGBV { get; set; }
    public bool WindowMode { get; set; }
    public bool Simple { get; set; }
    public StyleSetting Style { get; set; }
    public LogColorSetting LogColor { get; set; }
}

public record UiOtherConfigObj
{
    public HeadSetting Head { get; set; }
    public CardSetting Card { get; set; }
    public bool CloseBeforeLaunch { get; set; }
}

public record LaunchCheckConfigObj
{
    public GameCheckObj GameCheck { get; set; }
    public LaunchCheckSetting LaunchCheck { get; set; }
}

public record ServerOptConfigObj
{
    public string IP { get; set; }
    public ushort Port { get; set; }
    public bool Motd { get; set; }
    public bool JoinServer { get; set; }
    public string MotdColor { get; set; }
    public string MotdBackColor { get; set; }
    public bool AdminLaunch { get; set; }
    public bool GameAdminLaunch { get; set; }
}

public record ServerLockConfigObj
{
    public bool LockGame { get; set; }
    public string? GameName { get; set; }
    public bool LockLogin { get; set; }
    public List<LockLoginSetting> LockLogins { get; set; }
}

public record ServerUiConfigObj
{
    public bool EnableUI { get; set; }
    public bool CustomIcon { get; set; }
    public string? IconFile { get; set; }
    public bool CustomStart { get; set; }
    public string? StartIconFile { get; set; }
    public DisplayType DisplayType { get; set; }
    public string? StartText { get; set; }
}

public record ServerMusicConfigObj
{
    public bool PlayMusic { get; set; }
    public string? Music { get; set; }
    public int Volume { get; set; }
    public bool SlowVolume { get; set; }
    public bool MusicLoop { get; set; }
    public bool RunPause { get; set; }
}