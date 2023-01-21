using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record ResourcepackDisplayObj
{
    public string Local { get; set; }
    public string Description { get; set; }
    public int PackFormat { get; set; }
    public Bitmap Icon { get; set; }
    
    public ResourcepackObj Pack { get; set; }
}
