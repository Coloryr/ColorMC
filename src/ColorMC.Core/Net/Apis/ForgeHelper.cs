using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class ForgeHelper
{
    public static string ForgeWrapper =>
        LibrariesPath.BaseDir + "/io/github/zekerzhayard/ForgeWrapper/mmc3/ForgeWrapper-mmc3.jar";

    private static List<string>? SupportVersion;
    /// <summary>
    /// 获取支持的版本
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetSupportVersion(SourceLocal? local = null)
    {
        try
        {
            if (SupportVersion != null)
                return SupportVersion;

            if (local == SourceLocal.BMCLAPI
                || local == SourceLocal.MCBBS)
            {
                string url = UrlHelper.ForgeVersion(local);
                var data = await BaseClient.GetString(url);
                if (data.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }
                var obj = JsonConvert.DeserializeObject<List<string>>(data.Item2!);
                if (obj == null)
                    return null;

                SupportVersion = obj;

                return obj;
            }
            else
            {
                string url = UrlHelper.ForgeVersion(SourceLocal.Offical);
                var html = await BaseClient.GetString(url);
                if (html.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(html.Item2!);
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

                SupportVersion = list;

                return list;
            }
        }
        catch (Exception e)
        {
            Logs.Error("获取Forge支持版本错误", e);
            return null;
        }
    }

    /// <summary>
    /// 创建Forge运行库下载文件列表
    /// </summary>
    /// <param name="info">Forge启动数据</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns></returns>
    public static List<DownloadItemObj> MakeForgeLibs(ForgeLaunchObj info, string mc, string version)
    {
        var version1 = VersionPath.GetGame(mc)!;
        var v2 = CheckRule.GameLaunchVersion(version1);
        var list = new List<DownloadItemObj>();

        if (v2)
        {
            list.Add(BuildForgeUniversal(mc, version));
            list.Add(BuildForgeInster(mc, version));
            if (CheckRule.GameLaunchVersion117(mc))
            {

            }
            else
            {
                list.Add(BuildForgeLauncher(mc, version));
            }
        }
        else
        {

        }

        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith("net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
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

    private static string FixForgeUrl(string mc)
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

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="type">类型</param>
    /// <returns>下载项目</returns>
    private static DownloadItemObj BuildForgeItem(string mc, string version, string type)
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

    /// <summary>
    /// 创建Forge安装器下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    public static DownloadItemObj BuildForgeInster(string mc, string version)
    {
        return BuildForgeItem(mc, version, "installer");
    }

    /// <summary>
    /// 创建Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    public static DownloadItemObj BuildForgeUniversal(string mc, string version)
    {
        return BuildForgeItem(mc, version, "universal");
    }
    public static DownloadItemObj BuildForgeLauncher(string mc, string version)
    {
        return BuildForgeItem(mc, version, "launcher");
    }
    //public static DownloadItemObj BuildForgeClient(string mc, string version)
    //{
    //    return BuildForgeItem(mc, version, "client");
    //}

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

    /// <summary>
    /// 获取版本列表
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <param name="local">下载源</param>
    /// <returns>版本列表</returns>
    public static async Task<List<string>?> GetVersionList(string version, SourceLocal? local = null)
    {
        try
        {
            List<string> list = new();
            if (local == SourceLocal.BMCLAPI
                || local == SourceLocal.MCBBS)
            {
                string url = UrlHelper.ForgeVersions(version, local);
                var data = await BaseClient.GetString(url);
                if (data.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }
                var obj = JsonConvert.DeserializeObject<List<ForgeVersionObj1>>(data.Item2!);
                if (obj == null)
                    return null;

                var list1 = new List<Version>();
                foreach (var item in obj)
                {
                    list1.Add(new Version(item.version));
                }
                list1.Sort();
                list1.Reverse();

                foreach (var item in list1)
                {
                    list.Add(item.ToString());
                }
                return list;
            }
            else
            {
                string url = UrlHelper.ForgeVersions(version, SourceLocal.Offical);
                var data = await BaseClient.DownloadClient.GetAsync(url);

                string? html = null;
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
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Game.Error5"), e);
            return null;
        }
    }
}
