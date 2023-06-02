using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

/// <summary>
/// ���ʰ���ʾ
/// </summary>
public record ResourcepackDisplayObj
{
    /// <summary>
    /// ·��
    /// </summary>
    public string Local { get; set; }
    /// <summary>
    /// ����
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// ���汾
    /// </summary>
    public int PackFormat { get; set; }
    /// <summary>
    /// ͼ��
    /// </summary>
    public Bitmap Icon { get; set; }

    /// <summary>
    /// ����
    /// </summary>
    public ResourcepackObj Pack;
}
