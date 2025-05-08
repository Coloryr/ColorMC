using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record ProfileNameObj
{
    [JsonPropertyName("id")]
    public string UUID { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
