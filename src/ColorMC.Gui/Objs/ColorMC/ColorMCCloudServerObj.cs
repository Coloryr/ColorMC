namespace ColorMC.Gui.Objs.ColorMC;

/// <summary>
/// 映射云端数据
/// </summary>
public record ColorMCCloudServerObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 当前在线人数
    /// </summary>
    public string Now { get; set; }
    /// <summary>
    /// 最大在线人数
    /// </summary>
    public string Max { get; set; }

    public ColorMCCloudShareObj? Custom { get; set; }
}
