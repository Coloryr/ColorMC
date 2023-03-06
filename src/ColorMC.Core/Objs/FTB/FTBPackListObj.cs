using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.FTB;

public record FTBPackListObj
{
    public List<int> curseforge { get; set; }
    public List<int> packs { get; set; }
    public int total { get; set; }
    public long refreshed { get; set; }
    public string status { get; set; }
}
