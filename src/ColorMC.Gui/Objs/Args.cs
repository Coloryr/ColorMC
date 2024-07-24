using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.Objs;

public record DownloadModArg
{
    public DownloadItemObj Item;
    public ModInfoObj Info;
}

public record DownloadModItemArg : DownloadModArg
{
    public ModDisplayModel Mod;
}
