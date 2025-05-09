using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.ColorMC;

public record UpdateObj
{
    [JsonPropertyName("diff")]
    public string Diff { get; set; }
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }
    [JsonPropertyName("core")]
    public string Core { get; set; }
    [JsonPropertyName("gui")]
    public string Gui { get; set; }
}
