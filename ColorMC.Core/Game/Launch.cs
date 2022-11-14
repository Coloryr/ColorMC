using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Http.MoJang;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MoJang;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
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

            await GameDownload.Download(version);

            game = VersionPath.GetGame(obj.Version);
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

        var list2 = LibrariesPath.Check(game);
        foreach (var item in list2)
        {
            list.Add(new()
            {
                Overwrite = true,
                Url = UrlHelp.DownloadLibraries(item.downloads.artifact.url, BaseClient.Source),
                SHA1 = item.downloads.artifact.sha1,
                Local = $"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}",
                Name = item.name
            });
        }

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
                foreach (var item in list3)
                {
                    if (item.name.StartsWith("net.minecraftforge:forge:") 
                        && string.IsNullOrWhiteSpace(item.downloads.artifact.url))
                    {
                        list.Add(new()
                        {
                            Url = UrlHelp.DownloadForgeJar(obj.Version, obj.LoaderInfo.Version, BaseClient.Source),
                            Name = item.name,
                            Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" +
                                PathC.MakeForgeName(obj.Version, obj.LoaderInfo.Version),
                            SHA1 = item.downloads.artifact.sha1
                        });
                    }
                    else
                    {
                        list.Add(new()
                        {
                            Url = UrlHelp.DownloadForgeLib(item.downloads.artifact.url,
                                BaseClient.Source),
                            Name = item.name,
                            Local = $"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}",
                            SHA1 = item.downloads.artifact.sha1
                        });
                    }
                }
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

                await GameDownload.DownloadFabric(obj.Version, obj.LoaderInfo.Version);
            }
            else
            {
                foreach (var item in list3)
                {
                    var name = PathC.ToName(item.name);
                    list.Add(new()
                    {
                        Url = UrlHelp.DownloadFabric(BaseClient.Source) + name.Item1,
                        Name = name.Item2,
                        Local = $"{LibrariesPath.BaseDir}/{name.Item1}"
                    });
                }
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

    public static List<string> MakeV1JvmArg()
    {
        return new() 
        { 
            "-Djava.library.path=${natives_directory}", "-cp ${classpath}" 
        };
    }

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
            foreach (var item in forge.arguments.jvm)
            {
                arg.Add(item);
            }
        }
        else if(obj.Loader == Loaders.Fabric)
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
            return new() { forge.minecraftArguments };
        }

        var version = VersionPath.GetGame(obj.Version)!;
        return new() { version.minecraftArguments };
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

        jvmHead.AddRange(v2 ? MakeV2JvmArg(obj) : MakeV1JvmArg());

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
            jvmHead.Add($"-Dforgewrapper.installer={VersionPath.ForgeDir}/" +
                $"forge-{obj.Version}-{obj.LoaderInfo.Version}-installer.jar");
            jvmHead.Add($"-Dforgewrapper.minecraft={VersionPath.BaseDir}/{obj.Version}.jar");
        }

        //jvmHead.Append("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true ");

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
                gameArg.Add($"--server {server.ServerAddress}");
                   gameArg.Add($"--port {server.ServerPort}");
            }
            else
            {
                gameArg.Add($"--server {server.ServerAddress}");
                if (obj.StartServer.Port > 0)
                {
                    gameArg.Add($"--port {obj.StartServer.Port}");
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
                gameArg.Add($"--proxyHost {obj.ProxyHost.IP}");
            }
            if (obj.ProxyHost.Port != 0)
            {
                gameArg.Add($"--proxyPort {obj.ProxyHost.Port}");
            }
            if (!string.IsNullOrWhiteSpace(obj.ProxyHost.User))
            {
                gameArg.Add($"--proxyUser {obj.ProxyHost.User}");
            }
            if (!string.IsNullOrWhiteSpace(obj.ProxyHost.Password))
            {
                gameArg.Add($"--proxyPass {obj.ProxyHost.Password}");
            }
        }

        return gameArg;
    }

    public static List<string> GetLibs(GameSettingObj obj, bool v2)
    {
        List<string> list = new();
        var version = VersionPath.GetGame(obj.Version)!;
        var list1 = version.libraries;
        foreach (var item in list1)
        {
            if (CheckRule.CheckAllow(item.rules) && item.downloads.artifact != null)
            {
                list.Add($"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}");
            }
        }

        if (obj.Loader == Loaders.Forge)
        {
            var forge = VersionPath.GetForgeObj(obj)!;
            foreach (var item1 in forge.libraries)
            {
                if (item1.name.StartsWith("net.minecraftforge:forge:")
                    && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
                {
                    list.Add($"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" +
                                PathC.MakeForgeName(obj.Version, obj.LoaderInfo.Version));
                }
                else
                {
                    list.Add($"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}");
                }
            }

            if (v2)
            {
                list.Add(LibrariesPath.ForgeWrapper);
                list.Add($"{VersionPath.BaseDir}/{obj.Version}.jar");
            }
        }
        else if (obj.Loader == Loaders.Fabric) 
        {
            var fabric = VersionPath.GetFabricObj(obj)!;
            foreach (var item in fabric.libraries)
            {
                var name = PathC.ToName(item.name);
                list.Add($"{LibrariesPath.BaseDir}/{name.Item1}");
            }
        }

        return list;
    }

    private static string GetClassPaths(List<string> libs)
    {
        StringBuilder arg = new();

        foreach (var item in libs)
        {
            arg.Append($"{item};");
        }

        arg.Remove(arg.Length - 1, 1);

        return arg.ToString().Trim();
    }

    public static string ToList(List<UserPropertyObj> properties)
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
            _ => obj.Version
        };
        var libraries = GetLibs(obj, v2);
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
                {"${user_properties}",ToList(login.Properties) },
                {"${user_type}", $"{login.AuthType}" },
                {"${version_type}", "ColorMC" },
                {"${natives_directory}", $"{obj.Dir}/$natives" },
                {"${library_directory}",LibrariesPath.BaseDir },
                {"${classpath_separator}", ";" },
                {"${launcher_name}","ColorMC" },
                {"${launcher_version}", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                {"${classpath}", GetClassPaths(libraries) },
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
        list.AddRange(await GameArg(obj, v2));

        ReplaceAll(obj, login, list, v2);

        return list;
    }

    public static async Task<Process?> StartGame(GameSettingObj obj, LoginObj login, JvmConfigObj? jvmCfg = null) 
    {
        CoreMain.GameLaunch?.Invoke(obj, LaunchState.Check);
        var res = await CheckGameFile(obj);
        if (res == null)
            return null;

        if (res.Count != 0)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
            DownloadManager.FillAll(res);
            await DownloadManager.Start();
        }

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.JvmPrepare);

        var arg = await MakeArg(obj, login);
        foreach (var item in arg)
        {
            Console.WriteLine(item);
        }

        //JavaInfo? jvm = null;
        //if (jvmCfg == null)
        //{
        //    jvm = FindJvm(obj);
        //}
        //else
        //{
        //    jvm = JvmPath.GetInfo(jvmCfg.Name);
        //}

        //if (jvm == null)
        //{
        //    CoreMain.GameLaunch?.Invoke(obj, LaunchState.JvmError);
        //    return null;
        //}

        Process process = new();
        process.StartInfo.FileName = @"C:\Program Files\java\jdk17\bin\javaw.exe";
        process.StartInfo.WorkingDirectory = InstancesPath.GetDir(obj);
        foreach (var item in arg)
        {
            process.StartInfo.ArgumentList.Add(item);
        }

        process.StartInfo.RedirectStandardInput = true; //重定向标准输入
        process.StartInfo.RedirectStandardOutput = true; //重定向标准输出
        process.StartInfo.RedirectStandardError = true;//重定向错误输出

        process.OutputDataReceived += Process_OutputDataReceived;
        process.ErrorDataReceived += Process_ErrorDataReceived;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data);
    }
}
