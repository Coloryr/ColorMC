namespace ColorMC.Gui.Objs;

/// <summary>
/// 服务器包配置显示
/// </summary>
public record ServerPackConfigDisplayObj
{
    /// <summary>
    /// 组名
    /// </summary>
    public string Group { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    public string Url { get; set; }
}
