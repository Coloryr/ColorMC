using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace ColorMC.Gui.Objs;

public record ThemeObj
{
    public IBrush WindowBG;
    public IBrush ProgressBarBG;
    public IBrush MainGroupBG;
    public IBrush MainGroupBorder;
    public IBrush ItemBG;
    public IBrush GameItemBG;
    public IBrush TopViewBG;
}
