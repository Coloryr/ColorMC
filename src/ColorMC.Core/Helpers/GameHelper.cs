using System.Text;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.OtherLaunch;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Helpers;

public static class GameHelper
{
    /// <summary>
    /// ForgeWrapper位置
    /// </summary>
    public static string ForgeWrapper { get; private set; }

    /// <summary>
    /// OptifineWrapper位置
    /// </summary>
    public static string OptifineWrapper { get; private set; }

    /// <summary>
    /// ColorMCASM位置
    /// </summary>
    public static string ColorMCASM { get; private set; }

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
            "/io/github/zekerzhayard/ForgeWrapper/colormc-1.6.0/ForgeWrapper-colormc-1.6.0.jar");
        var file = new FileInfo(ForgeWrapper);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            PathHelper.WriteBytes(file.FullName, Resource1.ForgeWrapper_colormc_1_6_0);
        }
    }

    /// <summary>
    /// Optifine加载器
    /// </summary>
    public static void ReadyOptifineWrapper()
    {
        OptifineWrapper = Path.GetFullPath(LibrariesPath.BaseDir +
            "/com/coloryr/OptifineWrapper/1.0/OptifineWrapper-1.0.jar");
        var file = new FileInfo(OptifineWrapper);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            PathHelper.WriteBytes(file.FullName, Resource1.OptifineWrapper_1_1);
        }
    }

    /// <summary>
    /// Optifine加载器
    /// </summary>
    public static void ReadyColorMCASM()
    {
        ColorMCASM = Path.GetFullPath(LibrariesPath.BaseDir +
            "/com/coloryr/colormc/ColorMCASM/1.0/ColorMCASM-1.0-all.jar");
        var file = new FileInfo(ColorMCASM);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            PathHelper.WriteBytes(file.FullName, Resource1.ColorMCASM_1_0_all);
        }
    }

    /// <summary>
    /// 手机替换运行库
    /// </summary>
    /// <param name="item">运行库</param>
    /// <returns>运行库</returns>
    public static GameArgObj.Libraries ReplaceLib(GameArgObj.Libraries item)
    {
        if (item.name.StartsWith("net.java.dev.jna:jna:"))
        {
            string[] version = item.name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) >= 5 && int.Parse(version[1]) >= 13) return item;
            item.name = "net.java.dev.jna:jna:5.13.0";
            item.downloads.artifact.path = "net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar";
            item.downloads.artifact.sha1 = "1200e7ebeedbe0d10062093f32925a912020e747";
            item.downloads.artifact.url =
                BaseClient.Source == SourceLocal.Offical
                ? $"{UrlHelper.MavenUrl[0]}net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar"
                : $"{UrlHelper.MavenUrl[1]}net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar";
        }
        else if (item.name.StartsWith("com.github.oshi:oshi-core:"))
        {
            string[] version = item.name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) != 6 || int.Parse(version[1]) != 2) return item;
            item.name = "com.github.oshi:oshi-core:6.3.0";
            item.downloads.artifact.path = "com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar";
            item.downloads.artifact.sha1 = "9e98cf55be371cafdb9c70c35d04ec2a8c2b42ac";
            item.downloads.artifact.url =
                BaseClient.Source == SourceLocal.Offical
                ? $"{UrlHelper.MavenUrl[0]}com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar"
                : $"{UrlHelper.MavenUrl[1]}com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar";
        }
        else if (item.name.StartsWith("org.ow2.asm:asm-all:"))
        {
            string[] version = item.name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) >= 5) return item;
            item.name = "org.ow2.asm:asm-all:5.0.4";
            item.downloads.artifact.path = "org/ow2/asm/asm-all/5.0.4/asm-all-5.0.4.jar";
            item.downloads.artifact.sha1 = "e6244859997b3d4237a552669279780876228909";
            item.downloads.artifact.url =
                BaseClient.Source == SourceLocal.Offical
                ? $"{UrlHelper.MavenUrl[0]}org/ow2/asm/asm-all/5.0.4/asm-all-5.0.4.jar"
                : $"{UrlHelper.MavenUrl[1]}org/ow2/asm/asm-all/5.0.4/asm-all-5.0.4.jar";
        }

        return item;
    }

    /// <summary>
    /// 解压native
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <param name="stream">文件流</param>
    public static void UnpackNative(string version, Stream stream)
    {
        using var zFile = new ZipFile(stream);
        foreach (ZipEntry e in zFile)
        {
            if (e.Name.StartsWith("META-INF"))
            {
                continue;
            }
            else if (e.IsFile)
            {
                var file = Path.GetFullPath(LibrariesPath.GetNativeDir(version) + "/" + e.Name);
                if (File.Exists(file))
                {
                    continue;
                }

                using var stream1 = PathHelper.OpenWrite(file);
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
            else if (item.uid == "net.neoforged")
            {
                game.Loader = Loaders.NeoForge;
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
        if (list.TryGetValue("name", out var item4))
        {
            game.Name = item4;
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
            var temp = StringHelper.ArgParse(item1);
            game.JvmArg ??= new();
            game.JvmArg.LaunchPre = true;
            var data = new StringBuilder();
            foreach (var item in temp)
            {
                string item3 = item;
                if (item3.StartsWith('"'))
                {
                    item3 = item3[1..];
                }
                if (item3.EndsWith('"'))
                {
                    item3 = item3[..^1];
                }

                if (item3 == "$INST_JAVA")
                {
                    data.AppendLine(Launch.JAVA_LOCAL);
                }
                else if (item3 == "$INST_NAME")
                {
                    data.AppendLine(Launch.GAME_NAME);
                }
                else if (item3 == "$INST_DIR")
                {
                    data.AppendLine(Launch.GAME_DIR);
                }
                else if (item3 == "$INST_MC_DIR")
                {
                    data.AppendLine(Launch.GAME_DIR);
                }
                else if (item3 == "$INST_ID")
                {
                    data.AppendLine(Launch.GAME_UUID);
                }
                else if (item3 == "$INST_JAVA_ARGS")
                {
                    data.AppendLine(Launch.JAVA_ARG);
                }
                else
                {
                    data.AppendLine(item3);
                }
            }
            game.JvmArg.LaunchPreData = data.ToString();
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
            if (obj.launchInfo.launchArgument != null)
            {
                foreach (var item in obj.launchInfo.launchArgument)
                {
                    data += item + " ";
                }
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
            if (obj.launchInfo.javaArgument != null)
            {
                foreach (var item in obj.launchInfo.javaArgument)
                {
                    data += item + " ";
                }
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

    /// <summary>
    /// 转换到ColorMC数据
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static GameSettingObj? ToColorMC(this OfficialObj obj)
    {
        var game = new GameSettingObj()
        {
            Name = obj.id,
            Loader = Loaders.Normal
        };

        if (obj.patches != null)
        {
            foreach (var item1 in obj.patches)
            {
                var id = item1.id;
                var version = item1.version ?? "";
                if (id == "game")
                {
                    game.Version = version;
                }
                else if (id == "forge")
                {
                    game.LoaderVersion = version;
                    game.Loader = Loaders.Forge;
                }
                else if (id == "fabric")
                {
                    game.LoaderVersion = version;
                    game.Loader = Loaders.Fabric;
                }
                else if (id == "quilt")
                {
                    game.LoaderVersion = version;
                    game.Loader = Loaders.Quilt;
                }
                else if (id == "neoforge")
                {
                    game.LoaderVersion = version;
                    game.Loader = Loaders.NeoForge;
                }
            }
        }
        else
        {
            var versions = VersionPath.GetVersions();
            if (versions == null)
            {
                return null;
            }
            if (!string.IsNullOrWhiteSpace(obj.inheritsFrom))
            {
                if (versions.versions.Any(item => item.id == obj.inheritsFrom) == true)
                {
                    game.Version = obj.inheritsFrom;
                }
            }
            else
            {
                if (versions.versions.Any(item => item.id == obj.id) == true)
                {
                    game.Version = obj.id;
                }
            }

            foreach (var item in obj.libraries)
            {
                if (item.name.Contains("minecraftforge"))
                {
                    var names = item.name.Split(':');
                    if (names.Length >= 3 && (names[1] is "forge" or "fmlloader"))
                    {
                        var args = names[2].Split('-');
                        if (args.Length >= 2 && versions.versions.Any(item => item.id == args[0]))
                        {
                            game.Loader = Loaders.Forge;
                            game.LoaderVersion = args[1];
                        }
                        break;
                    }
                }
                else if (item.name.Contains("neoforged"))
                {
                    if (obj.arguments?.game is { } list)
                    {
                        for (int a = 0; a < list.Count; a++)
                        {
                            if (list[a] is "--fml.neoForgeVersion" or "--fml.forgeVersion" && list.Count > a + 1)
                            {
                                game.Loader = Loaders.NeoForge;
                                game.LoaderVersion = list[a + 1].ToString();
                                break;
                            }
                        }
                    }
                }
                else if (item.name.Contains("fabricmc"))
                {
                    var names = item.name.Split(':');
                    if (names.Length >= 3 && names[1] == "fabric-loader")
                    {
                        game.Loader = Loaders.Fabric;
                        game.LoaderVersion = names[2];
                        break;
                    }
                }
                else if (item.name.Contains("quiltmc"))
                {
                    var names = item.name.Split(':');
                    if (names.Length >= 3 && names[1] == "quilt-loader")
                    {
                        game.Loader = Loaders.Quilt;
                        game.LoaderVersion = names[2];
                        break;
                    }
                }
            }
        }
        
        return game;
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static async Task<List<string>> GetGameVersions(GameType type)
    {
        var list = new List<string>();
        var ver = await VersionPath.GetVersionsAsync();
        if (ver == null)
        {
            return list;
        }

        foreach (var item in ver.versions)
        {
            if (item.type == "release")
            {
                if (type == GameType.Release)
                {
                    list.Add(item.id);
                }
            }
            else if (item.type == "snapshot")
            {
                if (type == GameType.Snapshot)
                {
                    list.Add(item.id);
                }
            }
            else
            {
                if (type == GameType.Other)
                {
                    list.Add(item.id);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 导入文件夹
    /// </summary>
    /// <param name="name">实例名字</param>
    /// <param name="local">位置</param>
    /// <param name="unselect">排除的文件</param>
    /// <param name="group">游戏群组</param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>

    public static async Task<(bool, GameSettingObj?, Exception?)> AddGame(string name, string local, List<string>? unselect,
        string? group, ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        GameSettingObj? game = null;

        bool isfind = false;

        var file1 = Path.GetFullPath(local + "/" + "mmc-pack.json");
        var file2 = Path.GetFullPath(local + "/" + "instance.cfg");
        if (File.Exists(file1) && File.Exists(file2))
        {
            try
            {
                var mmc = JsonConvert.DeserializeObject<MMCObj>(PathHelper.ReadText(file1)!);
                if (mmc != null)
                {
                    var mmc1 = PathHelper.ReadText(file2)!;
                    game = ToColorMC(mmc, mmc1, out var icon);
                    game.Icon = icon + ".png";
                    isfind = true;
                }
            }
            catch
            { 
                
            }
        }

        if (!isfind)
        {
            var files = Directory.GetFiles(local);
            foreach (var item in files)
            {
                if (!item.EndsWith(".json"))
                {
                    continue;
                }

                try
                {
                    var obj1 = JsonConvert.DeserializeObject<OfficialObj>(PathHelper.ReadText(item)!);
                    if (obj1 != null && obj1.id != null)
                    {
                        game = ToColorMC(obj1);
                        isfind = true;
                        break;
                    }
                }
                catch
                {

                }
            }
        }

        game ??= new GameSettingObj()
        {
            Name = name,
            Version = (await GetGameVersions(GameType.Release))[0],
            Loader = Loaders.Normal
        };
        game.GroupName = group;

        game = await InstancesPath.CreateGame(game, request, overwirte);
        if (game == null)
        {
            return (false, null, null);
        }

        await game.CopyFile(local, unselect);

        return (true, game, null);
    }

    public static bool IsMinecraftVersion(string dir)
    {
        bool find = false;
        var files = PathHelper.GetFiles(dir);
        foreach (var item3 in files)
        {
            if (find)
            {
                break;
            }
            if (item3.Name.EndsWith(".json"))
            {
                try
                {
                    var obj = JObject.Parse(PathHelper.ReadText(item3.FullName)!);
                    if (obj.ContainsKey("id")
                        && obj.ContainsKey("arguments")
                        && obj.ContainsKey("mainClass"))
                    {
                        find = true;
                        break;
                    }
                }
                catch
                {

                }
            }
        }

        return find;
    }

    public static bool IsMMCVersion(string dir)
    {
        var file1 = Path.GetFullPath(dir + "/" + "mmc-pack.json");
        var file2 = Path.GetFullPath(dir + "/" + "instance.cfg");
        if (File.Exists(file1) && File.Exists(file2))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 扫描目录下的游戏版本
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static List<string> ScanVersions(string dir)
    {
        var list = new List<string>();
        var dirs = PathHelper.GetDirs(dir);
        foreach (var item in dirs)
        {
            if (item.Name == "versions")
            {
                var dirs1 = PathHelper.GetDirs(item.FullName);
                foreach (var item2 in dirs1)
                {
                    if (IsMinecraftVersion(item2.FullName))
                    {
                        list.Add(item2.FullName);
                    }
                }
                if (list.Count > 0)
                {
                    return list;
                }
            }
            if (item.Name == "instances")
            {
                var dirs1 = PathHelper.GetDirs(item.FullName);
                foreach (var item2 in dirs1)
                {
                    if (IsMMCVersion(item2.FullName))
                    {
                        list.Add(item2.FullName);
                    }
                }
                if (list.Count > 0)
                {
                    return list;
                }
            }

            if (IsMinecraftVersion(item.FullName))
            {
                list.Add(item.FullName);
                continue;
            }

            if (IsMMCVersion(item.FullName))
            {
                list.Add(item.FullName);
            }
        }

        return list;
    }

    public static async Task<bool> CheckNameAndAutoRename(GameSettingObj game, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte)
    {
        if (InstancesPath.HaveGameWithName(game.Name))
        {
            var res = await overwirte(game);
            if (!res)
            {
                res = await request(LanguageHelper.Get("Core.Game.Error20"));
                if (!res)
                {
                    return false;
                }
                int a = 1;
                do
                {
                    game.Name += $"({a})";
                }
                while (!InstancesPath.HaveGameWithName(game.Name));
            }
        }

        return true;
    }
}
