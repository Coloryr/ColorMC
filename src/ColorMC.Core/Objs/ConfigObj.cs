namespace ColorMC.Core.Objs;

public record JvmConfigObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Local { get; set; }
}

public record HttpObj
{
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceLocal Source { get; set; }
    /// <summary>
    /// 下载线程数
    /// </summary>
    public int DownloadThread { get; set; }

    /// <summary>
    /// 代理地址
    /// </summary>
    public string ProxyIP { get; set; }
    /// <summary>
    /// 代理端口
    /// </summary>
    public ushort ProxyPort { get; set; }
    /// <summary>
    /// 代理用户
    /// </summary>
    public string ProxyUser { get; set; }
    /// <summary>
    /// 代理密码
    /// </summary>
    public string ProxyPassword { get; set; }

    /// <summary>
    /// 登录使用代理
    /// </summary>
    public bool LoginProxy { get; set; }
    /// <summary>
    /// 下载使用代理
    /// </summary>
    public bool DownloadProxy { get; set; }
    /// <summary>
    /// 游戏使用代理
    /// </summary>
    public bool GameProxy { get; set; }

    /// <summary>
    /// 检查下载文件完整性
    /// </summary>
    public bool CheckFile { get; set; }

    /// <summary>
    /// 自动下载缺失文件
    /// </summary>
    public bool AutoDownload { get; set; }
}

/// <summary>
/// 自定义Dns设置
/// </summary>
public record DnsObj
{
    /// <summary>
    /// 是否启用自定义DNS
    /// </summary>
    public bool Enable { get; set; }
    /// <summary>
    /// DNS IP地址
    /// </summary>
    public List<string> Dns { get; set; }
    /// <summary>
    /// DNS over HTTPS地址
    /// </summary>
    public List<string> Https { get; set; }
    /// <summary>
    /// 启用的DNS类型
    /// </summary>
    public DnsType DnsType { get; set; }
    /// <summary>
    /// 是否对代理也启用
    /// </summary>
    public bool HttpProxy { get; set; }
}

/// <summary>
/// 游戏窗口设置
/// </summary>
public record WindowSettingObj
{
    /// <summary>
    /// 全屏
    /// </summary>
    public bool? FullScreen { get; set; }

    /// <summary>
    /// 高
    /// </summary>
    public uint? Height { get; set; }

    /// <summary>
    /// 宽
    /// </summary>
    public uint? Width { get; set; }
    /// <summary>
    /// 游戏标题
    /// </summary>
    public string? GameTitle { get; set; }
    /// <summary>
    /// 随机游戏标题
    /// </summary>
    public bool RandomTitle { get; set; }
    /// <summary>
    /// 循环游戏标题
    /// </summary>
    public bool CycTitle { get; set; }
    /// <summary>
    /// 循环游戏标题延迟
    /// </summary>
    public int TitleDelay { get; set; }
}

public enum GCType
{
    /// <summary>
    /// 自动选择
    /// </summary>
    Auto,
    /// <summary>
    /// G1垃圾回收器
    /// </summary>
    G1GC,
    /// <summary>
    /// 分代式GC
    /// </summary>
    ZGC,
    /// <summary>
    /// 不添加GC参数
    /// </summary>
    None
}

/// <summary>
/// 启动参数
/// </summary>
public record RunArgObj
{
    /// <summary>
    /// 删除原有的Jvm参数
    /// </summary>
    public bool? RemoveJvmArg { get; set; }
    /// <summary>
    /// 删除原有的游戏参数
    /// </summary>
    public bool? RemoveGameArg { get; set; }
    /// <summary>
    /// 自定义Jvm参数
    /// </summary>
    public string? JvmArgs { get; set; }
    /// <summary>
    /// 自定义游戏参数
    /// </summary>
    public string? GameArgs { get; set; }
    /// <summary>
    /// 自定义环境变量
    /// </summary>
    public string? JvmEnv { get; set; }
    /// <summary>
    /// GC模式
    /// </summary>
    public GCType? GC { get; set; }
    /// <summary>
    /// 自定义JavaAgent
    /// </summary>
    public string? JavaAgent { get; set; }
    /// <summary>
    /// 最大内存
    /// </summary>
    public uint? MaxMemory { get; set; }
    /// <summary>
    /// 最小内存
    /// </summary>
    public uint? MinMemory { get; set; }
    /// <summary>
    /// 启用ColorASM
    /// </summary>
    public bool ColorASM { get; set; }

    /// <summary>
    /// 启动前运行
    /// </summary>
    public bool LaunchPre { get; set; }
    /// <summary>
    /// 是否同时启动游戏
    /// </summary>
    public bool PreRunSame { get; set; }
    /// <summary>
    /// 启动前运行
    /// </summary>
    public string? LaunchPreData { get; set; }
    /// <summary>
    /// 启动后运行
    /// </summary>
    public bool LaunchPost { get; set; }
    /// <summary>
    /// 启动后运行
    /// </summary>
    public string? LaunchPostData { get; set; }
}

/// <summary>
/// 游戏文件检查
/// </summary>
public record GameCheckObj
{
    /// <summary>
    /// 检查游戏核心
    /// </summary>
    public bool CheckCore { get; set; }
    /// <summary>
    /// 检查运行库
    /// </summary>
    public bool CheckLib { get; set; }
    /// <summary>
    /// 检查资源文件
    /// </summary>
    public bool CheckAssets { get; set; }
    /// <summary>
    /// 检查模组
    /// </summary>
    public bool CheckMod { get; set; }

    /// <summary>
    /// 检查游戏核心
    /// </summary>
    public bool CheckCoreSha1 { get; set; }
    /// <summary>
    /// 检查运行库
    /// </summary>
    public bool CheckLibSha1 { get; set; }
    /// <summary>
    /// 检查资源文件
    /// </summary>
    public bool CheckAssetsSha1 { get; set; }
    /// <summary>
    /// 检查模组
    /// </summary>
    public bool CheckModSha1 { get; set; }
}

/// <summary>
/// Core配置文件
/// </summary>
public record ConfigObj
{
    /// <summary>
    /// 配置文件版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// Java列表
    /// </summary>
    public List<JvmConfigObj> JavaList { get; set; }
    /// <summary>
    /// 联网设置
    /// </summary>
    public HttpObj Http { get; set; }
    /// <summary>
    /// 内置DNS设置
    /// </summary>
    public DnsObj Dns { get; set; }
    /// <summary>
    /// 启动参数
    /// </summary>
    public RunArgObj DefaultJvmArg { get; set; }
    /// <summary>
    /// 游戏窗口设置
    /// </summary>
    public WindowSettingObj Window { get; set; }
    /// <summary>
    /// 游戏检查设置
    /// </summary>
    public GameCheckObj GameCheck { get; set; }
    /// <summary>
    /// 安全log4j
    /// </summary>
    public bool SafeLog4j { get; set; }
}
