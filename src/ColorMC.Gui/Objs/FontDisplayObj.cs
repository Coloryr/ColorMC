using Avalonia.Media;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 字体显示
/// </summary>
public record FontDisplayObj
{
    /// <summary>
    /// 字体名字
    /// </summary>
    public string FontName { get; init; }
    /// <summary>
    /// 字体样式
    /// </summary>
    public FontFamily FontFamily { get; init; }

    public override string ToString()
    {
        return FontName;
    }
}