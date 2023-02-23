using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthGameVersionObj
{
    public string version { get; set; }
    public string version_type { get; set; }
    public string date { get; set; }
    public bool major { get; set; }
}
