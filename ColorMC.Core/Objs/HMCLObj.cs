using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public record HMCLObj
{
    public string manifestType { get; set; }
    public int manifestVersion { get; set; }
    public string name { get; set; }
    public string version { get; set; }

}
