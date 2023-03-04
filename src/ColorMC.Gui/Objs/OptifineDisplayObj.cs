using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Optifine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record OptifineDisplayObj
{
    public string MC { get; set; }
    public string Version { get; set; }
    public string Date { get; set; }
    public string Forge { get; set; }

    public OptifineObj Data;
    public SourceLocal Local;
}
