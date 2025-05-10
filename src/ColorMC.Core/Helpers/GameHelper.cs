using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 游戏处理
/// </summary>
public static class GameHelper
{
    /// <summary>
    /// ForgeWrapper位置
    /// </summary>
    public static FileItemObj ForgeWrapper { get; private set; }

    /// <summary>
    /// OptifineWrapper位置
    /// </summary>
    public static FileItemObj OptifineWrapper { get; private set; }

    /// <summary>
    /// ColorMCASM位置
    /// </summary>
    public static FileItemObj ColorMCASM { get; private set; }

    /// <summary>
    /// Forge运行库修改映射
    /// </summary>
    /// <param name="item">运行库项目</param>
    /// <returns>运行库项目</returns>
    public static ForgeLaunchObj.ForgeLibrariesObj? MakeLibObj(string item)
    {
        var args = item.Split(":");
        if (args[0] == "net.minecraftforge" && args[1] == "forge")
        {
            return new()
            {
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
                Name = item,
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
        ForgeWrapper = new()
        {
            Name = "io.github.zekerzhayard:forgewrapper:colormc-1.6.0",
            Local = Path.Combine(LibrariesPath.BaseDir, "io", "github", "zekerzhayard", "forgewrapper",
            "colormc-1.6.0", "forgewrapper-colormc-1.6.0.jar")
        };
        var file = new FileInfo(ForgeWrapper.Local);
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
        OptifineWrapper = new()
        {
            Name = "com.coloryr:optifinewrapper:1.1",
            Local = Path.Combine(LibrariesPath.BaseDir, "com", "coloryr", "optifinewrapper", "1.1", "optifinewrapper-1.1.jar")
        };
        var file = new FileInfo(OptifineWrapper.Local);
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
    /// ColorMCASM加载器
    /// </summary>
    public static void ReadyColorMCASM()
    {
        ColorMCASM = new()
        {
            Name = "com.coloryr.colormc:colormcasm:1.1:all",
            Local = Path.Combine(LibrariesPath.BaseDir, "com", "coloryr", "colormc", "colormcasm", "1.1", "colormcasm-1.1-all.jar")
        };
        var file = new FileInfo(ColorMCASM.Local);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            PathHelper.WriteBytes(file.FullName, Resource1.ColorMCASM_1_1_all);
        }
    }

#if Phone
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
#endif
    /// <summary>
    /// 解压native
    /// </summary>
    /// <param name="native">解压路径</param>
    /// <param name="stream">文件流</param>
    public static void UnpackNative(string native, Stream stream)
    {
        using var zFile = new ZipArchive(stream);
        foreach (var e in zFile.Entries)
        {
            if (e.FullName.StartsWith("META-INF"))
            {
                continue;
            }
            var file = Path.Combine(native, e.Name);
            if (File.Exists(file))
            {
                continue;
            }

            using var stream2 = e.Open();
            PathHelper.WriteBytes(file, stream2);
        }
    }

    /// <summary>
    /// MMC配置转ColorMC
    /// </summary>
    /// <param name="mmc">MMC储存</param>
    /// <param name="mmc1">MMC储存</param>
    /// <returns>游戏设置</returns>
    public static MMCToColorMCRes ToColorMC(this MMCObj mmc, Dictionary<string, string> list)
    {
        var res = new MMCToColorMCRes();
        
        var game = new GameSettingObj
        {
            Loader = Loaders.Normal
        };

        //判断游戏与加载器版本
        foreach (var item in mmc.Components)
        {
            if (item.Uid == "net.minecraft")
            {
                game.Version = item.Version;
                if (string.IsNullOrEmpty(game.Version))
                {
                    game.Version = item.CachedVersion;
                }
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
            else
            {
                game.CustomLoader ??= new();
                game.CustomLoader.CustomJson = true;
            }
        }
        game.JvmArg = new();
        game.Window = new();
        //取出相关配置
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
            game.JvmArg.JvmArgs = item1.Replace(' ', '\n').Replace("\"", "");
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
            else if (item.Id == "neoforge")
            {
                game.Loader = Loaders.NeoForge;
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

            //从运行库判断加载器类型
            foreach (var item in obj.Libraries)
            {
                if (item.Name.Contains(Names.NameMinecraftForgeKey))
                {
                    var names = item.Name.Split(':');
                    if (names.Length >= 3 && (names[1] is Names.NameForgeKey or Names.NameFmlKey))
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
                else if (item.Name.Contains(Names.NameNeoForgedKey))
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
                else if (item.Name.Contains(Names.NameFabricMcKey))
                {
                    var names = item.Name.Split(':');
                    if (names.Length >= 3 && names[1] == Names.NameFabricLoaderKey)
                    {
                        game.Loader = Loaders.Fabric;
                        game.LoaderVersion = names[2];
                        break;
                    }
                }
                else if (item.Name.Contains(Names.NameQuiltMcKey))
                {
                    var names = item.Name.Split(':');
                    if (names.Length >= 3 && names[1] == Names.NameQuiltLoaderKey)
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
        var files = PathHelper.GetFiles(dir);
        foreach (var item3 in files)
        {
            if (!item3.Name.EndsWith(Names.NameJsonExt))
            {
                continue;
            }
            try
            {
                using var stream = PathHelper.OpenRead(item3.FullName);
                var obj = JsonUtils.ReadAsObj(stream);
                if (obj == null)
                {
                    continue;
                }
                if (obj.ContainsKey("id")
                    && (obj.ContainsKey("arguments") || obj.ContainsKey("minecraftArguments"))
                    && obj.ContainsKey("mainClass"))
                {
                    return true;
                }
            }
            catch
            {

            }
        }

        return false;
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
            //官器格式
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
            //MMC格式
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

            //是否只有一个官器版本
            if (IsMinecraftVersion(item.FullName))
            {
                list.Add(item.FullName);
                continue;
            }

            //是否只有一个MMC版本
            if (IsMMCVersion(item.FullName))
            {
                list.Add(item.FullName);
            }
        }

        return list;
    }

    /// <summary>
    /// 获取Forge的所有运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static ForgeGetFilesRes? GetForgeLibs(this GameSettingObj obj)
    {
        var neo = obj.Loader == Loaders.NeoForge;
        var version1 = VersionPath.GetVersion(obj.Version)!;
        var v2 = version1.IsGameVersionV2();

        var forge = neo ?
            VersionPath.GetNeoForgeObj(obj.Version, obj.LoaderVersion!) :
            VersionPath.GetForgeObj(obj.Version, obj.LoaderVersion!);
        if (forge == null)
        {
            return null;
        }

        //forge本体
        var list1 = GameDownloadHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!, neo, v2, false).ToList();

        var forgeinstall = neo ?
            VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!) :
            VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!);
        if (forgeinstall == null && v2)
        {
            return null;
        }

        //forge安装器
        if (forgeinstall != null)
        {
            var list2 = GameDownloadHelper.BuildForgeLibs(forgeinstall, obj.Version,
                obj.LoaderVersion!, neo, v2);
            return new() 
            { 
                Loaders = list1,
                Installs = [.. list2]
            };
        }

        return new()
        {
            Loaders = list1
        };
    }

    /// <summary>
    /// 获取Fabric的所有运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static List<FileItemObj>? GetFabricLibs(this GameSettingObj obj)
    {
        var fabric = VersionPath.GetFabricObj(obj.Version, obj.LoaderVersion!);
        if (fabric == null)
        {
            return null;
        }

        var list = new List<FileItemObj>();

        foreach (var item in fabric.Libraries)
        {
            var name = FuntionUtils.VersionNameToPath(item.Name);
            string file = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name}");
            list.Add(new()
            {
                Url = UrlHelper.DownloadFabric(item.Url, CoreHttpClient.Source) + name,
                Name = item.Name,
                Local = file
            });
        }

        return list;
    }

    /// <summary>
    /// 获取Quilt的所有运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static List<FileItemObj>? GetQuiltLibs(this GameSettingObj obj)
    {
        var quilt = VersionPath.GetQuiltObj(obj.Version, obj.LoaderVersion!);
        if (quilt == null)
        {
            return null;
        }

        var list = new List<FileItemObj>();

        foreach (var item in quilt.Libraries)
        {
            var name = FuntionUtils.VersionNameToPath(item.Name);
            string file = Path.GetFullPath($"{LibrariesPath.BaseDir}/{name}");

            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.Url, CoreHttpClient.Source) + name,
                Name = item.Name,
                Local = file
            });
        }

        return list;
    }

    /// <summary>
    /// 获取高清修复所有运行库
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static List<FileItemObj>? GetOptifineLibs(this GameSettingObj obj)
    {
        var optifine = obj.GetOptifine();
        if (optifine == null)
        {
            return null;
        }
        return [new()
        {
            Name = optifine.FileName,
            Local = LibrariesPath.GetOptifineFile(obj.Version, obj.LoaderVersion!),
            Overwrite = true,
            Url = optifine.Url1
        }];
    }
}
