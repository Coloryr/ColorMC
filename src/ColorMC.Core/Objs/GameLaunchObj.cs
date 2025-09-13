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
    public readonly HashSet<int> JavaVersions = [];
    public string MainClass;
    public string NativeDir;
    public GameArgObj.GameAssetIndexObj Assets;
    public FileItemObj GameJar;
    public FileItemObj? Log4JXml;
    public bool UseColorMCASM;
}
