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
    /// 服务器包版本
    /// </summary>
    public string PackVersion { get; set; }
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
    /// 实例名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 加载器类型
    /// </summary>
    public Loaders Loader { get; set; }
    /// <summary>
    /// 加载器版本
    /// </summary>
    public string? LoaderVersion { get; set; }
    /// <summary>
    /// 游戏发布类型
    /// </summary>
    public GameType GameType { get; set; }
    /// <summary>
    /// Jvm参数
    /// </summary>
    public RunArgObj? JvmArg { get; set; }
    /// <summary>
    /// 窗口设置
    /// </summary>
    public WindowSettingObj? Window { get; set; }
    /// <summary>
    /// 加入服务器设置
    /// </summary>
    public ServerObj? StartServer { get; set; }
    /// <summary>
    /// 高级Jvm设置
    /// </summary>
    public AdvanceJvmObj? AdvanceJvm { get; set; }

    [JsonInclude]
    public GameSettingObj Game;
}
