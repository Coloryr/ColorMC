using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Mclo;

public record McloResObj
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    //public string id { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    //public string raw { get; set; }
}
