namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 游戏Mod
/// </summary>
public record ModObj
{
    public string ModId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public HashSet<string> Author { get; set; }
    public string? Url { get; set; }
    public string Local { get; set; }
    public HashSet<string> Dependants { get; set; }
    public HashSet<Loaders> Loaders { get; set; }
    public SideType Side { get; set; }
    public bool Disable { get; set; }
    public bool CoreMod { get; set; }
    public bool ReadFail { get; set; }
    public string Sha1 { get; set; }
    public string Sha256 { get; set; }
    public GameSettingObj Game { get; set; }
    public List<ModObj>? InJar { get; set; }
}