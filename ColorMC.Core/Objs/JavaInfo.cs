using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public record JavaInfo
{
    public string Path { get; set; }
    public string Version { get; set; }
    public int MajorVersion
    {
        get
        {
            string[] vers = Version.Trim().Split('.', '_', '-', '+', 'u', 'U');
            if (vers[0] == "1")
            {
                return int.Parse(vers[1]);
            }
            else
            {
                return int.Parse(vers[0]);
            }
        }
    }
    public string Type { get; set; }
    public ArchEnum Arch { get; set; }
}
