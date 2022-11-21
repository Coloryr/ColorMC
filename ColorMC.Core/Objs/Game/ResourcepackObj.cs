using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record ResourcepackObj
{
    public record Pack
    {
        public string description { get; set; }
        public int pack_format { get; set; }
    }
    public Pack pack { get; set; }

    [JsonIgnore]
    public string Name { get; set; }
    [JsonIgnore]
    public byte[] Icon { get; set; }
}
