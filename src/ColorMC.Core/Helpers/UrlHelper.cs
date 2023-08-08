using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Optifine;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 网址处理
/// </summary>
public static class UrlHelper
{
    public const string BMCLAPI = "https://bmclapi2.bangbang93.com/";
    public const string MCBBS = "https://download.mcbbs.net/";

    public const string Authlib = "https://authlib-injector.yushi.moe/";

    public const string LittleSkin = "https://littleskin.cn/";

    public const string Nide8 = "https://auth.mc-user.com:233/";
    public const string Nide8Jar = "https://login.mc-user.com:233/index/jar";

    public const string CurseForgeDownload = "https://edge.forgecdn.net/";
    public const string CurseForge = "https://api.curseforge.com/v1/";

    public const string ModrinthDownload = "https://cdn.modrinth.com/";

    public const string Modrinth = "https://api.modrinth.com/v2/";
    public const string Forge = "https://maven.minecraftforge.net/";
    public const string Fabric = "https://maven.fabricmc.net/";
    public const string FabricMeta = "https://meta.fabricmc.net/";
    public const string Quilt = "https://maven.quiltmc.org/";
    public const string QuiltMeta = "https://meta.quiltmc.org/";
    public const string Optifine = "https://optifine.net/";
    public const string NeoForge = "https://maven.neoforged.net/";

    public const string Minecraft = "https://www.minecraft.net/";
    public const string MinecraftLib = "https://libraries.minecraft.net/";
    public const string MinecraftResources = "https://resources.download.minecraft.net/";
    public static readonly string[] Mojang =
    {
        "https://launchermeta.mojang.com/",
        "https://launcher.mojang.com/",
        "https://piston-data.mojang.com/"
    };

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
            SourceLocal.BMCLAPI => $"{BMCLAPI}mc/game/version_manifest_v2.json",
            SourceLocal.MCBBS => $"{MCBBS}mc/game/version_manifest_v2.json",
            _ => $"{Mojang[0]}mc/game/version_manifest_v2.json"
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

        foreach (var item in Mojang)
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

        url = url.Replace(MinecraftLib, to);

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
            _ => $"{MinecraftResources}{uuid[..2]}/{uuid}"
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
            _ => $"{Forge}net/minecraftforge/forge/{mc}-{version}/"
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
            _ => $"{NeoForge}releases/net/neoforged/forge/{mc}-{version}/"
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

        return url.Replace(Forge, to);
    }

    /// <summary>
    /// NeoForge运行库地址
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
    public static string GetFabricMeta(SourceLocal? local)
    {
        string? url = local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}fabric-meta/v2/versions",
            SourceLocal.MCBBS => $"{MCBBS}fabric-meta/v2/versions",
            _ => $"{FabricMeta}v2/versions"
        };

        return url;
    }

    /// <summary>
    /// Quilt地址
    /// </summary>
    public static string GetQuiltMeta(SourceLocal? local)
    {
        string? url = local switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}quilt-meta/v3/versions",
            //SourceLocal.MCBBS => $"{MCBBS}quilt-meta/v3/versions",
            _ => $"{QuiltMeta}v3/versions"
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

        return url.Replace(Fabric, replace);
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

        return url.Replace($"{Quilt}repository/release/", replace);
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
            _ => $"{Authlib}artifacts.json"
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
            _ => $"{Authlib}artifact/{obj.build_number}.json"
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
            _ => $"{Authlib}artifact/{obj.build_number}/authlib-injector-{obj.version}.jar"
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
            _ => $"{Forge}net/minecraftforge/forge/maven-metadata.xml"
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
            _ => $"{Forge}net/minecraftforge/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// NeoForge版本地址
    /// </summary>
    public static string NeoForgeVersion(SourceLocal? local)
    {
        return local switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft/{version}",
            //SourceLocal.MCBBS => $"{MCBBS}forge/minecraft/{version}",
            _ => $"{NeoForge}releases/net/neoforged/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// NeoForge版本地址
    /// </summary>
    public static string NeoForgeVersions(string version, SourceLocal? local)
    {
        return local switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft/{version}",
            //SourceLocal.MCBBS => $"{MCBBS}forge/minecraft/{version}",
            _ => $"{NeoForge}releases/net/neoforged/forge/maven-metadata.xml"
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
            if (old.StartsWith(Forge))
            {
                return (true, old.Replace(Forge,
                    random.Next() % 2 == 0 ? $"{BMCLAPI}maven" : $"{MCBBS}maven"));
            }
            else if (old.StartsWith(MinecraftLib))
            {
                return (true, old.Replace(MinecraftLib,
                   random.Next() % 2 == 0 ? $"{BMCLAPI}maven" : $"{MCBBS}maven"));
            }
            else if (old.StartsWith(Fabric))
            {
                return (true, old.Replace(Fabric,
                   random.Next() % 2 == 0 ? $"{BMCLAPI}maven" : $"{MCBBS}maven"));
            }
        }

        return (false, null);
    }

    /// <summary>
    /// Optifine信息
    /// </summary>
    public static string GetOptifine(SourceLocal? local)
    {
        return local switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}optifine/versionList",
            SourceLocal.MCBBS => $"{MCBBS}optifine/versionList",
            _ => $"{Optifine}downloads"
        };
    }

    /// <summary>
    /// Optifine下载
    /// </summary>
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
    public static string MakeDownloadUrl(SourceType? type, string pid, string fid, string file)
    {
        if (type == SourceType.CurseForge)
        {
            var fid1 = long.Parse(fid);
            return $"{CurseForgeDownload}files/{fid1 / 1000}/{fid1 % 1000}/{file}";
        }
        else if (type == SourceType.Modrinth)
        {
            return $"{ModrinthDownload}data/{pid}/versions/{fid}/{file}";
        }

        return "";
    }

    /// <summary>
    /// 创建下载地址
    /// </summary>
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
        else if (FuntionUtils.CheckNotNumber(item.Projcet) || FuntionUtils.CheckNotNumber(item.FileId))
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
