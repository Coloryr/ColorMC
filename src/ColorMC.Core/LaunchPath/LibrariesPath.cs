using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using System.Collections.Concurrent;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 运行库
/// </summary>
public static class LibrariesPath
{
    private const string Name = "libraries";
    public static string BaseDir { get; private set; }
    private static string NativeDir;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        NativeDir = BaseDir + $"/native-{SystemInfo.Os}-{SystemInfo.SystemArch}".ToLower();

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
        string dir = $"{NativeDir}/{version}";
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// 检查游戏运行库
    /// </summary>
    /// <param name="obj">游戏数据</param>
    /// <returns>丢失的库</returns>
    public static async Task<List<DownloadItemObj>> CheckGame(GameArgObj obj)
    {
        var list = new List<DownloadItemObj>();

        var list1 = await GameHelper.MakeGameLibs(obj);

        await Parallel.ForEachAsync(list1, async (item, cancel) =>
        {
            if (!File.Exists(item.Local))
            {
                list.Add(item);
                return;
            }
            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.Read,
                FileShare.Read);
            var sha1 = Funtcions.GenSha1(stream);
            if (!string.IsNullOrWhiteSpace(item.SHA1)
            && item.SHA1 != sha1)
            {
                list.Add(item);
            }

            item.Later?.Invoke(stream);

            return;
        });

        return list;
    }

    /// <summary>
    /// 检查Forge的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>丢失的库</returns>
    public static async Task<ConcurrentBag<DownloadItemObj>?> CheckForge(GameSettingObj obj)
    {
        var version1 = VersionPath.GetGame(obj.Version)!;
        var v2 = CheckRule.GameLaunchVersion(version1);
        if (v2)
        {
            ForgeHelper.ReadyForgeWrapper();
        }

        var forge = VersionPath.GetForgeObj(obj.Version, obj.LoaderVersion);
        if (forge == null)
            return null;

        var list = new ConcurrentBag<DownloadItemObj>();
        var list1 = ForgeHelper.MakeForgeLibs(forge, obj.Version, obj.LoaderVersion);

        //forge本体
        await Parallel.ForEachAsync(list1, async (item, cancel) =>
        {
            if (!File.Exists(item.Local))
            {
                list.Add(item);
                return;
            }
            if (item.SHA1 == null)
                return;

            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            var sha1 = Funtcions.GenSha1(stream);
            if (item.SHA1 != sha1)
            {
                list.Add(item);
            }
        });

        var forgeinstall = VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion);
        if (forgeinstall == null && v2)
            return null;

        //forge安装器
        if (forgeinstall != null)
        {
            await Parallel.ForEachAsync(forgeinstall.libraries, async (item, cacenl) =>
            {
                if (item.name.StartsWith("net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item.downloads.artifact.url))
                {
                    var item1 = ForgeHelper.BuildForgeUniversal(obj.Version, obj.LoaderVersion);
                    item1.SHA1 = item.downloads.artifact.sha1;
                    if (!File.Exists(item1.Local))
                    {
                        list.Add(item1);
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(item1.SHA1))
                    {
                        using var stream1 = new FileStream(item1.Local, FileMode.Open, FileAccess.ReadWrite,
                       FileShare.ReadWrite);
                        var sha11 = Funtcions.GenSha1(stream1);
                        if (sha11 != item1.SHA1)
                        {
                            list.Add(item1);
                        }
                    }

                    return;
                }

                string file = $"{BaseDir}/{item.downloads.artifact.path}";
                if (!File.Exists(file))
                {
                    list.Add(new()
                    {
                        Local = file,
                        Name = item.name,
                        SHA1 = item.downloads.artifact.sha1,
                        Url = item.downloads.artifact.url
                    });
                    return;
                }
                using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                var sha1 = Funtcions.GenSha1(stream);
                if (item.downloads.artifact.sha1 != sha1)
                {
                    list.Add(new()
                    {
                        Local = file,
                        Name = item.name,
                        SHA1 = item.downloads.artifact.sha1,
                        Url = item.downloads.artifact.url
                    });
                }
            });
        }

        return list;
    }

    /// <summary>
    /// 检查Fabric的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>丢失的库</returns>
    public static List<DownloadItemObj>? CheckFabric(GameSettingObj obj)
    {
        var fabric = VersionPath.GetFabricObj(obj.Version, obj.LoaderVersion);
        if (fabric == null)
            return null;

        var list = new List<DownloadItemObj>();

        foreach (var item in fabric.libraries)
        {
            var name = PathC.ToName(item.name);
            string file = $"{BaseDir}/{name.Item1}";
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = UrlHelper.DownloadFabric(item.url + name.Path, BaseClient.Source),
                    Name = name.Item2,
                    Local = $"{BaseDir}/{name.Item1}"
                });
                continue;
            }
        }

        return list;
    }

    /// <summary>
    /// 检查Quilt的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>丢失的库</returns>
    public static List<DownloadItemObj>? CheckQuilt(GameSettingObj obj)
    {
        var quilt = VersionPath.GetQuiltObj(obj.Version, obj.LoaderVersion);
        if (quilt == null)
            return null;

        var list = new List<DownloadItemObj>();

        foreach (var item in quilt.libraries)
        {
            var name = PathC.ToName(item.name);
            string file = $"{BaseDir}/{name.Item1}";
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = UrlHelper.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                    Name = name.Item2,
                    Local = $"{BaseDir}/{name.Item1}"
                });
                continue;
            }
        }

        return list;
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
}
