using Avalonia.Media;

namespace ColorMC.Gui.Objs;

public record FontDisplayObj
{
    public string FontName { get; init; }
    public FontFamily FontFamily { get; init; }

    public override string ToString()
    {
        return FontName;
    }
}