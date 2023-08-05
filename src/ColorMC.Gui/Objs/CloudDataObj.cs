using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record CloudDataObj
{
    public DateTime ConfigTime { get; set; }
    public List<string> Config { get; set; }
}
