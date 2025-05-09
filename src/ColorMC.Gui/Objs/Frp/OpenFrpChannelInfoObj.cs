using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpChannelInfoObj
{
    [JsonPropertyName("proxies")]
    public Dictionary<string, string> Proxies { get; set; }
}
