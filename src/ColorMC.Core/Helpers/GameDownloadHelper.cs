using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 游戏下载项目
/// </summary>
public static class GameDownloadHelper
{
    /// <summary>
    /// 检测下载源
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="fid"></param>
    /// <returns></returns>
    public static SourceType TestSourceType(string? pid, string? fid)
    {
        return CheckHelpers.CheckNotNumber(pid) || CheckHelpers.CheckNotNumber(fid)
            ? SourceType.Modrinth : SourceType.CurseForge;
    }

    /// <summary>
    /// 安全Log4j文件
    /// </summary>
    /// <param name="obj">游戏数据</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildLog4jItem(GameArgObj.LoggingObj obj)
    {
        return new FileItemObj
        {
            Name = "log4j2-xml",
            Url = obj.Client.File.Url,
            Local = Path.Combine(VersionPath.BaseDir, "log4j2", "log4j2-xml"),
            Sha1 = obj.Client.File.Sha1
        };
    }

    /// <summary>
    /// 游戏资源文件
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="hash">SHA1值</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildAssetsItem(string name, string hash)
    {
        return new FileItemObj
        {
            Name = name,
            Url = UrlHelper.DownloadAssets(hash, CoreHttpClient.Source),
            Local = Path.Combine(AssetsPath.ObjectsDir, hash[..2], hash),
            Sha1 = hash
        };
    }

    /// <summary>
    /// 游戏本体
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildGameItem(string mc)
    {
        var game = VersionPath.GetVersion(mc)!;
        var file = LibrariesPath.GetGameFile(mc);
        return new()
        {
            Url = CoreHttpClient.Source == SourceLocal.Offical ? game.Downloads.Client.Url
                : UrlHelper.DownloadGame(mc, CoreHttpClient.Source),
            Sha1 = game.Downloads.Client.Sha1,
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
    private static FileItemObj BuildForgeItem(string mc, string version, string type)
    {
        version += UrlHelper.FixForgeUrl(mc);
        var name = $"forge-{mc}-{version}-{type}";
        var url = UrlHelper.DownloadForgeJar(mc, version, CoreHttpClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.minecraftforge:forge:{mc}-{version}-{type}",
            Local = Path.Combine(LibrariesPath.BaseDir, "net", "minecraftforge", "forge", $"{mc}-{version}/{name}.jar"),
        };
    }

    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static FileItemObj BuildNeoForgeItem(string mc, string version, string type)
    {
        var v2222 = CheckHelpers.IsGameVersion1202(mc);
        var name = v2222
            ? $"neoforge-{version}-{type}"
            : $"forge-{mc}-{version}-{type}";
        var url = UrlHelper.DownloadNeoForgeJar(mc, version, CoreHttpClient.Source);

        var list = new List<string>()
        {
            LibrariesPath.BaseDir,
            "net",
            "neoforged"
        };

        if (v2222)
        {
            list.Add("neoforge");
            list.Add(version);
        }
        else
        {
            list.Add("forge");
            list.Add($"{mc}-{version}");
        }

        list.Add($"{name}.jar");

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.neoforged:{name}",
            Local = Path.Combine([.. list]),
        };
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildForgeInstaller(string mc, string version)
    {
        return BuildForgeItem(mc, version, Names.NameForgeFile1);
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildNeoForgeInstaller(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, Names.NameForgeFile1);
    }

    /// <summary>
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildForgeUniversal(string mc, string version)
    {
        return BuildForgeItem(mc, version, Names.NameForgeFile2);
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildNeoForgeClient(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, Names.NameForgeFile3);
    }

    /// <summary>
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildForgeClient(string mc, string version)
    {
        return BuildForgeItem(mc, version, Names.NameForgeFile3);
    }

    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildNeoForgeUniversal(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, Names.NameForgeFile2);
    }
    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static FileItemObj BuildForgeLauncher(string mc, string version)
    {
        var item = BuildForgeItem(mc, version, Names.NameForgeFile4);
        var name = $"forge-{mc}-{version}-{Names.NameForgeFile4}";
        item.Url = UrlHelper.DownloadForgeJar(mc, version, SourceLocal.Offical) + name + ".jar";

        return item;
    }

    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    //public static DownloadItemObj BuildNeoForgeLauncher(string mc, string version)
    //{
    //    return BuildNeoForgeItem(mc, version, "launcher");
    //}

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
    public static ICollection<FileItemObj> BuildForgeLibs(List<ForgeLaunchObj.ForgeLibrariesObj> info, string mc,
        string version, bool neo, bool v2, bool install)
    {
        var list = new Dictionary<string, FileItemObj>();

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

                var temp = new FileItemObj
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
                var item2 = new FileItemObj()
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

            if (item1.Name.EndsWith(Names.NameForgeFile2))
            {
                universal = true;
            }
            else if (item1.Name.EndsWith(Names.NameForgeFile1))
            {
                installer = true;
            }
            else if (item1.Name.EndsWith(Names.NameForgeFile4))
            {
                launcher = true;
            }
        }

        if (!installer && install)
        {
            list.Add(Names.NameForgeFile1, neo ?
                BuildNeoForgeInstaller(mc, version) :
                BuildForgeInstaller(mc, version));
        }
        if (!universal)
        {
            if (!neo || !CheckHelpers.IsGameVersion1202(mc))
            {
                list.Add(Names.NameForgeFile2, neo ?
                    BuildNeoForgeUniversal(mc, version) :
                    BuildForgeUniversal(mc, version));
            }
        }
        if (v2 && !CheckHelpers.IsGameVersion117(mc) && !launcher)
        {
            //list.Add(Names.NameForgeFile4, neo ?
            //    BuildNeoForgeLauncher(mc, version) :
            //    BuildForgeLauncher(mc, version));

            list.Add(Names.NameForgeFile4, BuildForgeLauncher(mc, version));
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
    public static ICollection<FileItemObj> BuildForgeLibs(ForgeLaunchObj info, string mc,
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
    public static ICollection<FileItemObj> BuildForgeLibs(ForgeInstallObj info, string mc,
        string version, bool neo, bool v2)
    {
        return BuildForgeLibs(info.Libraries, mc, version, neo, v2, true);
    }

    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">下载项目列表</param>
    public static async Task<ConcurrentBag<FileItemObj>> BuildGameLibsAsync(this GameArgObj obj, string native, GameSettingObj? game = null)
    {
        var list = new ConcurrentBag<FileItemObj>();
        var list1 = new HashSet<string>();
        //待补全的native
        var natives = new ConcurrentDictionary<string, bool>();
        var nativesarm = new ConcurrentBag<string>();
#if false
        Parallel.ForEach(obj.Libraries, new ParallelOptions()
        { 
            MaxDegreeOfParallelism = 1
        }, (item1) =>
#else
        Parallel.ForEach(obj.Libraries, (item1) =>
#endif
        {
            bool download = CheckHelpers.CheckAllow(item1.Rules);
            if (!download)
            {
                return;
            }

            bool isadd = false;
            //旧版
            if (item1.Url != null)
            {
                isadd = true;
                string file = FuntionUtils.VersionNameToPath(item1.Name);
                string url = item1.Url + file;
                list.Add(new()
                {
                    Name = item1.Name,
                    Url = UrlHelper.SwitchSource(url),
                    Local = $"{LibrariesPath.BaseDir}/{file}"
                });
            }
            //全系统
            if (item1.Downloads?.Artifact != null)
            {
                isadd = true;
                lock (list1)
                {
                    if (list1.Contains(item1.Downloads.Artifact.Sha1)
                        && item1.Downloads.Classifiers == null)
                    {
                        return;
                    }
                }
#if Phone
                //更换运行库
                if (SystemInfo.Os == OsType.Android)
                {
                    item1 = GameHelper.ReplaceLib(item1);
                }
#endif
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

                string file = item1.Downloads.Artifact.Path;
                if (string.IsNullOrEmpty(item1.Downloads.Artifact.Path))
                {
                    file = FuntionUtils.VersionNameToPath(item1.Name);
                }

                list.Add(new()
                {
                    Name = item1.Name,
                    Url = UrlHelper.DownloadLibraries(item1.Downloads.Artifact.Url, CoreHttpClient.Source),
                    Local = $"{LibrariesPath.BaseDir}/{file}",
                    Sha1 = item1.Downloads.Artifact.Sha1
                });

                lock (list1)
                {
                    list1.Add(item1.Downloads.Artifact.Sha1);
                }
            }

            //分系统
            if (item1.Downloads?.Classifiers != null)
            {
                isadd = true;
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

                    string file = lib.Path;
                    if (string.IsNullOrEmpty(lib.Path))
                    {
                        file = FuntionUtils.VersionNameToPath(item1.Name + "-native" + SystemInfo.Os);
                    }

                    var obj1 = new FileItemObj()
                    {
                        Name = item1.Name + "-native" + SystemInfo.Os,
                        Url = UrlHelper.DownloadLibraries(lib.Url, CoreHttpClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{file}",
                        Sha1 = lib.Sha1,
                        Later = (test) => GameHelper.UnpackNative(native, test)
                    };

                    list.Add(obj1);
                    lock (list1)
                    {
                        list1.Add(lib.Sha1);
                    }
                }
            }

            //在游戏目录中
            if (!isadd && item1.Name != null && game != null)
            {
                var item2 = GameHelper.MakeLibObj(item1.Name);
                if (item2 != null)
                {
                    list.Add(new()
                    {
                        Name = item1.Name,
                        Url = UrlHelper.DownloadLibraries(item2.Downloads.Artifact.Url, CoreHttpClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{item2.Downloads.Artifact.Path}"
                    });
                }
                else
                {
                    var dir = game.GetGameLibPath();
                    if (Directory.Exists(dir))
                    {
                        var file = Path.Combine(dir, FuntionUtils.VersionNameToFile(item1.Name));
                        list.Add(new()
                        {
                            Name = item1.Name,
                            Local = file
                        });
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

    ///// <summary>
    ///// 获取游戏下载项目
    ///// </summary>
    ///// <param name="obj">版本数据</param>
    ///// <returns>State下载状态
    ///// List下载项目列表</returns>
    //public static async Task<List<DownloadItemObj>?> BuildVersionDownloadAsync(VersionObj.VersionsObj obj)
    //{
    //    var list = new List<DownloadItemObj>();

    //    var obj1 = await VersionPath.AddGameAsync(obj);
    //    if (obj1 == null)
    //    {
    //        return null;
    //    }
    //    var obj2 = await GameAPI.GetAssets(obj1.AssetIndex.url);
    //    if (obj2 == null)
    //    {
    //        return null;
    //    }

    //    obj1.AddIndex(obj2.Text);
    //    list.Add(BuildGameItem(obj.Id));

    //    list.AddRange(await BuildGameLibsAsync(obj1));

    //    foreach (var item1 in obj2.Assets.Objects)
    //    {
    //        var obj3 = BuildAssetsItem(item1.Key, item1.Value.Hash);
    //        if (obj3.CheckToAdd(ConfigUtils.Config.GameCheck.CheckAssetsSha1))
    //        {
    //            list.Add(obj3);
    //        }
    //    }

    //    return list;
    //}

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static Task<ForgeGetFilesRes?> GetDownloadForgeLibs(this GameSettingObj obj)
    {
        return BuildForgeAsync(obj.Version, obj.LoaderVersion!, obj.Loader == Loaders.NeoForge);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    private static async Task<ForgeGetFilesRes?> BuildForgeAsync(string mc, string version, bool neo)
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

        using var stream1 = PathHelper.OpenRead(down.Local);
        if (stream1 == null)
        {
            return null;
        }
        using var zFile = new ZipArchive(stream1);
        ZipArchiveEntry? versionfile = null;
        ZipArchiveEntry? installfile = null;
        if (zFile.GetEntry(Names.NameVersionFile) is { } item)
        {
            versionfile = item;
        }
        if (zFile.GetEntry(Names.NameForgeInstallFile) is { } item1)
        {
            installfile = item1;
        }

        var list = new List<FileItemObj>();
        //1.12.2以上
        if (versionfile != null && installfile != null)
        {
            ForgeLaunchObj? info;
            try
            {
                var stream = versionfile.Open();
                info = JsonUtils.ToObj(stream, JsonType.ForgeLaunchObj);
                if (info == null)
                {
                    return null;
                }
                stream.Dispose();
                stream = versionfile.Open();
                VersionPath.AddGame(info, stream, mc, version, neo);
                stream.Dispose();
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error1"), e);
                return null;
            }

            var list1 = BuildForgeLibs(info, mc, version, neo, v2, false);

            ForgeInstallObj? info1;
            try
            {
                var stream = installfile.Open();
                info1 = JsonUtils.ToObj(stream, JsonType.ForgeInstallObj);
                if (info1 == null)
                {
                    return null;
                }
                stream.Dispose();
                stream = installfile.Open();
                VersionPath.AddGame(info1, stream, mc, version, neo);
                stream.Dispose();
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error2"), e);
                return null;
            }

            var list2 = BuildForgeLibs(info1, mc, version, neo, v2);
            return new()
            {
                Loaders = [.. list1],
                Installs = [.. list2]
            };
        }
        //旧forge
        if (installfile == null)
        {
            return null;
        }
        try
        {
            using var stream = installfile.Open();
            var obj = JsonUtils.ToObj(stream, JsonType.ForgeInstallOldObj);
            if (obj == null)
            {
                return null;
            }
            var info = new ForgeLaunchObj()
            {
                MainClass = obj.VersionInfo.MainClass,
                MinecraftArguments = obj.VersionInfo.MinecraftArguments,
                Libraries = []
            };
            foreach (var item2 in obj.VersionInfo.Libraries)
            {
                var item3 = GameHelper.MakeLibObj(item2.Name);
                if (item3 != null)
                {
                    info.Libraries.Add(item3);
                }
                else if (!string.IsNullOrWhiteSpace(item2.Url))
                {
                    var path = FuntionUtils.VersionNameToPath(item2.Name);
                    info.Libraries.Add(new()
                    {
                        Name = item2.Name,
                        Downloads = new()
                        {
                            Artifact = new()
                            {
                                Url = item2.Url + path,
                                Path = path
                            }
                        }
                    });
                }
            }

            using var mem = new MemoryStream();
            JsonUtils.ToString(mem, info, JsonType.ForgeLaunchObj);
            mem.Seek(0, SeekOrigin.Begin);
            VersionPath.AddGame(info, mem, mc, version, neo);

            return new()
            {
                Loaders = [.. BuildForgeLibs(info, mc, version, neo, v2, true)]
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error3"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目列表</returns>
    public static Task<List<FileItemObj>?> GetDownloadFabricLibs(this GameSettingObj obj)
    {
        return BuildFabricAsync(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>下载项目列表</returns>
    private static async Task<List<FileItemObj>?> BuildFabricAsync(string mc, string version)
    {
        var list = new List<FileItemObj>();
        var meta = await FabricAPI.GetMeta(CoreHttpClient.Source);
        if (meta == null)
        {
            return null;
        }

        FabricMetaObj.FabricMetaLoaderObj? fabric;

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

        using var stream = await FabricAPI.GetLoader(mc, version, CoreHttpClient.Source);
        if (stream == null)
        {
            return null;
        }
        using var mem = new MemoryStream();
        await stream.CopyToAsync(mem);
        mem.Seek(0, SeekOrigin.Begin);
        var meta1 = JsonUtils.ToObj(mem, JsonType.FabricLoaderObj);
        if (meta1 == null)
        {
            return null;
        }

        mem.Seek(0, SeekOrigin.Begin);
        VersionPath.AddGame(meta1, mem, mc, version);

        foreach (var item in meta1.Libraries)
        {
            var name = FuntionUtils.VersionNameToPath(item.Name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.Url, CoreHttpClient.Source) + name,
                Name = item.Name,
                Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name}")
            });
        }

        return list;
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目列表</returns>
    public static Task<List<FileItemObj>?> GetDownloadQuiltLibs(this GameSettingObj obj)
    {
        return BuildQuiltAsync(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>下载项目列表</returns>
    private static async Task<List<FileItemObj>?> BuildQuiltAsync(string mc, string? version = null)
    {
        var list = new List<FileItemObj>();
        var meta = await QuiltAPI.GetMeta(CoreHttpClient.Source);
        if (meta == null)
        {
            return null;
        }

        QuiltMetaObj.QuiltMetaLoaderObj? quilt;

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

        using var stream = await QuiltAPI.GetLoader(mc, version, CoreHttpClient.Source);
        if (stream == null)
        {
            return null;
        }
        using var mem = new MemoryStream();
        await stream.CopyToAsync(mem);
        mem.Seek(0, SeekOrigin.Begin);
        var meta1 = JsonUtils.ToObj(mem, JsonType.QuiltLoaderObj);
        if (meta1 == null)
        {
            return null;
        }

        mem.Seek(0, SeekOrigin.Begin);
        VersionPath.AddGame(meta1, mem, mc, version);

        foreach (var item in meta1.Libraries)
        {
            var name = FuntionUtils.VersionNameToPath(item.Name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.Url, CoreHttpClient.Source) + name,
                Name = item.Name,
                Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name}")
            });
        }

        return list;
    }

    /// <summary>
    /// 获取Optifine下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目列表</returns>
    public static Task<List<FileItemObj>?> GetDownloadOptifineLibs(this GameSettingObj obj)
    {
        return BuildOptifineAsync(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 创建optifine下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">optifine版本</param>
    /// <returns>下载项目列表</returns>
    private static async Task<List<FileItemObj>?> BuildOptifineAsync(string mc, string version)
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
                if (item.Local == SourceLocal.Offical)
                {
                    await OptifineAPI.GetOptifineDownloadUrl(item);
                }

                VersionPath.AddOptifine(item);

                if (item.Url1 == null)
                {
                    return null;
                }
                return [new()
                {
                    Name = item.FileName,
                    Local = LibrariesPath.GetOptifineFile(mc, version),
                    Overwrite = true,
                    Url = item.Url1
                }];
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
        using var stream1 = PathHelper.OpenRead(path);
        if (stream1 == null)
        {
            return new();
        }
        using var zFile = new ZipArchive(stream1);

        ForgeLaunchObj? obj1 = null;
        ForgeInstallObj? obj2 = null;

        if (zFile.GetEntry(Names.NameVersionFile) is { } item)
        {
            using var stream = item.Open();
            obj1 = JsonUtils.ToObj(stream, JsonType.ForgeLaunchObj);
        }
        if (zFile.GetEntry(Names.NameForgeInstallFile) is { } item1)
        {
            using var stream = item1.Open();
            obj2 = JsonUtils.ToObj(stream, JsonType.ForgeInstallObj);
        }

        if (obj1 == null || obj2 == null)
        {
            return null;
        }

        var list = new ConcurrentBag<FileItemObj>();

        //解包自定义加载器内容
        async Task UnpackAsync(ForgeInstallObj obj1)
        {
            foreach (var item in obj1.Libraries)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }
                //有原创下载地址
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
                    //在压缩包里面的文件
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
                            using var stream3 = item1.Open();
                            await PathHelper.WriteBytesAsync(local, stream3);
                        }
                    }
                }
            }
        }

        //解包自定义加载器内容
        async Task Unpack1Async(ForgeLaunchObj obj1)
        {
            foreach (var item in obj1.Libraries)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }
                //有原创下载地址
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
                    //在zip里面的内容
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
                            using var stream3 = item1.Open();
                            await PathHelper.WriteBytesAsync(local, stream3);
                        }
                    }
                }
            }
        }

        string name = "";

        try
        {
            await Unpack1Async(obj1);
            if (cancel.IsCancellationRequested)
            {
                return null;
            }
            await UnpackAsync(obj2);
            if (cancel.IsCancellationRequested)
            {
                return null;
            }
            name = obj2.Version;
            if (!obj2.Version.StartsWith(obj2.Profile))
            {
                name = $"{obj2.Profile}-{obj2.Version}";
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
