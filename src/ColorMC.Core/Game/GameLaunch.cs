using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏启动类
/// </summary>
public static class Launch
{
    public const string JAVA_LOCAL = "%JAVA_LOCAL%";
    public const string JAVA_ARG = "%JAVA_ARG%";
    public const string LAUNCH_DIR = "%LAUNCH_DIR%";
    public const string GAME_NAME = "%GAME_NAME%";
    public const string GAME_UUID = "%GAME_UUID%";
    public const string GAME_DIR = "%GAME_DIR%";
    public const string GAME_BASE_DIR = "%GAME_BASE_DIR%";

    /// <summary>
    /// 取消启动
    /// </summary>
    private static CancellationToken s_cancel;

    /// <summary>
    /// 获取Forge安装参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>参数</returns>
    public static List<string> MakeInstallForgeArg(this GameSettingObj obj)
    {
        var jvmHead = new List<string>
        {
            $"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}",
            $"-Dforgewrapper.installer={(obj.Loader == Loaders.NeoForge ?
            DownloadItemHelper.BuildNeoForgeInster(obj.Version, obj.LoaderVersion!).Local :
            DownloadItemHelper.BuildForgeInster(obj.Version, obj.LoaderVersion!).Local)}",
            $"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}",
            "-Dforgewrapper.justInstall=true"
        };

        Dictionary<LibVersionObj, string> list = new();

        var forge = obj.Loader == Loaders.NeoForge
                ? VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!)!
                : VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!)!;
        var list2 = DownloadItemHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!,
            obj.Loader == Loaders.NeoForge);
        list2.ForEach(a => list.AddOrUpdate(FuntionUtils.MakeVersionObj(a.Name), a.Local));

        GameHelper.ReadyForgeWrapper();
        list.AddOrUpdate(new(), GameHelper.ForgeWrapper);

        var libraries = new List<string>(list.Values);
        StringBuilder classpath = new();
        string sep = SystemInfo.Os == OsType.Windows ? ";" : ":";
        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.Get("Core.Launch.Info2"));

        //去除重复的classpath
        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.ClassPath))
        {
            var list1 = obj.AdvanceJvm.ClassPath.Split(";");
            foreach (var item1 in list1)
            {
                var path = Path.GetFullPath(item1);
                if (File.Exists(path))
                {
                    libraries.Add(item1);
                }
            }
        }

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            classpath.Append($"{item}{sep}");
        }
        classpath.Remove(classpath.Length - 1, 1);

        string cp = classpath.ToString().Trim();
        jvmHead.Add("-cp");
        jvmHead.Add(cp);

        var arg = MakeV2GameArg(obj);

        jvmHead.Add("io.github.zekerzhayard.forgewrapper.installer.Main");
        var forge1 = obj.Loader == Loaders.NeoForge
                ? obj.GetNeoForgeObj()!
                : obj.GetForgeObj()!;

        jvmHead.AddRange(forge1.arguments.game);

        return jvmHead;
    }

    /// <summary>
    /// 替换参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="jvm">JAVA位置</param>
    /// <param name="arg">JVM参数</param>
    /// <param name="item">命令</param>
    /// <returns>参数</returns>
    public static string ReplaceArg(this GameSettingObj obj, string jvm, List<string> arg, string item)
    {
        static string GetString(List<string> arg)
        {
            var data = new StringBuilder();
            foreach (var item in arg)
            {
                data.AppendLine(item);
            }

            return data.ToString();
        }

        return item
                .Replace(GAME_NAME, obj.Name)
                .Replace(GAME_UUID, obj.UUID)
                .Replace(GAME_DIR, obj.GetGamePath())
                .Replace(GAME_BASE_DIR, obj.GetBasePath())
                .Replace(LAUNCH_DIR, ColorMCCore.BaseDir)
                .Replace(JAVA_LOCAL, jvm)
                .Replace(JAVA_ARG, GetString(arg));
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cmd">命令</param>
    public static void CmdRun(GameSettingObj obj, string cmd)
    {
        var args = cmd.Split('\n');
        var file = args[0].Trim();
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
            info.ArgumentList.Add(args[a].Trim());
        }
        using var p = new Process()
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


    /// <summary>
    /// V1版Jvm参数
    /// </summary>
    private readonly static List<string> V1JvmArg = new()
    {
        "-Djava.library.path=${natives_directory}", "-cp", "${classpath}"
    };

    /// <summary>
    /// 创建V2版Jvm参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>参数</returns>
    private static List<string> MakeV2JvmArg(GameSettingObj obj)
    {
        var game = VersionPath.GetGame(obj.Version)!;
        if (game.arguments == null)
        {
            return V1JvmArg;
        }

        var arg = new List<string>();
        //添加原版参数
        foreach (object item in game.arguments.jvm)
        {
            if (item is string str)
            {
                if (SystemInfo.Os == OsType.Android)
                {
                    if (str.StartsWith("-Djava.library.path")
                        || str.StartsWith("-Djna.tmpdir")
                        || str.StartsWith("-Dorg.lwjgl.system.SharedLibraryExtractPath")
                        || str.StartsWith("-Dio.netty.native.workdir"))
                    {
                        continue;
                    }
                }

                arg.Add(str);
            }
            else if (item is JObject obj1)
            {
                var obj2 = obj1.ToObject<GameArgObj.Arguments.Jvm>();
                if (obj2 == null)
                {
                    continue;
                }
                if (!CheckHelpers.CheckAllow(obj2.rules))
                {
                    continue;
                }

                if (obj2.value is string item2)
                {
                    arg.Add(item2!);
                }
                else if (obj2.value is List<string> list)
                {
                    arg.AddRange(list);
                }
            }
        }

        //添加Mod加载器的参数
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
            if (forge.arguments.jvm != null)
            {
                foreach (var item in forge.arguments.jvm)
                {
                    arg.Add(item);
                }
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
    /// <param name="obj">游戏实例</param>
    /// <returns>启动参数</returns>
    private static List<string> MakeV1GameArg(GameSettingObj obj)
    {
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
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
        {
            return MakeV1GameArg(obj);
        }

        List<string> arg = new();
        foreach (object item in game.arguments.game)
        {
            if (item is string str)
            {
                arg.Add(str);
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

        //Mod加载器参数
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ? obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
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
    /// <returns>Jvm参数</returns>
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

        //javaagent
        if (!string.IsNullOrWhiteSpace(args.JavaAgent))
        {
            jvmHead.Add($"-javaagent:{args.JavaAgent.Trim()}");
        }

        //gc
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

        //mem
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
            foreach (var item in args.JvmArgs.Split("\n"))
            {
                jvmHead.Add(item.Trim());
            }
        }

        //loader
        if (v2 && obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            jvmHead.Add($"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}");
            jvmHead.Add($"-Dforgewrapper.installer={(obj.Loader == Loaders.NeoForge ?
                DownloadItemHelper.BuildNeoForgeInster(obj.Version, obj.LoaderVersion!).Local :
                DownloadItemHelper.BuildForgeInster(obj.Version, obj.LoaderVersion!).Local)}");
            jvmHead.Add($"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}");
            if (SystemInfo.Os == OsType.Android)
            {
                jvmHead.Add("-Dforgewrapper.justLaunch=true");
            }
        }

        //jvmHead.Add("-Djava.rmi.server.useCodebaseOnly=true");
        //jvmHead.Add("-XX:+UnlockExperimentalVMOptions");
        jvmHead.Add("-Dfml.ignoreInvalidMinecraftCertificates=true");
        jvmHead.Add("-Dfml.ignorePatchDiscrepancies=true");
        jvmHead.Add("-Dlog4j2.formatMsgNoLookups=true");
        //jvmHead.Add("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false");
        //jvmHead.Add("-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false");
        //jvmHead.Add($"-Dminecraft.client.jar={VersionPath.BaseDir}/{obj.Version}.jar");

        jvmHead.AddRange(v2 ? MakeV2JvmArg(obj) : V1JvmArg);

        //auth
        if (login.AuthType == AuthType.Nide8)
        {
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowNide8Injector}={login.Text1}");
            jvmHead.Add("-Dnide8auth.client=true");
        }
        else if (login.AuthType == AuthType.AuthlibInjector)
        {
            var res = await BaseClient.GetString(login.Text1);
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Item2!)}");
        }
        else if (login.AuthType == AuthType.LittleSkin)
        {
            var res = await BaseClient.GetString($"{UrlHelper.LittleSkin}api/yggdrasil");
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={UrlHelper.LittleSkin}api/yggdrasil");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Item2!)}");
        }
        else if (login.AuthType == AuthType.SelfLittleSkin)
        {
            var res = await BaseClient.GetString($"{login.Text1}/api/yggdrasil");
            jvmHead.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}/api/yggdrasil");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Item2!)}");
        }

        //log4j2-xml
        var game = VersionPath.GetGame(obj.Version)!;
        if (game.logging != null && ConfigUtils.Config.SafeLog4j)
        {
            var obj1 = DownloadItemHelper.BuildLog4jItem(game);
            jvmHead.Add(game.logging.client.argument.Replace("${path}", obj1.Local));
        }

        return jvmHead;
    }

    /// <summary>
    /// 创建游戏启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">V2模式</param>
    /// <returns>游戏启动参数</returns>
    private static List<string> GameArg(GameSettingObj obj, bool v2, WorldObj? world)
    {
        List<string> gameArg = new();
        gameArg.AddRange(v2 ? MakeV2GameArg(obj) : MakeV1GameArg(obj));

        if (SystemInfo.Os != OsType.Android)
        {
            //window
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
        }

        //--quickPlayMultiplayer

        if (world != null)
        {
            gameArg.Add($"--quickPlaySingleplayer");
            gameArg.Add($"{world.LevelName}");
        }
        else
        {
            if (obj.StartServer != null && !string.IsNullOrWhiteSpace(obj.StartServer.IP)
                && obj.StartServer.Port != null)
            {
                if (CheckHelpers.IsGameLaunchVersion120(obj.Version))
                {
                    gameArg.Add($"--quickPlayMultiplayer");
                    if (obj.StartServer.Port > 0)
                    {
                        gameArg.Add($"{obj.StartServer.IP}:{obj.StartServer.Port}");
                    }
                    else
                    {
                        gameArg.Add($"{obj.StartServer.IP}:25565");
                    }
                }
                else
                {
                    gameArg.Add($"--server");
                    gameArg.Add(obj.StartServer.IP);
                    if (obj.StartServer.Port > 0)
                    {
                        gameArg.Add($"--port");
                        gameArg.Add(obj.StartServer.Port.ToString()!);
                    }
                }
            }
        }

        //proxy
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

    /// <summary>
    /// 添加获取更新
    /// </summary>
    /// <param name="dic">源字典</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
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
    /// <returns>Lib地址列表</returns>
    private static async Task<List<string>> GetLibs(GameSettingObj obj, bool v2)
    {
        Dictionary<LibVersionObj, string> list = new();
        var version = VersionPath.GetGame(obj.Version)!;

        //GameLib
        var list1 = await DownloadItemHelper.BuildGameLibs(version);
        foreach (var item in list1)
        {
            if (item.Later == null)
            {
                //不添加lwjgl
                if (item.Name.Contains("org.lwjgl")
                    && (SystemInfo.Os == OsType.Android || (CheckHelpers.IsGameLaunchVersion120(obj.Version)
                        && (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge))))
                {
                    continue;
                }
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), item.Local);
            }
        }

        //LoaderLib
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;

            var list2 = DownloadItemHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!,
                obj.Loader == Loaders.NeoForge);

            list2.ForEach(a => list.AddOrUpdate(FuntionUtils.MakeVersionObj(a.Name), a.Local));

            if (v2)
            {
                list.AddOrUpdate(new(), GameHelper.ForgeWrapper);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.libraries)
            {
                var name = PathHelper.ToPathAndName(item.name);
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(name.Name),
                    Path.GetFullPath($"{LibrariesPath.BaseDir}/{name.Path}"));
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            foreach (var item in quilt.libraries)
            {
                var name = PathHelper.ToPathAndName(item.name);
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(name.Name),
                    Path.GetFullPath($"{LibrariesPath.BaseDir}/{name.Path}"));
            }
        }

        //游戏核心
        var list3 = new List<string>(list.Values);
        if (obj.Loader != Loaders.NeoForge)
        {
            list3.Add(LibrariesPath.GetGameFile(obj.Version));
        }

        return list3;
    }

    private static async Task<string> MakeClassPath(GameSettingObj obj, bool v2)
    {
        var libraries = await GetLibs(obj, v2);
        StringBuilder classpath = new();
        string sep = SystemInfo.Os == OsType.Windows ? ";" : ":";
        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.Get("Core.Launch.Info2"));

        //去除重复的classpath
        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.ClassPath))
        {
            var list = obj.AdvanceJvm.ClassPath.Split(";");
            var dir1 = obj.GetGamePath();
            var dir2 = obj.GetBasePath();
            foreach (var item1 in list)
            {
                var path = Path.GetFullPath(item1
                    .Replace(GAME_DIR, dir1)
                    .Replace(GAME_BASE_DIR, dir2));
                if (File.Exists(path))
                {
                    libraries.Add(item1);
                }
            }
        }

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            classpath.Append($"{item}{sep}");
            ColorMCCore.GameLog?.Invoke(obj, $"    {item}");
        }
        classpath.Remove(classpath.Length - 1, 1);

        return classpath.ToString().Trim();
    }

    /// <summary>
    /// 替换参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="args">所有参数参数</param>
    /// <param name="v2">V2模式</param>
    private static void ReplaceAll(GameSettingObj obj, LoginObj login, List<string> args, string classpath)
    {
        var version = VersionPath.GetGame(obj.Version)!;
        var assetsPath = AssetsPath.BaseDir;
        var gameDir = InstancesPath.GetGamePath(obj);
        var assetsIndexName = version.assets != null ? version.assets : "legacy";

        var version_name = obj.Loader switch
        {
            Loaders.Forge => $"forge-{obj.Version}-{obj.LoaderVersion}",
            Loaders.Fabric => $"fabric-{obj.Version}-{obj.LoaderVersion}",
            Loaders.Quilt => $"quilt-{obj.Version}-{obj.LoaderVersion}",
            _ => obj.Version
        };

        var sep = SystemInfo.Os == OsType.Windows ? ";" : ":";

        var argDic = new Dictionary<string, string>()
        {
            {"${auth_player_name}", login.UserName },
            {"${version_name}",version_name },
            {"${game_directory}",gameDir },
            {"${assets_root}",assetsPath },
            {"${assets_index_name}",assetsIndexName },
            {"${auth_uuid}",login.UUID },
            {"${auth_access_token}",string.IsNullOrWhiteSpace(login.AccessToken) ? "0" : login.AccessToken },
            {"${game_assets}",assetsPath },
            {"${user_properties}", "{}" },
            {"${user_type}", login.AuthType == AuthType.OAuth ? "msa" : "mojang" },
            {"${version_type}", "release" },
            {"${natives_directory}", SystemInfo.Os == OsType.Android
                ? "%natives_directory%" : LibrariesPath.GetNativeDir(obj.Version) },
            {"${library_directory}",LibrariesPath.BaseDir },
            {"${classpath_separator}", sep },
            {"${launcher_name}","ColorMC" },
            {"${launcher_version}", ColorMCCore.Version  },
            {"${classpath}", classpath },
        };

        for (int a = 0; a < args.Count; a++)
        {
            args[a] = argDic.Aggregate(args[a], (a, b) => a.Replace(b.Key, b.Value));
        }
    }

    /// <summary>
    /// 创建所有启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <returns></returns>
    private static async Task<List<string>> MakeArg(GameSettingObj obj, LoginObj login, WorldObj? world)
    {
        var list = new List<string>();
        var version = VersionPath.GetGame(obj.Version)!;
        var v2 = CheckHelpers.GameLaunchVersionV2(version);
        var classpath = await MakeClassPath(obj, v2);
        var jvmarg = await JvmArg(obj, v2, login);
        var gamearg = GameArg(obj, v2, world);
        ReplaceAll(obj, login, jvmarg, classpath);
        ReplaceAll(obj, login, gamearg, classpath);
        //jvm
        list.AddRange(jvmarg);

        //mainclass
        if (string.IsNullOrWhiteSpace(obj.AdvanceJvm?.MainClass))
        {
            if (obj.Loader == Loaders.Normal)
            {
                list.Add(version.mainClass);
            }
            //forgewrapper
            else if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
            {
                if (v2)
                {
                    list.Add("io.github.zekerzhayard.forgewrapper.installer.Main");
                }
                else
                {
                    var forge = obj.Loader == Loaders.NeoForge ? obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
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
        //gamearg
        list.AddRange(gamearg);

        return list;
    }

    /// <summary>
    /// 保持splash不开启
    /// </summary>
    /// <param name="obj">游戏实例</param>
    private static void ConfigSet(GameSettingObj obj)
    {
        var dir = obj.GetConfigPath();
        Directory.CreateDirectory(dir);
        var file = dir + "splash.properties";
        string data = PathHelper.ReadText(file) ?? "enabled=true";
        if (data.Contains("enabled=true"))
        {
            PathHelper.WriteText(file, data.Replace("enabled=true", "enabled=false"));
        }
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
    public static async Task<Process?> StartGame(this GameSettingObj obj, LoginObj login, WorldObj? world, CancellationToken token)
    {
        s_cancel = token;
        var stopwatch = new Stopwatch();

        //版本号检测
        if (string.IsNullOrWhiteSpace(obj.Version) ||
            (obj.Loader != Loaders.Normal && string.IsNullOrWhiteSpace(obj.LoaderVersion)))
        {
            throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error7"));
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
                    throw new LaunchException(LaunchState.LoginFail, Message!, Ex);

                throw new LaunchException(LaunchState.LoginFail, Message!);
            }
        }
        else
        {
            login = Obj!;
            login.Save();
        }

        if (s_cancel.IsCancellationRequested)
        {
            return null;
        }

        stopwatch.Stop();
        string temp = string.Format(LanguageHelper.Get("Core.Launch.Info4"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.GameLog?.Invoke(obj, temp);
        Logs.Info(temp);

        //检查游戏文件
        stopwatch.Restart();
        stopwatch.Start();
        var res = await CheckHelpers.CheckGameFile(obj, login, s_cancel);
        stopwatch.Stop();
        temp = string.Format(LanguageHelper.Get("Core.Launch.Info5"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.GameLog?.Invoke(obj, temp);
        Logs.Info(temp);

        if (ColorMCCore.GameRequest != null && obj.GetModeFast() && obj.Loader == Loaders.Normal)
        {
            var res1 = await ColorMCCore.GameRequest.Invoke(LanguageHelper.Get("Core.Launch.Info13"));
            if (!res1)
            {
                throw new LaunchException(LaunchState.Cancel,
                        LanguageHelper.Get("Core.Launch.Error8"));
            }
        }

        //下载缺失的文件
        if (!res.IsEmpty)
        {
            bool download = true;
            if (!ConfigUtils.Config.Http.AutoDownload)
            {
                if (ColorMCCore.GameRequest == null)
                {
                    throw new LaunchException(LaunchState.LostGame,
                        LanguageHelper.Get("Core.Launch.Error4"));
                }

                download = await ColorMCCore.GameRequest.Invoke(LanguageHelper.Get("Core.Launch.Info12"));
            }

            if (download)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.Download);

                stopwatch.Restart();
                stopwatch.Start();

                var ok = await DownloadManager.Start(res.ToList());
                if (!ok)
                {
                    throw new LaunchException(LaunchState.LostFile,
                        LanguageHelper.Get("Core.Launch.Error5"));
                }
                stopwatch.Stop();
                temp = string.Format(LanguageHelper.Get("Core.Launch.Info7"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.GameLog?.Invoke(obj, temp);
                Logs.Info(temp);
            }
        }

        //查找合适的JAVA
        stopwatch.Restart();
        stopwatch.Start();
        var path = obj.JvmLocal;
        JavaInfo? jvm = null;
        var game = VersionPath.GetGame(obj.Version)!;
        if (string.IsNullOrWhiteSpace(path))
        {
            var jv = game.javaVersion.majorVersion;
            jvm = JvmPath.GetInfo(obj.JvmName) ?? JvmPath.FindJava(jv);
            if (jvm == null)
            {
                ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.JavaError);
                ColorMCCore.NoJava?.Invoke(jv);
                throw new LaunchException(LaunchState.JavaError,
                        string.Format(LanguageHelper.Get("Core.Launch.Error6"), jv));
            }

            path = jvm.GetPath();
        }
        if (s_cancel.IsCancellationRequested)
        {
            return null;
        }

        //准备Jvm参数
        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.JvmPrepare);
        var arg = await MakeArg(obj, login, world);
        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.Get("Core.Launch.Info1"));
        bool hidenext = false;
        if (SystemInfo.Os != OsType.Android)
        {
            foreach (var item in arg)
            {
                if (hidenext)
                {
                    hidenext = false;
                    ColorMCCore.GameLog?.Invoke(obj, "******");
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
        }

        ColorMCCore.GameLog?.Invoke(obj, LanguageHelper.Get("Core.Launch.Info3"));
        ColorMCCore.GameLog?.Invoke(obj, path);

        if (s_cancel.IsCancellationRequested)
        {
            return null;
        }

        //启动前运行
        if (ColorMCCore.LaunchP != null && (obj.JvmArg?.LaunchPre == true
            || ConfigUtils.Config.DefaultJvmArg.LaunchPre))
        {
            var cmd1 = obj.JvmArg?.LaunchPreData;
            var cmd2 = ConfigUtils.Config.DefaultJvmArg.LaunchPreData;
            var start = string.IsNullOrWhiteSpace(cmd1) ? cmd2 : cmd1;
            if (!string.IsNullOrWhiteSpace(start) && await ColorMCCore.LaunchP.Invoke(true))
            {
                if (SystemInfo.Os == OsType.Android && start.StartsWith(JAVA_LOCAL))
                {
                    if (JvmPath.FindJava(8) is { } jvm1)
                    {
                        stopwatch.Start();
                        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LaunchPre);
                        start = ReplaceArg(obj, path!, arg, start);

                        var args = start.Split('\n');
                        var file = args[0].Trim();
                        if (file.StartsWith("./") || file.StartsWith("../"))
                        {
                            file = Path.GetFullPath(obj.GetBasePath() + "/" + file);
                        }
                        var arglist = new List<string>();
                        for (int a = 1; a < args.Length; a++)
                        {
                            arglist.Add(args[a].Trim());
                        }

                        await ColorMCCore.PhoneJvmRun.Invoke(obj, jvm1, obj.GetGamePath(), arglist);

                        stopwatch.Stop();
                        string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                            obj.Name, stopwatch.Elapsed.ToString());
                        ColorMCCore.GameLog?.Invoke(obj, temp1);
                        Logs.Info(temp1);
                    }
                }
                else
                {
                    stopwatch.Start();
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LaunchPre);
                    start = ReplaceArg(obj, path!, arg, start);
                    CmdRun(obj, start);
                    stopwatch.Stop();
                    string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                        obj.Name, stopwatch.Elapsed.ToString());
                    ColorMCCore.GameLog?.Invoke(obj, temp1);
                    Logs.Info(temp1);
                }
            }
            else
            {
                string temp2 = string.Format(LanguageHelper.Get("Core.Launch.Info10"),
                obj.Name);
                ColorMCCore.GameLog?.Invoke(obj, temp2);
                Logs.Info(temp2);
            }
        }

        if (s_cancel.IsCancellationRequested)
        {
            return null;
        }

        if (SystemInfo.Os == OsType.Android)
        {
            ConfigSet(obj);

            //安装Forge
            var version = VersionPath.GetGame(obj.Version)!;
            var v2 = CheckHelpers.GameLaunchVersionV2(version);
            if (v2 && obj.Loader is Loaders.Forge or Loaders.NeoForge)
            {
                var obj1 = obj.Loader is Loaders.Forge
                    ? VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!)!
                    : VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!)!;
                var install = await CheckHelpers.CheckForgeInstall(obj1, obj.LoaderVersion!, obj.Loader is Loaders.NeoForge);
                if (install)
                {
                    ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.InstallForge);
                    var jvm1 = JvmPath.FindJava(8) ?? throw new LaunchException(LaunchState.JavaError,
                            LanguageHelper.Get("Core.Launch.Error9"));
                    var res1 = await ColorMCCore.PhoneJvmRun.Invoke(obj, jvm1, obj.GetGamePath(), obj.MakeInstallForgeArg());
                    if (res1 != true)
                    {
                        throw new LaunchException(LaunchState.JavaError,
                            LanguageHelper.Get("Core.Launch.Error10"));
                    }
                }
            }

            ColorMCCore.PhoneGameLaunch?.Invoke(obj, jvm!, arg);
            return null;
        }

        //启动进程
        Process process = new()
        {
            EnableRaisingEvents = true
        };
        process.StartInfo.FileName = path;
        process.StartInfo.WorkingDirectory = obj.GetGamePath();
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
        temp = string.Format(LanguageHelper.Get("Core.Launch.Info6"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.GameLog?.Invoke(obj, temp);
        Logs.Info(temp);

        //启动后执行
        if (ColorMCCore.LaunchP != null && (obj.JvmArg?.LaunchPost == true
            || ConfigUtils.Config.DefaultJvmArg.LaunchPost))
        {
            var start = obj.JvmArg?.LaunchPostData;
            if (string.IsNullOrWhiteSpace(start))
            {
                start = ConfigUtils.Config.DefaultJvmArg.LaunchPostData;
            }
            if (!string.IsNullOrWhiteSpace(start))
            {
                var res1 = await ColorMCCore.LaunchP.Invoke(false);
                if (res1)
                {
                    if (SystemInfo.Os == OsType.Android)
                    { }
                    else
                    {
                        stopwatch.Start();
                        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.LaunchPost);
                        start = ReplaceArg(obj, path!, arg, start);
                        CmdRun(obj, start);
                        stopwatch.Stop();
                        string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info9"),
                            obj.Name, stopwatch.Elapsed.ToString());
                        ColorMCCore.GameLog?.Invoke(obj, temp1);
                        Logs.Info(temp1);
                    }
                }
            }
            else
            {
                string temp2 = string.Format(LanguageHelper.Get("Core.Launch.Info11"),
                obj.Name);
                ColorMCCore.GameLog?.Invoke(obj, temp2);
                Logs.Info(temp2);
            }
        }

        return process;
    }

    public delegate int DLaunch(int argc,
        string[] argv, /* main argc, argc */
        int jargc, string[] jargv,          /* java args */
        int appclassc, string[] appclassv,  /* app classpath */
        string fullversion,                 /* full version defined */
        string dotversion,                  /* dot version defined */
        string pname,                       /* program name */
        string lname,                       /* launcher name */
        bool javaargs,                      /* JAVA_ARGS */
        bool cpwildcard,                    /* classpath wildcard*/
        bool javaw,                         /* windows-only javaw */
        int ergo                            /* ergonomics class policy */
    );

    private static int s_argLength;
    private static string[] s_args;

    /// <summary>
    /// native启动
    /// </summary>
    /// <param name="info">Java信息</param>
    /// <param name="args">启动参数</param>
    public static int NativeLaunch(JavaInfo info, List<string> args)
    {
        string path;
        if (SystemInfo.Os == OsType.Android)
        {
            path = info.Path;
        }
        else
        {
            var info1 = new FileInfo(info.Path);
            path = info1.Directory?.Parent?.FullName!;
        }

        var local = SystemInfo.Os switch
        {
            OsType.Windows => PathHelper.GetFile(path, "jli.dll"),
            OsType.Linux => PathHelper.GetFile(path, "libjli.so"),
            OsType.MacOS => PathHelper.GetFile(path, "libjli.dylib"),
            OsType.Android => PathHelper.GetFile(path, "libjli.so"),
            _ => throw new NotImplementedException(),
        };
        if (File.Exists(local))
        {
            local = Path.GetFullPath(local);
        }

        //加载运行库
        var temp = NativeLoader.Loader.LoadLibrary(local);
        var temp1 = NativeLoader.Loader.GetProcAddress(temp, "JLI_Launch", false);
        var inv = Marshal.GetDelegateForFunctionPointer<DLaunch>(temp1);

        var args1 = new string[args.Count + 1];
        args1[0] = "java";

        for (int i = 1; i < args1.Length; i++)
        {
            args1[i] = args[i - 1];
        }

        s_argLength = args1.Length;
        s_args = args1;

        //启动游戏
        try
        {
            var res = inv(s_argLength, s_args, 0, null!, 0, null!,
                info.Version, "1.8", "java", "java", false, true, false, 0);

            var res1 = NativeLoader.Loader.CloseLibrary(temp);

            return res;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("Error", e, false);
        }

        return -1;
    }
}
