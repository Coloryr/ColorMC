using Avalonia.Media.Imaging;

namespace ColorMC.Gui.Objs;

/// <summary>
/// ��ͼ��ʾ
/// </summary>
public record ScreenshotDisplayObj
{
    /// <summary>
    /// ����
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// ͼƬ
    /// </summary>
    public Bitmap Image { get; set; }

    /// <summary>
    /// ·��
    /// </summary>
    public string Local;
}
