using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthGameVersionObj
{
    [JsonProperty("version")]
    public string Version { get; set; }
    //public string version_type { get; set; }
    //public string date { get; set; }
    //public bool major { get; set; }
}
