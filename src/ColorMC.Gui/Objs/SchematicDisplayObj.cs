using ColorMC.Core.Objs.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record SchematicDisplayObj
{
    public string Name { get; set; }
    public string Local { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }


    public SchematicObj Schematic;
}
