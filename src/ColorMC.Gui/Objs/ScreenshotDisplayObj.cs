using Avalonia.Media.Imaging;

namespace ColorMC.Gui.Objs;

public record ScreenshotDisplayObj
{
    public string Name { get; set; }
    public Bitmap Image { get; set; }

    public string Local { get; set; }
}
