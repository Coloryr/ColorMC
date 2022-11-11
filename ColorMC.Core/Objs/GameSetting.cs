using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public enum Loaders
{ 
    Normal, Forge, Fabric
}

public record LoaderInfo
{ 
    public string Name { get; set; }
    public string Version { get; set; }
}

public record GameSetting
{
    public string Dir { get; set; }
    public string Name { get; set; }
    public string JvmArgs { get; set; }
    public string Version { get; set; }
    public Loaders Loader { get; set; }
    public LoaderInfo LoaderInfo { get; set; }
}
