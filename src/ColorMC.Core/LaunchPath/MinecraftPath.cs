namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 游戏路径
/// </summary>
public static class MinecraftPath
{
    /// <summary>
    /// 游戏路径
    /// </summary>
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = Path.Combine(dir, Names.NameMinecraftKey);

        Directory.CreateDirectory(BaseDir);

        AssetsPath.Init(BaseDir);
        LibrariesPath.Init(BaseDir);
        InstancesPath.Init(BaseDir);
        VersionPath.Init(BaseDir);
    }
}
