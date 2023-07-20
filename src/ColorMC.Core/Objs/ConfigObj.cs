namespace ColorMC.Core.Objs;

public record JvmConfigObj
{
    public string Name { get; set; }
    public string Local { get; set; }
}

public record HttpObj
{
    public SourceLocal Source { get; set; }
    public int DownloadThread { get; set; }
    public string ProxyIP { get; set; }
    public ushort ProxyPort { get; set; }
    public string ProxyUser { get; set; }
    public string ProxyPassword { get; set; }

    public bool LoginProxy { get; set; }
    public bool DownloadProxy { get; set; }
    public bool GameProxy { get; set; }

    public bool CheckFile { get; set; }
    public bool CheckUpdate { get; set; }

    public bool AutoDownload { get; set; }
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
public record JvmArgObj
{
    public string? JvmArgs { get; set; }
    public string? GameArgs { get; set; }
    public string? GCArgument { get; set; }
    public GCType? GC { get; set; }
    public string? JavaAgent { get; set; }
    public uint? MaxMemory { get; set; }
    public uint? MinMemory { get; set; }

    public bool LaunchPre { get; set; }
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
    public JvmArgObj DefaultJvmArg { get; set; }
    public WindowSettingObj Window { get; set; }
    public GameCheckObj GameCheck { get; set; }
}
