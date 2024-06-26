using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.McMod;

public record McModTypsObj
{
    public List<string> types { get; set; }
    public List<string> sorts { get; set; }
    public List<string> versions { get; set; }
}
