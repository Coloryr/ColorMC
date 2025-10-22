using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ColorMC.Core.Config;
using ColorMC.Core.LaunchPath;
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
            var obj1 = VersionPath.GetVersion(version)
                ?? throw new FileNotFoundException(string.Format(LanguageHelper.Get("Core.Check.Error1"), version));
            return obj1.JavaVersion?.MajorVersion > 8;
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
        if (string.IsNullOrWhiteSpace(obj.Url))
        {
            return false;
        }
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
            if (ConfigUtils.Config.GameCheck?.CheckAssetsSha1 is not { } check || !obj1.CheckToAdd(check))
            {
                return;
            }

            list.Add(obj1);
            list1.Add(item.Value.Hash);
        });

        return list;
    }

    /// <summary>
    /// 更新游戏arg文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<GameArgObj> CheckGameArgFileAsync(this GameSettingObj obj)
    {
        var game = await VersionPath.CheckUpdateAsync(obj.Version);
        //不存在游戏
        if (game != null)
        {
            return game;
        }

        var var = await VersionPath.GetVersionsAsync();
        var version = var?.Versions.FirstOrDefault(a => a.Id == obj.Version)
                      ?? throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));
        _ = await VersionPath.AddGameAsync(version)
            ?? throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));
        game = VersionPath.GetVersion(obj.Version)
               ?? throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error1"));

        return game;
    }

    /// <summary>
    /// 检查游戏文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cancel">取消Token</param>
    /// <returns>下载列表</returns>
    public static async Task<ConcurrentBag<FileItemObj>> CheckGameFileAsync(this GameSettingObj obj, GameLaunchObj arg, CancellationToken cancel)
    {
        var list = new ConcurrentBag<FileItemObj>();

        var list1 = new List<Task>();

        //检查游戏核心文件
        if (ConfigUtils.Config.GameCheck?.CheckCore == true)
        {
            list1.Add(Task.Run(() =>
            {
                if (arg.GameJar.CheckToAdd(ConfigUtils.Config.GameCheck.CheckCoreSha1))
                {
                    list.Add(arg.GameJar);
                }

                if (arg.Log4JXml == null)
                {
                    return;
                }

                if (arg.Log4JXml.CheckToAdd(true))
                {
                    list.Add(arg.Log4JXml);
                }
            }, cancel));
        }

        //检查游戏资源文件
        if (ConfigUtils.Config.GameCheck?.CheckAssets == true)
        {
            list1.Add(Task.Run(() =>
            {
                var assets = arg.Assets.GetIndex();
                if (assets == null)
                {
                    return;
                }

                foreach (var item in assets.CheckAssets(cancel))
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
        if (ConfigUtils.Config.GameCheck?.CheckLib == true)
        {
            //检查游戏启动json
            list1.Add(Task.Run(() =>
            {
                Parallel.ForEach(arg.GameLibs, item =>
                {
                    if (CheckToAdd(item, ConfigUtils.Config.GameCheck.CheckLibSha1))
                    {
                        list.Add(item);
                    }
                });
                Parallel.ForEach(arg.LoaderLibs, item =>
                {
                    if (CheckToAdd(item, ConfigUtils.Config.GameCheck.CheckLibSha1))
                    {
                        list.Add(item);
                    }
                });
                Parallel.ForEach(arg.InstallerLibs, item =>
                {
                    if (CheckToAdd(item, ConfigUtils.Config.GameCheck.CheckLibSha1))
                    {
                        list.Add(item);
                    }
                });
            }, cancel));
        }

        //检查整合包mod
        if (obj.ModPack && ConfigUtils.Config.GameCheck?.CheckMod == true)
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
    public static async Task<FileItemObj?> CheckLoginCoreAsync(this LoginObj login, CancellationToken token)
    {
        var item1 = login.AuthType switch
        {
            AuthType.Nide8 => await AuthlibHelper.ReadyNide8Async(token),
            AuthType.AuthlibInjector => await AuthlibHelper.ReadyAuthlibInjectorAsync(token),
            AuthType.LittleSkin => await AuthlibHelper.ReadyAuthlibInjectorAsync(token),
            AuthType.SelfLittleSkin => await AuthlibHelper.ReadyAuthlibInjectorAsync(token),
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
}

