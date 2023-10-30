using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Mclo;

public record McloResObj
{
    public bool success { get; set; }
    public string id { get; set; }
    public string url { get; set; }
    public string raw { get; set; }
}
