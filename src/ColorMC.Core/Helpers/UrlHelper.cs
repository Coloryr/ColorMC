using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Objs.ServerPack;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 网址处理
/// </summary>
public static class UrlHelper
{
    public const string BMCLAPI = "https://bmclapi2.bangbang93.com/";

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

    public const string OptiFine = "https://optifine.net/";

    public const string NeoForge = "https://maven.neoforged.net/releases/";

    public const string Minecraft = "https://www.minecraft.net/";
    public const string MinecraftLib = "https://libraries.minecraft.net/";
    public const string MinecraftResources = "https://resources.download.minecraft.net/";

    public static readonly string[] Mojang =
    [
        "https://launchermeta.mojang.com/",
        "https://launcher.mojang.com/",
        "https://piston-data.mojang.com/",
        "https://piston-meta.mojang.com/"
    ];

    public static readonly string[] MavenUrl =
    [
        "https://repo1.maven.org/maven2/",
        "https://maven.aliyun.com/repository/public/"
    ];

    /// <summary>
    /// 修正Forge下载地址
    /// </summary>
    /// <param name="mc">游戏版本号</param>
    /// <returns>修正的名字</returns>
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
            _ => $"{Mojang[0]}mc/game/version_manifest_v2.json"
        };
    }

    /// <summary>
    /// 下载地址转换
    /// </summary>
    /// <param name="url">原始下载地址</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string Download(string url, SourceLocal? source)
    {
        string? to = source switch
        {
            SourceLocal.BMCLAPI => BMCLAPI,
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
    /// 游戏下载
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadGame(string mc, SourceLocal? source)
    {
        return $"{BMCLAPI}version/{mc}/client";
    }

    /// <summary>
    /// 运行库下载地址
    /// </summary>
    /// <param name="url">原始地址</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadLibraries(string url, SourceLocal? source)
    {
        string? to = source switch
        {
            SourceLocal.BMCLAPI => BMCLAPI + "maven/",
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
    /// <param name="uuid">资源UUID</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadAssets(string uuid, SourceLocal? source)
    {
        string? url = source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}assets/{uuid[..2]}/{uuid}",
            _ => $"{MinecraftResources}{uuid[..2]}/{uuid}"
        };

        return url;
    }

    /// <summary>
    /// Forge地址
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadForgeJar(string mc, string version, SourceLocal? source)
    {
        string? url = source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}maven/net/minecraftforge/forge/{mc}-{version}/",
            _ => $"{Forge}net/minecraftforge/forge/{mc}-{version}/"
        };

        return url;
    }

    /// <summary>
    /// NeoForge地址
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">neoforge版本</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadNeoForgeJar(string mc, string version, SourceLocal? source)
    {
        var v2222 = CheckHelpers.IsGameVersion1202(mc);
        var baseurl = v2222 ? $"neoforge/{version}/" : $"forge/{mc}-{version}/";
        string? url = source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}maven/net/neoforged/{baseurl}",
            _ => $"{NeoForge}net/neoforged/{baseurl}"
        };

        return url;
    }

    /// <summary>
    /// Forge运行库地址
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadForgeLib(string url, SourceLocal? source)
    {
        string? to = source switch
        {
            SourceLocal.BMCLAPI => BMCLAPI + "maven/",
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
    /// <param name="url">下载地址</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadNeoForgeLib(string url, SourceLocal? source)
    {
        string? to = source switch
        {
            SourceLocal.BMCLAPI => BMCLAPI + "maven/",
            _ => null
        };
        if (to == null)
        {
            return url;
        }

        url = url.Replace(NeoForge, to);

        return url;
    }

    /// <summary>
    /// Fabric地址
    /// </summary>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string GetFabricMeta(SourceLocal? source)
    {
        string? url = source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}fabric-meta/v2/versions",
            _ => $"{FabricMeta}v2/versions"
        };

        return url;
    }

    /// <summary>
    /// Quilt地址
    /// </summary>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string GetQuiltMeta(SourceLocal? source)
    {
        string? url = source switch
        {
            //SourceLocal.BMCLAPI => $"{BMCLAPI}quilt-meta/v3/versions",
            _ => $"{QuiltMeta}v3/versions"
        };

        return url;
    }

    /// <summary>
    /// Fabric地址
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadFabric(string url, SourceLocal? source)
    {
        string? replace = source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}maven/",
            _ => null
        };

        if (replace == null)
            return url;

        return url.Replace(Fabric, replace);
    }

    /// <summary>
    /// Quilt地址
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadQuilt(string url, SourceLocal? source)
    {
        string? replace = source switch
        {
            //SourceLocal.BMCLAPI => BMCLAPI + "maven/",
            _ => null
        };

        if (replace == null)
            return url;

        return url.Replace($"{Quilt}repository/release/", replace);
    }

    /// <summary>
    /// 外置登录信息地址
    /// </summary>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string AuthlibInjectorMeta(SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}mirrors/authlib-injector/artifacts.json",
            _ => $"{Authlib}artifacts.json"
        };
    }

    /// <summary>
    /// 外置登录地址
    /// </summary>
    /// <param name="obj">外置登录项目</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string AuthlibInjector(AuthlibInjectorMetaObj.Artifacts obj, SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}mirrors/authlib-injector/artifact/{obj.build_number}.json",
            _ => $"{Authlib}artifact/{obj.build_number}.json"
        };
    }

    /// <summary>
    /// 外置登录下载地址
    /// </summary>
    /// <param name="obj">外置登录项目</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string DownloadAuthlibInjector(AuthlibInjectorObj obj, SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}mirrors/authlib-injector/artifact/{obj.build_number}/authlib-injector-{obj.version}.jar",
            _ => $"{Authlib}artifact/{obj.build_number}/authlib-injector-{obj.version}.jar"
        };
    }

    /// <summary>
    /// Forge版本地址
    /// </summary>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string ForgeVersion(SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft",
            _ => $"{Forge}net/minecraftforge/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// Forge版本地址
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string ForgeVersions(string version, SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}forge/minecraft/{version}",
            _ => $"{Forge}net/minecraftforge/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// NeoForge版本地址
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string NeoForgeVersion(string version, SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}neoforge/list/{version}",
            _ => $"{NeoForge}net/neoforged/forge/maven-metadata.xml"
        };
    }

    /// <summary>
    /// NeoForge版本地址
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="v222">是否为新版本</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string NeoForgeVersions(string mc, bool v222, SourceLocal? source)
    {
        if (v222)
        {
            return source switch
            {
                SourceLocal.BMCLAPI => $"{BMCLAPI}neoforge/list/{mc}",
                _ => $"{NeoForge}net/neoforged/neoforge/maven-metadata.xml"
            };
        }
        else
        {
            return source switch
            {
                SourceLocal.BMCLAPI => $"{BMCLAPI}neoforge/list/{mc}",
                _ => $"{NeoForge}net/neoforged/forge/maven-metadata.xml"
            };
        }
    }

    /// <summary>
    /// 下载地址变换
    /// </summary>
    /// <param name="old">旧的地址</param>
    /// <returns>下载地址</returns>
    public static string? UrlChange(string old)
    {
        if (CoreHttpClient.Source == SourceLocal.Offical)
        {
            if (old.StartsWith(Forge))
            {
                return old.Replace(Forge, $"{BMCLAPI}maven");
            }
            else if (old.StartsWith(MinecraftLib))
            {
                return old.Replace(MinecraftLib, $"{BMCLAPI}maven");
            }
            else if (old.StartsWith(Fabric))
            {
                return old.Replace(Fabric, $"{BMCLAPI}maven");
            }
        }

        return null;
    }

    /// <summary>
    /// Optifine信息
    /// </summary>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string GetOptifine(SourceLocal? source)
    {
        return source switch
        {
            SourceLocal.BMCLAPI => $"{BMCLAPI}optifine/versionList",
            _ => $"{OptiFine}downloads"
        };
    }

    /// <summary>
    /// Optifine下载
    /// </summary>
    /// <param name="obj">Optifine项目</param>
    /// <param name="source">下载源</param>
    /// <returns>下载地址</returns>
    public static string OptifineDownload(OptifineListObj obj, SourceLocal? source)
    {
        return source switch
        {
            _ => $"{BMCLAPI}optifine/{obj.mcversion}/{obj.type}/{obj.patch}"
        };
    }

    /// <summary>
    /// 创建下载地址
    /// </summary>
    /// <param name="type">资源类型</param>
    /// <param name="pid">项目ID</param>
    /// <param name="fid">文件ID</param>
    /// <param name="file">文件名</param>
    /// <returns>下载地址</returns>
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
    /// <param name="item">服务器项目</param>
    /// <param name="type">文件类型</param>
    /// <param name="url">原始地址</param>
    /// <returns>下载地址</returns>
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

        else
        {
            return MakeDownloadUrl(DownloadItemHelper.TestSourceType(item.Projcet, item.FileId), item.Projcet,
                item.FileId, item.File);
        }
    }
}
