using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 游戏下载
/// </summary>
public static class GameDownloadHelper
{
    /// <summary>
    /// 下载游戏
    /// </summary>
    /// <param name="obj">版本数据</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> Download(VersionObj.Versions obj)
    {
        var list = new List<DownloadItemObj>();

        var obj1 = await GameAPI.GetGame(obj.url);
        if (obj1 == null)
        {
            return (GetDownloadState.Init, null);
        }

        VersionPath.AddGame(obj1);
        var obj2 = await GameAPI.GetAssets(obj1.assetIndex.url);
        if (obj2 == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        obj1.AddIndex(obj2);
        list.Add(new()
        {
            Name = $"{obj.id}.jar",
            Url = UrlHelper.Download(obj1.downloads.client.url, BaseClient.Source),
            Local = LibrariesPath.GetGameFile(obj.id),
            SHA1 = obj1.downloads.client.sha1
        });

        list.AddRange(await GameHelper.MakeGameLibs(obj1));

        foreach (var item1 in obj2.objects)
        {
            list.Add(new()
            {
                Name = item1.Key,
                Url = UrlHelper.DownloadAssets(item1.Value.hash, BaseClient.Source),
                Local = $"{AssetsPath.ObjectsDir}/{item1.Value.hash[..2]}/{item1.Value.hash}",
                SHA1 = item1.Value.hash
            });
        }

        return (GetDownloadState.End, list);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)> DownloadForge(GameSettingObj obj, bool neo)
    {
        return DownloadForge(obj.Version, obj.LoaderVersion!, neo);
    }

    /// <summary>
    /// 获取Forge下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> DownloadForge(string mc, string version, bool neo)
    {
        var version1 = VersionPath.GetGame(mc)!;
        bool v2 = CheckRuleUtils.GameLaunchVersion(version1);

        var down = neo ?
            GameHelper.BuildNeoForgeInster(mc, version) :
            GameHelper.BuildForgeInster(mc, version);
        try
        {
            var res = await DownloadManager.Start(new() { down });
            if (!res)
            {
                return (GetDownloadState.Init, null);
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Forge.Error4"), e, false);
            return (GetDownloadState.Init, null);
        }

        string name = $"forge-{mc}-{version}";
        using ZipFile zFile = new(down.Local);
        using MemoryStream stream1 = new();
        using MemoryStream stream2 = new();
        bool find1 = false;
        bool find2 = false;
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "version.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find1 = true;
            }
            else if (e.IsFile && e.Name == "install_profile.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream2);
                find2 = true;
            }
        }

        var list = new List<DownloadItemObj>();
        //1.12.2以上
        if (find1 && find2)
        {
            ForgeLaunchObj info;
            try
            {
                var array = stream1.ToArray();
                var data = Encoding.UTF8.GetString(stream1.ToArray());
                info = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;
                VersionPath.AddGame(info, array, mc, version, neo);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error1"), e);
                return (GetDownloadState.GetInfo, null);
            }

            list.AddRange(GameHelper.MakeForgeLibs(info, mc, version, neo));

            ForgeInstallObj info1;
            try
            {
                var array = stream2.ToArray();
                var data = Encoding.UTF8.GetString(array);
                info1 = JsonConvert.DeserializeObject<ForgeInstallObj>(data)!;
                VersionPath.AddGame(info1, array, mc, version, neo);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error2"), e);
                return (GetDownloadState.GetInfo, null);
            }

            list.AddRange(GameHelper.MakeForgeLibs(info1, mc, version, neo));
        }
        //旧forge
        else
        {
            ForgeInstallObj1 obj;
            byte[] array1 = stream2.ToArray();
            ForgeLaunchObj info;
            try
            {
                var data = Encoding.UTF8.GetString(array1);
                obj = JsonConvert.DeserializeObject<ForgeInstallObj1>(data)!;
                info = new()
                {
                    id = obj.versionInfo.id,
                    time = obj.versionInfo.time,
                    releaseTime = obj.versionInfo.releaseTime,
                    type = obj.versionInfo.type,
                    mainClass = obj.versionInfo.mainClass,
                    inheritsFrom = obj.versionInfo.inheritsFrom,
                    minecraftArguments = obj.versionInfo.minecraftArguments,
                    libraries = new()
                };
                foreach (var item in obj.versionInfo.libraries)
                {
                    var item1 = GameHelper.MakeLibObj(item);
                    if (item1 != null)
                    {
                        info.libraries.Add(item1);
                    }
                    else if (!string.IsNullOrWhiteSpace(item.url))
                    {
                        var path = PathCUtils.ToName(item.name);
                        info.libraries.Add(new()
                        {
                            name = item.name,
                            downloads = new()
                            {
                                artifact = new()
                                {
                                    url = item.url + path.Path,
                                    path = path.Path
                                }
                            }
                        });
                    }
                }

                VersionPath.AddGame(info, array1, mc, version, neo);

                list.AddRange(GameHelper.MakeForgeLibs(info, mc, version, neo));
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error3"), e);
                return (GetDownloadState.GetInfo, null);
            }
        }

        return (GetDownloadState.End, list);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)> DownloadFabric(GameSettingObj obj)
    {
        return DownloadFabric(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Fabric下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> DownloadFabric(string mc, string? version = null)
    {
        var list = new List<DownloadItemObj>();
        var meta = await FabricAPI.GetMeta(BaseClient.Source);
        if (meta == null)
        {
            return (GetDownloadState.Init, null);
        }

        FabricMetaObj.Loader? fabric;

        if (version != null)
        {
            fabric = meta.loader.Where(a => a.version == version).FirstOrDefault();
        }
        else
        {
            fabric = meta.loader.Where(a => a.stable == true).FirstOrDefault();
        }
        if (fabric == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        version = fabric.version;

        var data = await FabricAPI.GetLoader(mc, version, BaseClient.Source);
        if (data == null)
        {
            return (GetDownloadState.GetInfo, null);
        }
        var meta1 = JsonConvert.DeserializeObject<FabricLoaderObj>(data);
        if (meta1 == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        VersionPath.AddGame(meta1, data, mc, version);

        foreach (var item in meta1.libraries)
        {
            var name = PathCUtils.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                Name = name.Name,
                Local = $"{LibrariesPath.BaseDir}/{name.Path}"
            });

        }

        return (GetDownloadState.End, list);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static Task<(GetDownloadState State, List<DownloadItemObj>? List)> DownloadQuilt(GameSettingObj obj)
    {
        return DownloadQuilt(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Quilt下载项目
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>State下载状态
    /// List下载项目列表</returns>
    public static async Task<(GetDownloadState State, List<DownloadItemObj>? List)> DownloadQuilt(string mc, string? version = null)
    {
        var list = new List<DownloadItemObj>();
        var meta = await QuiltAPI.GetMeta(BaseClient.Source);
        if (meta == null)
        {
            return (GetDownloadState.Init, null);
        }

        QuiltMetaObj.Loader? quilt;

        if (version != null)
        {
            quilt = meta.loader.Where(a => a.version == version).FirstOrDefault();
        }
        else
        {
            quilt = meta.loader.FirstOrDefault();
        }
        if (quilt == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        version = quilt.version;

        var data = await QuiltAPI.GetLoader(mc, version, BaseClient.Source);
        if (data == null)
        {
            return (GetDownloadState.GetInfo, null);
        }
        var meta1 = JsonConvert.DeserializeObject<QuiltLoaderObj>(data);
        if (meta1 == null)
        {
            return (GetDownloadState.GetInfo, null);
        }

        VersionPath.AddGame(meta1, data, mc, version);

        foreach (var item in meta1.libraries)
        {
            var name = PathCUtils.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelper.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                Name = name.Name,
                Local = $"{LibrariesPath.BaseDir}/{name.Path}"
            });

        }

        return (GetDownloadState.End, list);
    }
}
