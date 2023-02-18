using ColorMC.Core.Objs.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record ShaderpackDisplayObj
{
    public string Name { get; set; }
    public string Local { get; set; }

    public ShaderpackObj Shaderpack { get; set; }
}
