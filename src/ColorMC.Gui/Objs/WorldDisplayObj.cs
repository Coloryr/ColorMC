using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

public record WorldDisplayObj
{
    public string Name { get; set; }
    public string Mode { get; set; }
    public string Time { get; set; }
    public string Local { get; set; }
    public string Difficulty { get; set; }
    public bool Hardcore { get; set; }
    public Bitmap? Pic { get; set; }

    public WorldObj World { get; set; }
}
