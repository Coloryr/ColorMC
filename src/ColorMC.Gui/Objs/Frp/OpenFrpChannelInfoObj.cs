using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpChannelInfoObj
{
    public Dictionary<string, string> proxies { get; set; }
}
