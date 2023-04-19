namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 服务器储存
/// </summary>
public record ServerInfoObj
{
    /// <summary>
    /// 地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
    /// <summary>
    /// 接收材质包
    /// </summary>
    public bool AcceptTextures { get; set; }
}
