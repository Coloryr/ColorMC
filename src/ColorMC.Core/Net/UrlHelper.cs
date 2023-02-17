using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net;

public static class UrlHelper
{
    private const string BMCLAPI = "https://bmclapi2.bangbang93.com/";
    private const string MCBBS = "https://download.mcbbs.net/";

    private static readonly string[] originServers =
      { "https://launchermeta.mojang.com/", "https://launcher.mojang.com/", "https://piston-data.mojang.com" };

    private static readonly string originServers1 = "https://libraries.minecraft.net/";

    private static readonly string originServers2 = "https://maven.minecraftforge.net/";

    private static readonly string originServers3 = "https://maven.fabricmc.net/";

    /// <summary>
    /// 游戏版本
    /// </summary>
    public static string GameVersion(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
            SourceLocal.MCBBS => "https://download.mcbbs.net/mc/game/version_manifest_v2.json",
            _ => "http://launchermeta.mojang.com/mc/game/version_manifest_v2.json"
        };
    }

    /// <summary>
    /// 下载地址
    /// </summary>
    public static string Download(string url, SourceLocal? local)
    {
        string? to = local switch
        {
            SourceLocal.BMCLAPI => BMCLAPI,
            SourceLocal.MCBBS => MCBBS,
            _ => null
        };
        if (to == null)
        {
            return url;
        }

        foreach (var item in originServers)
        {
            url = url.Replace(item, to);
        }

        return url;
    }

    /// <summary>
    /// 运行库地址
    /// </summary>
    public static string DownloadLibraries(string url, SourceLocal? local)
    {
        string? to = local switch
        {
            SourceLocal.BMCLAPI => BMCLAPI + "maven/",
            SourceLocal.MCBBS => MCBBS + "maven/",
            _ => null
        };
        if (to == null)
        {
            return url;
        }

        url = url.Replace(originServers1, to);

        return url;
    }

    /// <summary>
    /// 资源文件地址
    /// </summary>
    public static string DownloadAssets(string uuid, SourceLocal? local)
    {
        string? url = local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}assets/{uuid}",
            SourceLocal.MCBBS => $"{MCBBS}assets/{uuid}",
            _ => $"https://resources.download.minecraft.net/{uuid[..2]}/{uuid}"
        };

        return url;
    }

    /// <summary>
    /// Forge地址
    /// </summary>
    public static string DownloadForgeJar(string mc, string version, SourceLocal? local)
    {
        string? url = local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}maven/net/minecraftforge/forge/{mc}-{version}/",
            SourceLocal.MCBBS => $"{MCBBS}maven/net/minecraftforge/forge/{mc}-{version}/",
            _ => $"https://maven.minecraftforge.net/net/minecraftforge/forge/{mc}-{version}/"
        };

        return url;
    }

    /// <summary>
    /// Forge运行库地址
    /// </summary>
    public static string DownloadForgeLib(string url, SourceLocal? local)
    {
        string? to = local switch
        {
            SourceLocal.BMCLAPI => BMCLAPI + "maven/",
            SourceLocal.MCBBS => MCBBS + "maven/",
            _ => null
        };
        if (to == null)
        {
            return url;
        }

        url = url.Replace(originServers2, to);

        return url;
    }

    /// <summary>
    /// Fabric地址
    /// </summary>
    public static string FabricMeta(SourceLocal? local)
    {
        string? url = local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}fabric-meta/v2/versions",
            SourceLocal.MCBBS => $"{MCBBS}fabric-meta/v2/versions",
            _ => $"https://meta.fabricmc.net/v2/versions"
        };

        return url;
    }

    /// <summary>
    /// Quilt地址
    /// </summary>
    public static string QuiltMeta(SourceLocal? local)
    {
        string? url = local switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}quilt-meta/v3/versions",
            //SourceLocal.MCBBS => $"{MCBBS}quilt-meta/v3/versions",
            _ => $"https://meta.quiltmc.org/v3/versions"
        };

        return url;
    }

    /// <summary>
    /// Fabric地址
    /// </summary>
    public static string DownloadFabric(string url, SourceLocal? local)
    {
        string? replace = local switch
        {
            SourceLocal.BMCLAPI => BMCLAPI + "maven/",
            SourceLocal.MCBBS => MCBBS + "maven/",
            _ => null
        };

        if (replace == null)
            return url;

        return url.Replace("https://maven.fabricmc.net/", replace);
    }

    /// <summary>
    /// Quilt地址
    /// </summary>
    public static string DownloadQuilt(string url, SourceLocal? local)
    {
        string? replace = local switch
        {
            //SourceLocal.BMCLAPI => BMCLAPI + "maven/",
            //SourceLocal.MCBBS => MCBBS + "maven/",
            _ => null
        };

        if (replace == null)
            return url;

        return url.Replace("https://maven.quiltmc.org/repository/release/", replace);
    }

    /// <summary>
    /// 外置登录信息地址
    /// </summary>
    public static string AuthlibInjectorMeta(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/mirrors/authlib-injector/artifacts.json",
            SourceLocal.MCBBS => $"{MCBBS}/mirrors/authlib-injector/artifacts.json",
            _ => $"https://authlib-injector.yushi.moe/artifacts.json"
        };
    }

    /// <summary>
    /// 外置登录地址
    /// </summary>
    public static string AuthlibInjector(AuthlibInjectorMetaObj.Artifacts obj, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/mirrors/authlib-injector/artifact/{obj.build_number}.json",
            SourceLocal.MCBBS => $"{MCBBS}/mirrors/authlib-injector/artifact/{obj.build_number}.json",
            _ => $"https://authlib-injector.yushi.moe/artifact/{obj.build_number}.json"
        };
    }

    /// <summary>
    /// 外置登录下载地址
    /// </summary>
    public static string DownloadAuthlibInjector(AuthlibInjectorObj obj, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
            SourceLocal.MCBBS => $"{MCBBS}/mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
            _ => $"https://authlib-injector.yushi.moe/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar"
        };
    }

    /// <summary>
    /// Forge版本地址
    /// </summary>
    public static string ForgeVersion(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/forge/minecraft",
            SourceLocal.MCBBS => $"{MCBBS}/forge/minecraft",
            _ => "https://files.minecraftforge.net/net/minecraftforge/forge/"
        };
    }

    /// <summary>
    /// Forge版本地址
    /// </summary>
    public static string ForgeVersions(string version, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/forge/minecraft/",
            SourceLocal.MCBBS => $"{MCBBS}/forge/minecraft/",
            _ => "https://files.minecraftforge.net/net/minecraftforge/forge/index_"
        };
    }

    public static (bool, string?) UrlChange(string old)
    {
        var random = new Random();
        if (BaseClient.Source == SourceLocal.Offical)
        {
            if (old.StartsWith(originServers2))
            {
                return (true, old.Replace(originServers2,
                    random.Next() % 2 == 0 ? $"{BMCLAPI}/maven" : $"{MCBBS}/maven"));
            }
            else if (old.StartsWith(originServers1))
            {
                return (true, old.Replace(originServers1,
                   random.Next() % 2 == 0 ? $"{BMCLAPI}/maven" : $"{MCBBS}/maven"));
            }
            else if (old.StartsWith(originServers3))
            {
                return (true, old.Replace(originServers3,
                   random.Next() % 2 == 0 ? $"{BMCLAPI}/maven" : $"{MCBBS}/maven"));
            }
        }

        return (false, null);
    }
}
