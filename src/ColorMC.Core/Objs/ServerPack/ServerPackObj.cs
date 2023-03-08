using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.ServerPack;

public record ServerPackObj
{
    public string Url { get; set; }
    public string Version { get; set; }
    public string UI { get; set; }
    public string Text { get; set; }
    public bool LockMod { get; set; }
    public bool ForceUpdate { get; set; }

    public List<ModItem> Mod { get; set; }
    public List<ModItem> Resourcepack { get; set; }
    public List<ConfigPackObj> Config { get; set; }
}
