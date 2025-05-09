using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 材质包
/// </summary>
public record ResourcepackObj
{
    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }
    /// <summary>
    /// 材质包格式
    /// </summary>
    [JsonPropertyName("pack_format")]
    public int PackFormat { get; set; }

    [JsonIgnore]
    public string Sha1 { get; set; }
    [JsonIgnore]
    public string Sha256 { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    [JsonIgnore]
    public string Local { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    [JsonIgnore]
    public byte[] Icon { get; set; }
    /// <summary>
    /// 是否损坏
    /// </summary>
    [JsonIgnore]
    public bool Broken { get; set; }
}
