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

public record DnsObj
{
    public bool Enable { get; set; }
    public List<string> Dns { get; set; }
    public List<string> Https { get; set; }
    public DnsType DnsType { get; set; }
    public bool HttpProxy { get; set; }
}

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
    /// 默认G1垃圾回收器 兼容JAVA9
    /// </summary>
    G1GC = 0,

    /// <summary>
    /// 串行垃圾回收器
    /// </summary>
    SerialGC = 1,

    /// <summary>
    /// 并行垃圾回收器
    /// </summary>
    ParallelGC = 2,

    /// <summary>
    /// 并发标记扫描垃圾回收器
    /// </summary>
    CMSGC = 3,

    /// <summary>
    /// 设置为空（手动设置）
    /// </summary>
    User = 4
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
    /// 自定义GC参数
    /// </summary>
    public string? GCArgument { get; set; }
    /// <summary>
    /// 自定义环境变量
    /// </summary>
    public string? JvmEnv { get; set; }
    public GCType? GC { get; set; }
    public string? JavaAgent { get; set; }
    public uint? MaxMemory { get; set; }
    public uint? MinMemory { get; set; }

    public bool LaunchPre { get; set; }
    public bool PreRunSame { get; set; }
    public string? LaunchPreData { get; set; }
    public bool LaunchPost { get; set; }
    public string? LaunchPostData { get; set; }
}

public record GameCheckObj
{
    public bool CheckCore { get; set; }
    public bool CheckLib { get; set; }
    public bool CheckAssets { get; set; }
    public bool CheckMod { get; set; }

    public bool CheckCoreSha1 { get; set; }
    public bool CheckLibSha1 { get; set; }
    public bool CheckAssetsSha1 { get; set; }
    public bool CheckModSha1 { get; set; }
}

public record ConfigObj
{
    public LanguageType Language { get; set; }
    public string Version { get; set; }
    public List<JvmConfigObj> JavaList { get; set; }

    public HttpObj Http { get; set; }
    public DnsObj Dns { get; set; }
    public RunArgObj DefaultJvmArg { get; set; }
    public WindowSettingObj Window { get; set; }
    public GameCheckObj GameCheck { get; set; }
    public bool SafeLog4j { get; set; }
}
