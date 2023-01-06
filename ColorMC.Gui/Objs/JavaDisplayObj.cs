using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record JavaDisplayObj
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string MajorVersion { get; set; }
    public string Version { get; set; }
    public string Type { get; set; }
    public string Arch { get; set; }
}
