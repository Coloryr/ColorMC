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
    /// 检测下载源
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="fid"></param>
    /// <returns></returns>
    public static SourceType TestSourceType(string? pid, string? fid)
    {
        return FuntionUtils.CheckNotNumber(pid) || FuntionUtils.CheckNotNumber(fid)
            ? SourceType.Modrinth : SourceType.CurseForge;
    }

    /// <summary>
    /// 存档编辑器下载项目
    /// </summary>
    /// <returns></returns>
    public static DownloadItemObj BuildMcaselectorItem()
    {
        return new()
        {
            Name = "mcaselector-2.4.1",
            Local = Path.GetFullPath($"{ToolPath.BaseDir}/mcaselector-2.4.1.jar"),
            Url = "https://github.com/Querz/mcaselector/releases/download/2.4.1/mcaselector-2.4.1.jar"
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
            Url = obj.Logging.Client.File.Url,
            Local = Path.GetFullPath($"{VersionPath.BaseDir}/log4j2-xml"),
            Sha1 = obj.Logging.Client.File.Sha1
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
            Url = UrlHelper.DownloadAssets(hash, CoreHttpClient.Source),
            Local = Path.GetFullPath($"{AssetsPath.ObjectsDir}/{hash[..2]}/{hash}"),
            Sha1 = hash
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
            Url = CoreHttpClient.Source == SourceLocal.Offical ? game.Downloads.client.url
                : UrlHelper.DownloadGame(mc, CoreHttpClient.Source),
            Sha1 = game.Downloads.client.sha1,
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
        var url = UrlHelper.DownloadForgeJar(mc, version, CoreHttpClient.Source);

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
        var url = UrlHelper.DownloadNeoForgeJar(mc, version, CoreHttpClient.Source);

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

    /// <summary>
    /// 构建Forge运行库下载项目列表
    /// </summary>
    /// <param name="info">运行库列表</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">Forge版本</param>
    /// <param name="neo">是否为Neoforge</param>
    /// <param name="v2">是否为V2版本</param>
    /// <param name="install">是否包含安装器</param>
    /// <returns>下载项目列表</returns>
    public static ICollection<DownloadItemObj> BuildForgeLibs(List<ForgeLaunchObj.LibrariesObj> info, string mc,
        string version, bool neo, bool v2, bool install)
    {
        var list = new Dictionary<string, DownloadItemObj>();

        bool universal = false;
        bool installer = false;
        bool launcher = false;

        //运行库
        foreach (var item1 in info)
        {
            if (string.IsNullOrWhiteSpace(item1.Downloads.Artifact.Path))
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(item1.Downloads.Artifact.Url))
            {
                string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item1.Downloads.Artifact.Path}");

                var temp = new DownloadItemObj
                {
                    Name = item1.Name,
                    Local = local,
                    Sha1 = item1!.Downloads.Artifact.Sha1
                };
                if (!list.TryAdd(item1.Name, temp))
                {
                    list[item1.Name] = temp;
                }
            }
            else
            {
                var item2 = new DownloadItemObj()
                {
                    Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item1.Downloads.Artifact.Url,
                        CoreHttpClient.Source) :
                    UrlHelper.DownloadForgeLib(item1.Downloads.Artifact.Url,
                        CoreHttpClient.Source),
                    Name = item1.Name,
                    Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item1.Downloads.Artifact.Path}"),
                    Sha1 = item1.Downloads.Artifact.Sha1
                };
                if (!list.TryAdd(item1.Name, item2))
                {
                    list[item1.Name] = item2;
                }
            }

            if (item1.Name.EndsWith("universal"))
            {
                universal = true;
            }
            else if (item1.Name.EndsWith("installer"))
            {
                installer = true;
            }
            else if (item1.Name.EndsWith("launcher"))
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
        return BuildForgeLibs(info.Libraries, mc, version, neo, v2, install);
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
        return BuildForgeLibs(info.Libraries, mc, version, neo, v2, true);
    }

    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">下载项目列表</param>
    public static async Task<ConcurrentBag<DownloadItemObj>> BuildGameLibsAsync(GameArgObj obj)
    {
        var list = new ConcurrentBag<DownloadItemObj>();
        var list1 = new HashSet<string>();
        //待补全的native
        var natives = new ConcurrentDictionary<string, bool>();
        var nativesarm = new ConcurrentBag<string>();
        Parallel.ForEach(obj.Libraries, (item1) =>
        {
            bool download = CheckHelpers.CheckAllow(item1.Rules);
            if (!download)
            {
                return;
            }

            //全系统
            if (item1.Downloads.Artifact != null)
            {
                lock (list1)
                {
                    if (list1.Contains(item1.Downloads.Artifact.Sha1)
                        && item1.Downloads.Classifiers == null)
                    {
                        return;
                    }
                }

                //更换运行库
                if (SystemInfo.Os == OsType.Android)
                {
                    item1 = GameHelper.ReplaceLib(item1);
                }

                if (item1.Name.Contains("natives"))
                {
                    var index = item1.Name.LastIndexOf(':');
                    string key = item1.Name[..index];
                    if (item1.Name.EndsWith("arm64") || item1.Name.EndsWith("aarch_64"))
                    {
                        nativesarm.Add(key);
                        natives.TryRemove(key, out _);
                    }
                    //else if (item1.name.EndsWith("x86") || item1.name.EndsWith("x86_64"))
                    //{
                    //    if (SystemInfo.IsArm && !natives.ContainsKey(key))
                    //    {
                    //        natives.TryAdd(key, true);
                    //    }
                    //}
                    else
                    {
                        natives.TryAdd(key, true);
                    }
                }

                string file = $"{LibrariesPath.BaseDir}/{item1.Downloads.Artifact.Path}";

                var obj = new DownloadItemObj()
                {
                    Name = item1.Name,
                    Url = UrlHelper.DownloadLibraries(item1.Downloads.Artifact.Url, CoreHttpClient.Source),
                    Local = file,
                    Sha1 = item1.Downloads.Artifact.Sha1
                };
                list.Add(obj);

                lock (list1)
                {
                    list1.Add(item1.Downloads.Artifact.Sha1);
                }
            }

            //分系统
            if (item1.Downloads.Classifiers != null)
            {
                var lib = SystemInfo.Os switch
                {
                    OsType.Windows => item1.Downloads.Classifiers.NativesWindows,
                    OsType.Linux => item1.Downloads.Classifiers.NativesLinux,
                    OsType.MacOS => item1.Downloads.Classifiers.NativesOsx,
                    _ => null
                };

                if (lib == null && SystemInfo.Os == OsType.Windows)
                {
                    if (SystemInfo.SystemArch == ArchEnum.x86)
                    {
                        lib = item1.Downloads.Classifiers.NativesWindows32;
                    }
                    else
                    {
                        lib = item1.Downloads.Classifiers.NativesWindows64;
                    }
                }

                if (lib != null)
                {
                    if (list1.Contains(lib.Sha1))
                        return;

                    natives.TryAdd(item1.Name, true);

                    var obj1 = new DownloadItemObj()
                    {
                        Name = item1.Name + "-native" + SystemInfo.Os,
                        Url = UrlHelper.DownloadLibraries(lib.Url, CoreHttpClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{lib.Path}",
                        Sha1 = lib.Sha1,
                        Later = (test) => GameHelper.UnpackNative(obj.Id, test)
                    };

                    list.Add(obj1);
                    lock (list1)
                    {
                        list1.Add(lib.Sha1);
                    }
                }
            }
        });

        //Arm处理
        if (SystemInfo.IsArm)
        {
            foreach (var item in nativesarm)
            {
                natives.TryRemove(item, out _);
            }
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

                name = item + $":{path[1]}-{path[2]}-natives-{system}-aarch_64";
                dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-{system}-aarch_64.jar";

                item3 = await LocalMaven.MakeItemAsync(name, dir);
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
    public static async Task<List<DownloadItemObj>?> BuildVersionDownloadAsync(VersionObj.VersionsObj obj)
    {
        var list = new List<DownloadItemObj>();

        var obj1 = await VersionPath.AddGameAsync(obj);
        if (obj1 == null)
        {
            return null;
        }
        var obj2 = await GameAPI.GetAssets(obj1.AssetIndex.url);
        if (obj2 == null)
        {
            return null;
        }

        obj1.AddIndex(obj2.Text);
        list.Add(BuildGameItem(obj.Id));

        list.AddRange(await BuildGameLibsAsync(obj1));

        foreach (var item1 in obj2.Assets.Objects)
        {
            var obj3 = BuildAssetsItem(item1.Key, item1.Value.Hash);
            if (obj3.CheckToAdd(ConfigUtils.Config.GameCheck.CheckAssetsSha1))
            {
                list.Add(obj3);
            }
        }

        return list;
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<List<DownloadItemObj>?> BuildForge(GameSettingObj obj)
    {
        return BuildForgeAsync(obj.Version, obj.LoaderVersion!, obj.Loader == Loaders.NeoForge);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<List<DownloadItemObj>?> BuildForgeAsync(string mc, string version, bool neo)
    {
        var version1 = VersionPath.GetVersion(mc)!;
        var v2 = version1.IsGameVersionV2();

        var down = neo ?
            BuildNeoForgeInstaller(mc, version) :
            BuildForgeInstaller(mc, version);
        try
        {
            var res = await DownloadManager.StartAsync([down]);
            if (!res)
            {
                return null;
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Http.Forge.Error4"), e, false);
            return null;
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
                return null;
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
                return null;
            }

            list.AddRange(BuildForgeLibs(info1, mc, version, neo, v2));
        }
        //旧forge
        else
        {
            ForgeInstallNewObj obj;
            byte[] array1 = stream2.ToArray();
            ForgeLaunchObj info;
            try
            {
                var data = Encoding.UTF8.GetString(array1);
                obj = JsonConvert.DeserializeObject<ForgeInstallNewObj>(data)!;
                info = new()
                {
                    MainClass = obj.VersionInfo.MainClass,
                    MinecraftArguments = obj.VersionInfo.MinecraftArguments,
                    Libraries = []
                };
                foreach (var item in obj.VersionInfo.Libraries)
                {
                    var item1 = GameHelper.MakeLibObj(item);
                    if (item1 != null)
                    {
                        info.Libraries.Add(item1);
                    }
                    else if (!string.IsNullOrWhiteSpace(item.Url))
                    {
                        var path = PathHelper.NameToPath(item.Name);
                        info.Libraries.Add(new()
                        {
                            Name = item.Name,
                            Downloads = new()
                            {
                                Artifact = new()
                                {
                                    Url = item.Url + path,
                                    Path = path
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
                return null;
            }
        }

        return list;
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<List<DownloadItemObj>?> BuildFabric(GameSettingObj obj)
    {
        return BuildFabricAsync(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>下载项目列表</returns>
    private static async Task<List<DownloadItemObj>?> BuildFabricAsync(string mc, string version)
    {
        var list = new List<DownloadItemObj>();
        var meta = await FabricAPI.GetMeta(CoreHttpClient.Source);
        if (meta == null)
        {
            return null;
        }

        FabricMetaObj.LoaderObj? fabric;

        if (version != null)
        {
            fabric = meta.Loader.Where(a => a.Version == version).FirstOrDefault();
        }
        else
        {
            fabric = meta.Loader.Where(a => a.Stable == true).FirstOrDefault();
        }
        if (fabric == null)
        {
            return null;
        }

        version = fabric.Version;

        var data = await FabricAPI.GetLoader(mc, version, CoreHttpClient.Source);
        if (data == null)
        {
            return null;
        }
        var meta1 = JsonConvert.DeserializeObject<FabricLoaderObj>(data);
        if (meta1 == null)
        {
            return null;
        }

        VersionPath.AddGame(meta1, data, mc, version);

        foreach (var item in meta1.Libraries)
        {
            var name = PathHelper.NameToPath(item.Name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.Url + name, CoreHttpClient.Source),
                Name = item.Name,
                Local = $"{LibrariesPath.BaseDir}/{name}"
            });

        }

        return list;
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目列表</returns>
    public static Task<List<DownloadItemObj>?> BuildQuilt(GameSettingObj obj)
    {
        return BuildQuiltAsync(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>下载项目列表</returns>
    private static async Task<List<DownloadItemObj>?> BuildQuiltAsync(string mc, string? version = null)
    {
        var list = new List<DownloadItemObj>();
        var meta = await QuiltAPI.GetMeta(CoreHttpClient.Source);
        if (meta == null)
        {
            return null;
        }

        QuiltMetaObj.LoaderObj? quilt;

        if (version != null)
        {
            quilt = meta.Loader.Where(a => a.Version == version).FirstOrDefault();
        }
        else
        {
            quilt = meta.Loader.FirstOrDefault();
        }
        if (quilt == null)
        {
            return null;
        }

        version = quilt.Version;

        var data = await QuiltAPI.GetLoader(mc, version, CoreHttpClient.Source);
        if (data == null)
        {
            return null;
        }
        var meta1 = JsonConvert.DeserializeObject<QuiltLoaderObj>(data);
        if (meta1 == null)
        {
            return null;
        }

        VersionPath.AddGame(meta1, data, mc, version);

        foreach (var item in meta1.Libraries)
        {
            var name = PathHelper.NameToPath(item.Name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.Url + name, CoreHttpClient.Source),
                Name = item.Name,
                Local = $"{LibrariesPath.BaseDir}/{name}"
            });
        }

        return list;
    }

    /// <summary>
    /// 获取Optifine下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目列表</returns>
    public static Task<DownloadItemObj?> BuildOptifine(GameSettingObj obj)
    {
        return BuildOptifineAsync(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 创建optifine下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">optifine版本</param>
    /// <returns>下载项目列表</returns>
    private static async Task<DownloadItemObj?> BuildOptifineAsync(string mc, string version)
    {
        var list = await OptifineAPI.GetOptifineVersion();
        if (list == null)
        {
            return null;
        }

        foreach (var item in list)
        {
            if (item.Version == version && item.MCVersion == mc)
            {
                var url = await OptifineAPI.GetOptifineDownloadUrl(item);
                if (url == null)
                {
                    return null;
                }
                return new()
                {
                    Name = item.FileName,
                    Local = LibrariesPath.GetOptiFineLib(mc, version),
                    Overwrite = true,
                    Url = url
                };
            }
        }

        return null;
    }

    /// <summary>
    /// 获取自定义加载器运行库下载列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目列表</returns>
    public static Task<MakeDownloadNameItemsRes?> DecodeLoaderJarAsync(GameSettingObj obj)
    {
        return DecodeLoaderJarAsync(obj, obj.GetGameLoaderFile(), CancellationToken.None);
    }

    /// <summary>
    /// 获取自定义加载器运行库下载列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="path">下载路径</param>
    /// <returns>下载项目列表</returns>
    public static Task<MakeDownloadNameItemsRes?> DecodeLoaderJarAsync(GameSettingObj obj, string path)
    {
        return DecodeLoaderJarAsync(obj, path, CancellationToken.None);
    }

    /// <summary>
    /// 获取自定义加载器运行库下载列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="path">下载路径</param>
    /// <param name="cancel">取消Token</param>
    /// <returns>下载项目列表</returns>
    public static async Task<MakeDownloadNameItemsRes?> DecodeLoaderJarAsync(GameSettingObj obj, string path, CancellationToken cancel)
    {
        using var zFile = new ZipFile(PathHelper.OpenRead(path));
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
            return null;
        }

        var list = new ConcurrentBag<DownloadItemObj>();

        async Task UnpackAsync(ForgeInstallObj obj1)
        {
            foreach (var item in obj1.Libraries)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }
                if (!string.IsNullOrWhiteSpace(item.Downloads.Artifact.Url))
                {
                    string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.Downloads.Artifact.Path}");
                    {
                        using var read = PathHelper.OpenRead(local);
                        if (read != null)
                        {
                            string sha1 = HashHelper.GenSha1(read);
                            if (sha1 == item.Downloads.Artifact.Sha1)
                            {
                                continue;
                            }
                        }
                    }

                    list.Add(new()
                    {
                        Name = item.Name,
                        Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.Downloads.Artifact.Path}"),
                        Sha1 = item.Downloads.Artifact.Sha1,
                        Url = item.Downloads.Artifact.Url
                    });
                }
                else
                {
                    var item1 = zFile.GetEntry($"maven/{item.Downloads.Artifact.Path}");
                    if (item1 != null)
                    {
                        string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.Downloads.Artifact.Path}");
                        {
                            using var read = PathHelper.OpenRead(local);
                            if (read != null)
                            {
                                string sha1 = HashHelper.GenSha1(read);
                                if (sha1 == item.Downloads.Artifact.Sha1)
                                {
                                    continue;
                                }
                            }
                        }

                        {
                            using var stream3 = zFile.GetInputStream(item1);
                            await PathHelper.WriteBytesAsync(local, stream3);
                        }
                    }
                }
            }
        }

        async Task Unpack1Async(ForgeLaunchObj obj1)
        {
            foreach (var item in obj1.Libraries)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }
                if (!string.IsNullOrWhiteSpace(item.Downloads.Artifact.Url))
                {
                    string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.Downloads.Artifact.Path}");
                    {
                        using var read = PathHelper.OpenRead(local);
                        if (read != null)
                        {
                            string sha1 = HashHelper.GenSha1(read);
                            if (sha1 == item.Downloads.Artifact.Sha1)
                            {
                                continue;
                            }
                        }
                    }

                    list.Add(new()
                    {
                        Name = item.Name,
                        Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.Downloads.Artifact.Path}"),
                        Sha1 = item.Downloads.Artifact.Sha1,
                        Url = item.Downloads.Artifact.Url
                    });
                }
                else
                {
                    var item1 = zFile.GetEntry($"maven/{item.Downloads.Artifact.Path}");
                    if (item1 != null)
                    {
                        string local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.Downloads.Artifact.Path}");
                        {
                            using var read = PathHelper.OpenRead(local);
                            if (read != null)
                            {
                                string sha1 = HashHelper.GenSha1(read);
                                if (sha1 == item.Downloads.Artifact.Sha1)
                                {
                                    continue;
                                }
                            }
                        }

                        {
                            using var stream3 = zFile.GetInputStream(item1);
                            await PathHelper.WriteBytesAsync(local, stream3);
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

            await UnpackAsync(obj1);

            if (cancel.IsCancellationRequested)
            {
                return null;
            }

            array1 = stream1.ToArray();
            data = Encoding.UTF8.GetString(array1);
            var obj2 = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;

            await Unpack1Async(obj2);

            name = obj1.Version;
            if (!obj1.Version.StartsWith(obj1.Profile))
            {
                name = $"{obj1.Profile}-{obj1.Version}";
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

        return new MakeDownloadNameItemsRes
        {
            Name = name,
            List = list,
        };
    }
}
