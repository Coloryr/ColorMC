using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record SakuraFrpObj
{ 
    public string Key { get; set; }
}

public record FrpConfigObj
{
    public SakuraFrpObj SakuraFrp { get; set; }
}
