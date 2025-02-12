namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 截图项目
/// </summary>
public record ScreenshotObj
{
    /// <summary>
    /// 文件位置
    /// </summary>
    public required string File { get; set; }
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
}
