using ColorMC.Core.Objs.CurseForge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record FileDisplayObj
{
    public string Name { get; set; }
    public long Download { get; set; }
    public string Size { get; set; }
    public string Time { get; set; }

    public CurseForgeObj.Data.LatestFiles File;
}
