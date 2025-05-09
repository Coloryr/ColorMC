using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record SakuraFrpChannelObj
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    //public bool online { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("remote")]
    public string Remote { get; set; }
}
