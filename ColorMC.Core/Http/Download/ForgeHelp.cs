using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.Download;

public static class ForgeHelp
{
    public static void UnpackNative(string version, FileStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using ZipFile zFile = new(stream);
        foreach (ZipEntry e in zFile)
        {
            if (e.Name.StartsWith("META-INF"))
                continue;
            if (e.IsFile)
            {
                using var stream1 = new FileStream(LibrariesPath.GetNativeDir(version) + "/" + e.Name,
                    FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var stream2 = zFile.GetInputStream(e);
                stream2.CopyTo(stream1);
            }
        }
    }

    public static List<DownloadItem> MakeForgeLibs(ForgeLaunchObj info, string mc, string version)
    {
        var v2 = CheckRule.GameLaunchVersion(mc);
        var list = new List<DownloadItem>();
        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith("net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
                list.Add(BuildForgeUniversal(mc, version));

                if(v2) list.Add(BuildForgeLauncher(mc, version));
            }
            else
            {
                list.Add(new()
                {
                    Url = UrlHelp.DownloadForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source),
                    Name = item1.name,
                    Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                    SHA1 = item1.downloads.artifact.sha1
                });
            }
        }

        return list;
    }

    public static string FixForgeUrl(string mc)
    {
        if (mc == "1.7.2")
        {
            return "-mc172";
        }
        else if (mc == "1.7.10")
        {
            return "-1.7.10";
        }
        else if (mc == "1.8.9")
        {
            return "-1.8.9";
        }
        else if (mc == "1.9")
        {
            return "-1.9.0";
        }
        else if (mc == "1.9.4")
        {
            return "-1.9.4";
        }
        else if (mc == "1.10")
        {
            return "-1.10.0";
        }

        return string.Empty;
    }

    public static DownloadItem BuildForgeItem(string mc, string version, string type)
    {
        version += FixForgeUrl(mc);
        string name = $"forge-{mc}-{version}-{type}";
        string url = UrlHelp.DownloadForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = "net.minecraftforge:forge:" + name,
            Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/{mc}-{version}/{name}.jar",
        };
    }

    public static DownloadItem BuildForgeInster(string mc, string version)
    {
        return BuildForgeItem(mc, version, "installer");
    }

    public static DownloadItem BuildForgeUniversal(string mc, string version)
    {
        return BuildForgeItem(mc, version, "universal");
    }

    public static DownloadItem BuildForgeLauncher(string mc, string version)
    {
        return BuildForgeItem(mc, version, "launcher");
    }

    public static ForgeLaunchObj.Libraries MakeLibObj(ForgeInstallObj1.VersionInfo.Libraries item)
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
                        url = null
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

    public static string ForgeWrapper => LibrariesPath.BaseDir + "/io/github/zekerzhayard/ForgeWrapper/mmc3/ForgeWrapper-mmc3.jar";
    public static void ReadyForgeWrapper()
    {
        var file = new FileInfo(ForgeWrapper);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            File.WriteAllBytes(file.FullName, Resource1.ForgeWrapper_mmc3);
        }
    }
}
