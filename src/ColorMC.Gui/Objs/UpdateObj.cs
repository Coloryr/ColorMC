using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ColorMC.Gui.Objs;

public record UpdateObj
{
    [JsonProperty("diff")]
    public string Diff { get; set; }
    [JsonProperty("sha1")]
    public string Sha1 { get; set; }
    [JsonProperty("core")]
    public string Core { get; set; }
    [JsonProperty("gui")]
    public string Gui { get; set; }
}
