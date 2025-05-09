using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

/// <summary>
/// 游戏启动时的配置存储
/// </summary>
public record GameLaunchObj
{
    public List<FileItemObj> GameLibs = [];
    public List<FileItemObj> LoaderLibs = [];
    public List<FileItemObj> InstallerLibs = [];
    public List<string> JvmArgs = [];
    public List<string> GameArgs = [];
    public HashSet<int> JavaVersions = [];
    public string MainClass;
    public string NativeDir;
    public GameArgObj.GameAssetIndexObj Assets;
    public FileItemObj GameJar;
    public FileItemObj Log4JXml;
    public bool UseColorMCASM;
}
