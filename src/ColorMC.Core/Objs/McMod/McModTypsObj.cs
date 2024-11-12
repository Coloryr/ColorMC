using Newtonsoft.Json;

namespace ColorMC.Core.Objs.McMod;

public record McModTypsObj
{
    [JsonProperty("types")]
    public List<string> Types { get; set; }
    [JsonProperty("sorts")]
    public List<string> Sorts { get; set; }
    [JsonProperty("versions")]
    public List<string> Versions { get; set; }
}
