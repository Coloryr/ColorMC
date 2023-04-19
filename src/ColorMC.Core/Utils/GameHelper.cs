using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.OtherLaunch;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Concurrent;

namespace ColorMC.Core.Utils;

public static class GameHelper
{
    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">游戏数据</param>
    public static async Task<ConcurrentBag<DownloadItemObj>> MakeGameLibs(GameArgObj obj)
    {
        var list = new ConcurrentBag<DownloadItemObj>();
        var list1 = new HashSet<string>();
        var natives = new ConcurrentDictionary<string, string>();
        Parallel.ForEach(obj.libraries, (item1, cancel) =>
        {
            bool download = CheckRule.CheckAllow(item1.rules);
            if (!download)
                return;

            if (item1.downloads.artifact != null)
            {
                lock (list1)
                {
                    if (list1.Contains(item1.downloads.artifact.sha1)
                        && item1.downloads.classifiers == null)
                    {
                        return;
                    }
                }

                if (item1.name.Contains("natives"))
                {
                    var index = item1.name.LastIndexOf(':');
                    string key = item1.name[..index];
                    natives.TryAdd(key, key);
                }

                list.Add(new()
                {
                    Name = item1.name,
                    Url = UrlHelper.DownloadLibraries(item1.downloads.artifact.url, BaseClient.Source),
                    Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                    SHA1 = item1.downloads.artifact.sha1
                });

                lock (list1)
                {
                    list1.Add(item1.downloads.artifact.sha1);
                }
            }

            if (item1.downloads.classifiers != null)
            {
                var lib = SystemInfo.Os switch
                {
                    OsType.Windows => item1.downloads.classifiers.natives_windows,
                    OsType.Linux => item1.downloads.classifiers.natives_linux,
                    OsType.MacOS => item1.downloads.classifiers.natives_osx,
                    _ => null
                };

                if (lib == null)
                {
                    if (SystemInfo.Os == OsType.Windows)
                    {
                        if (SystemInfo.SystemArch == ArchEnum.x32)
                        {
                            lib = item1.downloads.classifiers.natives_windows_32;
                        }
                        else
                        {
                            lib = item1.downloads.classifiers.natives_windows_64;
                        }
                    }
                }

                if (lib != null)
                {
                    if (list1.Contains(lib.sha1))
                        return;

                    natives.TryAdd(item1.name, item1.name);

                    list.Add(new()
                    {
                        Name = item1.name + "-native" + SystemInfo.Os,
                        Url = UrlHelper.DownloadLibraries(lib.url, BaseClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{lib.path}",
                        SHA1 = lib.sha1,
                        Later = (test) => UnpackNative(obj.id, test)
                    });

                    list1.Add(lib.sha1);
                }
            }
        });

        if (SystemInfo.IsArm)
        {
            foreach (var item in natives.Keys)
            {
                var path = item.Split(':');
                var path1 = path[0].Split('.');
                var basedir = "";
                foreach (var item1 in path1)
                {
                    basedir += $"{item1}/";
                }
                if (SystemInfo.Os == OsType.Linux)
                {
                    var name = item + $":{path[1]}-{path[2]}-natives-linux-arm64";
                    var dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-linux-arm64.jar";

                    var item3 = await MakeItem(name, dir);
                    if (item3 != null)
                    {
                        list.Add(item3);
                    }
                }
                else if (SystemInfo.Os == OsType.Windows)
                {
                    var name = item + $":{path[1]}-{path[2]}-natives-windows-arm64";
                    var dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-windows-arm64.jar";
                    var item3 = await MakeItem(name, dir);
                    if (item3 != null)
                    {
                        list.Add(item3);
                    }
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 解压native
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <param name="stream">文件流</param>
    public static void UnpackNative(string version, FileStream stream)
    {
        using ZipFile zFile = new(stream);
        foreach (ZipEntry e in zFile)
        {
            if (e.Name.StartsWith("META-INF"))
                continue;
            if (e.IsFile)
            {
                string file = LibrariesPath.GetNativeDir(version) + "/" + e.Name;
                if (File.Exists(file))
                    continue;

                using var stream1 = new FileStream(file,
                    FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var stream2 = zFile.GetInputStream(e);
                stream2.CopyTo(stream1);
            }
        }
    }

    private static async Task<DownloadItemObj?> MakeItem(string name, string dir)
    {
        var item2 = LocalMaven.GetItem(name);
        if (item2 != null)
        {
            if (item2.Have)
            {
                return new()
                {
                    Name = name,
                    Url = item2.Url,
                    Local = $"{LibrariesPath.BaseDir}/{item2.Local}",
                    SHA1 = item2.SHA1
                };
            }
        }
        else
        {
            try
            {
                DownloadItemObj? item3 = null;
                var maven = new MavenItemObj()
                {
                    Name = name
                };
                var url = (BaseClient.Source == SourceLocal.Offical ?
                    UrlHelper.originServers4[0] :
                    UrlHelper.originServers4[1]) + dir;
                var res = await BaseClient.DownloadClient
                    .GetAsync(url + ".sha1",
                    HttpCompletionOption.ResponseHeadersRead);
                if (res.IsSuccessStatusCode)
                {
                    item3 = new DownloadItemObj()
                    {
                        Name = name,
                        Url = url,
                        Local = $"{LibrariesPath.BaseDir}/{dir}",
                        SHA1 = await res.Content.ReadAsStringAsync()
                    };

                    maven.Have = true;
                    maven.SHA1 = item3.SHA1;
                    maven.Url = item3.Url;
                    maven.Local = dir;
                }
                else
                {
                    maven.Have = false;
                }

                LocalMaven.AddItem(maven);

                return item3;
            }
            catch (Exception e)
            {
                Logs.Error("native load error", e);
            }
        }

        return null;
    }

    public static GameSettingObj MMCToColorMC(MMCObj mmc, string mmc1)
    {
        var list = Options.ReadOptions(mmc1, "=");
        var game = new GameSettingObj
        {
            Name = list["name"],
            Loader = Loaders.Normal
        };

        foreach (var item in mmc.components)
        {
            if (item.uid == "net.minecraft")
            {
                game.Version = item.version;
            }
            else if (item.uid == "net.minecraftforge")
            {
                game.Loader = Loaders.Forge;
                game.LoaderVersion = item.version;
            }
            else if (item.uid == "net.fabricmc.fabric-loader")
            {
                game.Loader = Loaders.Fabric;
                game.LoaderVersion = item.version;
            }
            else if (item.uid == "org.quiltmc.quilt-loader")
            {
                game.Loader = Loaders.Quilt;
                game.LoaderVersion = item.version;
            }
        }
        game.JvmArg = new();
        game.Window = new();
        if (list.TryGetValue("JvmArgs", out var item1))
        {
            game.JvmArg.JvmArgs = item1;
        }
        if (list.TryGetValue("MaxMemAlloc", out item1)
            && uint.TryParse(item1, out var item2))
        {
            game.JvmArg.MaxMemory = item2;
        }
        if (list.TryGetValue("MinMemAlloc", out item1)
             && uint.TryParse(item1, out item2))
        {
            game.JvmArg.MaxMemory = item2;
        }
        if (list.TryGetValue("MinecraftWinHeight", out item1)
            && uint.TryParse(item1, out item2))
        {
            game.Window.Height = item2;
        }
        if (list.TryGetValue("MinecraftWinWidth", out item1)
            && uint.TryParse(item1, out item2))
        {
            game.Window.Width = item2;
        }
        if (list.TryGetValue("LaunchMaximized", out item1))
        {
            game.Window.FullScreen = item1 == "true";
        }
        if (list.TryGetValue("JoinServerOnLaunch", out item1)
            && item1 == "true")
        {
            game.StartServer = new();
            if (list.TryGetValue("JoinServerOnLaunchAddress", out item1))
            {
                game.StartServer.IP = item1;
                game.StartServer.Port = 0;
            }
        }

        return game;
    }
}
