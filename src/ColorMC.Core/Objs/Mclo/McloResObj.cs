using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Mclo;

public record McloResObj
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    //public string id { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    //public string raw { get; set; }
}
