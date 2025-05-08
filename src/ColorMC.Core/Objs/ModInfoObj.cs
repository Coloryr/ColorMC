using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs;

/// <summary>
/// 模组在线信息
/// </summary>
public record ModInfoObj
{
    /// <summary>
    /// 游戏路径文件夹
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 文件名
    /// </summary>
    public string File { get; set; }
    /// <summary>
    /// 校验值
    /// </summary>
    [JsonPropertyName("SHA1")]
    public string Sha1 { get; set; }
    /// <summary>
    /// 下载连接
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 模组ID
    /// </summary>
    public string ModId { get; set; }
    /// <summary>
    /// 文件ID
    /// </summary>
    public string FileId { get; set; }
}
