using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 运行库路径
/// </summary>
public static class LibrariesPath
{
    /// <summary>
    /// 运行库路径
    /// </summary>
    public static string BaseDir { get; private set; }
    /// <summary>
    /// 本机二进制路径
    /// </summary>
    public static string NativeDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = Path.Combine(dir, Names.NameLibDir);
        NativeDir = Path.Combine(BaseDir, $"native-{SystemInfo.Os}-{SystemInfo.SystemArch}".ToLower());

        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(NativeDir);
    }

    /// <summary>
    /// native路径
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>路径</returns>
    public static string GetNativeDir(string? version)
    {
        string dir;
        if (version == null)
        {
            dir = Path.Combine(NativeDir, Names.NameDefaultDir);
        }
        else
        {
            dir = Path.Combine(NativeDir, version);
        }
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
        return Path.Combine(BaseDir, "net", "minecraft", "client", version, $"client-{version}.jar");
    }

    /// <summary>
    /// 获取游戏核心路径
    /// </summary>
    /// <param name="name">保存的名字</param>
    /// <returns></returns>
    public static string GetGameFileWithDir(string name)
    {
        return Path.Combine(BaseDir, "net", "minecraft", "client", $"{name}.jar");
    }

    /// <summary>
    /// 获取游戏核心路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>游戏路径</returns>
    public static string GetGameFile(this GameSettingObj obj)
    {
        return GetGameFile(obj.Version);
    }

    /// <summary>
    /// 获取OptiFine路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static string GetOptifineFile(this GameSettingObj obj)
    {
        return GetOptifineFile(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取OptiFine路径
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">optifine版本</param>
    /// <returns></returns>
    public static string GetOptifineFile(string mc, string version)
    {
        return Path.Combine(BaseDir, "optifine", "installer", $"OptiFine-{mc}-{version}.jar");
    }

    ///// <summary>
    ///// 获取自定义加载器运行库
    ///// </summary>
    ///// <param name="obj"></param>
    ///// <returns></returns>
    //public static List<(string Name, string Local)> GetCustomLoaderLibs(this GameSettingObj obj)
    //{
    //    if (obj.GetCustomLoaderObj() is { } obj1)
    //    {
    //        if (obj1.Loader is ForgeLaunchObj obj2)
    //        {
    //            var list = new List<(string, string)>();
    //            foreach (var item in obj2.Libraries)
    //            {
    //                list.Add((item.Name, Path.GetFullPath($"{BaseDir}/{item.Downloads.Artifact.Path}")));
    //            }

    //            return list;
    //        }
    //    }

    //    return [];
    //}

    /// <summary>
    /// 获取自定义加载器游戏参数
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetLoaderGameArg(this GameSettingObj obj)
    {
        if (obj.GetCustomLoaderObj() is { } obj1)
        {
            if (obj1.Loader is ForgeLaunchObj obj2)
            {
                return [.. obj2.MinecraftArguments.Split(" ")];
            }
        }

        return [];
    }

    /// <summary>
    /// 获取自定义加载器主类
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetLoaderMainClass(this GameSettingObj obj)
    {
        if (obj.GetCustomLoaderObj() is { } obj1)
        {
            if (obj1.Loader is ForgeLaunchObj obj2)
            {
                return obj2.MainClass;
            }
        }

        return "";
    }

    /// <summary>
    /// 获取所有Lib
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">V2模式</param>
    /// <returns>Lib地址列表</returns>
    public static List<string> GetLibs(this GameSettingObj obj, GameLaunchObj arg)
    {
        var gameList = new Dictionary<LibVersionObj, string>();

        foreach (var item in arg.GameLibs)
        {
            if (item.Later == null)
            {
                gameList.AddOrUpdate(FunctionUtils.MakeVersionObj(item.Name), Path.GetFullPath(item.Local));
            }
        }
        var output = new Dictionary<LibVersionObj, string>();

        if (obj.CustomLoader?.CustomJson == true)
        {
            foreach (var item in gameList)
            {
                output.AddOrUpdate(item.Key, item.Value);
            }

            return [.. output.Values, Path.GetFullPath(arg.GameJar.Local)];
        }

        var loaderList = new Dictionary<LibVersionObj, string>();

        foreach (var item in arg.LoaderLibs)
        {
            if (item.Later == null)
            {
                loaderList.AddOrUpdate(FunctionUtils.MakeVersionObj(item.Name), Path.GetFullPath(item.Local));
            }
        }

        //拼接运行库列表
        if (obj.Loader == Loaders.Custom && obj.CustomLoader?.OffLib == true)
        {
            foreach (var item in loaderList)
            {
                output.AddOrUpdate(item.Key, item.Value);
            }
            if (obj.CustomLoader?.RemoveLib != true)
            {
                foreach (var item in gameList)
                {
                    if (output.ContainsKey(item.Key))
                    {
                        continue;
                    }
                    output.Add(item.Key, item.Value);
                }
            }
        }
        else
        {
            if (obj.CustomLoader?.RemoveLib != true)
            {
                foreach (var item in gameList)
                {
                    output.AddOrUpdate(item.Key, item.Value);
                }
            }
            foreach (var item in loaderList)
            {
                output.AddOrUpdate(item.Key, item.Value);
            }
        }

        var output1 = output.Values.ToList();

        //游戏核心
        if (obj.Loader != Loaders.NeoForge)
        {
            output1.Add(Path.GetFullPath(arg.GameJar.Local));
        }

        return output1;
    }
}