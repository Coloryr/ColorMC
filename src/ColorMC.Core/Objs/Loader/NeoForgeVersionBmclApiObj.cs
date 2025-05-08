using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

public record NeoForgeVersionBmclApiObj
{
    //public string mcversion { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
    //public string rawVersion { get; set; }
}
