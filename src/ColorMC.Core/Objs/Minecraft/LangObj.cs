using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Minecraft;

public record LangObj
{
    [JsonProperty("language.name")]
    public string Name { get; set; }
    [JsonProperty("language.region")]
    public string Region { get; set; }
}
