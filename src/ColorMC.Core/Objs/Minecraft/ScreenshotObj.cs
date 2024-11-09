using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Minecraft;

public record ScreenshotObj
{
    public required string File { get; set; }
    public string Name { get; set; }
}
