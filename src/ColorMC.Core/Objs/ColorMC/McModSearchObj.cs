using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.ColorMC;

public record McModSearchObj
{
    [JsonPropertyName("type")]
    public int Type { get; set; }
    [JsonPropertyName("ids")]
    public List<string> Ids { get; set; }
}

public record McModSearchResObj
{
    [JsonPropertyName("res")]
    public int Res { get; set; }
    [JsonPropertyName("data")]
    public Dictionary<string, McModSearchItemObj> Data { get; set; }
}

