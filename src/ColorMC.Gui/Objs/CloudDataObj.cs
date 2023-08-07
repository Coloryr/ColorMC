using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

public record CloudDataObj
{
    public DateTime ConfigTime { get; set; }
    public List<string> Config { get; set; }
}
