using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.ColorMC;

public record McModTypsResObj
{
    [JsonPropertyName("res")]
    public int Res { get; set; }
    [JsonPropertyName("data")]
    public McModTypsObj Data { get; set; }
}

public record McModTypsObj
{
    [JsonPropertyName("types")]
    public List<string> Types { get; set; }
    [JsonPropertyName("sorts")]
    public List<string> Sorts { get; set; }
    [JsonPropertyName("versions")]
    public List<string> Versions { get; set; }
}
