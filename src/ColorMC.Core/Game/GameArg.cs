using System.Text;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

public static class GameArg
{
    /// <summary>
    /// V1版Jvm参数
    /// </summary>
    private static readonly string[] s_v1JvmArg =
    [
        "-Djava.library.path=${natives_directory}", "-cp", "${classpath}"
    ];

    /// <summary>
    /// 创建游戏启动参数
    /// </summary>
    /// <param name="game">游戏数据</param>
    /// <returns></returns>
    private static List<string> MakeV1GameArg(GameArgObj game)
    {
        return [.. game.MinecraftArguments.Split(' ')];
    }

    /// <summary>
    /// 创建游戏启动参数
    /// </summary>
    /// <param name="game">游戏数据</param>
    /// <returns></returns>
    private static List<string> MakeV2GameArg(GameArgObj game)
    {
        var list = new List<string>();
        foreach (object item in game.Arguments.Game)
        {
            if (item is string str)
            {
                list.Add(str);
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

        return list;
    }

    /// <summary>
    /// 创建V1版游戏参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>启动参数</returns>
    private static List<string> MakeLoaderV1GameArg(GameSettingObj obj, GameArgObj game)
    {
        switch (obj.Loader)
        {
            case Loaders.Forge:
            case Loaders.NeoForge:
                var forge = obj.Loader == Loaders.NeoForge ?
                obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
                return [.. forge.MinecraftArguments.Split(' ')];
            case Loaders.Fabric:
                var fabric = obj.GetFabricObj()!;
                return [.. MakeV1GameArg(game), .. fabric.Arguments.Game];
            case Loaders.Quilt:
                var quilt = obj.GetQuiltObj()!;
                return [.. MakeV1GameArg(game), .. quilt.Arguments.Game];
            case Loaders.Custom:
                return obj.GetLoaderGameArg();
            case Loaders.OptiFine:
                return
                [
                    .. MakeV1GameArg(game),
                    "--tweakClass",
                    "optifine.OptiFineTweaker"
                ];
            default:
                return [];
        }
    }

    /// <summary>
    /// 创建V2版游戏参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    private static List<string> MakeLoaderV2GameArg(GameSettingObj obj)
    {
        var game = VersionPath.GetVersion(obj.Version)!;
        if (game.Arguments == null)
        {
            return [];
        }

        var arg = new List<string>();

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
    /// 创建原版游戏Jvm参数
    /// </summary>
    /// <param name="v2"></param>
    /// <param name="game">游戏数据</param>
    /// <returns></returns>
    private static List<string> MakeV2JvmArg(GameArgObj game)
    {
        if (game.Arguments == null)
        {
            return [];
        }

        var list = new List<string>();
        //添加原版参数
        foreach (object item in game.Arguments.Jvm)
        {
            if (item is string str)
            {
                #region Phone
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
                #endregion
                list.Add(str);
            }
            else if (item is GameArgObj.ArgumentsObj.JvmObj obj1)
            {
                //检查是否需要使用
                if (!CheckHelpers.CheckAllow(obj1.Rules))
                {
                    continue;
                }

                if (obj1.Value is string item2)
                {
                    list.Add(item2!);
                }
                else if (obj1.Value is List<string> list1)
                {
                    list.AddRange(list1);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 创建加载器Jvm参数
    /// </summary>
    /// <param name="v2"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static List<string> MakeLoaderJvmArg(bool v2, GameSettingObj obj)
    {
        var list = new List<string>();
        switch (obj.Loader)
        {
            case Loaders.Forge:
            case Loaders.NeoForge:
                if (v2)
                {
                    list.Add($"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}");
                    list.Add($"-Dforgewrapper.installer={(obj.Loader == Loaders.NeoForge ?
                        GameDownloadHelper.BuildNeoForgeInstaller(obj.Version, obj.LoaderVersion!).Local :
                        GameDownloadHelper.BuildForgeInstaller(obj.Version, obj.LoaderVersion!).Local)}");
                    list.Add($"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}");

                    var forge = obj.Loader == Loaders.NeoForge ? obj.GetNeoForgeObj()! : obj.GetForgeObj()!;
                    if (forge.Arguments.Jvm != null)
                    {
                        list.AddRange(forge.Arguments.Jvm);
                    }
                    #region Phone
#if Phone
                    if (SystemInfo.Os == OsType.Android)
                    {
                        jvm.Add("-Dforgewrapper.justLaunch=true");
                    }
#endif
                    #endregion
                }
                break;
            case Loaders.Fabric:
                var fabric = obj.GetFabricObj()!;
                list.AddRange(fabric.Arguments.Jvm);
                break;
            //case Loaders.Quilt:
            //    var quilt = obj.GetQuiltObj()!;
            //    break;
            case Loaders.OptiFine:
                list.Add($"-Dlibdir={LibrariesPath.BaseDir}");
                list.Add($"-Dgamecore={LibrariesPath.GetGameFile(obj.Version)}");
                list.Add($"-Doptifine={LibrariesPath.GetOptifineFile(obj)}");
                if (v2)
                {
                    list.Add("--add-opens");
                    list.Add("java.base/java.lang=ALL-UNNAMED");
                    list.Add("--add-opens");
                    list.Add("java.base/java.util=ALL-UNNAMED");
                    list.Add("--add-opens");
                    list.Add("java.base/java.net=ALL-UNNAMED");
                    list.Add("--add-opens");
                    list.Add("java.base/jdk.internal.loader=ALL-UNNAMED");
                }
                break;
            case Loaders.Custom:
                list.AddRange(obj.GetLoaderGameArg());
                break;
        }

        return list;
    }

    /// <summary>
    /// 创建启动器游戏启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="world">启动的世界</param>
    /// <param name="server">加入的服务器</param>
    /// <returns>游戏启动参数</returns>
    private static List<string> MakeGameArg(this GameSettingObj obj, WorldObj? world, ServerObj? server)
    {
        var gameArg = new List<string>();
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
    /// 创建启动器Jvm参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="mixinport">注入通信端口</param>
    /// <returns>Jvm参数</returns>
    private static async Task<(List<string>, bool)> MakeJvmArgAsync(this GameSettingObj obj, LoginObj login, int? mixinport = null)
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
                    ?? ConfigUtils.Config.DefaultJvmArg.MinMemory,
                ColorASM = obj.JvmArg != null ? obj.JvmArg.ColorASM
                    : ConfigUtils.Config.DefaultJvmArg.ColorASM
            };
        }

        var jvm = new List<string>();
        bool useasm = false;
        //javaagent
        if (!string.IsNullOrWhiteSpace(args.JavaAgent))
        {
            jvm.Add($"-javaagent:{args.JavaAgent.Trim()}");
        }

        //colorasm
        if (args.ColorASM == false && mixinport > 0)
        {
            useasm = true;
            GameHelper.ReadyColorMCASM();
            jvm.Add("-Dcolormc.mixin.port=" + mixinport);
            jvm.Add("-Dcolormc.mixin.uuid=" + obj.UUID);
            jvm.Add($"-javaagent:{GameHelper.ColorMCASM.Local}");
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

        jvm.Add($"-Dcolormc.dir={ColorMCCore.BaseDir}");
        jvm.Add($"-Dcolormc.game.uuid={obj.UUID}");
        jvm.Add($"-Dcolormc.game.name={obj.Name}");
        jvm.Add($"-Dcolormc.game.version={obj.Version}");
        jvm.Add($"-Dcolormc.game.dir={obj.GetGamePath()}");

        return (jvm, useasm);
    }

    /// <summary>
    /// 替换参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="login">登录的账户</param>
    /// <param name="args">所有参数参数</param>
    /// <param name="classpath">classpath</param>
    /// <param name="native">native路径</param>
    /// <param name="native">native路径</param>
    private static void ReplaceAll(this GameSettingObj obj, LoginObj login, List<string> args, string classpath, string native, string asset)
    {
        var assetsPath = AssetsPath.BaseDir;
        var gameDir = InstancesPath.GetGamePath(obj);
        var assetsIndexName = asset ?? "legacy";

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
                ? "%natives_directory%" : native },
#else
            {"${natives_directory}", native },
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
    /// 创建所有启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="arg">游戏启动参数</param>
    /// <returns>参数列表</returns>
    public static List<string> MakeRunArg(this GameSettingObj obj, GameLaunchArg larg, GameLaunchObj arg, bool check)
    {
        var classpath = new StringBuilder();
        var sep = SystemInfo.Os == OsType.Windows ? ';' : ':';
        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info2"));

        if (arg.UseColorMCASM)
        {
            arg.GameLibs.Add(GameHelper.ColorMCASM);
        }

        var libraries = obj.GetLibs(arg);

        //附加的classpath
        if (!string.IsNullOrWhiteSpace(obj.AdvanceJvm?.ClassPath))
        {
            var list = obj.AdvanceJvm.ClassPath.Split(';');
            var dir1 = obj.GetGamePath();
            var dir2 = obj.GetBasePath();
            foreach (var item1 in list)
            {
                var path = item1
                    .Replace(Launch.GAME_DIR, dir1)
                    .Replace(Launch.GAME_BASE_DIR, dir2)
                    .Trim();
                path = Path.GetFullPath(path);
                if (!check || File.Exists(path))
                {
                    libraries.Add(path);
                }
            }
        }

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            if (!check || File.Exists(item))
            {
                classpath.Append($"{item}{sep}");
                ColorMCCore.OnGameLog(obj, $"    {item}");
            }
        }
        classpath.Remove(classpath.Length - 1, 1);

        var list1 = new List<string>();

        var cp = classpath.ToString().Trim();
        obj.ReplaceAll(larg.Auth, arg.JvmArgs, cp, arg.NativeDir, arg.Assets.Id);
        obj.ReplaceAll(larg.Auth, arg.GameArgs, cp, arg.NativeDir, arg.Assets.Id);
        //jvm
        list1.AddRange(arg.JvmArgs);

        //mainclass
        list1.Add(arg.MainClass);

        //gamearg
        list1.AddRange(arg.GameArgs);

        return list1;
    }

    /// <summary>
    /// 创建主类
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="version">游戏数据</param>
    /// <param name="v2">是否为v2版本</param>
    /// <returns>主类</returns>
    private static string MakeMainClass(this GameSettingObj obj)
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
    /// 创建游戏完整启动内容
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="gamearg"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    /// <exception cref="LaunchException"></exception>
    public static async Task<GameLaunchObj> MakeArgAsync(this GameSettingObj obj, GameLaunchArg arg1, CancellationToken cancel)
    {
        var arg = new GameLaunchObj();

        //创建启动器自定义jvmarg
        var jvmarg = await obj.MakeJvmArgAsync(arg1.Auth, arg1.Mixinport);
        //创建启动器自定义gamearg
        var gamearg = obj.MakeGameArg(arg1.World, arg1.Server);

        arg.UseColorMCASM = jvmarg.Item2;

        if (obj.CustomLoader?.CustomJson != true)
        {
            var game = await CheckHelpers.CheckGameArgFile(obj);
            var v2 = obj.IsGameVersionV2();
            arg.JavaVersions.Add(game.JavaVersion.MajorVersion);

            //处理运行库
            await Task.Run(async () =>
            {
                arg.NativeDir = LibrariesPath.GetNativeDir(obj.Version);
                arg.GameJar = GameDownloadHelper.BuildGameItem(game.Id);

                if (obj.Loader != Loaders.Custom || obj.CustomLoader?.RemoveLib != true)
                {
                    foreach (var item in await game.BuildGameLibsAsync(arg.NativeDir))
                    {
                        if (!string.IsNullOrWhiteSpace(item.Local))
                        {
                            arg.GameLibs.Add(item);
                        }
                    }
                }

                List<FileItemObj>? loader = null;
                List<FileItemObj>? install = null;
                //根据加载器处理
                switch (obj.Loader)
                {
                    case Loaders.Forge:
                    case Loaders.NeoForge:
                        var res2 = obj.GetForgeLibs();
                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        if (res2 == null)
                        {
                            res2 = await obj.GetDownloadForgeLibs();
                            if (res2 == null)
                            {
                                throw new LaunchException(LaunchState.LostLoader, LanguageHelper.Get("Core.Launch.Error3"));
                            }
                        }
                        loader = res2.Loaders;
                        install = res2.Installs;
                        if (v2)
                        {
                            GameHelper.ReadyForgeWrapper();
                            loader!.Add(GameHelper.ForgeWrapper);
                        }
                        break;
                    case Loaders.Fabric:
                        loader = obj.GetFabricLibs();
                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        loader ??= await obj.GetDownloadFabricLibs()
                            ?? throw new LaunchException(LaunchState.LostLoader, LanguageHelper.Get("Core.Launch.Error3"));
                        break;
                    case Loaders.Quilt:
                        loader = obj.GetQuiltLibs();
                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        loader ??= await obj.GetDownloadQuiltLibs()
                            ?? throw new LaunchException(LaunchState.LostLoader, LanguageHelper.Get("Core.Launch.Error3"));
                        break;
                    case Loaders.OptiFine:
                        loader = obj.GetOptifineLibs();
                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        loader ??= await obj.GetDownloadOptifineLibs()
                            ?? throw new LaunchException(LaunchState.LostLoader, LanguageHelper.Get("Core.Launch.Error3"));
                        GameHelper.ReadyOptifineWrapper();
                        loader.Add(GameHelper.OptifineWrapper);
                        break;
                    case Loaders.Custom:
                        if (obj.CustomLoader == null || !File.Exists(obj.GetGameLoaderFile()))
                        {
                            throw new LaunchException(LaunchState.LostLoader, LanguageHelper.Get("Core.Launch.Error3"));
                        }

                        if (cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        var res1 = await GameDownloadHelper.DecodeLoaderJarAsync(obj, obj.GetGameLoaderFile(), cancel)
                        ?? throw new LaunchException(LaunchState.LostLoader, LanguageHelper.Get("Core.Launch.Error3"));
                        loader = res1.List?.ToList();
                        break;
                }
                if (loader != null)
                {
                    foreach (var item in loader)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Local))
                        {
                            arg.LoaderLibs.Add(item);
                        }
                    }
                }
                if (install != null)
                {
                    foreach (var item in install)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Local))
                        {
                            arg.InstallerLibs.Add(item);
                        }
                    }
                }
            }, cancel);

            if (obj.JvmArg?.RemoveGameArg != true)
            {
                if (v2)
                {
                    arg.JvmArgs.AddRange(MakeV2JvmArg(game));
                }
                else
                {
                    arg.JvmArgs.AddRange(s_v1JvmArg);
                }
                arg.JvmArgs.AddRange(MakeLoaderJvmArg(v2, obj));
            }

            arg.JvmArgs.AddRange(jvmarg.Item1);

            if (obj.JvmArg?.RemoveGameArg != true)
            {
                if (v2)
                {
                    arg.GameArgs.AddRange(MakeV2GameArg(game));
                    arg.GameArgs.AddRange(MakeLoaderV2GameArg(obj));
                }
                else
                {
                    if (obj.Loader != Loaders.Normal)
                    {
                        arg.GameArgs.AddRange(MakeLoaderV1GameArg(obj, game));
                    }
                    else
                    {
                        arg.GameArgs.AddRange(MakeV1GameArg(game));
                    }
                }
            }

            arg.GameArgs.AddRange(gamearg);

            //材质与主类
            var assets = game.AssetIndex.GetIndex();
            if (assets == null)
            {
                //不存在json文件
                var res = await GameAPI.GetAssets(game.AssetIndex.Url)
                    ?? throw new LaunchException(LaunchState.AssetsError, LanguageHelper.Get("Core.Launch.Error2"));
                assets = res.Assets;
                game.AddIndex(res.Text);
            }

            arg.Assets = game.AssetIndex;

            arg.MainClass = MakeMainClass(obj);
        }
        else
        {
            GameArgObj.LoggingObj? logging = null;
            foreach (var item in obj.CustomJson)
            {
                //安全log4j
                if (item.Logging != null)
                {
                    logging = item.Logging;
                }

                if (item.Downloads != null)
                {
                    if (item.MinecraftVersion != null)
                    {
                        var file = LibrariesPath.GetGameFile(item.MinecraftVersion);
                        arg.NativeDir ??= LibrariesPath.GetNativeDir(obj.Version);
                        arg.GameJar = new()
                        {
                            Url = CoreHttpClient.Source == SourceLocal.Offical ? item.Downloads.Client.Url
                                : UrlHelper.DownloadGame(item.MinecraftVersion, CoreHttpClient.Source),
                            Sha1 = item.Downloads.Client.Sha1,
                            Local = file,
                            Name = $"{item.MinecraftVersion}.jar"
                        };
                    }
                    else
                    {
                        arg.GameJar = new()
                        {
                            Url = UrlHelper.DownloadSourceChange(item.Downloads.Client.Url, CoreHttpClient.Source),
                            Sha1 = item.Downloads.Client.Sha1,
                            Local = LibrariesPath.GetGameFileWithDir(item.Id),
                            Name = $"{item.MinecraftVersion}.jar"
                        };
                    }
                }

                //材质
                if (item.AssetIndex != null)
                {
                    var assets = item.AssetIndex.GetIndex();
                    if (assets == null)
                    {
                        var res = await GameAPI.GetAssets(item.AssetIndex.Url)
                            ?? throw new LaunchException(LaunchState.AssetsError, LanguageHelper.Get("Core.Launch.Error2"));
                        assets = res.Assets;
                        item.AddIndex(res.Text);
                    }
                    arg.Assets = item.AssetIndex;
                }

                //运行库
                if (item.Libraries != null)
                {
                    arg.NativeDir ??= LibrariesPath.GetNativeDir(null);
                    var list = await item.BuildGameLibsAsync(arg.NativeDir, obj);
                    foreach (var item1 in list)
                    {
                        if (!string.IsNullOrWhiteSpace(item1.Local))
                        {
                            arg.GameLibs.Add(item1);
                        }
                    }
                }

                //Java版本
                if (item.JavaVersion != null)
                {
                    arg.JavaVersions.Add(item.JavaVersion.MajorVersion);
                }

                //主类
                if (item.MainClass != null)
                {
                    arg.MainClass = item.MainClass;
                }

                //游戏参数
                if (item.MinecraftArguments != null)
                {
                    var args = MakeV1GameArg(item);
                    arg.GameArgs.AddRange(args);
                }

                //启动参数
                if (item.Arguments != null)
                {
                    var args = MakeV2GameArg(item);
                    arg.GameArgs.AddRange(args);
                    args = MakeV2JvmArg(item);
                    arg.JvmArgs.AddRange(args);
                }

                //下面是MMC特性
                if (item.MainJar is { } lib)
                {
                    bool download = CheckHelpers.CheckAllow(lib.Rules);
                    if (download)
                    {
                        var file = lib.Downloads.Artifact.Path;
                        if (string.IsNullOrEmpty(file))
                        {
                            file = FuntionUtils.VersionNameToPath(lib.Name);
                        }

                        arg.GameJar = new()
                        {
                            Name = lib.Name,
                            Url = UrlHelper.DownloadLibraries(lib.Downloads.Artifact.Url, CoreHttpClient.Source),
                            Local = $"{LibrariesPath.BaseDir}/{file}",
                            Sha1 = lib.Downloads.Artifact.Sha1
                        };
                    }
                }

                if (item.CompatibleJavaMajors != null)
                {
                    foreach (var item1 in item.CompatibleJavaMajors)
                    {
                        arg.JavaVersions.Add(item1);
                    }
                }

                if (item.AddJvmArgs != null)
                {
                    if (arg.JvmArgs.Count == 0)
                    {
                        arg.JvmArgs.AddRange(s_v1JvmArg);
                    }

                    arg.JvmArgs.AddRange(item.AddJvmArgs);
                }

                if (item.AddTweakers != null)
                {
                    arg.GameArgs.Add("--tweakClass");
                    foreach (var item1 in item.AddTweakers)
                    {
                        arg.GameArgs.Add(item1);
                    }
                }
            }

            if (arg.JvmArgs.Count == 0)
            {
                arg.JvmArgs.AddRange(s_v1JvmArg);
            }

            arg.JvmArgs.AddRange(jvmarg.Item1);
            arg.GameArgs.AddRange(gamearg);

            //log4j2-xml
            if (logging != null && ConfigUtils.Config.SafeLog4j)
            {
                var obj1 = GameDownloadHelper.BuildLog4jItem(logging);
                arg.Log4JXml = obj1;
                arg.JvmArgs.Add(logging.Client.Argument.Replace("${path}", obj1.Local));
            }
        }
        return arg;
    }

    public static GameArgObj.AssetIndexObj? FindAsset(this GameSettingObj obj)
    {
        if (obj.CustomLoader?.CustomJson != true)
        {
            var version = VersionPath.GetVersion(obj.Version);
            if (version != null)
            {
                return version.AssetIndex;
            }
        }
        else
        {
            GameArgObj.AssetIndexObj? assetIndex = null;
            foreach (var item in obj.CustomJson)
            {
                //材质
                if (item.AssetIndex != null)
                {
                    assetIndex = item.AssetIndex;
                }
            }
            return assetIndex;
        }

        return null;
    }
}
