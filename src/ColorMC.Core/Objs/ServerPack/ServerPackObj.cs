using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.ServerPack;

/// <summary>
/// 服务器实例
/// </summary>
public record ServerPackObj
{
    /// <summary>
    /// 服务器包信息
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// Mod列表
    /// </summary>
    public List<ServerModItemObj> Mod { get; set; }
    /// <summary>
    /// 资源包列表
    /// </summary>
    public List<ServerModItemObj> Resourcepack { get; set; }
    /// <summary>
    /// 配置文件列表
    /// </summary>
    public List<ConfigPackObj> Config { get; set; }
    /// <summary>
    /// 游戏实例
    /// </summary>
    [JsonIgnore]
    public GameSettingObj Game { get; set; }
}
