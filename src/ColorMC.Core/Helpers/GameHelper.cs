using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Concurrent;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 游戏文件处理
/// </summary>
public static class GameHelper
{
    /// <summary>
    /// ForgeWrapper位置
    /// </summary>
    public static string ForgeWrapper { get; private set; }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildForgeItem(string mc, string version, string type)
    {
        version += UrlHelper.FixForgeUrl(mc);
        string name = $"forge-{mc}-{version}-{type}";
        string url = UrlHelper.DownloadForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.minecraftforge:forge:{mc}-{version}-{type}",
            Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/{mc}-{version}/{name}.jar",
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildNeoForgeItem(string mc, string version, string type)
    {
        string name = $"forge-{mc}-{version}-{type}";
        string url = UrlHelper.DownloadNeoForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.neoforged:forge:{mc}-{version}-{type}",
            Local = $"{LibrariesPath.BaseDir}/net/neoforged/forge/{mc}-{version}/{name}.jar",
        };
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeInster(string mc, string version)
    {
        return BuildForgeItem(mc, version, "installer");
    }

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeInster(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "installer");
    }

    /// <summary>
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeUniversal(string mc, string version)
    {
        return BuildForgeItem(mc, version, "universal");
    }

    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeUniversal(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "universal");
    }
    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildForgeLauncher(string mc, string version)
    {
        return BuildForgeItem(mc, version, "launcher");
    }
    /// <summary>
    /// 创建NeoForge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj BuildNeoForgeLauncher(string mc, string version)
    {
        return BuildNeoForgeItem(mc, version, "launcher");
    }

    /// <summary>
    /// 获取所有Forge的Lib列表
    /// </summary>
    /// <param name="info">forge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static List<DownloadItemObj> MakeForgeLibs(ForgeLaunchObj info, string mc, string version, bool neo)
    {
        var version1 = VersionPath.GetGame(mc)!;
        var v2 = CheckRule.GameLaunchVersion(version1);
        var list = new List<DownloadItemObj>();

        if (v2)
        {
            list.Add(neo ?
                BuildNeoForgeInster(mc, version) :
                BuildForgeInster(mc, version));
            list.Add(neo ?
                BuildNeoForgeUniversal(mc, version) :
                BuildForgeUniversal(mc, version));

            if (!CheckRule.IsGameLaunchVersion117(mc))
            {
                list.Add(neo ?
                    BuildNeoForgeLauncher(mc, version) :
                    BuildForgeLauncher(mc, version));
            }
        }

        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith(neo ?
                "net.neoforged.forge:" : "net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
                //1.12.2及以下
                if (!v2)
                {
                    var temp = BuildForgeUniversal(mc, version);
                    temp.SHA1 = item1.downloads.artifact.sha1;
                    list.Add(temp);
                }
            }
            else
            {
                list.Add(new()
                {
                    Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source) :
                    UrlHelper.DownloadForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source),
                    Name = item1.name,
                    Local = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}"),
                    SHA1 = item1.downloads.artifact.sha1
                });
            }
        }

        return list;
    }

    /// <summary>
    /// 获取所有Forge的Lib列表
    /// </summary>
    /// <param name="info">forge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>下载项目列表</returns>
    public static List<DownloadItemObj> MakeForgeLibs(ForgeInstallObj info, string mc, string version, bool neo)
    {
        var list = new List<DownloadItemObj>();

        foreach (var item in info.libraries)
        {
            if (item.name.StartsWith(neo ? "net.neoforged.forge:" : "net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item.downloads.artifact.url))
            {
                var item1 = neo ?
                    BuildNeoForgeUniversal(mc, version) :
                    BuildForgeUniversal(mc, version);
                item1.SHA1 = item.downloads.artifact.sha1;
                if (!File.Exists(item1.Local))
                {
                    list.Add(item1);
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(item1.SHA1))
                {
                    using var stream1 = new FileStream(item1.Local, FileMode.Open, FileAccess.ReadWrite,
                   FileShare.ReadWrite);
                    var sha11 = Funtcions.GenSha1(stream1);
                    if (sha11 != item1.SHA1)
                    {
                        list.Add(item1);
                    }
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(item.downloads.artifact.url))
                continue;

            string file = Path.GetFullPath($"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}");
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Local = file,
                    Name = item.name,
                    SHA1 = item.downloads.artifact.sha1,
                    Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item.downloads.artifact.url,
                         BaseClient.Source) :
                    UrlHelper.DownloadForgeLib(item.downloads.artifact.url,
                         BaseClient.Source)
                });
                continue;
            }
            using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            var sha1 = Funtcions.GenSha1(stream);
            if (item.downloads.artifact.sha1 != sha1)
            {
                list.Add(new()
                {
                    Local = file,
                    Name = item.name,
                    SHA1 = item.downloads.artifact.sha1,
                    Url = neo ?
                    UrlHelper.DownloadNeoForgeLib(item.downloads.artifact.url,
                         BaseClient.Source) :
                    UrlHelper.DownloadForgeLib(item.downloads.artifact.url,
                         BaseClient.Source)
                });
            }
        }

        //if (!string.IsNullOrWhiteSpace(forgeinstall.data?.MOJMAPS?.client)
        //    && version1.downloads.client_mappings != null)
        //{
        //    var args = forgeinstall.data.MOJMAPS.client.Split(":");
        //    if (args[3] == "mappings@txt]")
        //    {
        //        string name1 = $"client-{args[2]}-mappings.txt";
        //        string local = $"{BaseDir}/net/minecraft/client/{args[2]}/{name1}";
        //        if (!File.Exists(local))
        //        {
        //            list.Add(new()
        //            {
        //                Url = version1.downloads.client_mappings.url,
        //                SHA1 = version1.downloads.client_mappings.sha1,
        //                Name = name1,
        //                Local = local
        //            });
        //        }
        //        else
        //        {
        //            using var stream = new FileStream(local, FileMode.Open, FileAccess.ReadWrite,
        //           FileShare.ReadWrite);
        //            var sha1 = await Funtcions.GenSha1Async(stream);
        //            if (sha1 != version1.downloads.client_mappings.sha1)
        //            {
        //                list.Add(new()
        //                {
        //                    Url = version1.downloads.client_mappings.url,
        //                    SHA1 = version1.downloads.client_mappings.sha1,
        //                    Name = name1,
        //                    Local = local
        //                });
        //            }
        //        }
        //    }
        //}

        return list;
    }

    /// <summary>
    /// Forge运行库修改映射
    /// </summary>
    public static ForgeLaunchObj.Libraries? MakeLibObj(ForgeInstallObj1.VersionInfo.Libraries item)
    {
        var args = item.name.Split(":");
        if (args[0] == "net.minecraftforge" && args[1] == "forge")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        url = ""
                    }
                }
            };
        }
        else if (args[0] == "net.minecraft" && args[1] == "launchwrapper")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"net/minecraft/launchwrapper/{args[2]}/launchwrapper-{args[2]}.jar",
                        url = $"https://libraries.minecraft.net/net/minecraft/launchwrapper/{args[2]}/launchwrapper-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "org.ow2.asm" && args[1] == "asm-all")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"org/ow2/asm/asm-all/{args[2]}/asm-all-{args[2]}.jar",
                        url = $"https://maven.minecraftforge.net/org/ow2/asm/asm-all/{args[2]}/asm-all-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "lzma" && args[1] == "lzma")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"lzma/lzma/{args[2]}/lzma-{args[2]}.jar",
                        url = $"https://libraries.minecraft.net/lzma/lzma/{args[2]}/lzma-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.sf.jopt-simple" && args[1] == "jopt-simple")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"net/sf/jopt-simple/jopt-simple/{args[2]}/jopt-simple-{args[2]}.jar",
                        url = $"https://libraries.minecraft.net/net/sf/jopt-simple/jopt-simple/{args[2]}/jopt-simple-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "com.google.guava" && args[1] == "guava")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"com/google/guava/guava/{args[2]}/guava-{args[2]}.jar",
                        url = $"https://libraries.minecraft.net/com/google/guava/guava/{args[2]}/guava-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "org.apache.commons" && args[1] == "commons-lang3")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"org/apache/commons/commons-lang3/{args[2]}/commons-lang3-{args[2]}.jar",
                        url = $"https://maven.minecraftforge.net/org/apache/commons/commons-lang3/{args[2]}/commons-lang3-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.java.jinput" && args[1] == "jinput")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"net/java/jinput/jinput/{args[2]}/jinput-{args[2]}.jar",
                        url = $"https://maven.minecraftforge.net/net/java/jinput/jinput/{args[2]}/jinput-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.java.jutils" && args[1] == "jutils")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"net/java/jutils/jutils/{args[2]}/jutils-{args[2]}.jar",
                        url = $"https://maven.minecraftforge.net/net/java/jutils/jutils/{args[2]}/jutils-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "java3d" && args[1] == "vecmath")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"java3d/vecmath/{args[2]}/vecmath-{args[2]}.jar",
                        url = $"https://libraries.minecraft.net/java3d/vecmath/{args[2]}/vecmath-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.sf.trove4j" && args[1] == "trove4j")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"net/sf/trove4j/trove4j/{args[2]}/trove4j-{args[2]}.jar",
                        url = $"https://maven.minecraftforge.net/net/sf/trove4j/trove4j/{args[2]}/trove4j-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "io.netty" && args[1] == "netty-all")
        {
            return new()
            {
                name = item.name,
                downloads = new()
                {
                    artifact = new()
                    {
                        path = $"io/netty/netty-all/{args[2]}/netty-all-{args[2]}.jar",
                        url = $"https://maven.minecraftforge.net/io/netty/netty-all/{args[2]}/netty-all-{args[2]}.jar"
                    }
                }
            };
        }

        return null;
    }

    /// <summary>
    /// Forge加载器
    /// </summary>
    public static void ReadyForgeWrapper()
    {
        ForgeWrapper = Path.GetFullPath(LibrariesPath.BaseDir +
            "/io/github/zekerzhayard/ForgeWrapper/mmc3/ForgeWrapper-mmc3.jar");
        var file = new FileInfo(GameHelper.ForgeWrapper);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            File.WriteAllBytes(file.FullName, Resource1.ForgeWrapper_mmc3);
        }
    }

    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">下载项目列表</param>
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

                if (lib == null && SystemInfo.Os == OsType.Windows)
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
                    lock (list1)
                    {
                        list1.Add(lib.sha1);
                    }
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

                    var item3 = await LocalMaven.MakeItem(name, dir);
                    if (item3 != null)
                    {
                        list.Add(item3);
                    }
                }
                else if (SystemInfo.Os == OsType.Windows)
                {
                    var name = item + $":{path[1]}-{path[2]}-natives-windows-arm64";
                    var dir = $"{basedir}{path[1]}/{path[2]}/{path[1]}-{path[2]}-natives-windows-arm64.jar";
                    var item3 = await LocalMaven.MakeItem(name, dir);
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

    /// <summary>
    /// MMC配置转ColorMC
    /// </summary>
    /// <param name="mmc">MMC储存</param>
    /// <param name="mmc1">MMC储存</param>
    /// <param name="icon">图标输出</param>
    /// <returns>游戏设置</returns>
    public static GameSettingObj ToColorMC(this MMCObj mmc, string mmc1, out string icon)
    {
        var list = Options.ReadOptions(mmc1, "=");
        var game = new GameSettingObj
        {
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
        if (list.TryGetValue("Name", out var item1))
        {
            game.Name = item1;
        }
        if (list.TryGetValue("JvmArgs", out item1))
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
        if (list.TryGetValue("PreLaunchCommand", out item1))
        {
            game.JvmArg ??= new();
            game.JvmArg.LaunchPre = true;
            game.JvmArg.LaunchPreData = item1;
        }
        if (list.TryGetValue("iconKey", out item1))
        {
            icon = item1;
        }
        else
        {
            icon = "";
        }

        return game;
    }

    /// <summary>
    /// HMCL配置转ColorMC
    /// </summary>
    /// <param name="obj">HMCL储存</param>
    /// <returns>游戏实例</returns>
    public static GameSettingObj ToColorMC(this HMCLObj obj)
    {
        var game = new GameSettingObj()
        {
            Name = obj.name,
            Loader = Loaders.Normal
        };

        foreach (var item in obj.addons)
        {
            if (item.id == "game")
            {
                game.Version = item.version;
            }
            else if (item.id == "forge")
            {
                game.Loader = Loaders.Forge;
                game.LoaderVersion = item.version;
            }
            else if (item.id == "fabric")
            {
                game.Loader = Loaders.Fabric;
                game.LoaderVersion = item.version;
            }
            else if (item.id == "quilt")
            {
                game.Loader = Loaders.Quilt;
                game.LoaderVersion = item.version;
            }
        }

        if (obj.launchInfo != null)
        {
            game.JvmArg = new()
            {
                MinMemory = obj.launchInfo.minMemory
            };
            string data = "";
            foreach (var item in obj.launchInfo.launchArgument)
            {
                data += item + " ";
            }
            if (!string.IsNullOrWhiteSpace(data))
            {
                game.JvmArg.GameArgs = data.Trim();
            }
            else
            {
                game.JvmArg.GameArgs = null;
            }

            data = "";
            foreach (var item in obj.launchInfo.javaArgument)
            {
                data += item + " ";
            }
            if (!string.IsNullOrWhiteSpace(data))
            {
                game.JvmArg.JvmArgs = data.Trim();
            }
            else
            {
                game.JvmArg.JvmArgs = null;
            }
        }

        return game;
    }
}
