using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

public static class MCPath
{
    public static string BaseDir { get; private set; }

    public static void Init()
    {
        Logs.Info($"MC文件夹在{ConfigUtils.Config.MCPath}");

        BaseDir = ConfigUtils.Config.MCPath;

        Directory.CreateDirectory(BaseDir);

        AssetsPath.Init(BaseDir);
        LibrariesPath.Init(BaseDir);
        InstancesPath.Init(BaseDir);
        VersionPath.Init(BaseDir);
    }
}
