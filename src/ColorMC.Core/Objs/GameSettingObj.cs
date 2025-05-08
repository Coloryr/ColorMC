using System.Text.Json.Serialization;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

/// <summary>
/// 加入服务器设置
/// </summary>
public record ServerObj
{
    /// <summary>
    /// 服务器地址
    /// </summary>
    public string? IP { get; set; }
    /// <summary>
    /// 服务器端口
    /// </summary>
    public ushort? Port { get; set; }
}

/// <summary>
/// 端口代理设置
/// </summary>
public record ProxyHostObj
{
    /// <summary>
    /// 服务器地址
    /// </summary>
    public string? IP { get; set; }
    /// <summary>
    /// 服务器端口
    /// </summary>
    public ushort? Port { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string? User { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
}

public record AdvanceJvmObj
{
    /// <summary>
    /// 自定义mainclass
    /// </summary>
    public string? MainClass { get; set; }
    /// <summary>
    /// 附加的classpath
    /// </summary>
    public string? ClassPath { get; set; }
}

public record LaunchDataObj
{
    /// <summary>
    /// 游戏统计
    /// </summary>
    public record TimeObj
    {
        /// <summary>
        /// 开始统计时间
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// 统计长度
        /// </summary>
        public TimeSpan Span { get; set; }
    }
    /// <summary>
    /// 实例添加时间
    /// </summary>
    public DateTime AddTime { get; set; }
    /// <summary>
    /// 上次启动时间
    /// </summary>
    public DateTime LastTime { get; set; }
    /// <summary>
    /// 游戏时间
    /// </summary>
    public TimeSpan GameTime { get; set; }
    /// <summary>
    /// 游戏统计
    /// </summary>
    public TimeSpan LastPlay { get; set; }
}

public record CustomLoader
{
    /// <summary>
    /// 后加载原版运行库
    /// </summary>
    public bool OffLib { get; set; }
    /// <summary>
    /// 删除原版运行库
    /// </summary>
    public bool RemoveLib { get; set; }
    /// <summary>
    /// 是否启用自定义启动配置
    /// </summary>
    public bool CustomJson { get; set; }
    /// <summary>
    /// 删除原有启动配置
    /// </summary>
    public bool RemoveJson { get; set; }
}

/// <summary>
/// 游戏实例
/// </summary>
public partial record GameSettingObj
{
    public string UUID { get; set; }
    /// <summary>
    /// 游戏名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 实例组名
    /// </summary>
    public string? GroupName { get; set; }
    /// <summary>
    /// 路径名
    /// </summary>
    public string DirName { get; set; }
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// Mod加载器类型
    /// </summary>
    public Loaders Loader { get; set; }
    /// <summary>
    /// Mod加载器版本
    /// </summary>
    public string? LoaderVersion { get; set; }
    /// <summary>
    /// Jvm参数
    /// </summary>
    public RunArgObj? JvmArg { get; set; }
    /// <summary>
    /// Jvm名字
    /// </summary>
    public string? JvmName { get; set; }
    /// <summary>
    /// Jvm路径
    /// </summary>
    public string? JvmLocal { get; set; }
    /// <summary>
    /// 窗口设置
    /// </summary>
    public WindowSettingObj? Window { get; set; }
    /// <summary>
    /// 加入服务器设置
    /// </summary>
    public ServerObj? StartServer { get; set; }
    /// <summary>
    /// 端口代理设置
    /// </summary>
    public ProxyHostObj? ProxyHost { get; set; }
    /// <summary>
    /// 高级Jvm设置
    /// </summary>
    public AdvanceJvmObj? AdvanceJvm { get; set; }
    /// <summary>
    /// 是否为整合包
    /// </summary>
    public bool ModPack { get; set; }
    /// <summary>
    /// 整合包类型
    /// </summary>
    public SourceType ModPackType { get; set; }
    /// <summary>
    /// 游戏发布类型
    /// </summary>
    public GameType GameType { get; set; }
    /// <summary>
    /// 整合包ID
    /// </summary>
    public string? PID { get; set; }
    /// <summary>
    /// 整合包版本
    /// </summary>
    public string? FID { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
    /// <summary>
    /// 服务器实例网址
    /// </summary>
    public string ServerUrl { get; set; }
    /// <summary>
    /// 自定义模组加载器
    /// </summary>
    public CustomLoader? CustomLoader { get; set; }
    /// <summary>
    /// 日志编码
    /// </summary>
    public int Encoding { get; set; }
    /// <summary>
    /// 自动打开日志窗口
    /// </summary>
    public bool LogAutoShow { get; set; }

    /// <summary>
    /// Mod信息
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, ModInfoObj> Mods;
    /// <summary>
    /// 游玩统计
    /// </summary>
    [JsonIgnore]
    public LaunchDataObj LaunchData;
    /// <summary>
    /// 自定义启动配置
    /// </summary>
    [JsonIgnore]
    public List<CustomGameArgObj> CustomJson;
}
