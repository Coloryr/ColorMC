namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 数据包储存
/// </summary>
public record DataPackObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; }
    public string Description { get; set; }
    public int PackFormat { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? Enable { get; set; }

    public WorldObj World;
}
