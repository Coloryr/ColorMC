using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 运行库路径
/// </summary>
public static class LibrariesPath
{
    public const string Name = "libraries";
    public static string BaseDir { get; private set; }
    public static string NativeDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = Path.GetFullPath(dir + "/" + Name);
        NativeDir = Path.GetFullPath($"{BaseDir}" + $"/native-{SystemInfo.Os}-{SystemInfo.SystemArch}".ToLower());

        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(NativeDir);
    }

    /// <summary>
    /// native路径
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>路径</returns>
    public static string GetNativeDir(string version)
    {
        var dir = Path.GetFullPath($"{NativeDir}/{version}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// 获取游戏核心路径
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>游戏路径</returns>
    public static string GetGameFile(string version)
    {
        return Path.GetFullPath($"{BaseDir}/net/minecraft/client/{version}/client-{version}.jar");
    }

    /// <summary>
    /// 获取OptiFine路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static string GetOptiFineLib(this GameSettingObj obj)
    {
        return GetOptiFineLib(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取OptiFine路径
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">optifine版本</param>
    /// <returns></returns>
    public static string GetOptiFineLib(string mc, string version)
    {
        return Path.GetFullPath($"{BaseDir}/optifine/installer/OptiFine-{mc}-{version}.jar");
    }

    /// <summary>
    /// 获取自定义加载器运行库
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<(string Name, string Local)> GetCustomLoaderLibs(this GameSettingObj obj)
    {
        if (VersionPath.GetCustomLoaderObj(obj.UUID) is { } obj1)
        {
            if (obj1.Loader is ForgeLaunchObj obj2)
            {
                var list = new List<(string, string)>();
                foreach (var item in obj2.libraries)
                {

                    list.Add((item.name, Path.GetFullPath($"{BaseDir}/{item.downloads.artifact.path}")));
                }

                return list;
            }
        }

        return [];
    }

    /// <summary>
    /// 获取自定义加载器游戏参数
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetLoaderGameArg(this GameSettingObj obj)
    {
        if (VersionPath.GetCustomLoaderObj(obj.UUID) is { } obj1)
        {
            if (obj1.Loader is ForgeLaunchObj obj2)
            {
                return new(obj2.minecraftArguments.Split(" "));
            }
        }

        return [];
    }

    public static string GetLoaderMainClass(this GameSettingObj obj)
    {
        if (VersionPath.GetCustomLoaderObj(obj.UUID) is { } obj1)
        {
            if (obj1.Loader is ForgeLaunchObj obj2)
            {
                return obj2.mainClass;
            }
        }

        return "";
    }
}