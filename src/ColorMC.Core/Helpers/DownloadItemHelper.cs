using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 游戏下载
/// </summary>
public static class DownloadItemHelper
{
    public static DownloadItemObj BuildLog4jItem(GameArgObj obj)
    {
        return new DownloadItemObj()
        {
            Name = "log4j2-xml",
            Url = obj.logging.client.file.url,
            Local = $"{VersionPath.BaseDir}/log4j2-xml",
            SHA1 = obj.logging.client.file.sha1
        };
    }

    public static DownloadItemObj BuildAssetsItem(string key, string hash)
    {
        return new DownloadItemObj()
        {
            Name = key,
            Url = UrlHelper.DownloadAssets(hash, BaseClient.Source),
            Local = $"{AssetsPath.ObjectsDir}/{hash[..2]}/{hash}",
            SHA1 = hash
        };
    }

    public static DownloadItemObj BuildGameItem(string mc)
    {
        var game = VersionPath.GetGame(mc)!;
        var file = LibrariesPath.GetGameFile(mc);
        return new()
        {
            Url = BaseClient.Source == SourceLocal.Offical ? game.downloads.client.url
                : UrlHelper.DownloadGame(mc, BaseClient.Source),
            SHA1 = game.downloads.client.sha1,
            Local = file,
            Name = $"{mc}.jar"
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildForgeItem(string mc, string version, string type)
    {
        version += UrlHelper.FixForgeUrl(mc);
        string name = $"forge-{mc}-{version}-{type}";
        string url = UrlHelper.DownloadForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.minecraftforge:forge:{mc}-{version}-{type}",
            Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/{mc}-{version}/{name}.jar",
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildNeoForgeItem(string mc, string version, string type)
    {
        string name = $"forge-{mc}-{version}-{type}";
        string url = UrlHelper.DownloadNeoForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.neoforged:forge:{mc}-{version}-{type}",
            Local = $"{LibrariesPath.BaseDir}/net/neoforged/forge/{mc}-{version}/{name}.jar",
        };
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeInster(string mc, string version)
    {
        return BuildForgeItem(mc, version, "installer");
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeInster(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "installer");
    }

    /// <summary>
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeUniversal(string mc, string version)
    {
        return BuildForgeItem(mc, version, "universal");
    }

    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeUniversal(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "universal");
    }
    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeLauncher(string mc, string version)
    {
        return BuildForgeItem(mc, version, "launcher");
    }
    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeLauncher(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "launcher");
    }

    /// <summary>
    /// 获取所有Forge的Lib列表
    /// </summary>
    /// <param name="info">forge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static List<DownloadItemObj> BuildForgeLibs(ForgeLaunchObj info, string mc, string version, bool neo)
    {
        var version1 = VersionPath.GetGame(mc)!;
        var v2 = CheckHelpers.GameLaunchVersionV2(version1);
        var list = new List<DownloadItemObj>();

        if (v2)
        {
            list.Add(neo ?
                BuildNeoForgeInster(mc, version) :
                BuildForgeInster(mc, version));
            list.Add(neo ?
                BuildNeoForgeUniversal(mc, version) :
                BuildForgeUniversal(mc, version));

            if (!CheckHelpers.IsGameLaunchVersion117(mc))
            {
                list.Add(neo ?
                    BuildNeoForgeLauncher(mc, version) :
                    BuildForgeLauncher(mc, version));
            }
        }

        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith(neo ?
                "net.neoforged.forge:" : "net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
                //1.12.2及以下
                if (!v2)
                {
                    var temp = BuildForgeUniversal(mc, version);
                    temp.SHA1 = item1.downloads.artifact.sha1;
                    list.Add(temp);
                }
            }
            else
            {
                list.Add(new()
                {
                    Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source) :
                    UrlHelper.DownloadForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source),
                    Name = item1.name,
                    Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}"),
                    SHA1 = item1.downloads.artifact.sha1
                });
            }
        }

        return list;
    }

    /// <summary>
    /// 获取所有Forge的Lib列表
    /// </summary>
    /// <param name="info">forge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static List<DownloadItemObj> BuildForgeLibs(ForgeInstallObj info, string mc, string version, bool neo)
    {
        var list = new List<DownloadItemObj>();

        foreach (var item in info.libraries)
        {
            if (item.name.StartsWith(neo ? "net.neoforged.forge:" : "net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item.downloads.artifact.url))
            {
                var item1 = neo ?
                    BuildNeoForgeUniversal(mc, version) :
                    BuildForgeUniversal(mc, version);
                item1.SHA1 = item.downloads.artifact.sha1;
                list.Add(item1);
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.downloads.artifact.url))
                continue;

            string file = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}");

            list.Add(new()
            {
                Local = file,
                Name = item.name,
                SHA1 = item.downloads.artifact.sha1,
                Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item.downloads.artifact.url,
                         BaseClient.Source) :
                    UrlHelper.DownloadForgeLib(item.downloads.artifact.url,
                         BaseClient.Source)
            });
        }

        return list;
    }

    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">下载项目列表</param>
    public static async Task<ConcurrentBag<DownloadItemObj>> BuildGameLibs(GameArgObj obj)
    {
        var list = new ConcurrentBag<DownloadItemObj>();
        var list1 = new HashSet<string>();
        var natives = new ConcurrentDictionary<string, string>();
        Parallel.ForEach(obj.libraries, (item1) =>
        {
            bool download = CheckHelpers.CheckAllow(item1.rules);
            if (!download)
                return;

            if (item1.downloads.artifact != null)
            {
                lock (list1)
                {
                    if (list1.Contains(item1.downloads.artifact.sha1)
                        && item1.downloads.classifiers == null)
                    {
                        return;
                    }
                }

                if (SystemInfo.Os == OsType.Android)
                {
                    item1 = GameHelper.ReplaceLib(item1);
                }

                if (item1.name.Contains("natives"))
                {
                    var index = item1.name.LastIndexOf(':');
                    string key = item1.name[..index];
                    natives.TryAdd(key, key);
                }

                string file = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}";

                var obj = new DownloadItemObj()
                {
                    Name = item1.name,
                    Url = UrlHelper.DownloadLibraries(item1.downloads.artifact.url, BaseClient.Source),
                    Local = file,
                    SHA1 = item1.downloads.artifact.sha1
                };
                list.Add(obj);

                lock (list1)
                {
                    list1.Add(item1.downloads.artifact.sha1);
                }
            }

            if (item1.downloads.classifiers != null)
            {
                var lib = SystemInfo.Os switch
                {
                    OsType.Windows => item1.downloads.classifiers.natives_windows,
                    OsType.Linux => item1.downloads.classifiers.natives_linux,
                    OsType.MacOS => item1.downloads.classifiers.natives_osx,
                    _ => null
                };

                if (lib == null && SystemInfo.Os == OsType.Windows)
                {
                    if (SystemInfo.SystemArch == ArchEnum.x32)
                    {
                        lib = item1.downloads.classifiers.natives_windows_32;
                    }
                    else
                    {
                        lib = item1.downloads.classifiers.natives_windows_64;
                    }
                }

                if (lib != null)
                {
                    if (list1.Contains(lib.sha1))
                        return;

                    natives.TryAdd(item1.name, item1.name);

                    var obj1 = new DownloadItemObj()
                    {
                        Name = item1.name + "-native" + SystemInfo.Os,
                        Url = UrlHelper.DownloadLibraries(lib.url, BaseClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{lib.path}",
                        SHA1 = lib.sha1,
                        Later = (test) => GameHelper.UnpackNative(obj.id, test)
                    };

                    list.Add(obj1);
                    lock (list1)
                    {
                        list1.Add(lib.sha1);
                    }
                }
            }
        });

        if (SystemInfo.IsArm)
        {
            foreach (var item in natives.Keys)
            {
                var path = item.Split(':');
                var path1 = path[0].Split('.');
                var basedir = "";
                foreach (var item1 in path1)
                {
                    basedir += $"{item1}/";
                }
                if (SystemInfo.Os == OsType.Linux)
                {
                    var name = item + $":{path[1]}-{path[2]}-natives-linux-arm64";
                    var dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-linux-arm64.jar";

                    var item3 = await LocalMaven.MakeItem(name, dir);
                    if (item3 != null)
                    {
                        list.Add(item3);
                    }
                }
                else if (SystemInfo.Os == OsType.Windows)
                {
                    var name = item + $":{path[1]}-{path[2]}-natives-windows-arm64";
                    var dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-windows-arm64.jar";
                    var item3 = await LocalMaven.MakeItem(name, dir);
                    if (item3 != null)
                    {
                        list.Add(item3);
                    }
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 下载游戏
    /// </summary>
    /// <param name="obj">版本数据</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> Download(VersionObj.Versions obj)
    {
        var list = new List<DownloadItemObj>();

        var obj1 = await VersionPath.AddGame(obj);
        if (obj1 == null)
        {
            return (GetDownloadState.Init, null);
        }
        var obj2 = await GameAPI.GetAssets(obj1.assetIndex.url);
        if (obj2.Item1 == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        obj1.AddIndex(obj2.Item2!);
        list.Add(BuildGameItem(obj.id));

        list.AddRange(await BuildGameLibs(obj1));

        foreach (var item1 in obj2.Item1.objects)
        {
            var obj3 = BuildAssetsItem(item1.Key, item1.Value.hash);
            if (CheckHelpers.CheckToAdd(obj3, ConfigUtils.Config.GameCheck.CheckAssetsSha1))
            {
                list.Add(obj3);
            }
        }

        return (GetDownloadState.End, list);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)> BuildForge(GameSettingObj obj, bool neo)
    {
        return BuildForge(obj.Version, obj.LoaderVersion!, neo);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> BuildForge(string mc, string version, bool neo)
    {
        var version1 = VersionPath.GetGame(mc)!;
        bool v2 = CheckHelpers.GameLaunchVersionV2(version1);

        var down = neo ?
            BuildNeoForgeInster(mc, version) :
            BuildForgeInster(mc, version);
        try
        {
            var res = await DownloadManager.Start(new() { down });
            if (!res)
            {
                return (GetDownloadState.Init, null);
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Forge.Error4"), e, false);
            return (GetDownloadState.Init, null);
        }

        using ZipFile zFile = new(down.Local);
        using MemoryStream stream1 = new();
        using MemoryStream stream2 = new();
        bool find1 = false;
        bool find2 = false;
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "version.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find1 = true;
            }
            else if (e.IsFile && e.Name == "install_profile.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream2);
                find2 = true;
            }
        }

        var list = new List<DownloadItemObj>();
        //1.12.2以上
        if (find1 && find2)
        {
            ForgeLaunchObj info;
            try
            {
                var array = stream1.ToArray();
                var data = Encoding.UTF8.GetString(stream1.ToArray());
                info = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;
                VersionPath.AddGame(info, array, mc, version, neo);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error1"), e);
                return (GetDownloadState.GetInfo, null);
            }

            list.AddRange(BuildForgeLibs(info, mc, version, neo));

            ForgeInstallObj info1;
            try
            {
                var array = stream2.ToArray();
                var data = Encoding.UTF8.GetString(array);
                info1 = JsonConvert.DeserializeObject<ForgeInstallObj>(data)!;
                VersionPath.AddGame(info1, array, mc, version, neo);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error2"), e);
                return (GetDownloadState.GetInfo, null);
            }

            list.AddRange(BuildForgeLibs(info1, mc, version, neo));
        }
        //旧forge
        else
        {
            ForgeInstallObj1 obj;
            byte[] array1 = stream2.ToArray();
            ForgeLaunchObj info;
            try
            {
                var data = Encoding.UTF8.GetString(array1);
                obj = JsonConvert.DeserializeObject<ForgeInstallObj1>(data)!;
                info = new()
                {
                    id = obj.versionInfo.id,
                    time = obj.versionInfo.time,
                    releaseTime = obj.versionInfo.releaseTime,
                    type = obj.versionInfo.type,
                    mainClass = obj.versionInfo.mainClass,
                    inheritsFrom = obj.versionInfo.inheritsFrom,
                    minecraftArguments = obj.versionInfo.minecraftArguments,
                    libraries = new()
                };
                foreach (var item in obj.versionInfo.libraries)
                {
                    var item1 = GameHelper.MakeLibObj(item);
                    if (item1 != null)
                    {
                        info.libraries.Add(item1);
                    }
                    else if (!string.IsNullOrWhiteSpace(item.url))
                    {
                        var path = PathHelper.ToName(item.name);
                        info.libraries.Add(new()
                        {
                            name = item.name,
                            downloads = new()
                            {
                                artifact = new()
                                {
                                    url = item.url + path.Path,
                                    path = path.Path
                                }
                            }
                        });
                    }
                }

                VersionPath.AddGame(info,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info)), mc, version, neo);

                list.AddRange(BuildForgeLibs(info, mc, version, neo));
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error3"), e);
                return (GetDownloadState.GetInfo, null);
            }
        }

        return (GetDownloadState.End, list);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)> BuildFabric(GameSettingObj obj)
    {
        return BuildFabric(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> BuildFabric(string mc, string? version = null)
    {
        var list = new List<DownloadItemObj>();
        var meta = await FabricAPI.GetMeta(BaseClient.Source);
        if (meta == null)
        {
            return (GetDownloadState.Init, null);
        }

        FabricMetaObj.Loader? fabric;

        if (version != null)
        {
            fabric = meta.loader.Where(a => a.version == version).FirstOrDefault();
        }
        else
        {
            fabric = meta.loader.Where(a => a.stable == true).FirstOrDefault();
        }
        if (fabric == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        version = fabric.version;

        var data = await FabricAPI.GetLoader(mc, version, BaseClient.Source);
        if (data == null)
        {
            return (GetDownloadState.GetInfo, null);
        }
        var meta1 = JsonConvert.DeserializeObject<FabricLoaderObj>(data);
        if (meta1 == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        VersionPath.AddGame(meta1, data, mc, version);

        foreach (var item in meta1.libraries)
        {
            var name = PathHelper.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                Name = name.Name,
                Local = $"{LibrariesPath.BaseDir}/{name.Path}"
            });

        }

        return (GetDownloadState.End, list);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)> BuildQuilt(GameSettingObj obj)
    {
        return BuildQuilt(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> BuildQuilt(string mc, string? version = null)
    {
        var list = new List<DownloadItemObj>();
        var meta = await QuiltAPI.GetMeta(BaseClient.Source);
        if (meta == null)
        {
            return (GetDownloadState.Init, null);
        }

        QuiltMetaObj.Loader? quilt;

        if (version != null)
        {
            quilt = meta.loader.Where(a => a.version == version).FirstOrDefault();
        }
        else
        {
            quilt = meta.loader.FirstOrDefault();
        }
        if (quilt == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        version = quilt.version;

        var data = await QuiltAPI.GetLoader(mc, version, BaseClient.Source);
        if (data == null)
        {
            return (GetDownloadState.GetInfo, null);
        }
        var meta1 = JsonConvert.DeserializeObject<QuiltLoaderObj>(data);
        if (meta1 == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        VersionPath.AddGame(meta1, data, mc, version);

        foreach (var item in meta1.libraries)
        {
            var name = PathHelper.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                Name = name.Name,
                Local = $"{LibrariesPath.BaseDir}/{name.Path}"
            });
        }

        return (GetDownloadState.End, list);
    }
}
