using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record ServerPackConfigDisplayObj
{
    public string Group { get; set; }
    public string Type { get; set; }
    public string Url { get; set; }
}
