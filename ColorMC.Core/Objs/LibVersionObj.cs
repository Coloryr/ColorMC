using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public record LibVersionObj
{
    public string Pack { get; set; }
    public string Name { get; set; }
    public string Verison { get; set; }
    public string Extr { get; set; }
}
