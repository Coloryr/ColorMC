using ColorMC.Core.Game.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record LastUser
{
    public string UUID { get; set; }
    public AuthType Type { get; set; }
}
public record GuiConfigObj
{
    public string Version { get; set; }
    public LastUser LastUser { get; set; }
    public string BackImage { get; set; }
}
