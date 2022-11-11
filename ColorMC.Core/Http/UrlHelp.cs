using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ColorMC.Core.Http;

public static class UrlHelp
{
    private const string BMCLAPI = "https://bmclapi2.bangbang93.com/";
    private const string MCBBS = "https://download.mcbbs.net";

    private static readonly string[] originServers =
      { "https://launchermeta.mojang.com/", "https://launcher.mojang.com/", "https://piston-data.mojang.com" };

    private static readonly string[] originServers1 =
      { "https://libraries.minecraft.net/"};

    public static string Version(SourceLocal? local)
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
            SourceLocal.BMCLAPI => BMCLAPI + "assets/" + uuid,
            SourceLocal.MCBBS => MCBBS + "assets/" + uuid,
            _ => "https://resources.download.minecraft.net/" + uuid[0..1] + uuid
        };

        return url;
    }
}
