using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public record FontDisplay
{
    public string FontName { get; init; }
    public FontFamily FontFamily { get; init; }

    public override string ToString()
    {
        return FontName;
    }
}