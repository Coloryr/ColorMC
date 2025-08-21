using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.ColorMC;

public record McModSearchObj
{
    [JsonPropertyName("type")]
    public int Type { get; set; }
    [JsonPropertyName("ids")]
    public List<string> Ids { get; set; }
    [JsonPropertyName("mcmod_type")]
    public int McmodType { get; set; }
}

public record McModSearchResObj
{
    [JsonPropertyName("res")]
    public int Res { get; set; }
    [JsonPropertyName("data")]
    public Dictionary<string, McModSearchItemObj> Data { get; set; }
}

