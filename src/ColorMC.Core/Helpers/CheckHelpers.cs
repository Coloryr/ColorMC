using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using System.Collections.Concurrent;

namespace ColorMC.Core.Helpers;

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
    public static bool GameLaunchVersion(GameArgObj version)
    {
        return version.minimumLauncherVersion > 18;
    }

    /// <summary>
    /// 是否是1.17以上版本
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool IsGameLaunchVersion117(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 17;
    }

    public static bool IsGameLaunchVersion120(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 20;
    }

    public static bool CheckToAdd(DownloadItemObj obj, bool sha1)
    {
        if (!File.Exists(obj.Local))
        {
            return true;
        }

        if (sha1)
        {
            using var data = File.OpenRead(obj.Local);
            return FuntionUtils.GenSha1(data) != obj.SHA1;
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
    public static async Task<ConcurrentBag<DownloadItemObj>> CheckGameFile(GameSettingObj obj, LoginObj login, CancellationToken cancel)
    {
        var list = new ConcurrentBag<DownloadItemObj>();

        //检查游戏启动json
        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.Check);
        var game = await VersionPath.CheckUpdate(obj.Version);
        if (game == null)
        {
            //不存在游戏
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostVersion);
            var var = await VersionPath.GetVersions();
            var version = var?.versions.Where(a => a.id == obj.Version).FirstOrDefault();
            if (version == null)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.VersionError);
                throw new LaunchException(LaunchState.VersionError,
                    LanguageHelper.Get("Core.Launch.Error1"));
            }

            var res1 = await DownloadItemHelper.Download(version);
            if (res1.State != GetDownloadState.End)
            {
                throw new LaunchException(LaunchState.VersionError,
                    LanguageHelper.Get("Core.Launch.Error1"));
            }

            res1.List!.ForEach(list.Add);

            game = VersionPath.GetGame(obj.Version);
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
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckVersion);
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
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckAssets);
                var assets = game.GetIndex();
                if (assets == null)
                {
                    //不存在json文件
                    (assets, var data) = await GameAPI.GetAssets(game.assetIndex.url);
                    if (assets == null)
                    {
                        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.AssetsError);
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
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckLib);
                var list2 = await game.CheckGameLib(cancel);
                if (list2.Count != 0)
                {
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLib);
                    foreach (var item in list2)
                    {
                        list.Add(item);
                    }
                }

                //检查加载器运行库
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckLoader);
                if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
                {
                    bool neo = obj.Loader == Loaders.NeoForge;
                    var list3 = await obj.CheckForgeLib(neo, cancel);
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                    if (list3 == null)
                    {
                        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLoader);

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
                            list.Add(item);
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
                        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLoader);

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
                            list.Add(item);
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
                        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLoader);

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
                            list.Add(item);
                        }
                    }
                }

                //检查外置登录器
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckLoginCore);

                if (login.AuthType == AuthType.Nide8)
                {
                    var item = await AuthlibHelper.ReadyNide8();
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                else if (login.AuthType is AuthType.AuthlibInjector
                    or AuthType.LittleSkin or AuthType.SelfLittleSkin)
                {
                    var item = await AuthlibHelper.ReadyAuthlibInjector();
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
            }, cancel));
        }

        //检查整合包mod
        if (obj.ModPack && ConfigUtils.Config.GameCheck.CheckMod)
        {
            list1.Add(Task.Run(async () =>
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckMods);

                var mods = await obj.GetMods();
                ModObj? mod = null;
                int find = 0;
                ModInfoObj?[] array = obj.Mods.Values.ToArray();
                for (int a = 0; a < array.Length; a++)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }

                    var item = array[a];
                    foreach (var item1 in mods)
                    {
                        if (item == null)
                            continue;
                        if (!ConfigUtils.Config.GameCheck.CheckModSha1)
                            continue;
                        if (item1.Sha1 == item.SHA1 ||
                            item1.Local.ToLower().EndsWith(item.File.ToLower()))
                        {
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

                        list.Add(new()
                        {
                            Url = item.Url,
                            Name = item.File,
                            Local = obj.GetModsPath() + item.File,
                            SHA1 = item.SHA1
                        });
                    }
                }
            }, cancel));
        }

        await Task.WhenAll(list1.ToArray());

        return list;
    }

    public static async Task<bool> CheckForgeInstall(ForgeInstallObj obj)
    {
        var sha = obj.data.MCP_VERSION.client[1..^1];
        string file = $"{LibrariesPath.BaseDir}/net/minecraft/client/" +
            $"{obj.minecraft}-{sha}/" +
            $"client-{obj.minecraft}-{sha}-slim.jar";
        file = Path.GetFullPath(file);
        if (!File.Exists(file))
        {
            return true;
        }
        using var stream = File.OpenRead(file);
        string sha1 = await FuntionUtils.GenSha1Async(stream);

        if (sha1 != obj.data.MC_SLIM_SHA.client)
        {
            return true;
        }

        file = $"{LibrariesPath.BaseDir}/net/minecraft/client/" +
            $"{obj.minecraft}-{sha}/" +
            $"client-{obj.minecraft}-{sha}-extra.jar";
        file = Path.GetFullPath(file);
        if (!File.Exists(file))
        {
            return true;
        }

        using var stream1 = File.OpenRead(file);
        sha1 = await FuntionUtils.GenSha1Async(stream1);

        return sha1 != obj.data.MC_EXTRA_SHA.client;
    }
}

