using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

public record LangObj
{
    [JsonPropertyName("language.name")]
    public string Name { get; set; }
    [JsonPropertyName("language.region")]
    public string Region { get; set; }
}
