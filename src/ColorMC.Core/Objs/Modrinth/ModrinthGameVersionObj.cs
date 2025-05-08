using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthGameVersionObj
{
    [JsonPropertyName("version")]
    public string Version { get; set; }
    //public string version_type { get; set; }
    //public string date { get; set; }
    //public bool major { get; set; }
}
