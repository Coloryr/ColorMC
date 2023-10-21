using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Minecraft;

public record LangObj
{
    [JsonProperty("language.name")]
    public string Name { get; set; }
    [JsonProperty("language.region")]
    public string Region { get; set; }
}
