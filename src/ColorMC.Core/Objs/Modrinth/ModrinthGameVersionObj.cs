namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthGameVersionObj
{
    public string version { get; set; }
    public string version_type { get; set; }
    public string date { get; set; }
    public bool major { get; set; }
}
