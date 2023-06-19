using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Core.Utils.Downloader;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏启动类
/// </summary>
public static class Launch
{
    /// <summary>
    /// 检查游戏文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <exception cref="LaunchException">抛出的错误</exception>
    /// <returns>下载列表</returns>
    private static async Task<List<DownloadItemObj>> CheckGameFile(GameSettingObj obj, LoginObj login)
    {
        var list = new List<DownloadItemObj>();

        //检查游戏启动json
        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.Check);
        var game = VersionPath.GetGame(obj.Version);
        if (game == null)
        {
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostVersion);

            var version = VersionPath.Versions?.versions.Where(a => a.id == obj.Version).FirstOrDefault();
            if (version == null)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.VersionError);
                throw new LaunchException(LaunchState.VersionError,
                    LanguageHelper.GetName("Core.Launch.Error1"));
            }

            var res1 = await GameDownloadHelper.Download(version);
            if (res1.State != GetDownloadState.End)
                throw new LaunchException(LaunchState.VersionError,
                    LanguageHelper.GetName("Core.Launch.Error1"));

            list.AddRange(res1.List!);

            game = VersionPath.GetGame(obj.Version);
        }

        if (game == null)
        {
            throw new LaunchException(LaunchState.VersionError, LanguageHelper.GetName("Core.Launch.Error1"));
        }

        //检查游戏核心文件
        if (ConfigUtils.Config.GameCheck.CheckCore)
        {
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckVersion);
            string file = LibrariesPath.GetGameFile(game.id);
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = game.downloads.client.url,
                    SHA1 = game.downloads.client.sha1,
                    Local = file,
                    Name = $"{obj.Version}.jar"
                });
            }
            else
            {
                using FileStream stream2 = new(file, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                stream2.Seek(0, SeekOrigin.Begin);
                string sha1 = Funtcions.GenSha1(stream2);
                if (sha1 != game.downloads.client.sha1)
                {
                    list.Add(new()
                    {
                        Url = game.downloads.client.url,
                        SHA1 = game.downloads.client.sha1,
                        Local = file,
                        Name = $"{obj.Version}.jar"
                    });
                }
            }
        }

        //检查游戏资源文件
        if (ConfigUtils.Config.GameCheck.CheckAssets)
        {
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckAssets);
            var assets = game.GetIndex();
            if (assets == null)
            {
                assets = await GameJsonObj.GetAssets(game.assetIndex.url);
                if (assets == null)
                {
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.AssetsError);
                    throw new LaunchException(LaunchState.AssetsError,
                        LanguageHelper.GetName("Core.Launch.Error2"));
                }
                game.AddIndex(assets);
            }

            var list1 = await assets.Check();
            foreach (var (Name, Hash) in list1)
            {
                list.Add(new()
                {
                    Overwrite = true,
                    Url = UrlHelper.DownloadAssets(Hash, BaseClient.Source),
                    SHA1 = Hash,
                    Local = $"{AssetsPath.ObjectsDir}/{Hash[..2]}/{Hash}",
                    Name = Name
                });
            }
        }

        //检查运行库
        if (ConfigUtils.Config.GameCheck.CheckLib)
        {
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckLib);
            var list2 = await game.CheckGameLib();
            if (list2.Count != 0)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLib);
                list.AddRange(list2);
            }

            //检查加载器运行库
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckLoader);
            if (obj.Loader == Loaders.Forge)
            {
                var list3 = await obj.CheckForgeLib();
                if (list3 == null)
                {
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLoader);

                    var list4 = await GameDownloadHelper.DownloadForge(obj);
                    if (list4.State != GetDownloadState.End)
                        throw new LaunchException(LaunchState.LostLoader,
                        LanguageHelper.GetName("Core.Launch.Error3"));

                    list.AddRange(list4.List!);
                }
                else
                {
                    list.AddRange(list3);
                }
            }
            else if (obj.Loader == Loaders.Fabric)
            {
                var list3 = obj.CheckFabricLib();
                if (list3 == null)
                {
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLoader);

                    var list4 = await GameDownloadHelper.DownloadFabric(obj);
                    if (list4.State != GetDownloadState.End)
                        throw new LaunchException(LaunchState.LostLoader,
                        LanguageHelper.GetName("Core.Launch.Error3"));

                    list.AddRange(list4.List!);
                }
                else
                {
                    list.AddRange(list3);
                }
            }
            else if (obj.Loader == Loaders.Quilt)
            {
                var list3 = obj.CheckQuiltLib();
                if (list3 == null)
                {
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LostLoader);

                    var list4 = await GameDownloadHelper.DownloadQuilt(obj);
                    if (list4.State != GetDownloadState.End)
                        throw new LaunchException(LaunchState.LostLoader,
                        LanguageHelper.GetName("Core.Launch.Error3"));

                    list.AddRange(list4.List!);
                }
                else
                {
                    list.AddRange(list3);
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
        }

        //检查整合包mod
        if (obj.ModPack && ConfigUtils.Config.GameCheck.CheckMod)
        {
            ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.CheckMods);

            var mods = await obj.GetMods();
            ModObj? mod = null;
            int find = 0;
            ModInfoObj?[] array = obj.Mods.Values.ToArray();
            for (int a = 0; a < array.Length; a++)
            {
                var item = array[a];
                foreach (var item1 in mods)
                {
                    if (item == null)
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
                    if (item == null)
                        continue;

                    list.Add(new()
                    {
                        Url = item.Url,
                        Name = item.File,
                        Local = obj.GetModsPath() + item.File,
                        SHA1 = item.SHA1
                    });
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 找到合适的Java
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>Java信息</returns>
    private static JavaInfo? FindJava(GameSettingObj obj)
    {
        var game = VersionPath.GetGame(obj.Version)!;
        var jv = game.javaVersion.majorVersion;
        var list = JvmPath.Jvms.Where(a => a.Value.MajorVersion == jv)
            .Select(a => a.Value);

        if (!list.Any())
        {
            if (jv > 8)
            {
                list = JvmPath.Jvms.Where(a => a.Value.MajorVersion >= jv)
                .Select(a => a.Value);
                if (!list.Any())
                    return null;
            }
            else
            {
                return null;
            }
        }
        var find = list.Where(a => a.Arch == SystemInfo.SystemArch);
        int max;
        if (find.Any())
        {
            max = find.Max(a => a.MajorVersion);
            return find.Where(x => x.MajorVersion == max).FirstOrDefault();
        }

        max = list.Max(a => a.MajorVersion);
        return list.Where(x => x.MajorVersion == max).FirstOrDefault();
    }

    private readonly static List<string> V1JvmArg = new()
    {
        "-Djava.library.path=${natives_directory}", "-cp", "${classpath}"
    };

    /// <summary>
    /// 创建V1版启动参数
    /// </summary>
    /// <returns></returns>
    private static List<string> MakeV1JvmArg() => V1JvmArg;

    /// <summary>
    /// 创建V2版启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    private static List<string> MakeV2JvmArg(GameSettingObj obj)
    {
        var game = VersionPath.GetGame(obj.Version)!;
        if (game.arguments == null)
            return MakeV1JvmArg();

        List<string> arg = new();
        foreach (var item in game.arguments.jvm)
        {
            if (item is string)
            {
                arg.Add(item);
            }
            else if (item is JObject obj1)
            {
                var obj2 = obj1.ToObject<GameArgObj.Arguments.Jvm>();
                if (obj2 == null)
                {
                    continue;
                }
                bool use = CheckRule.CheckAllow(obj2.rules);
                if (!use)
                    continue;

                if (obj2.value is string item2)
                {
                    arg.Add(item2!);
                }
                else if (obj2.value is JArray)
                {
                    foreach (string item1 in obj2.value)
                    {
                        arg.Add(item1);
                    }
                }
            }
        }

        if (obj.Loader == Loaders.Forge)
        {
            var forge = obj.GetForgeObj()!;
            if (forge.arguments.jvm != null)
                foreach (var item in forge.arguments.jvm)
                {
                    arg.Add(item);
                }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.arguments.jvm)
            {
                arg.Add(item);
            }
        }

        return arg;
    }

    /// <summary>
    /// 创建V1版游戏参数
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static List<string> MakeV1GameArg(GameSettingObj obj)
    {
        if (obj.Loader == Loaders.Forge)
        {
            var forge = obj.GetForgeObj()!;
            return new(forge.minecraftArguments.Split(" "));
        }

        var version = VersionPath.GetGame(obj.Version)!;
        return new(version.minecraftArguments.Split(" "));
    }

    /// <summary>
    /// 创建V2版游戏参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    private static List<string> MakeV2GameArg(GameSettingObj obj)
    {
        var game = VersionPath.GetGame(obj.Version)!;
        if (game.arguments == null)
            return MakeV1GameArg(obj);

        List<string> arg = new();
        foreach (var item in game.arguments.game)
        {
            if (item is string)
            {
                arg.Add(item);
            }
            //else if (item is JObject)
            //{
            //    JObject obj1 = item as JObject;
            //    var obj2 = obj1.ToObject<GameArgObj.Arguments.Jvm>();
            //    bool use = CheckRule.CheckAllow(obj2.rules);
            //    if (!use)
            //        continue;

            //    if (obj2.value is string)
            //    {
            //        string? item2 = obj2.value as string;
            //        arg.Append(item2).Append(' ');
            //    }
            //    else if (obj2.value is JArray)
            //    {
            //        foreach (string item1 in obj2.value)
            //        {
            //            arg.Append(item1).Append(' ');
            //        }
            //    }
            //}
        }

        if (obj.Loader == Loaders.Forge)
        {
            var forge = obj.GetForgeObj()!;
            foreach (var item in forge.arguments.game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.arguments.game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            foreach (var item in quilt.arguments.game)
            {
                arg.Add(item);
            }
        }

        return arg;
    }

    /// <summary>
    /// 创建Jvm参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">V2模式</param>
    /// <param name="login">登录的账户</param>
    /// <returns></returns>
    private static async Task<List<string>> JvmArg(GameSettingObj obj, bool v2, LoginObj login)
    {
        JvmArgObj args;

        if (obj.JvmArg == null)
        {
            args = ConfigUtils.Config.DefaultJvmArg;
        }
        else
        {
            args = new()
            {
                JvmArgs = obj.JvmArg.JvmArgs
                    ?? ConfigUtils.Config.DefaultJvmArg.JvmArgs,
                GCArgument = obj.JvmArg.GCArgument
                    ?? ConfigUtils.Config.DefaultJvmArg.GCArgument,
                GC = obj.JvmArg.GC
                    ?? ConfigUtils.Config.DefaultJvmArg.GC,
                JavaAgent = obj.JvmArg.JavaAgent
                    ?? ConfigUtils.Config.DefaultJvmArg.JavaAgent,
                MaxMemory = obj.JvmArg.MaxMemory
                    ?? ConfigUtils.Config.DefaultJvmArg.MaxMemory,
                MinMemory = obj.JvmArg.MinMemory
                    ?? ConfigUtils.Config.DefaultJvmArg.MinMemory
            };
        }

        List<string> jvmHead = new();

        if (!string.IsNullOrWhiteSpace(args.JavaAgent))
        {
            jvmHead.Add($"-javaagent:{args.JavaAgent.Trim()}");
        }

        switch (args.GC)
        {
            case GCType.G1GC:
                jvmHead.Add("-XX:+UseG1GC");
                break;
            case GCType.SerialGC:
                jvmHead.Add("-XX:+UseSerialGC");
                break;
            case GCType.ParallelGC:
                jvmHead.Add("-XX:+UseParallelGC");
                break;
            case GCType.CMSGC:
                jvmHead.Add("-XX:+UseConcMarkSweepGC");
                break;
            case GCType.User:
                if (!string.IsNullOrWhiteSpace(args.GCArgument))
                    jvmHead.Add(args.GCArgument.Trim());
                break;
            default:
                break;
        }

        if (args.MinMemory != 0)
        {
            jvmHead.Add($"-Xms{args.MinMemory}m");
        }
        if (args.MaxMemory != 0)
        {
            jvmHead.Add($"-Xmx{args.MaxMemory}m");
        }
        if (!string.IsNullOrWhiteSpace(args.JvmArgs))
        {
            jvmHead.AddRange(args.JvmArgs.Split(";"));
        }

        if (v2 && obj.Loader == Loaders.Forge)
        {
            jvmHead.Add($"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}");
            jvmHead.Add($"-Dforgewrapper.installer={ForgeAPI
                .BuildForgeInster(obj.Version, obj.LoaderVersion!).Local}");
            jvmHead.Add($"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}");
        }

        //jvmHead.Add("-Djava.rmi.server.useCodebaseOnly=true");
        //jvmHead.Add("-XX:+UnlockExperimentalVMOptions");
        //jvmHead.Add("-Dfml.ignoreInvalidMinecraftCertificates=true");
        //jvmHead.Add("-Dfml.ignorePatchDiscrepancies=true");
        //jvmHead.Add("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false");
        //jvmHead.Add("-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false");
        //jvmHead.Add($"-Dminecraft.client.jar={VersionPath.BaseDir}/{obj.Version}.jar");

        jvmHead.AddRange(v2 ? MakeV2JvmArg(obj) : MakeV1JvmArg());

        if (login.AuthType == AuthType.Nide8)
        {
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowNide8Injector}={login.Text1}");
            jvmHead.Add("-Dnide8auth.client=true");
        }
        else if (login.AuthType == AuthType.AuthlibInjector)
        {
            var res = await BaseClient.GetString(login.Text1);
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={Funtcions.GenBase64(res.Item2!)}");
        }
        else if (login.AuthType == AuthType.LittleSkin)
        {
            var res = await BaseClient.GetString("https://littleskin.cn/api/yggdrasil");
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}=https://littleskin.cn/api/yggdrasil");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={Funtcions.GenBase64(res.Item2!)}");
        }
        else if (login.AuthType == AuthType.SelfLittleSkin)
        {
            var res = await BaseClient.GetString($"{login.Text1}/api/yggdrasil");
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}/api/yggdrasil");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={Funtcions.GenBase64(res.Item2!)}");
        }

        return jvmHead;
    }

    /// <summary>
    /// 创建游戏启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">V2模式</param>
    /// <returns></returns>
    private static List<string> GameArg(GameSettingObj obj, bool v2)
    {
        List<string> gameArg = new();
        gameArg.AddRange(v2 ? MakeV2GameArg(obj) : MakeV1GameArg(obj));

        WindowSettingObj window;
        if (obj.Window == null)
        {
            window = ConfigUtils.Config.Window;
        }
        else
        {
            window = new()
            {
                FullScreen = obj.Window.FullScreen
                    ?? ConfigUtils.Config.Window.FullScreen,
                Width = obj.Window.Width
                    ?? ConfigUtils.Config.Window.Width,
                Height = obj.Window.Height
                    ?? ConfigUtils.Config.Window.Height,
            };
        }
        if (window.FullScreen == true)
        {
            gameArg.Add("--fullscreen");
        }
        else
        {
            if (window.Width > 0)
            {
                gameArg.Add($"--width");
                gameArg.Add($"{window.Width}");
            }
            if (window.Height > 0)
            {
                gameArg.Add($"--height");
                gameArg.Add($"{window.Height}");
            }
        }

        if (obj.StartServer != null && !string.IsNullOrWhiteSpace(obj.StartServer.IP)
            && obj.StartServer.Port != null)
        {
            gameArg.Add($"--server");
            gameArg.Add(obj.StartServer.IP);
            if (obj.StartServer.Port > 0)
            {
                gameArg.Add($"--port");
                gameArg.Add(obj.StartServer.Port.ToString()!);
            }
        }

        if (obj.ProxyHost != null)
        {
            if (!string.IsNullOrWhiteSpace(obj.ProxyHost.IP))
            {
                gameArg.Add($"--proxyHost");
                gameArg.Add(obj.ProxyHost.IP);
            }
            if (obj.ProxyHost.Port != null && obj.ProxyHost.Port != 0)
            {
                gameArg.Add($"--proxyPort");
                gameArg.Add(obj.ProxyHost.Port.ToString()!);
            }
            if (!string.IsNullOrWhiteSpace(obj.ProxyHost.User))
            {
                gameArg.Add($"--proxyUser");
                gameArg.Add(obj.ProxyHost.User);
            }
            if (!string.IsNullOrWhiteSpace(obj.ProxyHost.Password))
            {
                gameArg.Add($"--proxyPass");
                gameArg.Add(obj.ProxyHost.Password);
            }
        }
        else if (ConfigUtils.Config.Http.GameProxy)
        {
            if (!string.IsNullOrWhiteSpace(ConfigUtils.Config.Http.ProxyIP))
            {
                gameArg.Add($"--proxyHost");
                gameArg.Add(ConfigUtils.Config.Http.ProxyIP);
            }
            if (ConfigUtils.Config.Http.ProxyPort != 0)
            {
                gameArg.Add($"--proxyPort");
                gameArg.Add(ConfigUtils.Config.Http.ProxyPort.ToString());
            }
            if (!string.IsNullOrWhiteSpace(ConfigUtils.Config.Http.ProxyUser))
            {
                gameArg.Add($"--proxyUser");
                gameArg.Add(ConfigUtils.Config.Http.ProxyUser);
            }
            if (!string.IsNullOrWhiteSpace(ConfigUtils.Config.Http.ProxyPassword))
            {
                gameArg.Add($"--proxyPass");
                gameArg.Add(ConfigUtils.Config.Http.ProxyPassword);
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.JvmArg?.GameArgs))
        {
            gameArg.AddRange(obj.JvmArg.GameArgs.Split("\n"));
        }

        return gameArg;
    }

    private static void AddOrUpdate(this Dictionary<LibVersionObj, string> dic,
        LibVersionObj key, string value)
    {
        foreach (var item in dic)
        {
            if (item.Key.Equals(key))
            {
                dic.Remove(item.Key);
                break;
            }
        }

        dic.Add(key, value);
    }

    /// <summary>
    /// 获取所有Lib
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">V2模式</param>
    /// <returns></returns>
    private static async Task<List<string>> GetLibs(GameSettingObj obj, bool v2)
    {
        Dictionary<LibVersionObj, string> list = new();
        var version = VersionPath.GetGame(obj.Version)!;
        var list1 = await GameHelper.MakeGameLibs(version);
        foreach (var item in list1)
        {
            var key = PathC.MakeVersionObj(item.Name);

            if (item.Later == null)
                list.AddOrUpdate(key, item.Local);
        }

        if (obj.Loader == Loaders.Forge)
        {
            var forge = obj.GetForgeObj()!;

            var list2 = ForgeAPI.MakeForgeLibs(forge, obj.Version, obj.LoaderVersion!);

            list2.ForEach(a => list.AddOrUpdate(PathC.MakeVersionObj(a.Name), a.Local));

            if (v2)
            {
                list.AddOrUpdate(new(), ForgeAPI.ForgeWrapper);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.libraries)
            {
                var name = PathC.ToName(item.name);
                list.AddOrUpdate(PathC.MakeVersionObj(name.Name),
                    $"{LibrariesPath.BaseDir}/{name.Path}");
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            foreach (var item in quilt.libraries)
            {
                var name = PathC.ToName(item.name);
                list.AddOrUpdate(PathC.MakeVersionObj(name.Name),
                    $"{LibrariesPath.BaseDir}/{name.Path}");
            }
        }

        return new(list.Values) { LibrariesPath.GetGameFile(obj.Version) };
    }

    /// <summary>
    /// 替换参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="all_arg">参数</param>
    /// <param name="v2">V2模式</param>
    private static async Task ReplaceAll(GameSettingObj obj, LoginObj login, List<string> all_arg, bool v2)
    {
        var version = VersionPath.GetGame(obj.Version)!;
        string assetsPath = AssetsPath.BaseDir;
        string gameDir = InstancesPath.GetGamePath(obj);
        string assetsIndexName;
        if (version.assets != null)
        {
            assetsIndexName = version.assets;
        }
        else
        {
            assetsIndexName = "legacy";
        }
        string version_name = obj.Loader switch
        {
            Loaders.Forge => $"forge-{obj.Version}-{obj.LoaderVersion}",
            Loaders.Fabric => $"fabric-{obj.Version}-{obj.LoaderVersion}",
            Loaders.Quilt => $"quilt-{obj.Version}-{obj.LoaderVersion}",
            _ => obj.Version
        };
        var libraries = await GetLibs(obj, v2);
        StringBuilder classpath = new();
        string sep = SystemInfo.Os == OsType.Windows ? ";" : ":";
        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.GetName("Core.Launch.Info2"));

        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.ClassPath))
        {
            var list = obj.AdvanceJvm.ClassPath.Split(";");
            foreach (var item1 in list)
            {
                var path = Path.GetFullPath(item1);
                if (File.Exists(path))
                {
                    libraries.Add(item1);
                }
            }
        }

        foreach (var item in libraries)
        {
            classpath.Append($"{item}{sep}");
            ColorMCCore.GameLog?.Invoke(obj, $"    {item}");
        }
        classpath.Remove(classpath.Length - 1, 1);

        Dictionary<string, string> argDic = new()
            {
                {"${auth_player_name}", login.UserName },
                {"${version_name}",version_name },
                {"${game_directory}",gameDir },
                {"${assets_root}",assetsPath },
                {"${assets_index_name}",assetsIndexName },
                {"${auth_uuid}",login.UUID },
                {"${auth_access_token}",login.AccessToken },
                {"${game_assets}",assetsPath },
                {"${user_properties}", "{}" },
                {"${user_type}", login.AuthType == AuthType.OAuth ? "msa" : "legacy" },
                {"${version_type}", "ColorMC" },
                {"${natives_directory}", LibrariesPath.GetNativeDir(obj.Version) },
                {"${library_directory}",LibrariesPath.BaseDir },
                {"${classpath_separator}", sep },
                {"${launcher_name}","ColorMC" },
                {"${launcher_version}", ColorMCCore.Version },
                {"${classpath}", classpath.ToString().Trim() },
            };

        for (int a = 0; a < all_arg.Count; a++)
        {
            all_arg[a] = argDic.Aggregate(all_arg[a], (a, b) => a.Replace(b.Key, b.Value));
        }
    }

    /// <summary>
    /// 创建所有启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <returns></returns>
    private static async Task<List<string>> MakeArg(GameSettingObj obj, LoginObj login)
    {
        var list = new List<string>();
        var version = VersionPath.GetGame(obj.Version)!;
        var v2 = CheckRule.GameLaunchVersion(version);

        list.AddRange(await JvmArg(obj, v2, login));

        if (string.IsNullOrWhiteSpace(obj.AdvanceJvm?.MainClass))
        {
            if (obj.Loader == Loaders.Normal)
                list.Add(version.mainClass);
            else if (obj.Loader == Loaders.Forge)
            {
                if (v2)
                {
                    list.Add("io.github.zekerzhayard.forgewrapper.installer.Main");
                }
                else
                {
                    var forge = obj.GetForgeObj()!;
                    list.Add(forge.mainClass);
                }
            }
            else if (obj.Loader == Loaders.Fabric)
            {
                var fabric = obj.GetFabricObj()!;
                list.Add(fabric.mainClass);
            }
            else if (obj.Loader == Loaders.Quilt)
            {
                var quilt = obj.GetQuiltObj()!;
                list.Add(quilt.mainClass);
            }
        }
        else
        {
            list.Add(obj.AdvanceJvm.MainClass);
        }
        list.AddRange(GameArg(obj, v2));

        await ReplaceAll(obj, login, list, v2);

        return list;
    }

    /// <summary>
    /// 进程日志
    /// </summary>
    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        ColorMCCore.ProcessLog?.Invoke(sender as Process, e.Data);
    }

    /// <summary>
    /// 进程日志
    /// </summary>
    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        ColorMCCore.ProcessLog?.Invoke(sender as Process, e.Data);
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="jvmCfg">使用的Java</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns></returns>
    public static async Task<Process?> StartGame(this GameSettingObj obj, LoginObj login)
    {
        Stopwatch stopwatch = new();

        //启动前运行
        if (ColorMCCore.LaunchP != null && (obj.JvmArg?.LaunchPre == true
            || ConfigUtils.Config.DefaultJvmArg.LaunchPre))
        {
            string? start = obj.JvmArg?.LaunchPreData;
            if (string.IsNullOrWhiteSpace(start))
                start = ConfigUtils.Config.DefaultJvmArg.LaunchPreData;
            if (!string.IsNullOrWhiteSpace(start))
            {
                var res1 = await ColorMCCore.LaunchP.Invoke(true);
                if (res1)
                {
                    stopwatch.Start();
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LaunchPre);
                    var args = start.Split(' ');
                    var file = args[0];
                    if (file.StartsWith("./") || file.StartsWith("../"))
                    {
                        file = Path.GetFullPath(obj.GetBasePath() + "/" + file);
                    }
                    var arglist = new List<string>();

                    var info = new ProcessStartInfo(file)
                    {
                        WorkingDirectory = obj.GetGamePath(),
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    for (int a = 1; a < args.Length; a++)
                    {
                        info.ArgumentList.Add(args[a]);
                    }
                    var p = new Process()
                    {
                        EnableRaisingEvents = true,
                        StartInfo = info
                    };
                    p.OutputDataReceived += (a, b) =>
                    {
                        ColorMCCore.GameLog?.Invoke(obj, b.Data);
                    };
                    p.ErrorDataReceived += (a, b) =>
                    {
                        ColorMCCore.GameLog?.Invoke(obj, b.Data);
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();

                    stopwatch.Stop();
                    string temp1 = string.Format(LanguageHelper.GetName("Core.Launch.Info8"),
                        obj.Name, stopwatch.Elapsed.ToString());
                    ColorMCCore.GameLog?.Invoke(obj, temp1);
                    Logs.Info(temp1);
                }
            }
            else
            {
                string temp2 = string.Format(LanguageHelper.GetName("Core.Launch.Info10"),
                obj.Name);
                ColorMCCore.GameLog?.Invoke(obj, temp2);
                Logs.Info(temp2);
            }
        }

        //登录账户
        stopwatch.Restart();
        stopwatch.Start();
        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.Login);
        var (State, State1, Obj, Message, Ex) = await login.RefreshToken();
        if (State1 != LoginState.Done)
        {
            if (login.AuthType == AuthType.OAuth
                && !string.IsNullOrWhiteSpace(login.UUID)
                && ColorMCCore.OfflineLaunch != null
                && await ColorMCCore.OfflineLaunch(login) == true)
            {
                login = new()
                {
                    UserName = login.UserName,
                    UUID = login.UUID,
                    AuthType = AuthType.Offline
                };
            }
            else
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LoginFail);
                if (Ex != null)
                    throw new LaunchException(LaunchState.LoginFail, Ex);

                throw new LaunchException(LaunchState.LoginFail, Message!);
            }
        }
        else
        {
            login = Obj!;
            login.Save();
        }

        stopwatch.Stop();
        string temp = string.Format(LanguageHelper.GetName("Core.Launch.Info4"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.GameLog?.Invoke(obj, temp);
        Logs.Info(temp);

        stopwatch.Restart();
        stopwatch.Start();
        //检查游戏文件
        var res = await CheckGameFile(obj, login);
        stopwatch.Stop();
        temp = string.Format(LanguageHelper.GetName("Core.Launch.Info5"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.GameLog?.Invoke(obj, temp);
        Logs.Info(temp);

        //下载缺失的文件
        if (res.Count != 0)
        {
            bool download = true;
            if (!ConfigUtils.Config.Http.AutoDownload)
            {
                if (ColorMCCore.GameDownload == null)
                    throw new LaunchException(LaunchState.LostGame,
                        LanguageHelper.GetName("Core.Launch.Error4"));

                download = await ColorMCCore.GameDownload.Invoke(LaunchState.LostFile, obj);
            }

            if (download)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.Download);

                stopwatch.Restart();
                stopwatch.Start();

                var ok = await DownloadManager.Start(res);
                if (!ok)
                {
                    throw new LaunchException(LaunchState.LostFile,
                        LanguageHelper.GetName("Core.Launch.Error5"));
                }
                stopwatch.Stop();
                temp = string.Format(LanguageHelper.GetName("Core.Launch.Info7"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.GameLog?.Invoke(obj, temp);
                Logs.Info(temp);
            }
        }

        stopwatch.Restart();
        stopwatch.Start();

        //准备Jvm参数
        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.JvmPrepare);

        var arg = await MakeArg(obj, login);
        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.GetName("Core.Launch.Info1"));
        bool hidenext = false;
        foreach (var item in arg)
        {
            if (hidenext)
            {
                hidenext = false;
                ColorMCCore.GameLog?.Invoke(obj, "****************");
            }
            else
            {
                ColorMCCore.GameLog?.Invoke(obj, item);
            }
            var low = item.ToLower();
            if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
            {
                hidenext = true;
            }
        }

        var path = obj.JvmLocal;
        JavaInfo? jvm = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            jvm = JvmPath.GetInfo(obj.JvmName) ?? FindJava(obj);
            if (jvm == null)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.JavaError);
                ColorMCCore.NoJava?.Invoke();
                throw new LaunchException(LaunchState.JavaError,
                        LanguageHelper.GetName("Core.Launch.Error6"));
            }

            path = jvm.GetPath();
        }

        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.GetName("Core.Launch.Info3"));
        ColorMCCore.GameLog?.Invoke(obj, path);

        //NativeLaunch(jvm!, arg);
        //return null;

        //启动进程
        Process process = new()
        {
            EnableRaisingEvents = true
        };
        process.StartInfo.FileName = path;
        process.StartInfo.WorkingDirectory = InstancesPath.GetGamePath(obj);
        Directory.CreateDirectory(process.StartInfo.WorkingDirectory);
        foreach (var item in arg)
        {
            process.StartInfo.ArgumentList.Add(item);
        }

        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.OutputDataReceived += Process_OutputDataReceived;
        process.ErrorDataReceived += Process_ErrorDataReceived;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        stopwatch.Stop();
        temp = string.Format(LanguageHelper.GetName("Core.Launch.Info6"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.GameLog?.Invoke(obj, temp);
        Logs.Info(temp);

        //启动后执行
        if (ColorMCCore.LaunchP != null && (obj.JvmArg?.LaunchPost == true
            || ConfigUtils.Config.DefaultJvmArg.LaunchPost))
        {
            string? start = obj.JvmArg?.LaunchPostData;
            if (string.IsNullOrWhiteSpace(start))
                start = ConfigUtils.Config.DefaultJvmArg.LaunchPostData;
            if (!string.IsNullOrWhiteSpace(start))
            {
                var res1 = await ColorMCCore.LaunchP.Invoke(false);
                if (res1)
                {
                    stopwatch.Start();
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LaunchPost);
                    var args = start.Split(' ');
                    var file = args[0];
                    if (file.StartsWith("./") || file.StartsWith("../"))
                    {
                        file = Path.GetFullPath(obj.GetBasePath() + "/" + file);
                    }
                    var arglist = new List<string>();

                    var info = new ProcessStartInfo(file)
                    {
                        WorkingDirectory = obj.GetGamePath(),
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    for (int a = 1; a < args.Length; a++)
                    {
                        info.ArgumentList.Add(args[a]);
                    }

                    var p = new Process()
                    {
                        EnableRaisingEvents = true,
                        StartInfo = info
                    };
                    p.OutputDataReceived += (a, b) =>
                    {
                        ColorMCCore.GameLog?.Invoke(obj, b.Data);
                    };
                    p.ErrorDataReceived += (a, b) =>
                    {
                        ColorMCCore.GameLog?.Invoke(obj, b.Data);
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }

                stopwatch.Stop();
                string temp1 = string.Format(LanguageHelper.GetName("Core.Launch.Info9"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.GameLog?.Invoke(obj, temp1);
                Logs.Info(temp1);
            }
            else
            {
                string temp2 = string.Format(LanguageHelper.GetName("Core.Launch.Info11"),
                obj.Name);
                ColorMCCore.GameLog?.Invoke(obj, temp2);
                Logs.Info(temp2);
            }
        }

        return process;
    }

    //public delegate int Func1(int argc,
    //    string[] argv, /* main argc, argc */
    //    int jargc, string[] jargv,          /* java args */
    //    int appclassc, string[] appclassv,  /* app classpath */
    //    string fullversion,                 /* full version defined */
    //    string dotversion,                  /* dot version defined */
    //    string pname,                       /* program name */
    //    string lname,                       /* launcher name */
    //    bool javaargs,                      /* JAVA_ARGS */
    //    bool cpwildcard,                    /* classpath wildcard*/
    //    bool javaw,                         /* windows-only javaw */
    //    int ergo                            /* ergonomics class policy */
    //);

    //private static int Lenght;
    //private static string[] Args;

    //public static void NativeLaunch(JavaInfo info, List<string> args)
    //{
    //    var info1 = new FileInfo(info.Path);
    //    var path = info1.Directory?.Parent?.FullName;

    //    var local = path + SystemInfo.Os switch
    //    {
    //        OsType.Windows => "/bin/jli.dll",
    //        OsType.Linux => "/lib/libjli.so",
    //        OsType.MacOS => "/lib/libjli.dylib",
    //        _ => null
    //    };
    //    if (File.Exists(local))
    //    {
    //        local = Path.GetFullPath(local);
    //    }

    //    var temp = NativeLoader.Loader.LoadLibrary(local);
    //    var temp1 = NativeLoader.Loader.GetProcAddress(temp, "JLI_Launch", false);
    //    var inv = (Func1)Marshal.GetDelegateForFunctionPointer(temp1, typeof(Func1));

    //    //Environment.SetEnvironmentVariable("JAVA_HOME", path);
    //    //Environment.SetEnvironmentVariable("PATH", path + "/bin:" + path);
    //    //Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", path + "/lib:" + path + "/bin");

    //    var args1 = new string[args.Count + 1];
    //    args1[0] = "java";

    //    for (int i = 1; i < args1.Length; i++)
    //    {
    //        args1[i] = args[i - 1];
    //    }

    //    Lenght = args1.Length;
    //    Args = args1;

    //    new Thread(() =>
    //    {
    //        try
    //        {
    //            var res = inv(Lenght, Args, 0, null, 0, null, "", "", "", "", false, false, true, 0);

    //            var res1 = NativeLoader.Loader.CloseLibrary(temp);
    //        }
    //        catch (Exception e)
    //        {
    //            ColorMCCore.OnError?.Invoke("Error", e, false);
    //        }
    //    }).Start();
    //}
}
