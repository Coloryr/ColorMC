using System.Diagnostics;
using System.Text;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;

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
    /// V1版Jvm参数
    /// </summary>
    private static readonly string[] s_v1JvmArg =
    [
        "-Djava.library.path=${natives_directory}", "-cp", "${classpath}"
    ];
#if Phone
    /// <summary>
    /// 获取Forge安装参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">是否为1.13以上版本</param>
    /// <returns>安装参数</returns>
    private static List<string> MakeInstallForgeArg(this GameSettingObj obj, bool v2)
    {
        var jvm = new List<string>
        {
            $"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}",
            $"-Dforgewrapper.installer={(obj.Loader == Loaders.NeoForge ?
            DownloadItemHelper.BuildNeoForgeInstaller(obj.Version, obj.LoaderVersion!).Local :
            DownloadItemHelper.BuildForgeInstaller(obj.Version, obj.LoaderVersion!).Local)}",
            $"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}",
            "-Dforgewrapper.justInstall=true"
        };

        var list = new Dictionary<LibVersionObj, string>();

        var forge = obj.Loader == Loaders.NeoForge
                ? VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!)!
                : VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!)!;
        var list2 = DownloadItemHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!,
            obj.Loader == Loaders.NeoForge, v2);
        foreach (var item in list2)
        {
            list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), item.Local);
        }

        GameHelper.ReadyForgeWrapper();
        list.AddOrUpdate(new(), GameHelper.ForgeWrapper);

        var libraries = new List<string>(list.Values);
        var classpath = new StringBuilder();
        var sep = SystemInfo.Os == OsType.Windows ? ';' : ':';
        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info2"));

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            if (File.Exists(item))
            {
                classpath.Append($"{item}{sep}");
            }
        }

        var cp = classpath.ToString()[..^1].Trim();
        jvm.Add("-cp");
        jvm.Add(cp);

        //var arg = MakeV2GameArg(obj);

        jvm.Add("io.github.zekerzhayard.forgewrapper.installer.Main");
        var forge1 = obj.Loader == Loaders.NeoForge
                ? obj.GetNeoForgeObj()!
                : obj.GetForgeObj()!;

        jvm.AddRange(forge1.Arguments.Game);

        return jvm;
    }

    private static int PhoneCmdRun(this GameSettingObj obj, string start, JavaInfo jvm1, Dictionary<string, string> env)
    {
        var args = start.Split('\n');
        var arglist = new List<string>();
        for (int a = 1; a < args.Length; a++)
        {
            arglist.Add(args[a].Trim());
        }

        return PhoneCmdRun(obj, arglist, jvm1, env);
    }

    private static int PhoneCmdRun(this GameSettingObj obj, List<string> arglist, JavaInfo jvm1, Dictionary<string, string> env)
    {
        var res2 = ColorMCCore.PhoneJvmRun(obj, jvm1, obj.GetGamePath(), arglist, env);
        res2.StartInfo.RedirectStandardError = true;
        res2.StartInfo.RedirectStandardInput = true;
        res2.StartInfo.RedirectStandardOutput = true;
        res2.OutputDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        res2.ErrorDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        res2.Start();
        res2.BeginOutputReadLine();
        res2.BeginErrorReadLine();

        res2.WaitForExit();

        return res2.ExitCode;
    }
#endif

    /// <summary>
    /// 替换参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="jvm">JAVA位置</param>
    /// <param name="arg">JVM参数</param>
    /// <param name="item">命令</param>
    /// <returns>参数</returns>
    private static string ReplaceArg(this GameSettingObj obj, string jvm, List<string> arg, string item)
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
    /// <param name="env">运行环境</param>
    /// <param name="runsame">是否不等待结束</param>
    /// <param name="admin">管理员启动</param>
    private static void CmdRun(this GameSettingObj obj, string cmd, Dictionary<string, string> env, bool runsame, bool admin)
    {
        var args = cmd.Split('\n');
        var name = args[0].Trim();

        if (!File.Exists(name))
        {
            name = Path.Combine(obj.GetBasePath(), name);
        }
        if (!File.Exists(name))
        {
            ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Error14"), name));
            return;
        }

        var info = new ProcessStartInfo(name)
        {
            WorkingDirectory = obj.GetGamePath(),
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        foreach (var item in env)
        {
            info.Environment.Add(item.Key, item.Value);
        }
        for (int a = 1; a < args.Length; a++)
        {
            info.ArgumentList.Add(args[a].Trim());
        }
        var p = new Process()
        {
            EnableRaisingEvents = true,
            StartInfo = info
        };
        p.OutputDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        p.ErrorDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };

        //是否与游戏同时启动
        if (runsame)
        {
            p.Exited += (a, b) =>
            {
                p.Dispose();
            };
        }

        ProcessUtils.Launch(p, admin);
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        if (!runsame)
        {
            p.WaitForExit();
            p.Dispose();
        }
    }

    /// <summary>
    /// 创建V2版Jvm参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>参数</returns>
    private static List<string> MakeV2JvmArg(this GameSettingObj obj)
    {
        var game = VersionPath.GetVersion(obj.Version)!;
        if (game.Arguments == null)
        {
            return [.. s_v1JvmArg];
        }

        var arg = new List<string>();
        //添加原版参数
        foreach (object item in game.Arguments.jvm)
        {
            if (item is string str)
            {
#if Phone
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
#endif
                arg.Add(str);
            }
            else if (item is JObject obj1)
            {
                var obj2 = obj1.ToObject<GameArgObj.ArgumentsObj.Jvm>();
                if (obj2 == null)
                {
                    continue;
                }
                //检查是否需要使用
                if (!CheckHelpers.CheckAllow(obj2.rules))
                {
                    continue;
                }

                if (obj2.value is string item2)
                {
                    arg.Add(item2!);
                }
                else if (obj2.value is JArray list)
                {
                    var list1 = list.ToObject<List<string>>()!;
                    arg.AddRange(list1);
                }
            }
        }

        //添加Mod加载器的参数
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
            if (forge.Arguments.Jvm != null)
            {
                foreach (var item in forge.Arguments.Jvm)
                {
                    arg.Add(item);
                }
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.Arguments.Jvm)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Custom)
        {
            arg.AddRange(obj.GetLoaderGameArg());
        }

        return arg;
    }

    /// <summary>
    /// 创建V1版游戏参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>启动参数</returns>
    private static List<string> MakeV1GameArg(this GameSettingObj obj)
    {
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
            return [.. forge.MinecraftArguments.Split(' ')];
        }
        //使用OptiFine启动方式
        else if (obj.Loader == Loaders.OptiFine)
        {
            var version = VersionPath.GetVersion(obj.Version)!;
            return
            [
                .. version.MinecraftArguments.Split(' '),
                "--tweakClass",
                "optifine.OptiFineTweaker"
            ];
        }
        //使用自定义加载器
        else if (obj.Loader == Loaders.Custom)
        {
            return obj.GetLoaderGameArg();
        }
        else
        {
            var version = VersionPath.GetVersion(obj.Version)!;
            return [.. version.MinecraftArguments.Split(' ')];
        }
    }

    /// <summary>
    /// 创建V2版游戏参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    private static List<string> MakeV2GameArg(this GameSettingObj obj)
    {
        var game = VersionPath.GetVersion(obj.Version)!;
        if (game.Arguments == null)
        {
            return obj.MakeV1GameArg();
        }

        var arg = new List<string>();
        foreach (object item in game.Arguments.game)
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
            var forge = obj.Loader == Loaders.NeoForge
                ? obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
            foreach (var item in forge.Arguments.Game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            foreach (var item in fabric.Arguments.Game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            foreach (var item in quilt.Arguments.Game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.OptiFine)
        {
            arg.Add("--tweakClass");
            arg.Add("optifine.OptiFineTweaker");
        }
        else if (obj.Loader == Loaders.Custom)
        {
            arg.AddRange(obj.GetLoaderGameArg());
        }

        return arg;
    }

    /// <summary>
    /// 创建Jvm参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="jvm1">选择的Java</param>
    /// <param name="mixinport">注入通信端口</param>
    /// <returns>Jvm参数</returns>
    private static async Task<List<string>> MakeJvmArgAsync(this GameSettingObj obj,
        LoginObj login, JavaInfo jvm1, int? mixinport = null)
    {
        RunArgObj args;

        var v2 = obj.IsGameVersionV2();

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
                    ?? ConfigUtils.Config.DefaultJvmArg.MinMemory,
                ColorASM = obj.JvmArg != null ? obj.JvmArg.ColorASM
                    : ConfigUtils.Config.DefaultJvmArg.ColorASM
            };
        }

        var jvm = new List<string>();

        //javaagent
        if (!string.IsNullOrWhiteSpace(args.JavaAgent))
        {
            jvm.Add($"-javaagent:{args.JavaAgent.Trim()}");
        }

        if (args.ColorASM == false && mixinport > 0 && jvm1.MajorVersion >= 8)
        {
            GameHelper.ReadyColorMCASM();
            jvm.Add("-Dcolormc.mixin.port=" + mixinport);
            jvm.Add("-Dcolormc.mixin.uuid=" + obj.UUID);
            jvm.Add($"-javaagent:{GameHelper.ColorMCASM}");
        }

        //gc
        switch (args.GC)
        {
            case GCType.G1GC:
                jvm.Add("-XX:+UseG1GC");
                break;
            case GCType.SerialGC:
                jvm.Add("-XX:+UseSerialGC");
                break;
            case GCType.ParallelGC:
                jvm.Add("-XX:+UseParallelGC");
                break;
            case GCType.CMSGC:
                jvm.Add("-XX:+UseConcMarkSweepGC");
                break;
            case GCType.User:
                if (!string.IsNullOrWhiteSpace(args.GCArgument))
                    jvm.Add(args.GCArgument.Trim());
                break;
            default:
                break;
        }

        //mem
        if (args.MinMemory is { } min && min != 0)
        {
            jvm.Add($"-Xms{min}m");
        }
        if (args.MaxMemory is { } max && max != 0)
        {
            jvm.Add($"-Xmx{max}m");
        }
        if (!string.IsNullOrWhiteSpace(args.JvmArgs))
        {
            foreach (var item in args.JvmArgs.Split("\n"))
            {
                jvm.Add(item.Trim());
            }
        }

        if (obj.JvmArg?.RemoveJvmArg != true)
        {
            //loader
            if (v2 && obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
            {
                jvm.Add($"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}");
                jvm.Add($"-Dforgewrapper.installer={(obj.Loader == Loaders.NeoForge ?
                    DownloadItemHelper.BuildNeoForgeInstaller(obj.Version, obj.LoaderVersion!).Local :
                    DownloadItemHelper.BuildForgeInstaller(obj.Version, obj.LoaderVersion!).Local)}");
                jvm.Add($"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}");
#if Phone
                if (SystemInfo.Os == OsType.Android)
                {
                    jvm.Add("-Dforgewrapper.justLaunch=true");
                }
#endif
            }
            else if (obj.Loader == Loaders.OptiFine)
            {
                jvm.Add($"-Dlibdir={LibrariesPath.BaseDir}");
                jvm.Add($"-Dgamecore={LibrariesPath.GetGameFile(obj.Version)}");
                jvm.Add($"-Doptifine={LibrariesPath.GetOptiFineLib(obj)}");
                if (v2)
                {
                    jvm.Add("--add-opens");
                    jvm.Add("java.base/java.lang=ALL-UNNAMED");
                    jvm.Add("--add-opens");
                    jvm.Add("java.base/java.util=ALL-UNNAMED");
                    jvm.Add("--add-opens");
                    jvm.Add("java.base/java.net=ALL-UNNAMED");
                    jvm.Add("--add-opens");
                    jvm.Add("java.base/jdk.internal.loader=ALL-UNNAMED");
                }
            }

            //jvmHead.Add("-Djava.rmi.server.useCodebaseOnly=true");
            //jvmHead.Add("-XX:+UnlockExperimentalVMOptions");
            //jvm.Add("-Dfml.ignoreInvalidMinecraftCertificates=true");
            //jvm.Add("-Dfml.ignorePatchDiscrepancies=true");
            //jvm.Add("-Dlog4j2.formatMsgNoLookups=true");
            //jvmHead.Add("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false");
            //jvmHead.Add("-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false");
            //jvmHead.Add($"-Dminecraft.client.jar={VersionPath.BaseDir}/{obj.Version}.jar");

            jvm.AddRange(v2 ? obj.MakeV2JvmArg() : s_v1JvmArg);
        }

        //外置登陆器相关
        if (login.AuthType == AuthType.Nide8)
        {
            jvm.Add($"-javaagent:{AuthlibHelper.NowNide8Injector}={login.Text1}");
            jvm.Add("-Dnide8auth.client=true");
        }
        else if (login.AuthType == AuthType.AuthlibInjector)
        {
            var res = await CoreHttpClient.GetStringAsync(login.Text1);
            if (!res.State)
            {
                throw new LaunchException(LaunchState.LoginCoreError, LanguageHelper.Get("Core.Launch.Error12"));
            }
            jvm.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}");
            jvm.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Message!)}");
            jvm.Add("-Dauthlibinjector.side=client");
        }
        else if (login.AuthType == AuthType.LittleSkin)
        {
            var res = await CoreHttpClient.GetStringAsync($"{UrlHelper.LittleSkin}api/yggdrasil");
            if (!res.State)
            {
                throw new LaunchException(LaunchState.LoginCoreError, LanguageHelper.Get("Core.Launch.Error12"));
            }
            jvm.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={UrlHelper.LittleSkin}api/yggdrasil");
            jvm.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Message!)}");
            jvm.Add("-Dauthlibinjector.side=client");
        }
        else if (login.AuthType == AuthType.SelfLittleSkin)
        {
            var res = await CoreHttpClient.GetStringAsync($"{login.Text1}api/yggdrasil");
            if (!res.State)
            {
                throw new LaunchException(LaunchState.LoginCoreError, LanguageHelper.Get("Core.Launch.Error12"));
            }
            jvm.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}/api/yggdrasil");
            jvm.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Message!)}");
            jvm.Add("-Dauthlibinjector.side=client");
        }

        //log4j2-xml
        var game = VersionPath.GetVersion(obj.Version)!;
        if (game.Logging != null && ConfigUtils.Config.SafeLog4j)
        {
            var obj1 = DownloadItemHelper.BuildLog4jItem(game);
            jvm.Add(game.Logging.Client.Argument.Replace("${path}", obj1.Local));
        }

        jvm.Add($"-Dcolormc.dir={ColorMCCore.BaseDir}");
        jvm.Add($"-Dcolormc.game.uuid={obj.UUID}");
        jvm.Add($"-Dcolormc.game.name={obj.Name}");
        jvm.Add($"-Dcolormc.game.version={obj.Version}");
        jvm.Add($"-Dcolormc.game.dir={obj.GetGamePath()}");

        return jvm;
    }

    /// <summary>
    /// 创建游戏启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="world">启动的世界</param>
    /// <param name="server">加入的服务器</param>
    /// <returns>游戏启动参数</returns>
    private static List<string> MakeGameArg(this GameSettingObj obj, WorldObj? world, ServerObj? server)
    {
        var gameArg = new List<string>();
        if (obj.JvmArg?.RemoveGameArg != true)
        {
            var v2 = obj.IsGameVersionV2();
            gameArg.AddRange(v2 ? obj.MakeV2GameArg() : obj.MakeV1GameArg());
        }
#if !Phone
        //设置游戏窗口大小
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
#endif

        //--quickPlayMultiplayer
        //快速加入
        if (world != null)
        {
            gameArg.Add($"--quickPlaySingleplayer");
            gameArg.Add($"{world.LevelName}");
        }
        else
        {
            if (obj.StartServer != null)
            {
                server = obj.StartServer;
            }
            if (server != null && !string.IsNullOrWhiteSpace(server.IP) && server.Port != null)
            {
                if (CheckHelpers.IsGameVersion120(obj.Version))
                {
                    gameArg.Add($"--quickPlayMultiplayer");
                    if (server.Port > 0)
                    {
                        gameArg.Add($"{server.IP}:{server.Port}");
                    }
                    else
                    {
                        gameArg.Add($"{server.IP}:25565");
                    }
                }
                else
                {
                    gameArg.Add($"--server");
                    gameArg.Add(server.IP);
                    if (server.Port > 0)
                    {
                        gameArg.Add($"--port");
                        gameArg.Add(server.Port.ToString()!);
                    }
                    else
                    {
                        gameArg.Add($"--port");
                        gameArg.Add("25565");
                    }
                }
            }
        }

        //游戏代理
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

        //自定义游戏参数
        if (!string.IsNullOrWhiteSpace(obj.JvmArg?.GameArgs))
        {
            gameArg.AddRange(obj.JvmArg.GameArgs.Split("\n"));
        }

        return gameArg;
    }

    /// <summary>
    /// 创建Classpath
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="enableasm">是否添加注入</param>
    /// <returns>classpath</returns>
    private static async Task<string> MakeClassPathAsync(this GameSettingObj obj, bool enableasm, bool checklocal)
    {
        var libraries = await obj.GetLibsAsync();
        var classpath = new StringBuilder();
        var sep = SystemInfo.Os == OsType.Windows ? ';' : ':';
        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info2"));

        if (enableasm)
        {
            libraries.Add(GameHelper.ColorMCASM);
        }

        //附加的classpath
        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.ClassPath))
        {
            var list = obj.AdvanceJvm.ClassPath.Split(';');
            var dir1 = obj.GetGamePath();
            var dir2 = obj.GetBasePath();
            foreach (var item1 in list)
            {
                var path = item1
                    .Replace(GAME_DIR, dir1)
                    .Replace(GAME_BASE_DIR, dir2)
                    .Trim();
                path = Path.GetFullPath(path);
                if (!checklocal || File.Exists(path))
                {
                    libraries.Add(path);
                }
            }
        }

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            if (!checklocal || File.Exists(item))
            {
                classpath.Append($"{item}{sep}");
                ColorMCCore.OnGameLog(obj, $"    {item}");
            }
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
    /// <param name="classpath">classpath</param>
    private static void ReplaceAll(this GameSettingObj obj, LoginObj login, List<string> args, string classpath)
    {
        var version = VersionPath.GetVersion(obj.Version)!;
        var assetsPath = AssetsPath.BaseDir;
        var gameDir = InstancesPath.GetGamePath(obj);
        var assetsIndexName = version.Assets ?? "legacy";

        var version_name = obj.Loader switch
        {
            Loaders.Forge => $"forge-{obj.Version}-{obj.LoaderVersion}",
            Loaders.Fabric => $"fabric-{obj.Version}-{obj.LoaderVersion}",
            Loaders.Quilt => $"quilt-{obj.Version}-{obj.LoaderVersion}",
            Loaders.NeoForge => $"neoforge-{obj.Version}-{obj.LoaderVersion}",
            _ => obj.Version
        };

        var sep = SystemInfo.Os == OsType.Windows ? ";" : ":";

        var argDic = new Dictionary<string, string>()
        {
            {"${auth_player_name}", login.UserName },
            {"${version_name}", version_name },
            {"${game_directory}", gameDir },
            {"${assets_root}", assetsPath },
            {"${assets_index_name}", assetsIndexName },
            {"${auth_uuid}", login.UUID },
            {"${auth_access_token}", string.IsNullOrWhiteSpace(login.AccessToken) ? "0" : login.AccessToken },
            {"${game_assets}", assetsPath },
            {"${user_properties}", "{}" },
            {"${user_type}", login.AuthType == AuthType.OAuth ? "msa" : "mojang" },
            {"${version_type}", "release" },
#if Phone
            {"${natives_directory}", SystemInfo.Os == OsType.Android
                ? "%natives_directory%" : LibrariesPath.GetNativeDir(obj.Version) },
#else
            {"${natives_directory}", LibrariesPath.GetNativeDir(obj.Version) },
#endif
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
    /// 创建主类
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="version">游戏数据</param>
    /// <param name="v2">是否为v2版本</param>
    /// <returns>主类</returns>
    public static string MakeMainClass(this GameSettingObj obj)
    {
        var version = VersionPath.GetVersion(obj.Version)
            ?? throw new Exception(string.Format(LanguageHelper.Get("Core.Check.Error1"), obj.Version));
        var v2 = version.IsGameVersionV2();
        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.MainClass))
        {
            return obj.AdvanceJvm.MainClass;
        }
        else if (obj.Loader == Loaders.Normal)
        {
            return version.MainClass;
        }
        //forgewrapper
        else if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            if (v2)
            {
                return "io.github.zekerzhayard.forgewrapper.installer.Main";
            }
            else
            {
                var forge = obj.Loader == Loaders.NeoForge ? obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
                return forge.MainClass;
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            return fabric.MainClass;
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            return quilt.MainClass;
        }
        //optifinewrapper
        else if (obj.Loader == Loaders.OptiFine)
        {
            return "com.coloryr.optifinewrapper.OptifineWrapper";
        }
        else if (obj.Loader == Loaders.Custom)
        {
            return obj.GetLoaderMainClass();
        }

        return version.MainClass;
    }

    /// <summary>
    /// 创建所有启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="arg">游戏启动参数</param>
    /// <returns>参数列表</returns>
    private static async Task<List<string>> MakeArgAsync(this GameSettingObj obj, GameMakeArg arg)
    {
        var list = new List<string>();
        var jvmarg = await obj.MakeJvmArgAsync(arg.Login, arg.Jvm, arg.Mixinport);
        var classpath = await obj.MakeClassPathAsync(arg.Mixinport > 0, arg.CheckLocal);
        var gamearg = obj.MakeGameArg(arg.World, arg.Server);
        obj.ReplaceAll(arg.Login, jvmarg, classpath);
        obj.ReplaceAll(arg.Login, gamearg, classpath);
        //jvm
        list.AddRange(jvmarg);

        //mainclass
        list.Add(obj.MakeMainClass());

        //gamearg
        list.AddRange(gamearg);

        return list;
    }

    /// <summary>
    /// 同时启动多个实例
    /// </summary>
    /// <param name="objs">启动的游戏列表</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token">取消Token</param>
    /// <returns>启动结果</returns>
    public static async Task<Dictionary<GameSettingObj, GameLaunchRes>>
        StartGameAsync(this ICollection<GameSettingObj> objs, GameLaunchArg larg, CancellationToken token)
    {
        var stopwatch = new Stopwatch();

        var list = new Dictionary<GameSettingObj, GameLaunchRes>();
        var list2 = new List<(GameSettingObj, string, List<string>, Dictionary<string, string>)>();
        //登录账户
        stopwatch.Restart();
        stopwatch.Start();
        larg.Update2?.Invoke(objs.First(), LaunchState.Login);
        var temp1 = objs.First();
        try
        {
            //刷新账户token
            var res1 = await larg.Auth.RefreshTokenAsync();
            if (res1.LoginState != LoginState.Done)
            {
                if (larg.Auth.AuthType == AuthType.OAuth
                    && !string.IsNullOrWhiteSpace(larg.Auth.UUID)
                    && larg.Loginfail != null
                    && await larg.Loginfail(larg.Auth) == true)
                {
                    larg.Auth = new()
                    {
                        UserName = larg.Auth.UserName,
                        UUID = larg.Auth.UUID,
                        AuthType = AuthType.Offline
                    };
                }
                else
                {
                    larg.Update2?.Invoke(temp1, LaunchState.LoginFail);
                    if (res1.Ex != null)
                    {
                        throw new LaunchException(LaunchState.LoginFail, res1.Message!, res1.Ex);
                    }

                    throw new LaunchException(LaunchState.LoginFail, res1.Message!);
                }
            }
            else
            {
                larg.Auth = res1.Auth!;
                larg.Auth.Save();
            }
        }
        catch (Exception e)
        {
            list.Add(temp1, new() { Ex = e });
            return list;
        }

        stopwatch.Stop();
        string temp = string.Format(LanguageHelper.Get("Core.Launch.Info4"),
           objs.First().Name, stopwatch.Elapsed.ToString());
        Logs.Info(temp);

        if (token.IsCancellationRequested)
        {
            return list;
        }

        //逐个实例启动
        foreach (var obj in objs)
        {
            try
            {
                //版本号检测
                if (string.IsNullOrWhiteSpace(obj.Version)
                    || (obj.Loader != Loaders.Normal && string.IsNullOrWhiteSpace(obj.LoaderVersion))
                    || (obj.Loader == Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
                {
                    throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error7"));
                }

                if (obj.ModPackType == SourceType.ColorMC && !string.IsNullOrWhiteSpace(obj.ServerUrl))
                {
                    stopwatch.Restart();
                    stopwatch.Start();
                    var pack = await obj.ServerPackCheckAsync(new ServerPackCheckArg
                    {
                        State = larg.State,
                        Select = larg.Select
                    });
                    stopwatch.Stop();
                    temp = string.Format(LanguageHelper.Get("Core.Launch.Info14"),
                        obj.Name, stopwatch.Elapsed.ToString());
                    ColorMCCore.OnGameLog(obj, temp);
                    Logs.Info(temp);
                    if (pack == false)
                    {
                        if (larg.Request == null)
                        {
                            throw new LaunchException(LaunchState.VersionError,
                                string.Format(LanguageHelper.Get("Core.Launch.Info16"), obj.Name));
                        }
                        var res2 = await larg.Request(string.Format(LanguageHelper.Get("Core.Launch.Info15"), obj.Name));
                        if (!res2)
                        {
                            throw new LaunchException(LaunchState.Cancel, LanguageHelper.Get("Core.Launch.Error8"));
                        }
                    }
                }

                larg.Update2?.Invoke(obj, LaunchState.Check);

                //检查游戏文件
                stopwatch.Restart();
                stopwatch.Start();
                var res = await obj.CheckGameFileAsync(token);
                var res3 = await larg.Auth.CheckLoginCoreAsync();
                stopwatch.Stop();
                temp = string.Format(LanguageHelper.Get("Core.Launch.Info5"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.OnGameLog(obj, temp);
                Logs.Info(temp);

                if (res3 != null)
                {
                    res.Add(res3);
                }

                //下载缺失的文件
                if (!res.IsEmpty)
                {
                    bool download = true;
                    if (!ConfigUtils.Config.Http.AutoDownload && larg.Request != null)
                    {
                        download = await larg.Request(LanguageHelper.Get("Core.Launch.Info12"));
                    }

                    if (download)
                    {
                        larg.Update2?.Invoke(obj, LaunchState.Download);

                        stopwatch.Restart();
                        stopwatch.Start();

                        var ok = await DownloadManager.StartAsync([.. res]);
                        if (!ok)
                        {
                            throw new LaunchException(LaunchState.LostFile,
                                LanguageHelper.Get("Core.Launch.Error5"));
                        }
                        stopwatch.Stop();
                        temp = string.Format(LanguageHelper.Get("Core.Launch.Info7"),
                            obj.Name, stopwatch.Elapsed.ToString());
                        ColorMCCore.OnGameLog(obj, temp);
                        Logs.Info(temp);
                    }
                }

                //查找合适的JAVA
                stopwatch.Restart();
                stopwatch.Start();
                var path = obj.JvmLocal;
                JavaInfo? jvm = null;
                var game = VersionPath.GetVersion(obj.Version)!;
                if (string.IsNullOrWhiteSpace(path))
                {
                    if (JvmPath.Jvms.Count == 0)
                    {
                        var list1 = await JavaHelper.FindJava();
                        list1?.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
                    }
                    var jv = game.JavaVersion.majorVersion;
                    jvm = JvmPath.GetInfo(obj.JvmName) ?? JvmPath.FindJava(jv);
                    if (jvm == null)
                    {
                        larg.Update2?.Invoke(obj, LaunchState.JavaError);
                        larg.Nojava?.Invoke(jv);
                        throw new LaunchException(LaunchState.JavaError,
                                string.Format(LanguageHelper.Get("Core.Launch.Error6"), jv));
                    }

                    path = jvm.GetPath();
                }
                if (token.IsCancellationRequested)
                {
                    return list;
                }

                //准备Jvm参数
                larg.Update2?.Invoke(obj, LaunchState.JvmPrepare);
                var arg = await obj.MakeArgAsync(new GameMakeArg
                {
                    Jvm = jvm!,
                    Login = larg.Auth,
                    World = larg.World,
                    Server = larg.Server,
                    Mixinport = larg.Mixinport,
                    CheckLocal = true
                });
                ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info1"));
                bool hidenext = false;
                foreach (var item in arg)
                {
                    if (hidenext)
                    {
                        hidenext = false;
                        ColorMCCore.OnGameLog(obj, "******");
                    }
                    else
                    {
                        ColorMCCore.OnGameLog(obj, item);
                    }
                    var low = item.ToLower();
                    if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
                    {
                        hidenext = true;
                    }
                }

                ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info3"));
                ColorMCCore.OnGameLog(obj, path);

                if (token.IsCancellationRequested)
                {
                    return list;
                }

                //自定义启动参数
                var env = new Dictionary<string, string>();
                string envstr;
                if (obj.JvmArg?.JvmEnv is { } str)
                {
                    envstr = str;
                }
                else if (ConfigUtils.Config.DefaultJvmArg.JvmEnv is { } str1)
                {
                    envstr = str1;
                }
                else
                {
                    envstr = "";
                }

                if (!string.IsNullOrWhiteSpace(envstr))
                {
                    var list1 = envstr.Split('\n');
                    foreach (var item in list1)
                    {
                        var item1 = item.Trim();
                        var index = item1.IndexOf('=');
                        string key, value;
                        if (index == -1)
                        {
                            key = item1;
                            value = "";
                        }
                        else if (index + 1 == item1.Length)
                        {
                            key = item1[..^1];
                            value = "";
                        }
                        else
                        {
                            key = item1[..index];
                            value = item1[(index + 1)..];
                        }

                        if (!env.TryAdd(key, value))
                        {
                            env[key] = value;
                        }
                    }
                }

                if (token.IsCancellationRequested)
                {
                    return list;
                }

                list2.Add((obj, path, arg, env));
            }
            catch (Exception e)
            {
                list.Add(obj, new() { Ex = e });
            }
        }

        foreach (var item1 in list2)
        {
            //启动进程
            IGameHandel? handel;

            var process = new Process()
            {
                EnableRaisingEvents = true
            };
            process.StartInfo.FileName = item1.Item2;
            process.StartInfo.WorkingDirectory = item1.Item1.GetGamePath();
            Directory.CreateDirectory(process.StartInfo.WorkingDirectory);
            foreach (var item in item1.Item3)
            {
                process.StartInfo.ArgumentList.Add(item);
            }
            foreach (var item in item1.Item4)
            {
                process.StartInfo.Environment.Add(item.Key, item.Value);
            }

            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (a, b) =>
            {
                ColorMCCore.OnGameLog(item1.Item1, b.Data);
            };
            process.ErrorDataReceived += (a, b) =>
            {
                ColorMCCore.OnGameLog(item1.Item1, b.Data);
            };
            process.Exited += (a, b) =>
            {
                ColorMCCore.OnGameExit(item1.Item1, larg.Auth, process.ExitCode);
                process.Dispose();
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            handel = new DesktopGameHandel(process, item1.Item1.UUID);

            stopwatch.Stop();
            temp = string.Format(LanguageHelper.Get("Core.Launch.Info6"),
                item1.Item1.Name, stopwatch.Elapsed.ToString());
            ColorMCCore.OnGameLog(item1.Item1, temp);
            Logs.Info(temp);

            ColorMCCore.AddGameHandel(item1.Item1.UUID, handel);
            list.Add(item1.Item1, new() { Handel = handel });
        }

        return list;
    }

    /// <summary>
    /// 生成游戏启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">请求参数</param>
    /// <returns>启动参数</returns>
    public static async Task<CreateCmdRes> CreateGameCmd(this GameSettingObj obj, GameLaunchArg larg)
    {
        //版本号检测
        if (string.IsNullOrWhiteSpace(obj.Version)
            || (obj.Loader is not (Loaders.Normal or Loaders.Custom) && string.IsNullOrWhiteSpace(obj.LoaderVersion))
            || (obj.Loader is Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
        {
            return new() { Message = LanguageHelper.Get("Core.Launch.Error7") };
        }

        //登录账户
        larg.Update2?.Invoke(obj, LaunchState.Login);
        var res1 = await larg.Auth.RefreshTokenAsync();
        if (res1.LoginState != LoginState.Done)
        {
            if (larg.Auth.AuthType == AuthType.OAuth
                && !string.IsNullOrWhiteSpace(larg.Auth.UUID)
                && larg.Loginfail != null
                && await larg.Loginfail(larg.Auth) == true)
            {
                larg.Auth = new()
                {
                    UserName = larg.Auth.UserName,
                    UUID = larg.Auth.UUID,
                    AuthType = AuthType.Offline
                };
            }
            else
            {
                larg.Update2?.Invoke(obj, LaunchState.LoginFail);
                if (res1.Ex != null)
                    return new() { Message = res1.Message! };

                return new() { Message = res1.Message! };
            }
        }
        else
        {
            larg.Auth = res1.Auth!;
            larg.Auth.Save();
        }

        //是否为服务器实例
        if (obj.ModPackType == SourceType.ColorMC && !string.IsNullOrWhiteSpace(obj.ServerUrl))
        {
            var pack = await obj.ServerPackCheckAsync(new ServerPackCheckArg
            {
                State = larg.State,
                Select = larg.Select
            });
            if (pack == false)
            {
                if (larg.Request == null)
                {
                    return new()
                    {
                        Message =
                         string.Format(LanguageHelper.Get("Core.Launch.Info16"), obj.Name)
                    };
                }
                var res2 = await larg.Request(string.Format(LanguageHelper.Get("Core.Launch.Info15"), obj.Name));
                if (!res2)
                {
                    return new() { Message = LanguageHelper.Get("Core.Launch.Error8") };
                }
            }
        }

        larg.Update2?.Invoke(obj, LaunchState.Check);

        //检查游戏文件
        var res = await obj.CheckGameFileAsync(CancellationToken.None);
        var res3 = await larg.Auth.CheckLoginCoreAsync();

        if (res3 != null)
        {
            res.Add(res3);
        }

        var path = obj.JvmLocal;
        JavaInfo? jvm = null;
        var game = VersionPath.GetVersion(obj.Version)!;
        if (string.IsNullOrWhiteSpace(path))
        {
            if (JvmPath.Jvms.Count == 0)
            {
                var list1 = await JavaHelper.FindJava();
                list1?.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
            }
            var jv = game.JavaVersion.majorVersion;
            jvm = JvmPath.GetInfo(obj.JvmName) ?? JvmPath.FindJava(jv);
            if (jvm == null)
            {
                larg.Update2?.Invoke(obj, LaunchState.JavaError);
                larg.Nojava?.Invoke(jv);
                return new() { Message = string.Format(LanguageHelper.Get("Core.Launch.Error6"), jv) };
            }

            path = jvm.GetPath();
        }

        if (!File.Exists(path))
        {
            return new() { Message = LanguageHelper.Get("Core.Launch.Error13") };
        }

        //准备Jvm参数
        larg.Update2?.Invoke(obj, LaunchState.JvmPrepare);
        var arg = await obj.MakeArgAsync(new GameMakeArg
        {
            Jvm = jvm!,
            Login = larg.Auth,
            World = larg.World,
            Server = larg.Server,
            Mixinport = larg.Mixinport
        });

        //自定义启动参数
        var env = new Dictionary<string, string>();
        string envstr;
        if (obj.JvmArg?.JvmEnv is { } str)
        {
            envstr = str;
        }
        else if (ConfigUtils.Config.DefaultJvmArg.JvmEnv is { } str1)
        {
            envstr = str1;
        }
        else
        {
            envstr = "";
        }

        //需要处理环境变量
        if (!string.IsNullOrWhiteSpace(envstr))
        {
            var list1 = envstr.Split('\n');
            foreach (var item in list1)
            {
                var item1 = item.Trim();
                var index = item1.IndexOf('=');
                string key, value;
                if (index == -1)
                {
                    key = item1;
                    value = "";
                }
                else if (index + 1 == item1.Length)
                {
                    key = item1[..^1];
                    value = "";
                }
                else
                {
                    key = item1[..index];
                    value = item1[(index + 1)..];
                }

                if (!env.TryAdd(key, value))
                {
                    env[key] = value;
                }
            }
        }

        larg.Update2?.Invoke(obj, LaunchState.End);

        return new CreateCmdRes()
        {
            Res = true,
            Args = arg,
            Dir = obj.GetGamePath(),
            Java = path,
            Envs = env
        };
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token">取消Token</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns>游戏句柄</returns>
    public static async Task<IGameHandel?> StartGameAsync(this GameSettingObj obj, GameLaunchArg larg, CancellationToken token)
    {
        var stopwatch = new Stopwatch();

        //版本号检测
        if (string.IsNullOrWhiteSpace(obj.Version)
            || (obj.Loader is not (Loaders.Normal or Loaders.Custom) && string.IsNullOrWhiteSpace(obj.LoaderVersion))
            || (obj.Loader is Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
        {
            throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error7"));
        }

        //登录账户
        stopwatch.Restart();
        stopwatch.Start();
        larg.Update2?.Invoke(obj, LaunchState.Login);
        var res1 = await larg.Auth.RefreshTokenAsync();
        if (res1.LoginState != LoginState.Done)
        {
            if (larg.Auth.AuthType == AuthType.OAuth
                && !string.IsNullOrWhiteSpace(larg.Auth.UUID)
                && larg.Loginfail != null
                && await larg.Loginfail(larg.Auth) == true)
            {
                larg.Auth = new()
                {
                    UserName = larg.Auth.UserName,
                    UUID = larg.Auth.UUID,
                    AuthType = AuthType.Offline
                };
            }
            else
            {
                larg.Update2?.Invoke(obj, LaunchState.LoginFail);
                if (res1.Ex != null)
                    throw new LaunchException(LaunchState.LoginFail, res1.Message!, res1.Ex);

                throw new LaunchException(LaunchState.LoginFail, res1.Message!);
            }
        }
        else
        {
            larg.Auth = res1.Auth!;
            larg.Auth.Save();
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        stopwatch.Stop();
        string temp = string.Format(LanguageHelper.Get("Core.Launch.Info4"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        //处理服务器实例
        if (obj.ModPackType == SourceType.ColorMC && !string.IsNullOrWhiteSpace(obj.ServerUrl))
        {
            stopwatch.Restart();
            stopwatch.Start();
            var pack = await obj.ServerPackCheckAsync(new ServerPackCheckArg
            {
                State = larg.State,
                Select = larg.Select
            });
            stopwatch.Stop();
            temp = string.Format(LanguageHelper.Get("Core.Launch.Info14"),
                obj.Name, stopwatch.Elapsed.ToString());
            ColorMCCore.OnGameLog(obj, temp);
            Logs.Info(temp);
            if (pack == false)
            {
                if (larg.Request == null)
                {
                    throw new LaunchException(LaunchState.VersionError,
                        string.Format(LanguageHelper.Get("Core.Launch.Info16"), obj.Name));
                }
                var res2 = await larg.Request(string.Format(LanguageHelper.Get("Core.Launch.Info15"), obj.Name));
                if (!res2)
                {
                    throw new LaunchException(LaunchState.Cancel,
                            LanguageHelper.Get("Core.Launch.Error8"));
                }
            }
        }

        larg.Update2?.Invoke(obj, LaunchState.Check);

        //检查游戏文件
        stopwatch.Restart();
        stopwatch.Start();
        var res = await obj.CheckGameFileAsync(token);
        var res3 = await larg.Auth.CheckLoginCoreAsync();
        stopwatch.Stop();
        temp = string.Format(LanguageHelper.Get("Core.Launch.Info5"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        if (res3 != null)
        {
            res.Add(res3);
        }

        //下载缺失的文件
        if (!res.IsEmpty)
        {
            bool download = true;
            if (!ConfigUtils.Config.Http.AutoDownload && larg.Request != null)
            {
                download = await larg.Request(LanguageHelper.Get("Core.Launch.Info12"));
            }

            if (download)
            {
                larg.Update2?.Invoke(obj, LaunchState.Download);

                stopwatch.Restart();
                stopwatch.Start();

                var ok = await DownloadManager.StartAsync([.. res]);
                if (!ok)
                {
                    throw new LaunchException(LaunchState.LostFile,
                        LanguageHelper.Get("Core.Launch.Error5"));
                }
                stopwatch.Stop();
                temp = string.Format(LanguageHelper.Get("Core.Launch.Info7"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.OnGameLog(obj, temp);
                Logs.Info(temp);
            }
        }

        //查找合适的JAVA
        stopwatch.Restart();
        stopwatch.Start();
        var path = obj.JvmLocal;
        JavaInfo? jvm = null;
        var game = VersionPath.GetVersion(obj.Version)!;
        if (string.IsNullOrWhiteSpace(path))
        {
            if (JvmPath.Jvms.Count == 0)
            {
                var list1 = await JavaHelper.FindJava();
                list1?.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
            }
            var jv = game.JavaVersion.majorVersion;
            jvm = JvmPath.GetInfo(obj.JvmName) ?? JvmPath.FindJava(jv);
            if (jvm == null)
            {
                larg.Update2?.Invoke(obj, LaunchState.JavaError);
                larg.Nojava?.Invoke(jv);
                throw new LaunchException(LaunchState.JavaError,
                        string.Format(LanguageHelper.Get("Core.Launch.Error6"), jv));
            }

            path = jvm.GetPath();
        }
        if (token.IsCancellationRequested)
        {
            return null;
        }

        if (!File.Exists(path))
        {
            throw new LaunchException(LaunchState.JavaError,
                LanguageHelper.Get("Core.Launch.Error13"));
        }

        //准备Jvm参数
        larg.Update2?.Invoke(obj, LaunchState.JvmPrepare);
        var arg = await obj.MakeArgAsync(new GameMakeArg
        {
            Jvm = jvm!,
            Login = larg.Auth,
            World = larg.World,
            Server = larg.Server,
            Mixinport = larg.Mixinport,
            CheckLocal = true
        });
        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info1"));
        bool hidenext = false;
        foreach (var item in arg)
        {
            if (hidenext)
            {
                hidenext = false;
                ColorMCCore.OnGameLog(obj, "******");
            }
            else
            {
                ColorMCCore.OnGameLog(obj, item);
            }
            var low = item.ToLower();
            if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
            {
                hidenext = true;
            }
        }

        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info3"));
        ColorMCCore.OnGameLog(obj, path);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //自定义启动参数
        var env = new Dictionary<string, string>();
        string envstr;
        if (obj.JvmArg?.JvmEnv is { } str)
        {
            envstr = str;
        }
        else if (ConfigUtils.Config.DefaultJvmArg.JvmEnv is { } str1)
        {
            envstr = str1;
        }
        else
        {
            envstr = "";
        }

        if (!string.IsNullOrWhiteSpace(envstr))
        {
            var list1 = envstr.Split('\n');
            foreach (var item in list1)
            {
                var item1 = item.Trim();
                var index = item1.IndexOf('=');
                string key, value;
                if (index == -1)
                {
                    key = item1;
                    value = "";
                }
                else if (index + 1 == item1.Length)
                {
                    key = item1[..^1];
                    value = "";
                }
                else
                {
                    key = item1[..index];
                    value = item1[(index + 1)..];
                }

                if (!env.TryAdd(key, value))
                {
                    env[key] = value;
                }
            }
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //启动前运行
        if (obj.JvmArg?.LaunchPre == true || ConfigUtils.Config.DefaultJvmArg.LaunchPre)
        {
            var cmd1 = obj.JvmArg?.LaunchPreData;
            var cmd2 = ConfigUtils.Config.DefaultJvmArg.LaunchPreData;
            var start = string.IsNullOrWhiteSpace(cmd1) ? cmd2 : cmd1;
            var prerun = obj.JvmArg == null ? ConfigUtils.Config.DefaultJvmArg.PreRunSame : obj.JvmArg.PreRunSame;
            if (!string.IsNullOrWhiteSpace(start) &&
                (larg.Pre == null || await larg.Pre(true)))
            {
#if Phone
                if (SystemInfo.Os == OsType.Android && start.StartsWith(JAVA_LOCAL))
                {
                    if (JvmPath.FindJava(8) is { } jvm1)
                    {
                        stopwatch.Start();
                        larg.Update2?.Invoke(obj, LaunchState.LaunchPre);
                        start = ReplaceArg(obj, path!, arg, start);
                        obj.PhoneCmdRun(start, jvm1, env);
                        stopwatch.Stop();
                        string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                            obj.Name, stopwatch.Elapsed.ToString());
                        ColorMCCore.OnGameLog(obj, temp1);
                    }
                }
#else
                stopwatch.Start();
                larg.Update2?.Invoke(obj, LaunchState.LaunchPre);
                start = ReplaceArg(obj, path!, arg, start);
                obj.CmdRun(start, env, prerun, larg.Admin);
                stopwatch.Stop();
                string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.OnGameLog(obj, temp1);
                Logs.Info(temp1);
#endif
            }
            else
            {
                string temp2 = string.Format(LanguageHelper.Get("Core.Launch.Info10"),
                obj.Name);
                ColorMCCore.OnGameLog(obj, temp2);
                Logs.Info(temp2);
            }
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }
#if Phone
        if (SystemInfo.Os == OsType.Android)
        {
            //安装Forge
            var version = VersionPath.GetVersion(obj.Version)!;
            var v2 = version.IsGameVersionV2();
            if (v2 && obj.Loader is Loaders.Forge or Loaders.NeoForge)
            {
                var obj1 = obj.Loader is Loaders.Forge
                    ? VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!)!
                    : VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!)!;
                var install = CheckHelpers.CheckForgeInstall(obj1,
                    obj.LoaderVersion!, obj.Loader is Loaders.NeoForge);
                if (install)
                {
                    larg.Update2?.Invoke(obj, LaunchState.InstallForge);
                    var jvm1 = JvmPath.FindJava(8) ?? throw new LaunchException(LaunchState.JavaError,
                            LanguageHelper.Get("Core.Launch.Error9"));

                    var res2 = obj.PhoneCmdRun(obj.MakeInstallForgeArg(v2), jvm1, env);

                    if (res2 != 0)
                    {
                        throw new LaunchException(LaunchState.LoaderError,
                            LanguageHelper.Get("Core.Launch.Error10"));
                    }
                }
            }
        }
#endif
        if (token.IsCancellationRequested)
        {
            return null;
        }

        //启动进程
        IGameHandel? handel;
#if Phone
        if (SystemInfo.Os == OsType.Android)
        {
            handel = ColorMCCore.PhoneGameLaunch(larg.Auth, obj, jvm!, arg, env);
        }
#else
        var process = new Process()
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
        foreach (var item in env)
        {
            process.StartInfo.Environment.Add(item.Key, item.Value);
        }

        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Exited += (a, b) =>
        {
            ColorMCCore.OnGameExit(obj, larg.Auth, process.ExitCode);
            process.Dispose();
        };

        if (!ProcessUtils.Launch(process, larg.Admin))
        {
            //监听日志
            Encoding? encoding = null;
            if (obj.Encoding == 1)
            {
                try
                {
                    encoding = Encoding.GetEncoding("gbk");
                }
                catch
                {

                }
            }
            else
            {
                encoding = Encoding.UTF8;
            }
            new Thread(() =>
            {
                bool stop = false;
                using var reader = new StreamReader(process.StandardOutput.BaseStream, encoding!);
                process.Exited += (a, b) =>
                {
                    stop = true;
                };
                while (!stop)
                {
                    var output = reader.ReadLine();
                    ColorMCCore.OnGameLog(obj, output);
                }
            })
            {
                Name = "ColorMC-Game:" + obj.UUID + ":T1"
            }.Start();
            new Thread(() =>
            {
                bool stop = false;
                using var reader = new StreamReader(process.StandardError.BaseStream, encoding!);
                process.Exited += (a, b) =>
                {
                    stop = true;
                };
                while (!stop)
                {
                    var output = reader.ReadLine();
                    ColorMCCore.OnGameLog(obj, output);
                }
            })
            {
                Name = "ColorMC-Game:" + obj.UUID + ":T2"
            }.Start();
        }
        else
        {
            ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Game.Info2"));
        }

        handel = new DesktopGameHandel(process, obj.UUID);
#endif
        stopwatch.Stop();
        temp = string.Format(LanguageHelper.Get("Core.Launch.Info6"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        ColorMCCore.AddGameHandel(obj.UUID, handel);

        //启动后执行
        if (obj.JvmArg?.LaunchPost == true || ConfigUtils.Config.DefaultJvmArg.LaunchPost)
        {
            var start = obj.JvmArg?.LaunchPostData;
            if (string.IsNullOrWhiteSpace(start))
            {
                start = ConfigUtils.Config.DefaultJvmArg.LaunchPostData;
            }
            if (!string.IsNullOrWhiteSpace(start) &&
                (larg.Pre == null || await larg.Pre(false)))
            {
#if Phone
                if (SystemInfo.Os == OsType.Android)
                {
                    if (JvmPath.FindJava(8) is { } jvm1)
                    {
                        stopwatch.Start();
                        larg.Update2?.Invoke(obj, LaunchState.LaunchPost);
                        start = ReplaceArg(obj, path!, arg, start);
                        obj.PhoneCmdRun(start, jvm1, env);
                        stopwatch.Stop();
                        string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info9"),
                            obj.Name, stopwatch.Elapsed.ToString());
                        ColorMCCore.OnGameLog(obj, temp1);
                    }
                }
#else
                stopwatch.Start();
                larg.Update2?.Invoke(obj, LaunchState.LaunchPost);
                start = ReplaceArg(obj, path!, arg, start);
                obj.CmdRun(start, env, false, larg.Admin);
                stopwatch.Stop();
                ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Info9"),
                    obj.Name, stopwatch.Elapsed.ToString()));
#endif
            }
            else
            {
                ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Info11"),
                obj.Name));
            }
        }

        return handel;
    }
}
