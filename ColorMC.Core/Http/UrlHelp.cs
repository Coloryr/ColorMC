using ColorMC.Core.Objs.Game;

namespace ColorMC.Core.Http;

public static class UrlHelp
{
    private const string BMCLAPI = "https://bmclapi2.bangbang93.com/";
    private const string MCBBS = "https://download.mcbbs.net";

    private static readonly string[] originServers =
      { "https://launchermeta.mojang.com/", "https://launcher.mojang.com/", "https://piston-data.mojang.com" };

    private static readonly string[] originServers1 =
      { "https://libraries.minecraft.net/"};

    private static readonly string[] originServers2 =
      { "https://maven.minecraftforge.net/"};

    private static readonly string[] originServers3 =
      { "https://maven.fabricmc.net/"};

    public static string GameVersion(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
            SourceLocal.MCBBS => "https://download.mcbbs.net/mc/game/version_manifest_v2.json",
            _ => "http://launchermeta.mojang.com/mc/game/version_manifest_v2.json"
        };
    }

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

        foreach (var item in originServers1)
        {
            url = url.Replace(item, to);
        }

        return url;
    }

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

        foreach (var item in originServers2)
        {
            url = url.Replace(item, to);
        }

        return url;
    }

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

    public static string AuthlibInjectorMeta(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/mirrors/authlib-injector/artifacts.json",
            SourceLocal.MCBBS => $"{MCBBS}/mirrors/authlib-injector/artifacts.json",
            _ => $"https://authlib-injector.yushi.moe/artifacts.json"
        };
    }

    public static string AuthlibInjector(AuthlibInjectorMetaObj.Artifacts obj, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/mirrors/authlib-injector/artifact/{obj.build_number}.json",
            SourceLocal.MCBBS => $"{MCBBS}/mirrors/authlib-injector/artifact/{obj.build_number}.json",
            _ => $"https://authlib-injector.yushi.moe/artifact/{obj.build_number}.json"
        };
    }

    public static string DownloadAuthlibInjector(AuthlibInjectorObj obj, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}/mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
            SourceLocal.MCBBS => $"{MCBBS}/mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
            _ => $"https://authlib-injector.yushi.moe/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar"
        };
    }
}
