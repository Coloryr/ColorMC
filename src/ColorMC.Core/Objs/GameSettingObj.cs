using Newtonsoft.Json;

namespace ColorMC.Core.Objs;

/// <summary>
/// 加载器类型
/// </summary>
public enum Loaders
{
    /// <summary>
    /// 无Mod加载器
    /// </summary>
    Normal,
    /// <summary>
    /// Forge加载器
    /// </summary>
    Forge,
    /// <summary>
    /// Fabric加载器
    /// </summary>
    Fabric,
    /// <summary>
    /// Quilt加载器
    /// </summary>
    Quilt,
    /// <summary>
    /// NeoForge加载器
    /// </summary>
    NeoForge
}

public enum GameType
{
    Release,
    Snapshot,
    Other
}

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

/// <summary>
/// 游戏实例
/// </summary>
public record GameSettingObj
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
    public JvmArgObj? JvmArg { get; set; }
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
    /// Mod信息
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, ModInfoObj> Mods { get; set; }
    /// <summary>
    /// 游玩统计
    /// </summary>
    [JsonIgnore]
    public LaunchDataObj LaunchData { get; set; }
    [JsonIgnore]
    public bool Empty;
}
