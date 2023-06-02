using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 材质包显示
/// </summary>
public record ResourcepackDisplayObj
{
    /// <summary>
    /// 路径
    /// </summary>
    public string Local { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 包版本
    /// </summary>
    public int PackFormat { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public Bitmap Icon { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public ResourcepackObj Pack;
}
