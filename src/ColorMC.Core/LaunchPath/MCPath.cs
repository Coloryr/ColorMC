using ColorMC.Core.Helpers;

namespace ColorMC.Core.LaunchPath;

public static class MCPath
{
    private const string Name = "minecraft";
    public static string BaseDir { get; private set; } = "";

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir + Name;

        Logs.Info(string.Format(LanguageHelper.GetName("Core.Path.Info1"), BaseDir));

        Directory.CreateDirectory(BaseDir);

        AssetsPath.Init(BaseDir);
        LibrariesPath.Init(BaseDir);
        InstancesPath.Init(BaseDir);
        VersionPath.Init(BaseDir);
    }
}
