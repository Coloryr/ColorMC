using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record ProfileNameObj
{
    [JsonProperty("id")]
    public string UUID { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}
