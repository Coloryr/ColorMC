using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.Frp;

public record SakuraFrpGetChannelObj
{
    [JsonPropertyName("query")]
    public int Query { get; set; }
}
