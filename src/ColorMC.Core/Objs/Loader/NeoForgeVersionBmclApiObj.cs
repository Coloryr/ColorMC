using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Loader;

public record NeoForgeVersionBmclApiObj
{
    //public string mcversion { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
    //public string rawVersion { get; set; }
}
