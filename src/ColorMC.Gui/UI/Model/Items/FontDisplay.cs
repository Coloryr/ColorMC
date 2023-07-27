using Avalonia.Media;

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