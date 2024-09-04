using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.Frp;

public record FrpShareObj
{
    public string Version { get; set; }
    public string Text { get; set; }
    public bool IsLoader { get; set; }
    public int Loader { get; set; }
}
