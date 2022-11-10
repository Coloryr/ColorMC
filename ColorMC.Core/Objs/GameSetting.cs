using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public record GameSetting
{
    public string Dir { get; set; }
    public string Name { get; set; }
    public string JvmArgs { get; set; }
    public string Version { get; set; }
}
