using ColorMC.Core.Objs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record ServerPackModDisplayObj
{
    public bool Check { get; set; }
    public string FileName { get; set; }
    public string PID { get; set; }
    public string FID { get; set; }
    public SourceType Source { get; set; }
    public string Sha1 { get; set; }
    public string Url { get; set; }

    public ModDisplayObj Obj;
}
