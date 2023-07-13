using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Optifine;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

public static class UrlHelper
{
    public const string BMCLAPI = "https://bmclapi2.bangbang93.com/";
    public const string MCBBS = "https://download.mcbbs.net/";

    public const string LittleSkin = "https://littleskin.cn/";

    public const string Nide8 = "https://auth.mc-user.com:233/";
    public const string Nide8Jar = "https://login.mc-user.com:233/index/jar";

    public const string CurseForgeDownload = "https://edge.forgecdn.net/";
    public const string CurseForgeUrl = "https://api.curseforge.com/v1/";

    public const string ModrinthUrl = "https://api.modrinth.com/v2/";

    public const string OptifineUrl = "https://optifine.net/";

    public const string NeoForgeUrl = "https://maven.neoforged.net/";

    private static readonly string[] MojangUrl =
    {
        "https://launchermeta.mojang.com/",
        "https://launcher.mojang.com/",
        "https://piston-data.mojang.com"
    };

    private const string MinecraftLibUrl = "https://libraries.minecraft.net/";

    private const string ForgeUrl = "https://maven.minecraftforge.net/";

    private const string FabricUrl = "https://maven.fabricmc.net/";

    public static readonly string[] MavenUrl =
    {
        "https://repo1.maven.org/maven2/",
        "https://maven.aliyun.com/repository/public/"
    };

    /// <summary>
    /// 修正Forge下载地址
    /// </summary>
    /// <param name="mc">游戏版本号</param>
    /// <returns></returns>
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

        foreach (var item in MojangUrl)
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

        url = url.Replace(MinecraftLibUrl, to);

        return url;
    }

    /// <summary>
    /// 资源文件地址
    /// </summary>
    public static string DownloadAssets(string uuid, SourceLocal? local)
    {
        string? url = local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}assets/{uuid[..2]}/{uuid}",
            SourceLocal.MCBBS => $"{MCBBS}assets/{uuid[..2]}/{uuid}",
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
    /// NeoForge地址
    /// </summary>
    public static string DownloadNeoForgeJar(string mc, string version, SourceLocal? local)
    {
        string? url = local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}maven/net/neoforged/forge/{mc}-{version}/",
            SourceLocal.MCBBS => $"{MCBBS}maven/net/neoforged/forge/{mc}-{version}/",
            _ => $"https://maven.neoforged.net/releases/net/neoforged/forge/{mc}-{version}/"
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

        url = url.Replace(ForgeUrl, to);

        return url;
    }

    /// <summary>
    /// Forge运行库地址
    /// </summary>
    public static string DownloadNeoForgeLib(string url, SourceLocal? local)
    {
        return url;
        //string? to = local switch
        //{
        //    SourceLocal.BMCLAPI => BMCLAPI + "maven/",
        //    SourceLocal.MCBBS => MCBBS + "maven/",
        //    _ => null
        //};
        //if (to == null)
        //{
        //    return url;
        //}

        //url = url.Replace(NeoForgeUrl, to);

        //return url;
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
            SourceLocal.BMCLAPI => $"{BMCLAPI}maven/",
            SourceLocal.MCBBS => $"{MCBBS}maven/",
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
            SourceLocal.BMCLAPI => $"{BMCLAPI}mirrors/authlib-injector/artifacts.json",
            SourceLocal.MCBBS => $"{MCBBS}mirrors/authlib-injector/artifacts.json",
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
            SourceLocal.BMCLAPI => $"{BMCLAPI}mirrors/authlib-injector/artifact/{obj.build_number}.json",
            SourceLocal.MCBBS => $"{MCBBS}mirrors/authlib-injector/artifact/{obj.build_number}.json",
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
            SourceLocal.BMCLAPI => $"{BMCLAPI}mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
            SourceLocal.MCBBS => $"{MCBBS}mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
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
            SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft",
            SourceLocal.MCBBS => $"{MCBBS}forge/minecraft",
            //_ => "https://files.minecraftforge.net/net/minecraftforge/forge/"
            _ => "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// Forge版本地址
    /// </summary>
    public static string ForgeVersions(string version, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft/{version}",
            SourceLocal.MCBBS => $"{MCBBS}forge/minecraft/{version}",
            //_ => $"https://files.minecraftforge.net/net/minecraftforge/forge/index_{version}.html"
            _ => "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml"
        };
    }

    public static string NeoForgeVersion(SourceLocal? local)
    {
        return local switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft/{version}",
            //SourceLocal.MCBBS => $"{MCBBS}forge/minecraft/{version}",
            //_ => $"https://files.minecraftforge.net/net/minecraftforge/forge/index_{version}.html"
            _ => $"{NeoForgeUrl}releases/net/neoforged/forge/maven-metadata.xml"
        };
    }

    public static string NeoForgeVersions(string version, SourceLocal? local)
    {
        return local switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft/{version}",
            //SourceLocal.MCBBS => $"{MCBBS}forge/minecraft/{version}",
            //_ => $"https://files.minecraftforge.net/net/minecraftforge/forge/index_{version}.html"
            _ => $"{NeoForgeUrl}releases/net/neoforged/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// Url地址变换
    /// </summary>
    /// <param name="old"></param>
    /// <returns></returns>
    public static (bool, string?) UrlChange(string old)
    {
        var random = new Random();
        if (BaseClient.Source == SourceLocal.Offical)
        {
            if (old.StartsWith(ForgeUrl))
            {
                return (true, old.Replace(ForgeUrl,
                    random.Next() % 2 == 0 ? $"{BMCLAPI}maven" : $"{MCBBS}maven"));
            }
            else if (old.StartsWith(MinecraftLibUrl))
            {
                return (true, old.Replace(MinecraftLibUrl,
                   random.Next() % 2 == 0 ? $"{BMCLAPI}maven" : $"{MCBBS}maven"));
            }
            else if (old.StartsWith(FabricUrl))
            {
                return (true, old.Replace(FabricUrl,
                   random.Next() % 2 == 0 ? $"{BMCLAPI}maven" : $"{MCBBS}maven"));
            }
        }

        return (false, null);
    }

    /// <summary>
    /// Optifine信息
    /// </summary>
    /// <param name="local"></param>
    /// <returns></returns>
    public static string Optifine(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}optifine/versionList",
            SourceLocal.MCBBS => $"{MCBBS}optifine/versionList",
            _ => "https://optifine.net/downloads"
        };
    }

    /// <summary>
    /// Optifine下载
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="local"></param>
    /// <returns></returns>
    public static string OptifineDownload(OptifineListObj obj, SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.MCBBS => $"{MCBBS}optifine/{obj.mcversion}/HD_U/{obj.patch}",
            _ => $"{BMCLAPI}optifine/{obj.mcversion}/HD_U/{obj.patch}"
        };
    }

    /// <summary>
    /// 创建下载地址
    /// </summary>
    /// <param name="type">资源来源</param>
    /// <param name="pid"></param>
    /// <param name="fid"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string MakeDownloadUrl(SourceType? type, string pid, string fid, string file)
    {
        if (type == SourceType.CurseForge)
        {
            var fid1 = long.Parse(fid);
            return $"https://edge.forgecdn.net/files/{fid1 / 1000}/{fid1 % 1000}/{file}";
        }
        else if (type == SourceType.Modrinth)
        {
            return $"https://cdn.modrinth.com/data/{pid}/versions/{fid}/{file}";
        }

        return "";
    }

    /// <summary>
    /// 创建下载地址
    /// </summary>
    /// <param name="item"></param>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string MakeUrl(ServerModItemObj item, FileType type, string url)
    {
        if (string.IsNullOrWhiteSpace(item.Projcet) || string.IsNullOrWhiteSpace(item.FileId))
        {
            if (!string.IsNullOrWhiteSpace(item.Url))
            {
                return item.Url;
            }
            else if (!string.IsNullOrWhiteSpace(url))
            {
                return url + type switch
                {
                    FileType.Mod => " mods/",
                    FileType.Resourcepack => "resourcepacks/",
                    _ => "/"
                } + item.File;
            }
            else
            {
                return "";
            }
        }
        else if (Funtcions.CheckNotNumber(item.Projcet) || Funtcions.CheckNotNumber(item.FileId))
        {
            return MakeDownloadUrl(SourceType.Modrinth, item.Projcet,
                item.FileId, item.File);
        }
        else
        {
            return MakeDownloadUrl(SourceType.CurseForge, item.Projcet,
                item.FileId, item.File);
        }
    }
}
