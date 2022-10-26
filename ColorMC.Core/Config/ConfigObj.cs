using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Config;

public record ConfigObj
{
    public string Version { get; set; }
    public string MCPath { get; set; }

}
