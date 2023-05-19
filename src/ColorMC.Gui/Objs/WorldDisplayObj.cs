using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

public record WorldDisplayObj
{
    public string Name;
    public string Mode;
    public string Time;
    public string Local;
    public string Difficulty;
    public bool Hardcore;
    public Bitmap? Pic;

    public WorldObj World;
}
