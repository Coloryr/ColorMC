using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Frp;

public record SakuraFrpChannelObj
{
    public int id { get; set; }
    public string name { get; set; }
    public bool online { get; set; }
    public string type { get; set; }
    public int remote { get; set; }
}
