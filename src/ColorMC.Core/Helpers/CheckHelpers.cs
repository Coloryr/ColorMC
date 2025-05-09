using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ColorMC.Core.Config;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文件检查
/// </summary>
public static partial class CheckHelpers
{
    [GeneratedRegex("[^0-9]+")]
    private static partial Regex Regex1();

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex Regex2();

    /// <summary>
    /// 检查是否为数字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CheckNotNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;
        return Regex1().IsMatch(input);
    }

    /// <summary>
    /// 检查是否为英文数字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CheckIs(string input)
    {
        return Regex2().IsMatch(input);
    }

    /// <summary>
    /// 检查是否允许
    /// </summary>
    /// <param name="list">规则列表</param>
    /// <returns>是否允许</returns>
    public static bool CheckAllow(List<GameArgObj.GameLibrariesObj.GameRulesObj> list)
    {
        bool allow = true;
        if (list == null)
        {
            return true;
        }

        foreach (var item2 in list)
        {
            var action = item2.Action;
            if (action == "allow")
            {
                if (item2.Os == null)
                {
                    allow = true;
                    continue;
                }
                var os = item2.Os.Name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    allow = true;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    allow = true;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    allow = true;
                }
                else
                {
                    allow = false;
                }

                var arch = item2.Os.Arch;
                if (arch == "x86" && !SystemInfo.IsArm)
                {
                    allow = true;
                }
            }
            else if (action == "disallow")
            {
                if (item2.Os == null)
                {
                    allow = false;
                    continue;
                }
                var os = item2.Os.Name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    allow = false;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    allow = false;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    allow = false;
                }
                else
                {
                    allow = true;
                }

                var arch = item2.Os.Arch;
                if (arch == "x86" && !SystemInfo.IsArm)
                {
                    allow = false;
                }
            }
        }

        return allow;
    }

    /// <summary>
    /// 是否为V2版本
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>是否为V2版本</returns>
    /// <exception cref="FileNotFoundException">没有找到实例信息</exception>
    public static bool IsGameVersionV2(this GameSettingObj obj)
    {
        var version = VersionPath.GetVersion(obj.Version);
        return version == null
            ? throw new FileNotFoundException(string.Format(LanguageHelper.Get("Core.Check.Error1"), obj.Version))
            : version.IsGameVersionV2();
    }

    /// <summary>
    /// 是否为V2版本
    /// </summary>
    /// <param name="version">游戏数据</param>
    /// <returns>是否为V2版本</returns>
    public static bool IsGameVersionV2(this GameArgObj version)
    {
        return version.MinimumLauncherVersion > 18;
    }

    /// <summary>
    /// 是否为1.17以上版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>是否为1.17以上版本</returns>
    public static bool IsGameVersion117(string version)
    {
        try
        {
            var version1 = new Version(version);
            return version1.Minor >= 17;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 是否是1.20以上版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>是否是1.20以上版本</returns>
    public static bool IsGameVersion120(string version)
    {
        try
        {
            var version1 = new Version(version);
            return version1.Minor >= 20;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 是否是1.20.2以上版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>是否是1.20.2以上版本</returns>
    public static bool IsGameVersion1202(string version)
    {
        try
        {
            var version1 = new Version(version);
            if (version1.Minor > 20)
            {
                return true;
            }
            return version1.Minor >= 20 && version1.Build >= 2;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 是否添加任务
    /// </summary>
    /// <param name="obj">下载项目</param>
    /// <param name="sha1">是否比较SHA1值</param>
    /// <returns>是否需要添加</returns>
    public static bool CheckToAdd(this FileItemObj obj, bool sha1)
    {
        if (!File.Exists(obj.Local))
        {
            return true;
        }

        if (sha1 && !string.IsNullOrEmpty(obj.Sha1))
        {
            using var data = PathHelper.OpenRead(obj.Local)!;
            return HashHelper.GenSha1(data) != obj.Sha1;
        }

        return false;
    }

    /// <summary>
    /// 检查丢失的资源
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <param name="cancel">取消Token</param>
    /// <returns>下载列表</returns>
    public static ConcurrentBag<FileItemObj> CheckAssets(this AssetsObj obj, CancellationToken cancel)
    {
        var list1 = new ConcurrentBag<string>();
        var list = new ConcurrentBag<FileItemObj>();
        Parallel.ForEach(obj.Objects, (item) =>
        {
            if (cancel.IsCancellationRequested)
                return;

            if (list1.Contains(item.Value.Hash))
                return;

            var obj1 = GameDownloadHelper.BuildAssetsItem(item.Key, item.Value.Hash);
            if (obj1.CheckToAdd(ConfigUtils.Config.GameCheck.CheckAssetsSha1))
            {
                list.Add(obj1);
                list1.Add(item.Value.Hash);
            }
        });

        return list;
    }

    /// <summary>
    /// 更新游戏arg文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<GameArgObj> CheckGameArgFile(this GameSettingObj obj)
    {
        var game = await VersionPath.CheckUpdateAsync(obj.Version);
        //不存在游戏
        if (game == null)
        {
            var var = await VersionPath.GetVersionsAsync();
            var version = var?.Versions.Where(a => a.Id == obj.Version).FirstOrDefault()
                ?? throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));
            var res1 = await VersionPath.AddGameAsync(version)
                ?? throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));
            game = VersionPath.GetVersion(obj.Version)
                ?? throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));
        }

        return game;
    }

    /// <summary>
    /// 检查游戏文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cancel">取消Token</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns>下载列表</returns>
    public static async Task<ConcurrentBag<FileItemObj>> CheckGameFileAsync(this GameSettingObj obj, GameLaunchObj arg, CancellationToken cancel)
    {
        var list = new ConcurrentBag<FileItemObj>();

        var list1 = new List<Task>();

        //检查游戏核心文件
        if (ConfigUtils.Config.GameCheck.CheckCore)
        {
            list1.Add(Task.Run(() =>
            {
                if (arg.GameJar.CheckToAdd(ConfigUtils.Config.GameCheck.CheckCoreSha1))
                {
                    list.Add(arg.GameJar);
                }

                if (arg.Log4JXml != null)
                {
                    if (arg.Log4JXml.CheckToAdd(true))
                    {
                        list.Add(arg.Log4JXml);
                    }
                }
            }, cancel));
        }

        //检查游戏资源文件
        if (ConfigUtils.Config.GameCheck.CheckAssets)
        {
            list1.Add(Task.Run(() =>
            {
                var assets = arg.Assets.GetIndex();
                if (assets != null)
                {
                    var list1 = assets.CheckAssets(cancel);
                    foreach (var item in list1)
                    {
                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        list.Add(item);
                    }
                }
            }, cancel));
        }

        //检查运行库
        if (ConfigUtils.Config.GameCheck.CheckLib)
        {
            //检查游戏启动json
            list1.Add(Task.Run(() =>
            {
                Parallel.ForEach(arg.GameLibs, (item) =>
                {
                    if (CheckToAdd(item, ConfigUtils.Config.GameCheck.CheckLibSha1))
                    {
                        list.Add(item);
                    }
                });
                Parallel.ForEach(arg.LoaderLibs, (item) =>
                {
                    if (CheckToAdd(item, ConfigUtils.Config.GameCheck.CheckLibSha1))
                    {
                        list.Add(item);
                    }
                });
                Parallel.ForEach(arg.InstallerLibs, (item) =>
                {
                    if (CheckToAdd(item, ConfigUtils.Config.GameCheck.CheckLibSha1))
                    {
                        list.Add(item);
                    }
                });
            }, cancel));
        }

        //检查整合包mod
        if (obj.ModPack && ConfigUtils.Config.GameCheck.CheckMod)
        {
            list1.Add(Task.Run(() =>
            {
                var mods = PathHelper.GetAllFiles(obj.GetModsPath());
                var shas = new Dictionary<string, string>();
                int find = 0;
                var array = new LinkedList<ModInfoObj>(obj.Mods.Values);
                var node = array.First;
                while (node != null)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }

                    var item = node.Value;
                    if (item.Path != Names.NameGameModDir)
                    {
                        node = node.Next;
                        array.Remove(item);
                        continue;
                    }
                    bool isfind = false;
                    foreach (var item1 in mods.ToArray())
                    {
                        var file1 = Path.GetFileNameWithoutExtension(item1.FullName.ToLower());
                        var file2 = Path.GetFileNameWithoutExtension(item.File.ToLower());
                        if (file1 != file2)
                        {
                            continue;
                        }

                        if (ConfigUtils.Config.GameCheck.CheckModSha1)
                        {
                            if (shas.TryGetValue(item1.FullName, out var sha1))
                            {
                                if (sha1 != item.Sha1)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                using var file = PathHelper.OpenRead(item1.FullName)!;
                                sha1 = HashHelper.GenSha1(file);
                                shas.TryAdd(item1.FullName, sha1);
                                if (sha1 != item.Sha1)
                                {
                                    continue;
                                }
                            }
                        }

                        mods.Remove(item1);
                        find++;
                        var node1 = node.Next;
                        array.Remove(node);
                        node = node1;
                        isfind = true;
                        break;
                    }
                    if (!isfind)
                    {
                        node = node?.Next;
                    }
                }

                if (array.Count == 0)
                {
                    return;
                }

                //添加缺失的mod
                foreach (var item in array.Where(item1 => item1 != null))
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    var path = Path.Combine(obj.GetGamePath(), item.Path, item.File);
                    list.Add(new()
                    {
                        Url = item.Url,
                        Name = item.Name,
                        Local = path,
                        Sha1 = item.Sha1
                    });
                }
            }, cancel));
        }

        await Task.WhenAll([.. list1]);

        return list;
    }

    /// <summary>
    /// 检查外置登录器
    /// </summary>
    /// <param name="login">保存的账户</param>
    /// <returns>下载项目</returns>
    /// <exception cref="LaunchException">启动失败</exception>
    public static async Task<FileItemObj?> CheckLoginCoreAsync(this LoginObj login)
    {
        var item1 = login.AuthType switch
        {
            AuthType.Nide8 => await AuthlibHelper.ReadyNide8Async(),
            AuthType.AuthlibInjector => await AuthlibHelper.ReadyAuthlibInjectorAsync(),
            AuthType.LittleSkin => await AuthlibHelper.ReadyAuthlibInjectorAsync(),
            AuthType.SelfLittleSkin => await AuthlibHelper.ReadyAuthlibInjectorAsync(),
            _ => new MakeDownloadItemRes { State = true }
        };
        if (!item1.State)
        {
            throw new LaunchException(LaunchState.LoginCoreError, LanguageHelper.Get("Core.Launch.Error11"));
        }
        else if (item1.Item != null)
        {
            return item1.Item;
        }

        return null;
    }

#if Phone
    /// <summary>
    /// 检查是否需要安装Forge
    /// </summary>
    /// <param name="obj">Forge安装数据</param>
    /// <param name="fgversion">Forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>是否需要安装</returns>
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
            if (IsGameVersion1202(obj.Minecraft))
            {
                file = $"{LibrariesPath.BaseDir}/net/neoforged/neoforged/{fgversion}" +
                    $"neoforge-{fgversion}-client.jar";
            }
            else
            {
                file = $"{LibrariesPath.BaseDir}/net/neoforged/forge/{fgversion}" +
                    $"{obj.Minecraft}-{fgversion}/" +
                    $"forge-{obj.Minecraft}-{fgversion}-client.jar";
            }
        }
        else
        {
            file = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" +
            $"{obj.Minecraft}-{fgversion}/" +
            $"forge-{obj.Minecraft}-{fgversion}-client.jar";
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
#endif
    ///// <summary>
    ///// 检查Forge的运行库
    ///// </summary>
    ///// <param name="obj">游戏实例</param>
    ///// <param name="cancel">取消Token</param>
    ///// <returns>下载列表</returns>
    //public static async Task<ConcurrentBag<FileItemObj>?> CheckForgeLibAsync(this GameSettingObj obj, CancellationToken cancel)
    //{
    //    var list1 = obj.GetForgeLibs();
    //    if (list1 == null)
    //    {
    //        return null;
    //    }
    //    var list = new ConcurrentBag<FileItemObj>();

    //    await Parallel.ForEachAsync(list1, cancel, async (item, cancel) =>
    //    {
    //        if (cancel.IsCancellationRequested)
    //        {
    //            return;
    //        }

    //        if (!File.Exists(item.Local))
    //        {
    //            list.Add(item);
    //            return;
    //        }
    //        if (item.Sha1 == null)
    //        {
    //            return;
    //        }

    //        if (!ConfigUtils.Config.GameCheck.CheckLibSha1)
    //        {
    //            return;
    //        }
    //        using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.ReadWrite,
    //            FileShare.ReadWrite);
    //        var sha1 = await HashHelper.GenSha1Async(stream);
    //        if (item.Sha1 != sha1)
    //        {
    //            list.Add(item);
    //        }
    //    });

    //    return list;
    //}

    /// <summary>
    /// 检查Fabric的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cancel">取消Token</param>
    /// <returns>下载列表</returns>
    public static List<FileItemObj>? CheckFabricLib(this GameSettingObj obj, CancellationToken cancel)
    {
        var fabric = VersionPath.GetFabricObj(obj.Version, obj.LoaderVersion!);
        if (fabric == null)
        {
            return null;
        }

        var list = new List<FileItemObj>();

        foreach (var item in fabric.Libraries)
        {
            if (cancel.IsCancellationRequested)
            {
                break;
            }

            var name = FuntionUtils.VersionNameToPath(item.Name);
            string file = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name}");
            if (File.Exists(file))
            {
                continue;
            }

            list.Add(new()
            {
                Url = UrlHelper.DownloadFabric(item.Url, CoreHttpClient.Source) + name,
                Name = item.Name,
                Local = file
            });
        }

        return list;
    }

    /// <summary>
    /// 检查Quilt的运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cancel">取消Token</param>
    /// <returns>下载列表</returns>
    public static List<FileItemObj>? CheckQuiltLib(this GameSettingObj obj, CancellationToken cancel)
    {
        var quilt = VersionPath.GetQuiltObj(obj.Version, obj.LoaderVersion!);
        if (quilt == null)
        {
            return null;
        }

        var list = new List<FileItemObj>();

        foreach (var item in quilt.Libraries)
        {
            if (cancel.IsCancellationRequested)
            {
                return null;
            }

            var name = FuntionUtils.VersionNameToPath(item.Name);
            string file = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name}");
            if (File.Exists(file))
            {
                continue;
            }

            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.Url, CoreHttpClient.Source) + name,
                Name = item.Name,
                Local = file
            });
        }

        return list;
    }

    ///// <summary>
    ///// 检查游戏运行库
    ///// </summary>
    ///// <param name="obj">游戏数据</param>
    ///// <param name="cancel">取消Token</param>
    ///// <returns>下载列表</returns>
    //public static async Task<List<DownloadItemObj>> CheckGameLibAsync(this GameArgObj obj, CancellationToken cancel)
    //{
    //    var list = new List<DownloadItemObj>();
    //    var list1 = await DownloadItemHelper.BuildGameLibsAsync(obj);

    //    await Parallel.ForEachAsync(list1, cancel, async (item, cancel) =>
    //    {
    //        if (cancel.IsCancellationRequested)
    //            return;

    //        if (!File.Exists(item.Local))
    //        {
    //            list.Add(item);
    //            return;
    //        }
    //        if (ConfigUtils.Config.GameCheck.CheckLibSha1)
    //        {
    //            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.Read,
    //                FileShare.Read);
    //            var sha1 = await HashHelper.GenSha1Async(stream);
    //            if (!string.IsNullOrWhiteSpace(item.Sha1) && item.Sha1 != sha1)
    //            {
    //                list.Add(item);
    //                return;
    //            }
    //        }
    //        if (item.Later != null)
    //        {
    //            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.Read,
    //                    FileShare.Read);
    //            item.Later.Invoke(stream);
    //        }

    //        return;
    //    });

    //    return list;
    //}

    /// <summary>
    /// 检测OptiFine是否存在
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>是否存在</returns>
    public static bool CheckOptifineLib(this GameSettingObj obj)
    {
        return File.Exists(LibrariesPath.GetOptifineFile(obj));
    }
}

