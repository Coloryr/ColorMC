using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.ServerPack;

public record ConfigPackObj
{
    public string Name { get; set; }
    public string Sha1 { get; set; }
    public bool Zip { get; set; }
}
