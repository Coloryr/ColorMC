using Newtonsoft.Json;

namespace ColorMC.Core.Objs.ServerPack;

/// <summary>
/// 服务器包
/// </summary>
public record ServerPackObj
{
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string MCVersion { get; set; }
    /// <summary>
    /// 加载器类型
    /// </summary>
    public Loaders Loader { get; set; }
    /// <summary>
    /// 加载器版本
    /// </summary>
    public string LoaderVersion { get; set; }
    /// <summary>
    /// 基础网址
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// UI文件
    /// </summary>
    public string UI { get; set; }
    /// <summary>
    /// 说明文本
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// 强制更新
    /// </summary>
    public bool ForceUpdate { get; set; }

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

    [JsonIgnore]
    public GameSettingObj Game;
}
