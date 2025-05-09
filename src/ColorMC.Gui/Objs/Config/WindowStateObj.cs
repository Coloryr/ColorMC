using Avalonia.Controls;

namespace ColorMC.Gui.Objs.Config;

public record WindowStateObj
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public WindowState WindowState { get; set; }
}
