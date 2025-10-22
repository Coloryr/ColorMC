namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 数据包信息
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
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 格式版本号
    /// </summary>
    public int PackFormat { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? Enable { get; set; }
    /// <summary>
    /// 存档
    /// </summary>
    public SaveObj World;
}
