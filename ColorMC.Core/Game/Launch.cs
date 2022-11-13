using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColorMC.Core.Objs.Game.FabircMetaObj;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        var assets = AssetsPath.GetIndex(obj.Version);
        if (assets == null)
        {
            assets = await Get.GetAssets(game.assetIndex.url);
            if (assets == null)
            {
                CoreMain.GameLaunch?.Invoke(obj, LaunchState.AssetsError);
                return null;
            }
            AssetsPath.AddIndex(assets, game.id);
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

                await GameDownload.DownloadForge(obj.Version, obj.LoaderInfo.Version);
            }
            else
            {
                foreach (var item in list3)
                {
                    if (item.name.StartsWith("net.minecraftforge:forge:"))
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

    public static string? MakeV1JvmArg()
    {
        return "-Djava.library.path=${natives_directory} -cp ${classpath}";
    }

    public static string? MakeV2JvmArg(GameSettingObj obj) 
    {
        var game = VersionPath.GetGame(obj.Version)!;
        if (game.arguments == null)
            return MakeV1JvmArg();

        StringBuilder arg = new StringBuilder();
        foreach (var item in game.arguments.jvm)
        {
            if (item is string)
            {
                arg.Append(item).Append(" ");
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
                    arg.Append(obj2.value.ToString()).Append(" ");
                }
                else if (obj2.value is JArray)
                {
                    foreach (string item1 in obj2.value)
                    {
                        arg.Append(item1).Append(" ");
                    }
                }
            }
        }

        return arg.ToString();
    }

    public static string? JvmArg(GameSettingObj obj, LoginObj login, JvmConfigObj? jvmCfg = null) 
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

        JvmArgObj args = new();

        if (obj.JvmArg == null)
        {
            ConfigUtils.Config.DefaultJvmArg.CopyTo(args);
        }
        else
        {
            args.AdvencedGameArguments = obj.JvmArg.AdvencedGameArguments ?? 
                ConfigUtils.Config.DefaultJvmArg.AdvencedGameArguments;
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

        StringBuilder jvmHead = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(args.JavaAgent))
        {
            jvmHead.Append("-javaagent:");
            jvmHead.Append(args.JavaAgent.Trim());
            jvmHead.Append(' ');
        }

        var v2 = CheckRule.GameLaunchVersion(obj.Version);


        return null;
    }

    public static async Task StartGame(GameSettingObj obj, LoginObj login, JvmConfigObj? jvm = null) 
    {
        CoreMain.GameLaunch?.Invoke(obj, LaunchState.Check);
        var res = await CheckGameFile(obj);
        if (res == null)
            return;

        if (res.Count != 0)
        {
            CoreMain.GameLaunch?.Invoke(obj, LaunchState.Download);
            await DownloadManager.Start();
        }

        CoreMain.GameLaunch?.Invoke(obj, LaunchState.JvmPrepare);
        string? arg = JvmArg(obj, login, jvm);
        if (arg == null)
            return;
    }
}
