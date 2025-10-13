using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public class BlocksObj
{
    public string Id { get; set; }
    public Dictionary<string, string> Tex { get; set; }
    public string Today { get; set; }
    public DateTime Time { get; set; }
}
