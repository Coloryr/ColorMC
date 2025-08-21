using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record SakuraFrpGetChannelObj
{
    [JsonPropertyName("query")]
    public int Query { get; set; }
}
