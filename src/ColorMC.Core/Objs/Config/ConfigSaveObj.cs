using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Config;

public record ConfigSaveObj
{
    public string Name;
    public object Obj;
    public string Local;
}
