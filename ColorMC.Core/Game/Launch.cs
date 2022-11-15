using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ColorMC.Core.Game;

public enum LaunchState
{
    Check, LostVersion, LostLib, LostAssets, LostLoader, Download,
    JvmPrepare,
    VersionError, AssetsError, LoaderError, JvmError
}

public static class Launch
{
    public static async Task<List<DownloadItem>?> CheckGameFile(GameSettingObj obj)
    {
        var list = new List<DownloadItem>();
        var game = VersionPath.GetGame(obj.Version);
        if (game == null)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostVersion);
            var res = CoreMain.GameDownload?.Invoke(obj);
            if (res != true)
                return null;

            var version = VersionPath.Versions?.versions.Where(a => a.id == obj.Version).FirstOrDefault();
            if (version == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.VersionError);
                return null;
            }

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

        string file = $"{VersionPath.BaseDir}/{obj.Version}.jar";
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
            string sha1 = Sha1.GenSha1(stream2);
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
        foreach (var item in list1)
        {
            list.Add(new()
            {
                Overwrite = true,
                Url = UrlHelp.DownloadAssets(item, BaseClient.Source),
                SHA1 = item,
                Local = $"{AssetsPath.ObjectsDir}/{item[..2]}/{item}",
                Name = item
            });
        }

        list.AddRange(LibrariesPath.CheckGame(game));

        if (obj.Loader == Loaders.Forge)
        {
            var list3 = LibrariesPath.CheckForge(obj);
            if (list3 == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.LostLoader);
                var res = CoreMain.GameDownload?.Invoke(obj);

                if (res != true)
                    return null;

                var list4 = await GameDownload.DownloadForge(obj.Version, obj.LoaderInfo.Version);
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
                var res = CoreMain.GameDownload?.Invoke(obj);

                if (res != true)
                    return null;

                var list4 = await GameDownload.DownloadFabric(obj.Version, obj.LoaderInfo.Version);
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
                var res = CoreMain.GameDownload?.Invoke(obj);

                if (res != true)
                    return null;

                var list4 = await GameDownload.DownloadQuilt(obj.Version, obj.LoaderInfo.Version);
                if (list4.State != DownloadState.End)
                    return null;

                list.AddRange(list4.List!);
            }
            else
            {
                list.AddRange(list3);
            }
        }

        return list;
    }

    public static JavaInfo? FindJvm(GameSettingObj obj)
    {
        var game = VersionPath.GetGame(obj.Version)!;
        var jv = game.javaVersion.majorVersion;
        var list = JvmPath.Jvms.Where(a => a.Value.MajorVersion >= jv)
            .Select(a => a.Value);

        if (!list.Any())
        {
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

    public static List<string> JvmArg(GameSettingObj obj, bool v2)
    {
        JvmArgObj args = new();

        if (obj.JvmArg == null)
        {
            ConfigUtils.Config.DefaultJvmArg.CopyTo(args);
        }
        else
        {
            args.AdvencedJvmArguments = obj.JvmArg.AdvencedJvmArguments ??
                ConfigUtils.Config.DefaultJvmArg.AdvencedJvmArguments;
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
            case JvmArgObj.GCType.G1GC:
                jvmHead.Add("-XX:+UseG1GC");
                break;
            case JvmArgObj.GCType.SerialGC:
                jvmHead.Add("-XX:+UseSerialGC");
                break;
            case JvmArgObj.GCType.ParallelGC:
                jvmHead.Add("-XX:+UseParallelGC");
                break;
            case JvmArgObj.GCType.CMSGC:
                jvmHead.Add("-XX:+UseConcMarkSweepGC");
                break;
            case JvmArgObj.GCType.User:
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
        if (!string.IsNullOrWhiteSpace(args.AdvencedJvmArguments))
        {
            jvmHead.Add(args.AdvencedJvmArguments);
        }

        if (v2 && obj.Loader == Loaders.Forge)
        {
            jvmHead.Add($"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}");
            jvmHead.Add($"-Dforgewrapper.installer={ForgeHelp.BuildForgeInster(obj.Version, obj.LoaderInfo.Version).Local}");
            jvmHead.Add($"-Dforgewrapper.minecraft={VersionPath.BaseDir}/{obj.Version}.jar");
        }

        //jvmHead.Add("-Djava.rmi.server.useCodebaseOnly=true");
        //jvmHead.Add("-XX:+UnlockExperimentalVMOptions");
        //jvmHead.Add("-Dfml.ignoreInvalidMinecraftCertificates=true");
        //jvmHead.Add("-Dfml.ignorePatchDiscrepancies=true");
        //jvmHead.Add("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false");
        //jvmHead.Add("-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false");
        //jvmHead.Add($"-Dminecraft.client.jar={VersionPath.BaseDir}/{obj.Version}.jar");

        jvmHead.AddRange(v2 ? MakeV2JvmArg(obj) : MakeV1JvmArg());

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

            if (server.State == ServerInfo.StateType.GOOD)
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

        if (!string.IsNullOrWhiteSpace(obj.AdvencedGameArguments))
        {
            gameArg.Add(obj.AdvencedGameArguments.Trim());
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

        return gameArg;
    }

    public static void AddOrUpdate(this Dictionary<LibVersionObj, string> dic, 
        LibVersionObj key, string value)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }

    public static List<string> GetLibs(GameSettingObj obj, bool v2)
    {
        Dictionary<LibVersionObj, string> list = new();
        var version = VersionPath.GetGame(obj.Version)!;
        var list1 = GameHelp.MakeGameLibs(version);
        foreach (var item in list1)
        {
            if (item.Later == null)
                list.AddOrUpdate(PathC.MakeVersionObj(item.Name), item.Local);
        }

        if (obj.Loader == Loaders.Forge)
        {
            var forge = VersionPath.GetForgeObj(obj)!;

            var list2 = ForgeHelp.MakeForgeLibs(forge, obj.Version, obj.LoaderInfo.Version);

            list2.ForEach(a => list.AddOrUpdate(PathC.MakeVersionObj(a.Name), a.Local));

            if (v2)
            {
                list.AddOrUpdate(new(), ForgeHelp.ForgeWrapper);
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

        return new(list.Values) { $"{VersionPath.BaseDir}/{obj.Version}.jar" };
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
        string gameDir = InstancesPath.GetDir(obj);
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
            Loaders.Forge => $"forge-{obj.Version}-{obj.LoaderInfo.Version}",
            Loaders.Fabric => $"fabric-{obj.Version}-{obj.LoaderInfo.Version}",
            Loaders.Quilt => $"quilt-{obj.Version}-{obj.LoaderInfo.Version}",
            _ => obj.Version
        };
        var libraries = GetLibs(obj, v2);
        StringBuilder arg = new();
        foreach (var item in libraries)
        {
            arg.Append($"{item};");
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
                //{"${auth_session}",login.Token },
                {"${game_assets}",assetsPath },
                {"${user_properties}",UserPropertyToList(login.Properties) },
                {"${user_type}", "legacy" },
                {"${version_type}", "ColorMC" },
                {"${natives_directory}", LibrariesPath.GetNativeDir(obj.Version) },
                {"${library_directory}",LibrariesPath.BaseDir },
                {"${classpath_separator}", ";" },
                {"${launcher_name}","ColorMC" },
                {"${launcher_version}", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
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

        list.AddRange(JvmArg(obj, v2));
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
        CoreMain.GameLaunch?.Invoke(obj, LaunchState.Check);
        var res = await CheckGameFile(obj);
        if (res == null)
            return null;

        if (res.Count != 0)
        {
            DownloadManager.Clear();
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
            DownloadManager.FillAll(res);
            var ok = await DownloadManager.Start();
            if(!ok)
            {
                return null;    
            }
        }

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.JvmPrepare);

        var arg = await MakeArg(obj, login);
        foreach (var item in arg)
        {
            Console.WriteLine(item);
        }

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

        Process process = new();
        process.StartInfo.FileName = jvm.Path;
        process.StartInfo.WorkingDirectory = InstancesPath.GetDir(obj);
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
        CoreMain.ProcessLog(sender as Process, e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        CoreMain.ProcessLog(sender as Process, e.Data);
    }
}
