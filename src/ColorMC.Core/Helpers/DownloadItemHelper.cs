using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Config;
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

namespace ColorMC.Core.Helpers;

/// <summary>
/// 游戏下载项目
/// </summary>
public static class DownloadItemHelper
{
    /// <summary>
    /// 地图编辑器下载项目
    /// </summary>
    /// <returns></returns>
    public static DownloadItemObj BuildMcaselectorItem()
    {
        return new()
        {
            Name = "mcaselector-2.2.2",
            Local = Path.GetFullPath($"{ToolPath.BaseDir}/mcaselector-2.2.2.jar"),
            Url = "https://github.com/Querz/mcaselector/releases/download/2.2.2/mcaselector-2.2.2.jar"
        };
    }
    /// <summary>
    /// 安全Log4j文件
    /// </summary>
    /// <param name="obj">游戏数据</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildLog4jItem(GameArgObj obj)
    {
        return new DownloadItemObj
        {
            Name = "log4j2-xml",
            Url = obj.logging.client.file.url,
            Local = Path.GetFullPath($"{VersionPath.BaseDir}/log4j2-xml"),
            SHA1 = obj.logging.client.file.sha1
        };
    }

    /// <summary>
    /// 游戏资源文件
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="hash">SHA1值</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildAssetsItem(string name, string hash)
    {
        return new DownloadItemObj
        {
            Name = name,
            Url = UrlHelper.DownloadAssets(hash, BaseClient.Source),
            Local = Path.GetFullPath($"{AssetsPath.ObjectsDir}/{hash[..2]}/{hash}"),
            SHA1 = hash
        };
    }

    /// <summary>
    /// 游戏本体
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildGameItem(string mc)
    {
        var game = VersionPath.GetVersion(mc)!;
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
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildForgeItem(string mc, string version, string type)
    {
        version += UrlHelper.FixForgeUrl(mc);
        var name = $"forge-{mc}-{version}-{type}";
        var url = UrlHelper.DownloadForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.minecraftforge:forge:{mc}-{version}-{type}",
            Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/net/minecraftforge/forge/{mc}-{version}/{name}.jar"),
        };
    }

    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildNeoForgeItem(string mc, string version, string type)
    {
        var v2222 = CheckHelpers.IsGameVersion1202(mc);
        var name = v2222
            ? $"neoforge-{version}-{type}"
            : $"forge-{mc}-{version}-{type}";
        var url = UrlHelper.DownloadNeoForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.neoforged:{name}",
            Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/net/neoforged/" +
            $"{(v2222 ? $"neoforge/{version}" : $"forge/{mc}-{version}")}/{name}.jar"),
        };
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeInstaller(string mc, string version)
    {
        return BuildForgeItem(mc, version, "installer");
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeInstaller(string mc, string version)
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
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeClient(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "client");
    }

    /// <summary>
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeClient(string mc, string version)
    {
        return BuildForgeItem(mc, version, "client");
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
        var item = BuildForgeItem(mc, version, "launcher");
        var name = $"forge-{mc}-{version}-launcher";
        item.Url = UrlHelper.DownloadForgeJar(mc, version, SourceLocal.Offical) + name + ".jar";

        return item;
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

    public static ICollection<DownloadItemObj> BuildForgeLibs(List<ForgeLaunchObj.Libraries> info, string mc,
        string version, bool neo, bool v2, bool install)
    {
        var list = new Dictionary<string, DownloadItemObj>();

        bool universal = false;
        bool installer = false;
        bool launcher = false;

        //运行库
        foreach (var item1 in info)
        {
            if (string.IsNullOrWhiteSpace(item1.downloads.artifact.path))
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
                string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}");

                var temp = new DownloadItemObj
                {
                    Name = item1.name,
                    Local = local,
                    SHA1 = item1!.downloads.artifact.sha1
                };
                if (!list.TryAdd(item1.name, temp))
                {
                    list[item1.name] = temp;
                }
            }
            else
            {
                var item2 = new DownloadItemObj()
                {
                    Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source) :
                    UrlHelper.DownloadForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source),
                    Name = item1.name,
                    Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}"),
                    SHA1 = item1.downloads.artifact.sha1
                };
                if (!list.TryAdd(item1.name, item2))
                {
                    list[item1.name] = item2;
                }
            }

            if (item1.name.EndsWith("universal"))
            {
                universal = true;
            }
            else if (item1.name.EndsWith("installer"))
            {
                installer = true;
            }
            else if (item1.name.EndsWith("launcher"))
            {
                launcher = true;
            }
        }

        if (!installer && install)
        {
            list.Add("installer", neo ?
                BuildNeoForgeInstaller(mc, version) :
                BuildForgeInstaller(mc, version));
        }
        if (!universal)
        {
            list.Add("universal", neo ?
                BuildNeoForgeUniversal(mc, version) :
                BuildForgeUniversal(mc, version));
        }
        if (v2 && !CheckHelpers.IsGameVersion117(mc) && !launcher)
        {
            list.Add("launcher", neo ?
                BuildNeoForgeLauncher(mc, version) :
                BuildForgeLauncher(mc, version));
        }

        return list.Values;
    }

    /// <summary>
    /// 获取所有Forge的Lib列表
    /// </summary>
    /// <param name="info">forge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static ICollection<DownloadItemObj> BuildForgeLibs(ForgeLaunchObj info, string mc,
        string version, bool neo, bool v2, bool install)
    {
        return BuildForgeLibs(info.libraries, mc, version, neo, v2, install);
    }

    /// <summary>
    /// 获取所有Forge的Lib列表
    /// </summary>
    /// <param name="info">forge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static ICollection<DownloadItemObj> BuildForgeLibs(ForgeInstallObj info, string mc,
        string version, bool neo, bool v2)
    {
        return BuildForgeLibs(info.libraries, mc, version, neo, v2, true);
    }

    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">下载项目列表</param>
    public static async Task<ConcurrentBag<DownloadItemObj>> BuildGameLibsAsync(GameArgObj obj)
    {
        var list = new ConcurrentBag<DownloadItemObj>();
        var list1 = new HashSet<string>();
        var natives = new ConcurrentDictionary<string, bool>();
        Parallel.ForEach(obj.libraries, (item1) =>
        {
            bool download = CheckHelpers.CheckAllow(item1.rules);
            if (!download)
            {
                return;
            }

            //全系统
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

                //更换运行库
                if (SystemInfo.Os == OsType.Android)
                {
                    item1 = GameHelper.ReplaceLib(item1);
                }

                if (item1.name.Contains("natives"))
                {
                    var index = item1.name.LastIndexOf(':');
                    string key = item1.name[..index];
                    if (item1.name.EndsWith("arm64"))
                    {
                        natives.TryRemove(key, out _);
                        //if (!SystemInfo.IsArm)
                        //{
                        //    return;
                        //}
                    }
                    else if (item1.name.EndsWith("x86"))
                    {
                        if (SystemInfo.IsArm && !natives.ContainsKey(key))
                        {
                            natives.TryAdd(key, true);
                        }
                        //if (SystemInfo.Is64Bit)
                        //{
                        //    return;
                        //}
                    }
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

            //分系统
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
                    if (SystemInfo.SystemArch == ArchEnum.x86)
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

                    natives.TryAdd(item1.name, true);

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

        //Arm处理
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
                string system = SystemInfo.Os switch
                {
                    OsType.Linux => "linux",
                    OsType.MacOS => "macos",
                    _ => "windows"
                };
                var name = item + $":{path[1]}-{path[2]}-natives-{system}-arm64";
                var dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-{system}-arm64.jar";

                var item3 = await LocalMaven.MakeItemAsync(name, dir);
                if (item3 != null)
                {
                    list.Add(item3);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 获取游戏下载项目
    /// </summary>
    /// <param name="obj">版本数据</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        DownloadAsync(VersionObj.Versions obj)
    {
        var list = new List<DownloadItemObj>();

        var obj1 = await VersionPath.AddGameAsync(obj);
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

        list.AddRange(await BuildGameLibsAsync(obj1));

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
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildForge(GameSettingObj obj, bool neo)
    {
        return BuildForgeAsync(obj.Version, obj.LoaderVersion!, neo);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildForgeAsync(string mc, string version, bool neo)
    {
        var version1 = VersionPath.GetVersion(mc)!;
        var v2 = CheckHelpers.IsGameVersionV2(version1);

        var down = neo ?
            BuildNeoForgeInstaller(mc, version) :
            BuildForgeInstaller(mc, version);
        try
        {
            var res = await DownloadManager.StartAsync([down]);
            if (!res)
            {
                return (GetDownloadState.Init, null);
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Http.Forge.Error4"), e, false);
            return (GetDownloadState.Init, null);
        }

        using var zFile = new ZipFile(down.Local);
        using var stream1 = new MemoryStream();
        using var stream2 = new MemoryStream();
        var find1 = false;
        var find2 = false;
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

            list.AddRange(BuildForgeLibs(info, mc, version, neo, v2, true));

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

            list.AddRange(BuildForgeLibs(info1, mc, version, neo, v2));
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
                    libraries = []
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
                        var path = PathHelper.ToPathAndName(item.name);
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

                list.AddRange(BuildForgeLibs(info, mc, version, neo, v2, true));
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
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildFabric(GameSettingObj obj)
    {
        return BuildFabricAsync(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildFabricAsync(string mc, string version)
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
            var name = PathHelper.ToPathAndName(item.name);
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
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildQuilt(GameSettingObj obj)
    {
        return BuildQuiltAsync(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildQuiltAsync(string mc, string? version = null)
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
            var name = PathHelper.ToPathAndName(item.name);
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
    /// 获取Optifine下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildOptifine(GameSettingObj obj)
    {
        return BuildOptifineAsync(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 创建optifine下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">optifine版本</param>
    /// <returns></returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)>
        BuildOptifineAsync(string mc, string version)
    {
        var list = await OptifineAPI.GetOptifineVersion();
        if (list.Item1 == null)
        {
            return (GetDownloadState.Init, null);
        }

        foreach (var item in list.Item2!)
        {
            if (item.Version == version && item.MCVersion == mc)
            {
                var url = await OptifineAPI.GetOptifineDownloadUrl(item);
                if (url == null)
                {
                    return (GetDownloadState.GetInfo, null);
                }
                return (GetDownloadState.End,
                [
                    new()
                    {
                        Name = item.FileName,
                        Local = LibrariesPath.GetOptiFineLib(mc, version),
                        Overwrite = true,
                        Url = url
                    }
                ]);
            }
        }

        return (GetDownloadState.Init, null);
    }

    public static Task<(ConcurrentBag<DownloadItemObj>?, string? name)>
       DecodeLoaderJarAsync(GameSettingObj obj)
    {
        return DecodeLoaderJarAsync(obj, obj.GetGameLoaderFile(), CancellationToken.None);
    }

    public static Task<(ConcurrentBag<DownloadItemObj>?, string? name)>
        DecodeLoaderJarAsync(GameSettingObj obj, string path)
    {
        return DecodeLoaderJarAsync(obj, path, CancellationToken.None);
    }

    public static async Task<(ConcurrentBag<DownloadItemObj>?, string? name)>
        DecodeLoaderJarAsync(GameSettingObj obj, string path, CancellationToken cancel)
    {
        using var zFile = new ZipFile(path);
        using var stream1 = new MemoryStream();
        using var stream2 = new MemoryStream();
        var find1 = false;
        var find2 = false;
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "version.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1, cancel);
                find1 = true;
            }
            else if (e.IsFile && e.Name == "install_profile.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream2, cancel);
                find2 = true;
            }
        }

        if (!find1 || !find2)
        {
            return (null, null);
        }

        var list = new ConcurrentBag<DownloadItemObj>();

        async Task Unpack(ForgeInstallObj obj1)
        {
            foreach (var item in obj1.libraries)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }
                if (!string.IsNullOrWhiteSpace(item.downloads.artifact.url))
                {
                    string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}");
                    {
                        using var read = PathHelper.OpenRead(local);
                        if (read != null)
                        {
                            string sha1 = HashHelper.GenSha1(read);
                            if (sha1 == item.downloads.artifact.sha1)
                            {
                                continue;
                            }
                        }
                    }

                    list.Add(new()
                    {
                        Name = item.name,
                        Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}"),
                        SHA1 = item.downloads.artifact.sha1,
                        Url = item.downloads.artifact.url
                    });
                }
                else
                {
                    var item1 = zFile.GetEntry($"maven/{item.downloads.artifact.path}");
                    if (item1 != null)
                    {
                        string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}");
                        {
                            using var read = PathHelper.OpenRead(local);
                            if (read != null)
                            {
                                string sha1 = HashHelper.GenSha1(read);
                                if (sha1 == item.downloads.artifact.sha1)
                                {
                                    continue;
                                }
                            }
                        }

                        {
                            using var write = PathHelper.OpenWrite(local);
                            using var stream3 = zFile.GetInputStream(item1);
                            await stream3.CopyToAsync(write, cancel);
                        }
                    }
                }
            }
        }

        string name = "";

        try
        {
            byte[] array1 = stream2.ToArray();
            var data = Encoding.UTF8.GetString(array1);
            var obj1 = JsonConvert.DeserializeObject<ForgeInstallObj>(data)!;

            await Unpack(obj1);

            if (cancel.IsCancellationRequested)
            {
                return (null, null);
            }

            array1 = stream1.ToArray();
            data = Encoding.UTF8.GetString(array1);
            var obj2 = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;

            await Unpack(obj1);

            name = obj1.version;
            if (!obj1.version.StartsWith(obj1.profile))
            {
                name = $"{obj1.profile}-{obj1.version}";
            }

            obj.CustomLoader ??= new();

            VersionPath.AddGame(new CustomLoaderObj()
            {
                Type = CustomLoaderType.ForgeLaunch,
                Loader = obj2
            }, obj.UUID);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error3"), e);
        }

        return (list, name);
    }
}
