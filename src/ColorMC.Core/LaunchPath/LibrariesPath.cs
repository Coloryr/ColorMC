using ColorMC.Core.Helpers;
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
                foreach (var item in obj2.Libraries)
                {

                    list.Add((item.Name, Path.GetFullPath($"{BaseDir}/{item.Downloads.Artifact.Path}")));
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
                return new(obj2.MinecraftArguments.Split(" "));
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
    public static async Task<List<string>> GetLibsAsync(this GameSettingObj obj)
    {
        var v2 = obj.IsGameVersionV2();
        var list = new Dictionary<LibVersionObj, string>();
        var version = VersionPath.GetVersion(obj.Version)!;

        if (obj.Loader == Loaders.Custom && obj.CustomLoader?.OffLib == true)
        {
            var list1 = await DownloadItemHelper.BuildGameLibsAsync(version);
            foreach (var item in list1)
            {
                if (item.Later == null)
                {
                    //不添加lwjgl
                    if (SystemInfo.Os == OsType.Android && item.Name.Contains("org.lwjgl"))
                    {
                        continue;
                    }
                    list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), Path.GetFullPath(item.Local));
                }
            }
        }
        else
        {
            var list1 = await DownloadItemHelper.BuildGameLibsAsync(version);
            foreach (var item in list1)
            {
                if (item.Later == null)
                {
                    //不添加lwjgl
                    if (SystemInfo.Os == OsType.Android && item.Name.Contains("org.lwjgl"))
                    {
                        continue;
                    }
                    list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), Path.GetFullPath(item.Local));
                }
            }
        }

        //LoaderLib
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;

            var list2 = DownloadItemHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!,
                obj.Loader == Loaders.NeoForge, v2, false);

            foreach (var item in list2)
            {
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), item.Local);
            }

            if (v2)
            {
                list.AddOrUpdate(new(), GameHelper.ForgeWrapper);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.Libraries)
            {
                var name = PathHelper.NameToPath(item.Name);
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name),
                    Path.GetFullPath($"{BaseDir}/{name}"));
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            foreach (var item in quilt.Libraries)
            {
                var name = PathHelper.NameToPath(item.Name);
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name),
                    Path.GetFullPath($"{BaseDir}/{name}"));
            }
        }
        else if (obj.Loader == Loaders.OptiFine)
        {
            GameHelper.ReadyOptifineWrapper();
            list.AddOrUpdate(new(), GameHelper.OptifineWrapper);
        }
        else if (obj.Loader == Loaders.Custom)
        {
            var list2 = obj.GetCustomLoaderLibs();
            foreach (var (Name, Local) in list2)
            {
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(Name), Local);
            }
        }

        //游戏核心
        var list3 = new List<string>(list.Values);
        if (obj.Loader != Loaders.NeoForge)
        {
            list3.Add(GetGameFile(obj.Version));
        }

        return list3;
    }
}