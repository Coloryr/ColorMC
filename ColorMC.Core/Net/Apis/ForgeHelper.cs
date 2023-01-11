using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class ForgeHelper
{
    public static async Task<List<string>?> GetSupportVersion()
    {
        if (BaseClient.Source == SourceLocal.BMCLAPI
            || BaseClient.Source == SourceLocal.MCBBS)
        {
            string url = UrlHelper.ForgeVersion(BaseClient.Source);
            var data = await BaseClient.GetString(url);
            var obj = JsonConvert.DeserializeObject<List<string>>(data);
            if (obj == null)
                return null;

            return obj;
        }
        else
        {
            string url = UrlHelper.ForgeVersion(SourceLocal.Offical);
            var html = await BaseClient.GetString(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.Descendants("li")
                .Where(x => x.Attributes["class"]?.Value == "li-version-list");
            if (nodes == null)
                return null;
            List<string> list = new();

            foreach (var item in nodes)
            {
                var nodes1 = item.SelectNodes("ul/li/a");
                if (nodes1 == null)
                    return null;

                foreach (var item1 in nodes1)
                {
                    list.Add(item1.InnerText.Trim());
                }

                var nodes2 = item.SelectNodes("ul/li")
                    .Where(a => a.HasClass("elem-active"));

                foreach (var item1 in nodes2)
                {
                    list.Add(item1.InnerText.Trim());
                }
            }

            return list;
        }
    }

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

        if (v2)
        {
            //list.Add(BuildForgeLauncher(mc, version));
            //list.Add(BuildForgeInster(mc, version));
            //list.Add(BuildForgeClient(mc, version));
            //list.Add(BuildForgeUniversal(mc, version));
        }

        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith("net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
                continue;
            }
            else
            {
                list.Add(new()
                {
                    Url = UrlHelper.DownloadForgeLib(item1.downloads.artifact.url,
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
        string url = UrlHelper.DownloadForgeJar(mc, version, BaseClient.Source);

        return new()
        {
            Url = url + name + ".jar",
            Name = $"net.minecraftforge:forge:{mc}-{version}-{type}",
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

    //public static DownloadItem BuildForgeLauncher(string mc, string version)
    //{
    //    return BuildForgeItem(mc, version, "launcher");
    //}

    public static DownloadItem BuildForgeClient(string mc, string version)
    {
        return BuildForgeItem(mc, version, "client");
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

    public static async Task<List<string>?> GetVersionList(string version, SourceLocal? local = null)
    {
        List<string> list = new();
        if (local == SourceLocal.BMCLAPI
            || local == SourceLocal.MCBBS)
        {
            string url = UrlHelper.ForgeVersions(version, local) + version;
            var data = await BaseClient.GetString(url);
            var obj = JsonConvert.DeserializeObject<List<ForgeVersionObj1>>(data);
            if (obj == null)
                return null;

            foreach (var item in obj)
            {
                list.Add(item.version);
            }
            return list;
        }
        else
        {
            string url = UrlHelper.ForgeVersions(version, SourceLocal.Offical) + version + ".html";
            var data = await BaseClient.DownloadClient.GetAsync(url);

            string html = null;
            if (data.IsSuccessStatusCode)
            {
                html = await data.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.Descendants("table")
                .Where(x => x.Attributes["class"]?.Value == "download-list").FirstOrDefault();
            if (nodes == null)
                return null;
            var nodes1 = nodes.Descendants("tbody").FirstOrDefault();
            if (nodes1 == null)
                return null;

            foreach (var item in nodes1.Descendants("tr"))
            {
                var item1 = item.Descendants("td").Where(x => x.Attributes["class"]?.Value == "download-version").FirstOrDefault();
                if (item1 != null)
                {
                    string item2 = item1.InnerText.Trim();
                    if (item2.Contains("Branch:"))
                    {
                        int a = item2.IndexOf(' ');
                        item2 = item2[..a].Trim();
                    }
                    list.Add(item2);
                }
            }
            return list;
        }
    }
}
