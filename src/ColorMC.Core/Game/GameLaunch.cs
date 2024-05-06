using System.Diagnostics;
using System.Text;
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
    /// 获取Forge安装参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>参数</returns>
    public static List<string> MakeInstallForgeArg(this GameSettingObj obj, bool v2)
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

        jvm.AddRange(forge1.arguments.game);

        return jvm;
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
    public static void CmdRun(GameSettingObj obj, string cmd, Dictionary<string, string> env)
    {
        var args = cmd.Split('\n');
        var file = Path.GetFullPath(obj.GetBasePath() + "/" + args[0].Trim());
        var arglist = new List<string>();

        var info = new ProcessStartInfo(file)
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
        using var p = new Process()
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

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
    }


    /// <summary>
    /// V1版Jvm参数
    /// </summary>
    private readonly static List<string> V1JvmArg =
    [
        "-Djava.library.path=${natives_directory}", "-cp", "${classpath}"
    ];

    /// <summary>
    /// 创建V2版Jvm参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>参数</returns>
    private static List<string> MakeV2JvmArg(GameSettingObj obj)
    {
        var game = VersionPath.GetVersion(obj.Version)!;
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
    private static List<string> MakeV1GameArg(GameSettingObj obj)
    {
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
            return new(forge.minecraftArguments.Split(' '));
        }
        else if (obj.Loader == Loaders.OptiFine)
        {
            var version = VersionPath.GetVersion(obj.Version)!;
            return new(version.minecraftArguments.Split(' '))
            {
                "--tweakClass",
                "optifine.OptiFineTweaker"
            };
        }
        else if (obj.Loader == Loaders.Custom)
        {
            return obj.GetLoaderGameArg();
        }
        else
        {
            var version = VersionPath.GetVersion(obj.Version)!;
            return new(version.minecraftArguments.Split(' '));
        }
    }

    /// <summary>
    /// 创建V2版游戏参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    private static List<string> MakeV2GameArg(GameSettingObj obj)
    {
        var game = VersionPath.GetVersion(obj.Version)!;
        if (game.arguments == null)
        {
            return MakeV1GameArg(obj);
        }

        var arg = new List<string>();
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
            var forge = obj.Loader == Loaders.NeoForge
                ? obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
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
    /// <param name="v2">V2模式</param>
    /// <param name="login">登录的账户</param>
    /// <returns>Jvm参数</returns>
    private static async Task<List<string>> JvmArgAsync(GameSettingObj obj, bool v2,
        LoginObj login, JavaInfo jvm1, int? mixinport)
    {
        RunArgObj args;

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

        var jvm = new List<string>();

        //javaagent
        if (!string.IsNullOrWhiteSpace(args.JavaAgent))
        {
            jvm.Add($"-javaagent:{args.JavaAgent.Trim()}");
        }

        if (mixinport > 0 && jvm1.MajorVersion > 8)
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
                if (SystemInfo.Os == OsType.Android)
                {
                    jvm.Add("-Dforgewrapper.justLaunch=true");
                }
            }
            else if (obj.Loader == Loaders.OptiFine)
            {
                jvm.Add($"-Dlibdir={LibrariesPath.BaseDir}");
                jvm.Add($"-Dgamecore={LibrariesPath.GetGameFile(obj.Version)}");
                jvm.Add($"-Doptifine={Path.GetFullPath(LibrariesPath.GetOptiFineLib(obj))}");
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
            jvm.Add("-Dfml.ignoreInvalidMinecraftCertificates=true");
            jvm.Add("-Dfml.ignorePatchDiscrepancies=true");
            jvm.Add("-Dlog4j2.formatMsgNoLookups=true");
            //jvmHead.Add("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false");
            //jvmHead.Add("-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false");
            //jvmHead.Add($"-Dminecraft.client.jar={VersionPath.BaseDir}/{obj.Version}.jar");

            jvm.AddRange(v2 ? MakeV2JvmArg(obj) : V1JvmArg);
        }

        //auth
        if (login.AuthType == AuthType.Nide8)
        {
            jvm.Add($"-javaagent:{AuthlibHelper.NowNide8Injector}={login.Text1}");
            jvm.Add("-Dnide8auth.client=true");
        }
        else if (login.AuthType == AuthType.AuthlibInjector)
        {
            var res = await BaseClient.GetStringAsync(login.Text1);
            jvm.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}");
            jvm.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Item2!)}");
        }
        else if (login.AuthType == AuthType.LittleSkin)
        {
            var res = await BaseClient.GetStringAsync($"{UrlHelper.LittleSkin}api/yggdrasil");
            jvm.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={UrlHelper.LittleSkin}api/yggdrasil");
            jvm.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Item2!)}");
        }
        else if (login.AuthType == AuthType.SelfLittleSkin)
        {
            var res = await BaseClient.GetStringAsync($"{login.Text1}api/yggdrasil");
            jvm.Add($"-javaagent:{AuthlibHelper.NowAuthlibInjector}={login.Text1}/api/yggdrasil");
            jvm.Add($"-Dauthlibinjector.yggdrasil.prefetched={HashHelper.GenBase64(res.Item2!)}");
        }

        //log4j2-xml
        var game = VersionPath.GetVersion(obj.Version)!;
        if (game.logging != null && ConfigUtils.Config.SafeLog4j)
        {
            var obj1 = DownloadItemHelper.BuildLog4jItem(game);
            jvm.Add(game.logging.client.argument.Replace("${path}", obj1.Local));
        }

        return jvm;
    }

    /// <summary>
    /// 创建游戏启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">V2模式</param>
    /// <returns>游戏启动参数</returns>
    private static List<string> GameArg(GameSettingObj obj, bool v2, WorldObj? world)
    {
        var gameArg = new List<string>();
        if (obj.JvmArg?.RemoveGameArg != true)
        {
            gameArg.AddRange(v2 ? MakeV2GameArg(obj) : MakeV1GameArg(obj));
        }

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
                if (CheckHelpers.IsGameVersion120(obj.Version))
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
    private static async Task<List<string>> GetLibsAsync(GameSettingObj obj, bool v2)
    {
        var list = new Dictionary<LibVersionObj, string>();
        var version = VersionPath.GetVersion(obj.Version)!;

        //LoaderLib
        if (obj.Loader == Loaders.Forge || obj.Loader == Loaders.NeoForge)
        {
            var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;

            var list2 = DownloadItemHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!,
                obj.Loader == Loaders.NeoForge, v2, false);

            foreach (var item in list2)
            {
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), item.Local);
            }

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
        else if (obj.Loader == Loaders.OptiFine)
        {
            GameHelper.ReadyOptifineWrapper();
            list.AddOrUpdate(new(), GameHelper.OptifineWrapper);
        }
        else if (obj.Loader == Loaders.Custom)
        {
            var list2 = obj.GetCustomLoaderLibs();
            foreach (var (Name, Local) in list2)
            {
                list.AddOrUpdate(FuntionUtils.MakeVersionObj(Name), Local);
            }
        }

        //GameLib
        if (obj.Loader == Loaders.Custom && obj.CustomLoader?.OffLib == true)
        {
            var list1 = await DownloadItemHelper.BuildGameLibsAsync(version);
            foreach (var item in list1)
            {
                if (item.Later == null)
                {
                    //不添加lwjgl
                    if (item.Name.Contains("org.lwjgl") && SystemInfo.Os == OsType.Android)
                    {
                        continue;
                    }
                    list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), Path.GetFullPath(item.Local));
                }
            }
        }
        else
        {
            var list1 = await DownloadItemHelper.BuildGameLibsAsync(version);
            foreach (var item in list1)
            {
                if (item.Later == null)
                {
                    //不添加lwjgl
                    if (item.Name.Contains("org.lwjgl") && SystemInfo.Os == OsType.Android)
                    {
                        continue;
                    }
                    list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), Path.GetFullPath(item.Local));
                }
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

    /// <summary>
    /// 创建Classpath
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">是否为v2版本</param>
    /// <returns>结果</returns>
    private static async Task<string> MakeClassPathAsync(GameSettingObj obj, bool v2, bool enableasm)
    {
        var libraries = await GetLibsAsync(obj, v2);
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
                if (File.Exists(path))
                {
                    libraries.Add(path);
                }
            }
        }

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            if (File.Exists(item))
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
    /// <param name="v2">V2模式</param>
    private static void ReplaceAll(GameSettingObj obj, LoginObj login, List<string> args, string classpath)
    {
        var version = VersionPath.GetVersion(obj.Version)!;
        var assetsPath = AssetsPath.BaseDir;
        var gameDir = InstancesPath.GetGamePath(obj);
        var assetsIndexName = version.assets ?? "legacy";

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
    /// 创建主类
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="version">游戏数据</param>
    /// <param name="v2">是否为v2版本</param>
    /// <returns>主类</returns>
    private static string MakeMainClass(GameSettingObj obj, GameArgObj version, bool v2)
    {
        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.MainClass))
        {
            return obj.AdvanceJvm.MainClass;
        }
        else if (obj.Loader == Loaders.Normal)
        {
            return version.mainClass;
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
                return forge.mainClass;
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = obj.GetFabricObj()!;
            return fabric.mainClass;
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = obj.GetQuiltObj()!;
            return quilt.mainClass;
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

        return "";
    }

    /// <summary>
    /// 创建所有启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <returns></returns>
    private static async Task<List<string>> MakeArgAsync(GameSettingObj obj, LoginObj login,
        WorldObj? world, JavaInfo jvm, int? mixinport)
    {
        var list = new List<string>();
        var version = VersionPath.GetVersion(obj.Version)!;
        var v2 = CheckHelpers.IsGameVersionV2(version);
        var jvmarg = await JvmArgAsync(obj, v2, login, jvm, mixinport);
        var classpath = await MakeClassPathAsync(obj, v2, mixinport > 0);
        var gamearg = GameArg(obj, v2, world);
        ReplaceAll(obj, login, jvmarg, classpath);
        ReplaceAll(obj, login, gamearg, classpath);
        //jvm
        list.AddRange(jvmarg);

        //mainclass
        list.Add(MakeMainClass(obj, version, v2));

        //gamearg
        list.AddRange(gamearg);

        return list;
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="jvmCfg">使用的Java</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns></returns>
    public static async Task<IGameHandel?> StartGameAsync(this GameSettingObj obj, LoginObj login,
        WorldObj? world, ColorMCCore.Request request, ColorMCCore.LaunchP pre,
        ColorMCCore.UpdateState state, ColorMCCore.ChoiseCall select,
        ColorMCCore.NoJava nojava, ColorMCCore.LoginFailRun loginfail,
        ColorMCCore.GameLaunch update2, int? mixinport,
        CancellationToken token)
    {
        var stopwatch = new Stopwatch();

        //版本号检测
        if (string.IsNullOrWhiteSpace(obj.Version)
            || (obj.Loader != Loaders.Normal && string.IsNullOrWhiteSpace(obj.LoaderVersion))
            || (obj.Loader == Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
        {
            throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error7"));
        }

        //登录账户
        stopwatch.Restart();
        stopwatch.Start();
        update2(obj, LaunchState.Login);
        var (State, State1, Obj, Message, Ex) = await login.RefreshTokenAsync();
        if (State1 != LoginState.Done)
        {
            if (login.AuthType == AuthType.OAuth
                && !string.IsNullOrWhiteSpace(login.UUID)
                && await loginfail(login) == true)
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
                update2(obj, LaunchState.LoginFail);
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

        if (token.IsCancellationRequested)
        {
            return null;
        }

        stopwatch.Stop();
        string temp = string.Format(LanguageHelper.Get("Core.Launch.Info4"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        if (obj.ModPackType == SourceType.ColorMC && !string.IsNullOrWhiteSpace(obj.ServerUrl))
        {
            stopwatch.Restart();
            stopwatch.Start();
            var pack = await obj.ServerPackCheckAsync(state, select);
            stopwatch.Stop();
            temp = string.Format(LanguageHelper.Get("Core.Launch.Info14"),
                obj.Name, stopwatch.Elapsed.ToString());
            ColorMCCore.OnGameLog(obj, temp);
            Logs.Info(temp);
            if (pack == false)
            {
                var res1 = await request(string.Format(LanguageHelper.Get("Core.Launch.Info15"), obj.Name));
                if (!res1)
                {
                    throw new LaunchException(LaunchState.Cancel,
                            LanguageHelper.Get("Core.Launch.Error8"));
                }
            }
        }

        //检查游戏文件
        stopwatch.Restart();
        stopwatch.Start();
        var res = await CheckHelpers.CheckGameFileAsync(obj, login, update2, token);
        stopwatch.Stop();
        temp = string.Format(LanguageHelper.Get("Core.Launch.Info5"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        if (obj.GetModeFast() && obj.Loader == Loaders.Normal)
        {
            var res1 = await request(string.Format(LanguageHelper.Get("Core.Launch.Info13"), obj.Name));
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
                download = await request(LanguageHelper.Get("Core.Launch.Info12"));
            }

            if (download)
            {
                update2(obj, LaunchState.Download);

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
            var jv = game.javaVersion.majorVersion;
            jvm = JvmPath.GetInfo(obj.JvmName) ?? JvmPath.FindJava(jv);
            if (jvm == null)
            {
                update2(obj, LaunchState.JavaError);
                nojava();
                throw new LaunchException(LaunchState.JavaError,
                        string.Format(LanguageHelper.Get("Core.Launch.Error6"), jv));
            }

            path = jvm.GetPath();
        }
        if (token.IsCancellationRequested)
        {
            return null;
        }

        //准备Jvm参数
        update2(obj, LaunchState.JvmPrepare);
        var arg = await MakeArgAsync(obj, login, world, jvm!, mixinport);
        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info1"));
        bool hidenext = false;
        if (SystemInfo.Os != OsType.Android)
        {
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
        if ((obj.JvmArg?.LaunchPre == true || ConfigUtils.Config.DefaultJvmArg.LaunchPre))
        {
            var cmd1 = obj.JvmArg?.LaunchPreData;
            var cmd2 = ConfigUtils.Config.DefaultJvmArg.LaunchPreData;
            var start = string.IsNullOrWhiteSpace(cmd1) ? cmd2 : cmd1;
            if (!string.IsNullOrWhiteSpace(start) && await pre(true))
            {
                if (SystemInfo.Os == OsType.Android && start.StartsWith(JAVA_LOCAL))
                {
                    if (JvmPath.FindJava(8) is { } jvm1)
                    {
                        stopwatch.Start();
                        update2(obj, LaunchState.LaunchPre);
                        start = ReplaceArg(obj, path!, arg, start);

                        var args = start.Split('\n');
                        var file = Path.GetFullPath(obj.GetBasePath() + "/" + args[0].Trim());
                        var arglist = new List<string>();
                        for (int a = 1; a < args.Length; a++)
                        {
                            arglist.Add(args[a].Trim());
                        }

                        var res1 = ColorMCCore.PhoneJvmRun(obj, jvm1, obj.GetGamePath(), arglist, env);
                        res1.StartInfo.RedirectStandardError = true;
                        res1.StartInfo.RedirectStandardInput = true;
                        res1.StartInfo.RedirectStandardOutput = true;
                        res1.OutputDataReceived += (a, b) =>
                        {
                            ColorMCCore.OnGameLog(obj, b.Data);
                        };
                        res1.ErrorDataReceived += (a, b) =>
                        {
                            ColorMCCore.OnGameLog(obj, b.Data);
                        };
                        res1.Start();
                        res1.BeginOutputReadLine();
                        res1.BeginErrorReadLine();

                        await res1.WaitForExitAsync(token);
                        stopwatch.Stop();
                        string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                            obj.Name, stopwatch.Elapsed.ToString());
                        ColorMCCore.OnGameLog(obj, temp1);
                        Logs.Info(temp1);
                    }
                }
                else
                {
                    stopwatch.Start();
                    update2(obj, LaunchState.LaunchPre);
                    start = ReplaceArg(obj, path!, arg, start);
                    CmdRun(obj, start, env);
                    stopwatch.Stop();
                    string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                        obj.Name, stopwatch.Elapsed.ToString());
                    ColorMCCore.OnGameLog(obj, temp1);
                    Logs.Info(temp1);
                }
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

        if (SystemInfo.Os == OsType.Android)
        {
            //安装Forge
            var version = VersionPath.GetVersion(obj.Version)!;
            var v2 = CheckHelpers.IsGameVersionV2(version);
            if (v2 && obj.Loader is Loaders.Forge or Loaders.NeoForge)
            {
                var obj1 = obj.Loader is Loaders.Forge
                    ? VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!)!
                    : VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!)!;
                var install = CheckHelpers.CheckForgeInstall(obj1,
                    obj.LoaderVersion!, obj.Loader is Loaders.NeoForge);
                if (install)
                {
                    update2(obj, LaunchState.InstallForge);
                    var jvm1 = JvmPath.FindJava(8) ?? throw new LaunchException(LaunchState.JavaError,
                            LanguageHelper.Get("Core.Launch.Error9"));
                    using var res1 = ColorMCCore.PhoneJvmRun(obj, jvm1,
                        obj.GetGamePath(), obj.MakeInstallForgeArg(v2), env);
                    res1.StartInfo.RedirectStandardError = true;
                    res1.StartInfo.RedirectStandardInput = true;
                    res1.StartInfo.RedirectStandardOutput = true;
                    res1.OutputDataReceived += (a, b) =>
                    {
                        ColorMCCore.OnGameLog(obj, b.Data);
                    };
                    res1.ErrorDataReceived += (a, b) =>
                    {
                        ColorMCCore.OnGameLog(obj, b.Data);
                    };
                    res1.Start();
                    res1.BeginOutputReadLine();
                    res1.BeginErrorReadLine();

                    res1.WaitForExit();
                    if (res1.ExitCode != 0)
                    {
                        throw new LaunchException(LaunchState.LoaderError,
                            LanguageHelper.Get("Core.Launch.Error10"));
                    }
                }
            }
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //启动进程
        Process? process;

        if (SystemInfo.Os == OsType.Android)
        {
            process = ColorMCCore.PhoneGameLaunch(obj, jvm!, arg, env);
        }
        else
        {
            process = new()
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
        }

        if (process == null)
        {
            return null;
        }

        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.OutputDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        process.ErrorDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        process.Exited += (a, b) =>
        {
            ColorMCCore.OnGameExit(obj, login, process.ExitCode);
            process.Dispose();
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        stopwatch.Stop();
        temp = string.Format(LanguageHelper.Get("Core.Launch.Info6"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        var handel = new DesktopGameHandel(process, obj.UUID);
        ColorMCCore.AddGameHandel(obj.UUID, handel);

        //启动后执行
        if ((obj.JvmArg?.LaunchPost == true || ConfigUtils.Config.DefaultJvmArg.LaunchPost))
        {
            var start = obj.JvmArg?.LaunchPostData;
            if (string.IsNullOrWhiteSpace(start))
            {
                start = ConfigUtils.Config.DefaultJvmArg.LaunchPostData;
            }
            if (!string.IsNullOrWhiteSpace(start))
            {
                var res1 = await pre(false);
                if (res1)
                {
                    if (SystemInfo.Os == OsType.Android)
                    { }
                    else
                    {
                        stopwatch.Start();
                        update2(obj, LaunchState.LaunchPost);
                        start = ReplaceArg(obj, path!, arg, start);
                        CmdRun(obj, start, env);
                        stopwatch.Stop();
                        ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Info9"),
                            obj.Name, stopwatch.Elapsed.ToString()));
                    }
                }
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
