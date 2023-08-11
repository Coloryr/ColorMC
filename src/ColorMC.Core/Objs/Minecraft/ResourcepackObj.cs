namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 资源包
/// </summary>
public record ResourcepackObj
{
    /// <summary>
    /// 描述
    /// </summary>
    public string description { get; set; }
    /// <summary>
    /// 资源包格式
    /// </summary>
    public int pack_format { get; set; }

    /// <summary>
    /// Sha1
    /// </summary>
    public string Sha1 { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Local { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public byte[] Icon { get; set; }
    /// <summary>
    /// 是否损坏
    /// </summary>
    public bool Broken { get; set; }
}
