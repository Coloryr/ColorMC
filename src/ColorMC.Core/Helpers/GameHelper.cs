using System.Text;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.OtherLaunch;
using ICSharpCode.SharpZipLib.Zip;
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
    /// <param name="item">运行库项目</param>
    /// <returns>运行库项目</returns>
    public static ForgeLaunchObj.LibrariesObj? MakeLibObj(ForgeInstallNewObj.VersionInfoObj.LibrariesObj item)
    {
        var args = item.Name.Split(":");
        if (args[0] == "net.minecraftforge" && args[1] == "forge")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Url = ""
                    }
                }
            };
        }
        else if (args[0] == "net.minecraft" && args[1] == "launchwrapper")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"net/minecraft/launchwrapper/{args[2]}/launchwrapper-{args[2]}.jar",
                        Url = $"https://libraries.minecraft.net/net/minecraft/launchwrapper/{args[2]}/launchwrapper-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "org.ow2.asm" && args[1] == "asm-all")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"org/ow2/asm/asm-all/{args[2]}/asm-all-{args[2]}.jar",
                        Url = $"https://maven.minecraftforge.net/org/ow2/asm/asm-all/{args[2]}/asm-all-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "lzma" && args[1] == "lzma")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"lzma/lzma/{args[2]}/lzma-{args[2]}.jar",
                        Url = $"https://libraries.minecraft.net/lzma/lzma/{args[2]}/lzma-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.sf.jopt-simple" && args[1] == "jopt-simple")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"net/sf/jopt-simple/jopt-simple/{args[2]}/jopt-simple-{args[2]}.jar",
                        Url = $"https://libraries.minecraft.net/net/sf/jopt-simple/jopt-simple/{args[2]}/jopt-simple-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "com.google.guava" && args[1] == "guava")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"com/google/guava/guava/{args[2]}/guava-{args[2]}.jar",
                        Url = $"https://libraries.minecraft.net/com/google/guava/guava/{args[2]}/guava-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "org.apache.commons" && args[1] == "commons-lang3")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"org/apache/commons/commons-lang3/{args[2]}/commons-lang3-{args[2]}.jar",
                        Url = $"https://maven.minecraftforge.net/org/apache/commons/commons-lang3/{args[2]}/commons-lang3-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.java.jinput" && args[1] == "jinput")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"net/java/jinput/jinput/{args[2]}/jinput-{args[2]}.jar",
                        Url = $"https://maven.minecraftforge.net/net/java/jinput/jinput/{args[2]}/jinput-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.java.jutils" && args[1] == "jutils")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"net/java/jutils/jutils/{args[2]}/jutils-{args[2]}.jar",
                        Url = $"https://maven.minecraftforge.net/net/java/jutils/jutils/{args[2]}/jutils-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "java3d" && args[1] == "vecmath")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"java3d/vecmath/{args[2]}/vecmath-{args[2]}.jar",
                        Url = $"https://libraries.minecraft.net/java3d/vecmath/{args[2]}/vecmath-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "net.sf.trove4j" && args[1] == "trove4j")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"net/sf/trove4j/trove4j/{args[2]}/trove4j-{args[2]}.jar",
                        Url = $"https://maven.minecraftforge.net/net/sf/trove4j/trove4j/{args[2]}/trove4j-{args[2]}.jar"
                    }
                }
            };
        }
        else if (args[0] == "io.netty" && args[1] == "netty-all")
        {
            return new()
            {
                Name = item.Name,
                Downloads = new()
                {
                    Artifact = new()
                    {
                        Path = $"io/netty/netty-all/{args[2]}/netty-all-{args[2]}.jar",
                        Url = $"https://maven.minecraftforge.net/io/netty/netty-all/{args[2]}/netty-all-{args[2]}.jar"
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
            "/com/coloryr/OptifineWrapper/1.1/OptifineWrapper-1.1.jar");
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
    public static GameArgObj.LibrariesObj ReplaceLib(GameArgObj.LibrariesObj item)
    {
        if (item.Name.StartsWith("net.java.dev.jna:jna:"))
        {
            string[] version = item.Name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) >= 5 && int.Parse(version[1]) >= 13) return item;
            item.Name = "net.java.dev.jna:jna:5.13.0";
            item.Downloads.Artifact.Path = "net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar";
            item.Downloads.Artifact.Sha1 = "1200e7ebeedbe0d10062093f32925a912020e747";
            item.Downloads.Artifact.Url =
                CoreHttpClient.Source == SourceLocal.Offical
                ? $"{UrlHelper.MavenUrl[0]}net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar"
                : $"{UrlHelper.MavenUrl[1]}net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar";
        }
        else if (item.Name.StartsWith("com.github.oshi:oshi-core:"))
        {
            string[] version = item.Name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) != 6 || int.Parse(version[1]) != 2) return item;
            item.Name = "com.github.oshi:oshi-core:6.3.0";
            item.Downloads.Artifact.Path = "com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar";
            item.Downloads.Artifact.Sha1 = "9e98cf55be371cafdb9c70c35d04ec2a8c2b42ac";
            item.Downloads.Artifact.Url =
                CoreHttpClient.Source == SourceLocal.Offical
                ? $"{UrlHelper.MavenUrl[0]}com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar"
                : $"{UrlHelper.MavenUrl[1]}com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar";
        }
        else if (item.Name.StartsWith("org.ow2.asm:asm-all:"))
        {
            string[] version = item.Name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) >= 5) return item;
            item.Name = "org.ow2.asm:asm-all:5.0.4";
            item.Downloads.Artifact.Path = "org/ow2/asm/asm-all/5.0.4/asm-all-5.0.4.jar";
            item.Downloads.Artifact.Sha1 = "e6244859997b3d4237a552669279780876228909";
            item.Downloads.Artifact.Url =
                CoreHttpClient.Source == SourceLocal.Offical
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
                var file = Path.Combine(LibrariesPath.GetNativeDir(version), e.Name);
                if (File.Exists(file))
                {
                    continue;
                }

                using var stream2 = zFile.GetInputStream(e);
                PathHelper.WriteBytes(file, stream2);
            }
        }
    }

    /// <summary>
    /// 解压native
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <param name="stream">文件流</param>
    public static void UnpackSave(string version, Stream stream)
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
                var file = Path.Combine(LibrariesPath.GetNativeDir(version), e.Name);
                if (File.Exists(file))
                {
                    continue;
                }

                using var stream2 = zFile.GetInputStream(e);
                PathHelper.WriteBytes(file, stream2);
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
    public static MMCToColorMCRes ToColorMC(this MMCObj mmc, string mmc1)
    {
        var res = new MMCToColorMCRes();
        var list = Options.ReadOptions(mmc1, "=");
        var game = new GameSettingObj
        {
            Loader = Loaders.Normal
        };

        foreach (var item in mmc.Components)
        {
            if (item.Uid == "net.minecraft")
            {
                game.Version = item.Version;
            }
            else if (item.Uid == "net.minecraftforge")
            {
                game.Loader = Loaders.Forge;
                game.LoaderVersion = item.Version;
            }
            else if (item.Uid == "net.neoforged")
            {
                game.Loader = Loaders.NeoForge;
                game.LoaderVersion = item.Version;
            }
            else if (item.Uid == "net.fabricmc.fabric-loader")
            {
                game.Loader = Loaders.Fabric;
                game.LoaderVersion = item.Version;
            }
            else if (item.Uid == "org.quiltmc.quilt-loader")
            {
                game.Loader = Loaders.Quilt;
                game.LoaderVersion = item.Version;
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
        res.Game = game;
        if (list.TryGetValue("iconKey", out item1))
        {
            res.Icon = item1;
        }

        return res;
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
            Name = obj.Name,
            Loader = Loaders.Normal
        };

        foreach (var item in obj.Addons)
        {
            if (item.Id == "game")
            {
                game.Version = item.Version;
            }
            else if (item.Id == "forge")
            {
                game.Loader = Loaders.Forge;
                game.LoaderVersion = item.Version;
            }
            else if (item.Id == "fabric")
            {
                game.Loader = Loaders.Fabric;
                game.LoaderVersion = item.Version;
            }
            else if (item.Id == "quilt")
            {
                game.Loader = Loaders.Quilt;
                game.LoaderVersion = item.Version;
            }
        }

        if (obj.LaunchInfo != null)
        {
            game.JvmArg = new()
            {
                MinMemory = obj.LaunchInfo.MinMemory
            };
            string data = "";
            if (obj.LaunchInfo.LaunchArgument != null)
            {
                foreach (var item in obj.LaunchInfo.LaunchArgument)
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
            if (obj.LaunchInfo.JavaArgument != null)
            {
                foreach (var item in obj.LaunchInfo.JavaArgument)
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
    /// <param name="obj">官方实例</param>
    /// <returns>游戏实例</returns>
    public static GameSettingObj? ToColorMC(this OfficialObj obj)
    {
        var game = new GameSettingObj()
        {
            Name = obj.Id,
            Loader = Loaders.Normal
        };

        if (obj.Patches != null)
        {
            foreach (var item1 in obj.Patches)
            {
                var id = item1.Id;
                var version = item1.Version ?? "";
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
            if (!string.IsNullOrWhiteSpace(obj.InheritsFrom))
            {
                if (versions.Versions.Any(item => item.Id == obj.InheritsFrom) == true)
                {
                    game.Version = obj.InheritsFrom;
                }
            }
            else
            {
                if (versions.Versions.Any(item => item.Id == obj.Id) == true)
                {
                    game.Version = obj.Id;
                }
            }

            foreach (var item in obj.Libraries)
            {
                if (item.Name.Contains("minecraftforge"))
                {
                    var names = item.Name.Split(':');
                    if (names.Length >= 3 && (names[1] is "forge" or "fmlloader"))
                    {
                        var args = names[2].Split('-');
                        if (args.Length >= 2 && versions.Versions.Any(item => item.Id == args[0]))
                        {
                            game.Loader = Loaders.Forge;
                            game.LoaderVersion = args[1];
                        }
                        break;
                    }
                }
                else if (item.Name.Contains("neoforged"))
                {
                    if (obj.Arguments?.Game is { } list)
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
                else if (item.Name.Contains("fabricmc"))
                {
                    var names = item.Name.Split(':');
                    if (names.Length >= 3 && names[1] == "fabric-loader")
                    {
                        game.Loader = Loaders.Fabric;
                        game.LoaderVersion = names[2];
                        break;
                    }
                }
                else if (item.Name.Contains("quiltmc"))
                {
                    var names = item.Name.Split(':');
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
    /// <returns>游戏版本</returns>
    public static async Task<List<string>> GetGameVersions(GameType type)
    {
        var list = new List<string>();
        var ver = await VersionPath.GetVersionsAsync();
        if (ver == null)
        {
            return list;
        }

        if (type == GameType.All)
        {
            foreach (var item in ver.Versions)
            {
                list.Add(item.Id);
            }

            return list;
        }

        foreach (var item in ver.Versions)
        {
            if (item.Type == "release")
            {
                if (type == GameType.Release)
                {
                    list.Add(item.Id);
                }
            }
            else if (item.Type == "snapshot")
            {
                if (type == GameType.Snapshot)
                {
                    list.Add(item.Id);
                }
            }
            else
            {
                if (type == GameType.Other)
                {
                    list.Add(item.Id);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 是否为Minecraft原版版本
    /// </summary>
    /// <param name="dir">路径</param>
    /// <returns>是否存在版本</returns>
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
                        && (obj.ContainsKey("arguments") || obj.ContainsKey("minecraftArguments"))
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

    /// <summary>
    /// 是否为MMC版本
    /// </summary>
    /// <param name="dir">路径</param>
    /// <returns>是否存在</returns>
    public static bool IsMMCVersion(string dir)
    {
        if (File.Exists(Path.Combine(dir, Names.NameMMCJsonFile)) 
            && File.Exists(Path.Combine(dir, Names.NameMMCCfgFile)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 扫描目录下的游戏版本
    /// </summary>
    /// <param name="dir">路径</param>
    /// <returns>版本列表</returns>
    public static List<string> ScanVersions(string dir)
    {
        var list = new List<string>();
        var dirs = PathHelper.GetDirs(dir);
        dirs.Insert(0, new DirectoryInfo(dir));
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
}
