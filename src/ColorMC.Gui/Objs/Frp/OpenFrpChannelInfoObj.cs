using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpChannelInfoObj
{
    public Dictionary<string, string> proxies { get; set; }
}
