using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文件检查
/// </summary>
public static class CheckHelpers
{
    /// <summary>
    /// 检查是否允许
    /// </summary>
    public static bool CheckAllow(List<GameArgObj.Libraries.Rules> list)
    {
        bool download = true;
        if (list == null)
        {
            return true;
        }

        foreach (var item2 in list)
        {
            var action = item2.action;
            if (action == "allow")
            {
                if (item2.os == null)
                {
                    download = true;
                    continue;
                }
                var os = item2.os.name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    download = true;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    download = true;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    download = true;
                }
                else
                {
                    download = false;
                }
            }
            else if (action == "disallow")
            {
                if (item2.os == null)
                {
                    download = false;
                    continue;
                }
                var os = item2.os.name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    download = false;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    download = false;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    download = false;
                }
                else
                {
                    download = true;
                }
            }
        }

        return download;
    }

    /// <summary>
    /// 是否V2版本
    /// </summary>
    public static bool IsGameVersionV2(GameArgObj version)
    {
        return version.minimumLauncherVersion > 18;
    }

    /// <summary>
    /// 是否是1.17以上版本
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool IsGameVersion117(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 17;
    }

    /// <summary>
    /// 是否是1.20以上版本
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool IsGameVersion120(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 20;
    }

    /// <summary>
    /// 是否是1.20.2以上版本
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool IsGameVersion1202(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 20 && version1.Build >= 2;
    }

    /// <summary>
    /// 是否添加任务
    /// </summary>
    /// <param name="obj">下载项目</param>
    /// <param name="sha1">比较SHA1值</param>
    /// <returns></returns>
    public static bool CheckToAdd(DownloadItemObj obj, bool sha1)
    {
        if (!File.Exists(obj.Local))
        {
            return true;
        }

        if (sha1)
        {
            using var data = PathHelper.OpenRead(obj.Local)!;
            return HashHelper.GenSha1(data) != obj.SHA1;
        }

        return false;
    }

    /// <summary>
    /// 检查丢失的资源
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <returns>丢失列表</returns>
    public static ConcurrentBag<DownloadItemObj> CheckAssets(AssetsObj obj, CancellationToken cancel)
    {
        var list1 = new ConcurrentBag<string>();
        var list = new ConcurrentBag<DownloadItemObj>();
        Parallel.ForEach(obj.objects, (item) =>
        {
            if (cancel.IsCancellationRequested)
                return;

            if (list1.Contains(item.Value.hash))
                return;

            var obj1 = DownloadItemHelper.BuildAssetsItem(item.Key, item.Value.hash);
            if (CheckToAdd(obj1, ConfigUtils.Config.GameCheck.CheckAssetsSha1))
            {
                list.Add(obj1);
                list1.Add(item.Value.hash);
            }
        });

        return list;
    }

    /// <summary>
    /// 检查游戏文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns>下载列表</returns>
    public static async Task<ConcurrentBag<DownloadItemObj>> CheckGameFileAsync(GameSettingObj obj, LoginObj login,
        ColorMCCore.GameLaunch update2, CancellationToken cancel)
    {
        var list = new ConcurrentBag<DownloadItemObj>();

        //检查游戏启动json
        update2(obj, LaunchState.Check);
        var game = await VersionPath.CheckUpdateAsync(obj.Version);
        if (game == null)
        {
            //不存在游戏
            update2(obj, LaunchState.LostVersion);
            var var = await VersionPath.GetVersionsAsync();
            var version = var?.versions.Where(a => a.id == obj.Version).FirstOrDefault();
            if (version == null)
            {
                update2(obj, LaunchState.VersionError);
                throw new LaunchException(LaunchState.VersionError,
                    LanguageHelper.Get("Core.Launch.Error1"));
            }

            var res1 = await DownloadItemHelper.DownloadAsync(version);
            if (res1.State != GetDownloadState.End)
            {
                throw new LaunchException(LaunchState.VersionError,
                    LanguageHelper.Get("Core.Launch.Error1"));
            }

            res1.List!.ForEach(list.Add);

            game = VersionPath.GetVersion(obj.Version);
        }

        if (game == null)
        {
            throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));
        }

        var list1 = new List<Task>();

        //检查游戏核心文件
        if (ConfigUtils.Config.GameCheck.CheckCore)
        {
            list1.Add(Task.Run(() =>
            {
                update2(obj, LaunchState.CheckVersion);
                var obj1 = DownloadItemHelper.BuildGameItem(game.id);
                if (CheckToAdd(obj1, ConfigUtils.Config.GameCheck.CheckCoreSha1))
                {
                    list.Add(obj1);
                }

                if (game.logging != null)
                {
                    obj1 = DownloadItemHelper.BuildLog4jItem(game);
                    if (CheckToAdd(obj1, true))
                    {
                        list.Add(obj1);
                    }
                }
            }, cancel));
        }

        //检查游戏资源文件
        if (ConfigUtils.Config.GameCheck.CheckAssets)
        {
            list1.Add(Task.Run(async () =>
            {
                update2(obj, LaunchState.CheckAssets);
                var assets = game.GetIndex();
                if (assets == null)
                {
                    //不存在json文件
                    (assets, var data) = await GameAPI.GetAssets(game.assetIndex.url);
                    if (assets == null)
                    {
                        update2(obj, LaunchState.AssetsError);
                        throw new LaunchException(LaunchState.AssetsError,
                            LanguageHelper.Get("Core.Launch.Error2"));
                    }
                    game.AddIndex(data!);
                }

                var list1 = CheckAssets(assets, cancel);
                foreach (var item in list1)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    list.Add(item);
                }
            }, cancel));
        }

        //检查运行库
        if (ConfigUtils.Config.GameCheck.CheckLib)
        {
            list1.Add(Task.Run(async () =>
            {
                if (obj.Loader != Loaders.Custom || obj.CustomLoader?.OffLib != true)
                {
                    update2(obj, LaunchState.CheckLib);
                    var list2 = await game.CheckGameLibAsync(cancel);
                    if (list2.Count != 0)
                    {
                        update2(obj, LaunchState.LostLib);
                        foreach (var item in list2)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Url))
                            {
                                list.Add(item);
                            }
                        }
                    }
                }

                //检查加载器运行库
                update2(obj, LaunchState.CheckLoader);
                if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
                {
                    bool neo = obj.Loader == Loaders.NeoForge;
                    var list3 = await obj.CheckForgeLibAsync(neo, cancel);
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    if (list3 == null)
                    {
                        update2(obj, LaunchState.LostLoader);

                        var list4 = await DownloadItemHelper.BuildForge(obj, neo);
                        if (list4.State != GetDownloadState.End)
                            throw new LaunchException(LaunchState.LostLoader,
                                LanguageHelper.Get("Core.Launch.Error3"));

                        list4.List!.ForEach(list.Add);
                    }
                    else
                    {
                        foreach (var item in list3)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Url))
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                else if (obj.Loader == Loaders.Fabric)
                {
                    var list3 = obj.CheckFabricLib(cancel);
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    if (list3 == null)
                    {
                        update2(obj, LaunchState.LostLoader);

                        var list4 = await DownloadItemHelper.BuildFabric(obj);
                        if (list4.State != GetDownloadState.End)
                            throw new LaunchException(LaunchState.LostLoader,
                            LanguageHelper.Get("Core.Launch.Error3"));

                        list4.List!.ForEach(list.Add);
                    }
                    else
                    {
                        foreach (var item in list3)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Url))
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                else if (obj.Loader == Loaders.Quilt)
                {
                    var list3 = obj.CheckQuiltLib(cancel);
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    if (list3 == null)
                    {
                        update2(obj, LaunchState.LostLoader);

                        var list4 = await DownloadItemHelper.BuildQuilt(obj);
                        if (list4.State != GetDownloadState.End)
                            throw new LaunchException(LaunchState.LostLoader,
                            LanguageHelper.Get("Core.Launch.Error3"));

                        list4.List!.ForEach(list.Add);
                    }
                    else
                    {
                        foreach (var item in list3)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Url))
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                else if (obj.Loader == Loaders.OptiFine)
                {
                    if (obj.CheckOptifineLib() == false)
                    {
                        var list4 = await DownloadItemHelper.BuildOptifine(obj);
                        if (list4.State != GetDownloadState.End)
                            throw new LaunchException(LaunchState.LostLoader,
                            LanguageHelper.Get("Core.Launch.Error3"));

                        list4.List!.ForEach(list.Add);
                    }
                }
                else if (obj.Loader == Loaders.Custom)
                {
                    if (obj.CustomLoader == null || !File.Exists(obj.GetGameLoaderFile()))
                    {
                        throw new LaunchException(LaunchState.LostLoader,
                                LanguageHelper.Get("Core.Launch.Error3"));
                    }
                    var list3 = await DownloadItemHelper.DecodeLoaderJarAsync(obj, obj.GetGameLoaderFile(), cancel);
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    if (list3.Item1 == null)
                    {
                        throw new LaunchException(LaunchState.LostLoader,
                                LanguageHelper.Get("Core.Launch.Error3"));
                    }
                    else
                    {
                        foreach (var item in list3.Item1)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Url))
                            {
                                list.Add(item);
                            }
                        }
                    }
                }

                //检查外置登录器
                update2(obj, LaunchState.CheckLoginCore);

                var item1 = login.AuthType switch
                {
                    AuthType.Nide8 => await AuthlibHelper.ReadyNide8(),
                    AuthType.AuthlibInjector => await AuthlibHelper.ReadyAuthlibInjectorAsync(),
                    AuthType.LittleSkin => await AuthlibHelper.ReadyAuthlibInjectorAsync(),
                    AuthType.SelfLittleSkin => await AuthlibHelper.ReadyAuthlibInjectorAsync(),
                    _ => (true, null)
                };
                if (!item1.Item1)
                {
                    throw new LaunchException(LaunchState.LoginCoreError,
                        LanguageHelper.Get("Core.Launch.Error11"));
                }
                else if (item1.Item2 != null)
                {
                    list.Add(item1.Item2);
                }
            }, cancel));
        }

        //检查整合包mod
        if (obj.ModPack && ConfigUtils.Config.GameCheck.CheckMod)
        {
            list1.Add(Task.Run(() =>
            {
                update2(obj, LaunchState.CheckMods);

                var mods = PathHelper.GetAllFile(obj.GetModsPath());
                FileInfo? mod = null;
                int find = 0;
                ModInfoObj?[] array = [.. obj.Mods.Values];
                for (int a = 0; a < array.Length; a++)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }

                    var item = array[a];
                    foreach (var item1 in mods)
                    {
                        if (item == null || item.Path != "mods")
                        {
                            continue;
                        }
                        if (item1.FullName.ToLower().EndsWith(item.File.ToLower()))
                        {
                            if (ConfigUtils.Config.GameCheck.CheckModSha1)
                            {
                                using var file = PathHelper.OpenRead(item1.FullName)!;
                                if (HashHelper.GenSha1(file) != item.SHA1)
                                {
                                    continue;
                                }
                            }
                            mod = item1;
                            break;
                        }
                    }
                    if (mod != null)
                    {
                        mods.Remove(mod);
                        find++;
                        mod = null;
                        array[a] = null;
                    }
                }
                //添加缺失的mod
                if (find != array.Length)
                {
                    foreach (var item in array)
                    {
                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        if (item == null)
                        {
                            continue;
                        }

                        if (item.Path != "mods")
                        {
                            var path = Path.GetFullPath($"{obj.GetGamePath()}/{item.Path}/{item.File}");
                            if (File.Exists(path))
                            {
                                if (ConfigUtils.Config.GameCheck.CheckModSha1)
                                {
                                    using var file = PathHelper.OpenRead(path)!;
                                    if (HashHelper.GenSha1(file) != item.SHA1)
                                    {
                                        list.Add(new()
                                        {
                                            Url = item.Url,
                                            Name = item.Name,
                                            Local = path,
                                            SHA1 = item.SHA1
                                        });
                                    }
                                }
                            }
                            else
                            {
                                list.Add(new()
                                {
                                    Url = item.Url,
                                    Name = item.Name,
                                    Local = path,
                                    SHA1 = item.SHA1
                                });
                            }
                        }
                        else
                        {
                            list.Add(new()
                            {
                                Url = item.Url,
                                Name = item.Name,
                                Local = obj.GetModsPath() + item.File,
                                SHA1 = item.SHA1
                            });
                        }
                    }
                }
            }, cancel));
        }

        await Task.WhenAll(list1.ToArray());

        return list;
    }

    /// <summary>
    /// 检查是否需要安装Forge
    /// </summary>
    /// <param name="obj">Forge安装数据</param>
    /// <param name="fgversion">Forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>结果</returns>
    public static bool CheckForgeInstall(ForgeInstallObj obj, string fgversion, bool neo)
    {
        //silm
        //var version = obj.data.MCP_VERSION.client[1..^1];
        //string file = $"{LibrariesPath.BaseDir}/net/minecraft/client/" +
        //    $"{obj.minecraft}-{version}/" +
        //    $"client-{obj.minecraft}-{version}-slim.jar";
        //file = Path.GetFullPath(file);
        //if (!File.Exists(file))
        //{
        //    return true;
        //}
        //using var stream = PathHelper.OpenRead(file)!;
        //string sha1 = await HashHelper.GenSha1Async(stream);

        //if (sha1 != obj.data.MC_SLIM_SHA.client[1..^1])
        //{
        //    return true;
        //}

        string file;
        if (neo)
        {
            if (IsGameVersion1202(obj.minecraft))
            {
                file = $"{LibrariesPath.BaseDir}/net/neoforged/neoforged/{fgversion}" +
                    $"neoforge-{fgversion}-client.jar";
            }
            else
            {
                file = $"{LibrariesPath.BaseDir}/net/neoforged/forge/{fgversion}" +
                    $"{obj.minecraft}-{fgversion}/" +
                    $"forge-{obj.minecraft}-{fgversion}-client.jar";
            }
        }
        else
        {
            file = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" +
            $"{obj.minecraft}-{fgversion}/" +
            $"forge-{obj.minecraft}-{fgversion}-client.jar";
        }

        file = Path.GetFullPath(file);
        if (!File.Exists(file))
        {
            return true;
        }

        return false;
        //using var stream2 = PathHelper.OpenRead(file)!;
        //var sha1 = await HashHelper.GenSha1Async(stream2);

        //if (sha1 != obj.data.PATCHED_SHA.client[1..^1])
        //{
        //    return true;
        //}

        //extra
        //file = $"{LibrariesPath.BaseDir}/net/minecraft/client/" +
        //    $"{obj.minecraft}-{version}/" +
        //    $"client-{obj.minecraft}-{version}-extra.jar";
        //file = Path.GetFullPath(file);
        //if (!File.Exists(file))
        //{
        //    return true;
        //}

        //using var stream1 = PathHelper.OpenRead(file)!;
        //sha1 = await HashHelper.GenSha1Async(stream1);

        //return sha1 != obj.data.MC_EXTRA_SHA.client[1..^1];
    }

    /// <summary>
    /// 检查Forge的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>丢失的库</returns>
    public static async Task<ConcurrentBag<DownloadItemObj>?>
        CheckForgeLibAsync(this GameSettingObj obj, bool neo, CancellationToken cancel)
    {
        var version1 = VersionPath.GetVersion(obj.Version)!;
        var v2 = IsGameVersionV2(version1);
        if (v2)
        {
            GameHelper.ReadyForgeWrapper();
        }

        var forge = neo ?
            VersionPath.GetNeoForgeObj(obj.Version, obj.LoaderVersion!) :
            VersionPath.GetForgeObj(obj.Version, obj.LoaderVersion!);
        if (forge == null)
            return null;

        //forge本体
        var list1 = DownloadItemHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!, neo, v2, true).ToList();

        var forgeinstall = neo ?
            VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!) :
            VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!);
        if (forgeinstall == null && v2)
            return null;

        //forge安装器
        if (forgeinstall != null)
        {
            var list2 = DownloadItemHelper.BuildForgeLibs(forgeinstall, obj.Version,
                obj.LoaderVersion!, neo, v2);
            list1.AddRange(list2);
        }

        var list = new ConcurrentBag<DownloadItemObj>();

        await Parallel.ForEachAsync(list1, cancel, async (item, cancel) =>
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            if (!File.Exists(item.Local))
            {
                list.Add(item);
                return;
            }
            if (item.SHA1 == null)
            {
                return;
            }

            if (!ConfigUtils.Config.GameCheck.CheckLibSha1)
            {
                return;
            }
            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            var sha1 = await HashHelper.GenSha1Async(stream);
            if (item.SHA1 != sha1)
            {
                list.Add(item);
            }
        });

        return list;
    }

    /// <summary>
    /// 检查Fabric的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>丢失的库</returns>
    public static List<DownloadItemObj>? CheckFabricLib(this GameSettingObj obj, CancellationToken cancel)
    {
        var fabric = VersionPath.GetFabricObj(obj.Version, obj.LoaderVersion!);
        if (fabric == null)
            return null;

        var list = new List<DownloadItemObj>();

        foreach (var item in fabric.libraries)
        {
            if (cancel.IsCancellationRequested)
                break;

            var name = PathHelper.ToPathAndName(item.name);
            string file = $"{LibrariesPath.BaseDir}/{name.Path}";
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = UrlHelper.DownloadFabric(item.url + name.Path, BaseClient.Source),
                    Name = name.Name,
                    Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name.Path}")
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
    public static List<DownloadItemObj>? CheckQuiltLib(this GameSettingObj obj, CancellationToken cancel)
    {
        var quilt = VersionPath.GetQuiltObj(obj.Version, obj.LoaderVersion!);
        if (quilt == null)
            return null;

        var list = new List<DownloadItemObj>();

        foreach (var item in quilt.libraries)
        {
            if (cancel.IsCancellationRequested)
                return null;

            var name = PathHelper.ToPathAndName(item.name);
            string file = $"{LibrariesPath.BaseDir}/{name.Path}";
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = UrlHelper.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                    Name = name.Name,
                    Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name.Path}")
                });
                continue;
            }
        }

        return list;
    }

    /// <summary>
    /// 检查游戏运行库
    /// </summary>
    /// <param name="obj">游戏数据</param>
    /// <returns>丢失的库</returns>
    public static async Task<List<DownloadItemObj>>
        CheckGameLibAsync(this GameArgObj obj, CancellationToken cancel)
    {
        var list = new List<DownloadItemObj>();
        var list1 = await DownloadItemHelper.BuildGameLibsAsync(obj);

        await Parallel.ForEachAsync(list1, cancel, async (item, cancel) =>
        {
            if (cancel.IsCancellationRequested)
                return;

            if (!File.Exists(item.Local))
            {
                list.Add(item);
                return;
            }
            if (ConfigUtils.Config.GameCheck.CheckLibSha1)
            {
                using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
                var sha1 = await HashHelper.GenSha1Async(stream);
                if (!string.IsNullOrWhiteSpace(item.SHA1) && item.SHA1 != sha1)
                {
                    list.Add(item);
                    return;
                }
            }
            if (item.Later != null)
            {
                using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.Read,
                        FileShare.Read);
                item.Later.Invoke(stream);
            }

            return;
        });

        return list;
    }

    /// <summary>
    /// 检测OptiFine是否存在
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool CheckOptifineLib(this GameSettingObj obj)
    {
        return File.Exists(LibrariesPath.GetOptiFineLib(obj));
    }
}

