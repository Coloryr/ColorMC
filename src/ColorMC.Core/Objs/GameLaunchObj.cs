using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

/// <summary>
/// 游戏启动时的配置存储
/// </summary>
public record GameLaunchObj
{
    public readonly List<FileItemObj> GameLibs = [];
    public readonly List<FileItemObj> LoaderLibs = [];
    public readonly List<FileItemObj> InstallerLibs = [];
    public readonly List<string> JvmArgs = [];
    public readonly List<string> GameArgs = [];
    public readonly SortedSet<int> JavaVersions = [];
    public string MainClass;
    public string NativeDir;
    public string UUID;
    public GameArgObj.GameAssetIndexObj Assets;
    public FileItemObj GameJar;
    public FileItemObj? Log4JXml;
    public bool UseColorMCASM;
}

/// <summary>
/// 游戏实例实际运行使用的参数
/// </summary>
public record GameRunObj
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Obj;
    /// <summary>
    /// 使用的账户
    /// </summary>
    public LoginObj Auth;
    /// <summary>
    /// 运行路径
    /// </summary>
    public string Path;
    /// <summary>
    /// 启动参数
    /// </summary>
    public List<string> Arg;
    /// <summary>
    /// 运行环境
    /// </summary>
    public Dictionary<string, string> Env;
    /// <summary>
    /// 是否管理员启动
    /// </summary>
    public bool Admin;
}