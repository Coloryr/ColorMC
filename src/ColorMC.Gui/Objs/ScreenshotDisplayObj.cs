using Avalonia.Media.Imaging;

namespace ColorMC.Gui.Objs;

/// <summary>
/// ½ØÍ¼ÏÔÊ¾
/// </summary>
public record ScreenshotDisplayObj
{
    /// <summary>
    /// Ãû×Ö
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Í¼Æ¬
    /// </summary>
    public Bitmap Image { get; set; }

    /// <summary>
    /// Â·¾¶
    /// </summary>
    public string Local;
}
