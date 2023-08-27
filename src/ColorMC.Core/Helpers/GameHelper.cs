using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;

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
            "/io/github/zekerzhayard/ForgeWrapper/1.5.6/ForgeWrapper-1.5.6.jar");
        var file = new FileInfo(ForgeWrapper);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            File.WriteAllBytes(file.FullName, Resource1.ForgeWrapper_1_5_6);
        }
    }

    public static GameArgObj.Libraries ReplaceLib(GameArgObj.Libraries item)
    {
        if (item.name.StartsWith("net.java.dev.jna:jna:"))
        {
            string[] version = item.name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) >= 5 && int.Parse(version[1]) >= 13) return item;
            item.name = "net.java.dev.jna:jna:5.13.0";
            item.downloads.artifact.path = "net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar";
            item.downloads.artifact.sha1 = "1200e7ebeedbe0d10062093f32925a912020e747";
            item.downloads.artifact.url = "https://repo1.maven.org/maven2/net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar";
        }
        else if (item.name.StartsWith("com.github.oshi:oshi-core:"))
        {
            string[] version = item.name.Split(":")[2].Split(".");
            if (int.Parse(version[0]) >= 5 && int.Parse(version[1]) >= 13) return item;
            item.name = "com.github.oshi:oshi-core:6.3.0";
            item.downloads.artifact.path = "com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar";
            item.downloads.artifact.sha1 = "9e98cf55be371cafdb9c70c35d04ec2a8c2b42ac";
            item.downloads.artifact.url = "https://repo1.maven.org/maven2/com/github/oshi/oshi-core/6.3.0/oshi-core-6.3.0.jar";
        }

        return item;
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
            var temp = FuntionUtils.ArgParse(item1);
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
