using ColorMC.Core.Game.Auth;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace ColorMC.Core.Game;

public enum LaunchState
{
    Login, Check, CheckVersion, CheckLib, CheckAssets, CheckLoader, CheckLoginCore,
    LostVersion, LostLib, LostLoader, LostLoginCore, LostGame,
    Download,
    JvmPrepare, 
    VersionError, AssetsError, LoaderError, JvmError, LoginFail
}

public static class Launch
{
    public static async Task<List<DownloadItem>?> CheckGameFile(GameSettingObj obj, LoginObj login)
    {
        var list = new List<DownloadItem>();
        CoreMain.GameLaunch?.Invoke(obj, LaunchState.CheckVersion);
        var game = VersionPath.GetGame(obj.Version);
        if (game == null)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostVersion);
            if (CoreMain.GameDownload == null)
                return null;
            var res = await CoreMain.GameDownload.Invoke(LaunchState.LostVersion, obj);
            if (res != true)
                return null;

            var version = VersionPath.Versions?.versions.Where(a => a.id == obj.Version).FirstOrDefault();
            if (version == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.VersionError);
                return null;
            }

            CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
            var res1 = await GameDownload.Download(version);
            if (res1.State != DownloadState.End)
                return null;

            list.AddRange(res1.List!);

            game = VersionPath.GetGame(obj.Version)!;
        }

        if (game == null)
        {
            return null;
        }

        string file = LibrariesPath.MakeGameDir(game.id);
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
            using FileStream stream2 = new(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
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

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.CheckAssets);
        var assets = AssetsPath.GetIndex(game);
        if (assets == null)
        {
            assets = await Get.GetAssets(game.assetIndex.url);
            if (assets == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.AssetsError);
                return null;
            }
            AssetsPath.AddIndex(assets, game);
        }

        var list1 = AssetsPath.Check(assets);
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

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.CheckLib);
        var list2 = LibrariesPath.CheckGame(game);
        if (list2.Count != 0)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostLib);
            list.AddRange(list2);
        }

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.CheckLoader);
        if (obj.Loader == Loaders.Forge)
        {
            var list3 = LibrariesPath.CheckForge(obj);
            if (list3 == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostLoader);
                if (CoreMain.GameDownload == null)
                    return null;
                var res = await CoreMain.GameDownload.Invoke(LaunchState.LostLoader, obj);
                if (res != true)
                    return null;

                CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
                var list4 = await GameDownload.DownloadForge(obj.Version, obj.LoaderVersion);
                if (list4.State != DownloadState.End)
                    return null;

                list.AddRange(list4.List!);
            }
            else
            {
                list.AddRange(list3);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var list3 = LibrariesPath.CheckFabric(obj);
            if (list3 == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostLoader);
                if (CoreMain.GameDownload == null)
                    return null;
                var res = await CoreMain.GameDownload.Invoke(LaunchState.LostLoader, obj);
                if (res != true)
                    return null;

                CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
                var list4 = await GameDownload.DownloadFabric(obj.Version, obj.LoaderVersion);
                if (list4.State != DownloadState.End)
                    return null;

                list.AddRange(list4.List!);
            }
            else
            {
                list.AddRange(list3);
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var list3 = LibrariesPath.CheckQuilt(obj);
            if (list3 == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostLoader);
                if (CoreMain.GameDownload == null)
                    return null;
                var res = await CoreMain.GameDownload.Invoke(LaunchState.LostLoader, obj);
                if (res != true)
                    return null;

                CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
                var list4 = await GameDownload.DownloadQuilt(obj.Version, obj.LoaderVersion);
                if (list4.State != DownloadState.End)
                    return null;

                list.AddRange(list4.List!);
            }
            else
            {
                list.AddRange(list3);
            }
        }

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.CheckLoginCore);

        if (login.AuthType == AuthType.Nide8)
        {
            var item = AuthHelper.ReadyNide8();
            if (item != null)
            {
                list.Add(item);
            }
        }
        else if (login.AuthType is AuthType.AuthlibInjector
            or AuthType.LittleSkin or AuthType.SelfLittleSkin)
        {
            await AuthHelper.ReadyAuthlibInjector();
        }

        return list;
    }

    public static JavaInfo? FindJvm(GameSettingObj obj)
    {
        var game = VersionPath.GetGame(obj.Version)!;
        var jv = game.javaVersion.majorVersion;
        var list = JvmPath.Jvms.Where(a => a.Value.MajorVersion == jv)
            .Select(a => a.Value);

        if (!list.Any() && jv > 8)
        {
            list = JvmPath.Jvms.Where(a => a.Value.MajorVersion >= jv)
            .Select(a => a.Value);
            if (!list.Any())
                return null;
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

    public readonly static List<string> V1JvmArg = new()
    {
        "-Djava.library.path=${natives_directory}", "-cp", "${classpath}"
    };

    public static List<string> MakeV1JvmArg() => V1JvmArg;

    public static List<string> MakeV2JvmArg(GameSettingObj obj)
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
            else if (item is JObject)
            {
                JObject obj1 = item as JObject;
                var obj2 = obj1.ToObject<GameArgObj.Arguments.Jvm>();
                bool use = CheckRule.CheckAllow(obj2.rules);
                if (!use)
                    continue;

                if (obj2.value is string)
                {
                    string? item2 = obj2.value as string;
                    arg.Add(item2);
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
            var forge = VersionPath.GetForgeObj(obj)!;
            if (forge.arguments.jvm != null)
                foreach (var item in forge.arguments.jvm)
                {
                    arg.Add(item);
                }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = VersionPath.GetFabricObj(obj)!;
            foreach (var item in fabric.arguments.jvm)
            {
                arg.Add(item);
            }
        }

        return arg;
    }

    public static List<string> MakeV1GameArg(GameSettingObj obj)
    {
        if (obj.Loader == Loaders.Forge)
        {
            var forge = VersionPath.GetForgeObj(obj)!;
            return new(forge.minecraftArguments.Split(" "));
        }

        var version = VersionPath.GetGame(obj.Version)!;
        return new(version.minecraftArguments.Split(" "));
    }

    public static List<string> MakeV2GameArg(GameSettingObj obj)
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
            var forge = VersionPath.GetForgeObj(obj)!;
            foreach (var item in forge.arguments.game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = VersionPath.GetFabricObj(obj)!;
            foreach (var item in fabric.arguments.game)
            {
                arg.Add(item);
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = VersionPath.GetQuiltObj(obj)!;
            foreach (var item in quilt.arguments.game)
            {
                arg.Add(item);
            }
        }

        return arg;
    }

    public static async Task<List<string>> JvmArg(GameSettingObj obj, bool v2, LoginObj login)
    {
        JvmArgObj args = new();

        if (obj.JvmArg == null)
        {
            ConfigUtils.Config.DefaultJvmArg.CopyTo(args);
        }
        else
        {
            args.JvmArgs = obj.JvmArg.JvmArgs ??
                ConfigUtils.Config.DefaultJvmArg.JvmArgs;
            args.GCArgument = obj.JvmArg.GCArgument ??
                ConfigUtils.Config.DefaultJvmArg.GCArgument;
            args.GC = obj.JvmArg.GC ??
                ConfigUtils.Config.DefaultJvmArg.GC;
            args.JavaAgent = obj.JvmArg.JavaAgent ??
                ConfigUtils.Config.DefaultJvmArg.JavaAgent;
            args.MaxMemory = obj.JvmArg.MaxMemory ??
                ConfigUtils.Config.DefaultJvmArg.MaxMemory;
            args.MinMemory = obj.JvmArg.MinMemory ??
                ConfigUtils.Config.DefaultJvmArg.MinMemory;
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
            jvmHead.Add(args.JvmArgs);
        }

        if (v2 && obj.Loader == Loaders.Forge)
        {
            jvmHead.Add($"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}");
            jvmHead.Add($"-Dforgewrapper.installer={ForgeHelper.BuildForgeInster(obj.Version, obj.LoaderVersion).Local}");
            jvmHead.Add($"-Dforgewrapper.minecraft={LibrariesPath.MakeGameDir(obj.Version)}");
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
            jvmHead.Add($"-javaagent:{AuthHelper.BuildNide8Item().Local}={login.Text1}");
            jvmHead.Add("-Dnide8auth.client=true");
        }
        else if (login.AuthType == AuthType.AuthlibInjector)
        {
            var res = await BaseClient.GetString(login.Text1);
            jvmHead.Add($"-javaagent:{AuthHelper.NowAuthlibInjector}={login.Text1}");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={Funtcions.GenBase64(res)}");
        }
        else if (login.AuthType == AuthType.LittleSkin)
        {
            var res = await BaseClient.GetString("https://littleskin.cn/api/yggdrasil");
            jvmHead.Add($"-javaagent:{AuthHelper.NowAuthlibInjector}=https://littleskin.cn/api/yggdrasil");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={Funtcions.GenBase64(res)}");
        }
        else if (login.AuthType == AuthType.SelfLittleSkin)
        {
            var res = await BaseClient.GetString($"{login.Text1}/api/yggdrasil");
            jvmHead.Add($"-javaagent:{AuthHelper.NowAuthlibInjector}={login.Text1}/api/yggdrasil");
            jvmHead.Add($"-Dauthlibinjector.yggdrasil.prefetched={Funtcions.GenBase64(res)}");
        }

        return jvmHead;
    }

    public static async Task<List<string>> GameArg(GameSettingObj obj, bool v2)
    {
        List<string> gameArg = new();
        gameArg.AddRange(v2 ? MakeV2GameArg(obj) : MakeV1GameArg(obj));

        WindowSettingObj window = obj.Window ?? ConfigUtils.Config.Window;
        if (window.FullScreen)
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

        if (obj.StartServer != null && !string.IsNullOrWhiteSpace(obj.StartServer.IP))
        {
            ServerInfo server = new(obj.StartServer.IP, obj.StartServer.Port);
            await server.StartGetServerInfo();

            if (server.State == StateType.GOOD)
            {
                gameArg.Add($"--server");
                gameArg.Add(server.ServerAddress);
                gameArg.Add($"--port");
                gameArg.Add(server.ServerPort.ToString());
            }
            else
            {
                gameArg.Add($"--server");
                gameArg.Add(server.ServerAddress);
                if (obj.StartServer.Port > 0)
                {
                    gameArg.Add($"--port");
                    gameArg.Add(obj.StartServer.Port.ToString());
                }
            }
        }

        if (obj.ProxyHost != null)
        {
            if (!string.IsNullOrWhiteSpace(obj.ProxyHost.IP))
            {
                gameArg.Add($"--proxyHost");
                gameArg.Add(obj.ProxyHost.IP);
            }
            if (obj.ProxyHost.Port != 0)
            {
                gameArg.Add($"--proxyPort");
                gameArg.Add(obj.ProxyHost.Port.ToString());
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

        if (obj.JvmArg?.GameArgs != null)
        {
            gameArg.Add(obj.JvmArg.GameArgs);
        }

        return gameArg;
    }

    public static void AddOrUpdate(this Dictionary<LibVersionObj, string> dic,
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

    public static List<string> GetLibs(GameSettingObj obj, bool v2)
    {
        Dictionary<LibVersionObj, string> list = new();
        var version = VersionPath.GetGame(obj.Version)!;
        var list1 = GameHelper.MakeGameLibs(version);
        foreach (var item in list1)
        {
            var key = PathC.MakeVersionObj(item.Name);
            //if (key.Name == "lwjgl-platform")
            //    continue;

            if (item.Later == null)
                list.AddOrUpdate(key, item.Local);
        }

        if (obj.Loader == Loaders.Forge)
        {
            var forge = VersionPath.GetForgeObj(obj)!;

            var list2 = ForgeHelper.MakeForgeLibs(forge, obj.Version, obj.LoaderVersion);

            list2.ForEach(a => list.AddOrUpdate(PathC.MakeVersionObj(a.Name), a.Local));

            if (v2)
            {
                list.AddOrUpdate(new(), ForgeHelper.ForgeWrapper);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = VersionPath.GetFabricObj(obj)!;
            foreach (var item in fabric.libraries)
            {
                var name = PathC.ToName(item.name);
                list.AddOrUpdate(PathC.MakeVersionObj(name.Name), $"{LibrariesPath.BaseDir}/{name.Path}");
            }
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = VersionPath.GetQuiltObj(obj)!;
            foreach (var item in quilt.libraries)
            {
                var name = PathC.ToName(item.name);
                list.AddOrUpdate(PathC.MakeVersionObj(name.Name), $"{LibrariesPath.BaseDir}/{name.Path}");
            }
        }

        return new(list.Values) { LibrariesPath.MakeGameDir(obj.Version) };
    }

    public static string UserPropertyToList(List<UserPropertyObj> properties)
    {
        if (properties == null)
        {
            return "{}";
        }
        var sb = new StringBuilder();
        foreach (var item in properties)
        {
            sb.Append($"{item.name}:[{item.value}],");
        }
        var totalSb = new StringBuilder();
        totalSb.Append('{').Append(sb.ToString().TrimEnd(',').Trim()).Append('}');
        return totalSb.ToString();
    }

    public static void ReplaceAll(GameSettingObj obj, LoginObj login, List<string> all_arg, bool v2)
    {
        var version = VersionPath.GetGame(obj.Version)!;
        string assetsPath = AssetsPath.BaseDir;
        string gameDir = InstancesPath.GetGameDir(obj);
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
        var libraries = GetLibs(obj, v2);
        StringBuilder arg = new();
        string sep = SystemInfo.Os == OsType.Windows ? ";" : ":";
        foreach (var item in libraries)
        {
            arg.Append($"{item}{sep}");
        }
        arg.Remove(arg.Length - 1, 1);
        string classpath = arg.ToString().Trim();

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
                {"${user_properties}",UserPropertyToList(login.Properties) },
                {"${user_type}", "legacy" },
                {"${version_type}", "ColorMC" },
                {"${natives_directory}", LibrariesPath.GetNativeDir(obj.Version) },
                {"${library_directory}",LibrariesPath.BaseDir },
                {"${classpath_separator}", sep },
                {"${launcher_name}","ColorMC" },
                {"${launcher_version}", CoreMain.Version },
                {"${classpath}", classpath },
            };

        for (int a = 0; a < all_arg.Count; a++)
        {
            all_arg[a] = argDic.Aggregate(all_arg[a], (a, b) => a.Replace(b.Key, b.Value));
        }
    }

    public static async Task<List<string>> MakeArg(GameSettingObj obj, LoginObj login)
    {
        var list = new List<string>();
        var version = VersionPath.GetGame(obj.Version)!;
        var v2 = CheckRule.GameLaunchVersion(obj.Version);

        list.AddRange(await JvmArg(obj, v2, login));
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
                var forge = VersionPath.GetForgeObj(obj)!;
                list.Add(forge.mainClass);
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var fabric = VersionPath.GetFabricObj(obj)!;
            list.Add(fabric.mainClass);
        }
        else if (obj.Loader == Loaders.Quilt)
        {
            var quilt = VersionPath.GetQuiltObj(obj)!;
            list.Add(quilt.mainClass);
        }

        list.AddRange(await GameArg(obj, v2));

        ReplaceAll(obj, login, list, v2);

        return list;
    }

    public static async Task<Process?> StartGame(this GameSettingObj obj, LoginObj login, JvmConfigObj? jvmCfg = null)
    {
        JavaInfo? jvm = null;
        if (jvmCfg == null)
        {
            jvm = FindJvm(obj);
        }
        else
        {
            jvm = JvmPath.GetInfo(jvmCfg.Name);
        }

        if (jvm == null)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.JvmError);
            return null;
        }

        CoreMain.GameLog?.Invoke(obj, "游戏使用的JAVA");
        CoreMain.GameLog?.Invoke(obj, jvm.Path);

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.Login);
        var login1 = await login.RefreshToken();
        if (login1.State1 != LoginState.Done)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.LoginFail);
            return null;
        }

        login = login1.Obj!;
        AuthDatabase.SaveAuth(login);

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.Check);
        var res = await CheckGameFile(obj, login);
        if (res == null)
            return null;

        if (res.Count != 0)
        {
            if (CoreMain.GameDownload == null)
                return null;
            var res1 = await CoreMain.GameDownload.Invoke(LaunchState.LostGame, obj);
            if (res1 != true)
                return null;

            DownloadManager.Clear();
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
            DownloadManager.FillAll(res);
            var ok = await DownloadManager.Start();
            if (!ok)
            {
                return null;
            }
        }

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.JvmPrepare);

        var arg = await MakeArg(obj, login);
        CoreMain.GameLog?.Invoke(obj, "游戏启动参数");
        foreach (var item in arg)
        {
            CoreMain.GameLog?.Invoke(obj, item);
        }

        Process process = new();
        process.StartInfo.FileName = jvm.Path;
        process.StartInfo.WorkingDirectory = InstancesPath.GetGameDir(obj);
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

        return process;
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        CoreMain.ProcessLog?.Invoke(sender as Process, e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        CoreMain.ProcessLog?.Invoke(sender as Process, e.Data);
    }
}
