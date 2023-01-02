using ColorMC.Core.Http.Apis;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Text;

namespace ColorMC.Core.Http.Download;

public enum DownloadState
{
    Init, GetInfo, End
}

public static class GameDownload
{
    public static async Task<(DownloadState State, List<DownloadItem>? List)> Download(VersionObj.Versions obj)
    {
        var list = new List<DownloadItem>();

        var obj1 = await Get.GetGame(obj.url);
        if (obj1 == null)
            return (DownloadState.Init, null);

        VersionPath.AddGame(obj1);
        var obj2 = await Get.GetAssets(obj1.assetIndex.url);
        if (obj2 == null)
            return (DownloadState.GetInfo, null);

        AssetsPath.AddIndex(obj2, obj1);
        list.Add(new()
        {
            Name = $"{obj.id}.jar",
            Url = UrlHelp.Download(obj1.downloads.client.url, BaseClient.Source),
            Local = LibrariesPath.MakeGameDir(obj.id),
            SHA1 = obj1.downloads.client.sha1
        });

        list.AddRange(GameHelp.MakeGameLibs(obj1));

        foreach (var item1 in obj2.objects)
        {
            list.Add(new()
            {
                Name = item1.Key,
                Url = UrlHelp.DownloadAssets(item1.Value.hash, BaseClient.Source),
                Local = $"{AssetsPath.ObjectsDir}/{item1.Value.hash[..2]}/{item1.Value.hash}",
                SHA1 = item1.Value.hash
            });
        }

        return (DownloadState.End, list);
    }

    public static Task<(DownloadState State, List<DownloadItem>? List)> DownloadForge(GameSettingObj obj)
    {
        return DownloadForge(obj.Version, obj.LoaderVersion);
    }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadForge(string mc, string version)
    {
        bool v2 = CheckRule.GameLaunchVersion(mc);

        var down = ForgeHelper.BuildForgeInster(mc, version);
        try
        {
            await DownloadManager.Download(down);
        }
        catch
        {
            return (DownloadState.Init, null);
        }

        string name = $"forge-{mc}-{version}";
        using ZipFile zFile = new(down.Local);
        using MemoryStream stream1 = new();
        using MemoryStream stream2 = new();
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "version.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
            }
            else if (e.IsFile && e.Name == "install_profile.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream2);
            }
        }

        var list = new List<DownloadItem>();
        if (v2 || mc == "1.12.2")
        {
            byte[] array1 = stream1.ToArray();
            ForgeLaunchObj info;
            try
            {
                var data = Encoding.UTF8.GetString(array1);
                info = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;
                File.WriteAllBytes($"{VersionPath.ForgeDir}/{name}.json", array1);
            }
            catch (Exception e)
            {
                Logs.Error("读取forge信息错误", e);
                return (DownloadState.GetInfo, null);
            }

            list.AddRange(ForgeHelper.MakeForgeLibs(info, mc, version));

            byte[] array2 = stream2.ToArray();
            ForgeInstallObj info1;
            var data1 = Encoding.UTF8.GetString(array2);
            try
            {
                info1 = JsonConvert.DeserializeObject<ForgeInstallObj>(data1)!;
                File.WriteAllBytes($"{VersionPath.ForgeDir}/{name}-install.json", stream2.ToArray());
            }
            catch (Exception e)
            {
                Logs.Error("读取forge安装信息错误", e);
                return (DownloadState.GetInfo, null);
            }

            foreach (var item1 in info1.libraries)
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
                    var item1 = ForgeHelper.MakeLibObj(item);
                    if (item1 != null)
                    {
                        info.libraries.Add(item1);
                    }
                    else if (!string.IsNullOrWhiteSpace(item.url))
                    {
                        var path = PathC.ToName(item.name);
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

                File.WriteAllText($"{VersionPath.ForgeDir}/{name}.json", JsonConvert.SerializeObject(info));

                list.AddRange(ForgeHelper.MakeForgeLibs(info, mc, version));
            }
            catch (Exception e)
            {
                Logs.Error("读取forge信息错误", e);
                return (DownloadState.GetInfo, null);
            }
        }

        return (DownloadState.End, list);
    }

    public static Task<(DownloadState State, List<DownloadItem>? List)> DownloadFabric(GameSettingObj obj)
    {
        return DownloadFabric(obj.Version, obj.LoaderVersion);
    }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadFabric(string mc, string? version = null)
    {
        var list = new List<DownloadItem>();
        var meta = await FabricHelper.GetMeta(BaseClient.Source);
        if (meta == null)
        {
            return (DownloadState.Init, null);
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
            return (DownloadState.GetInfo, null);
        }

        version = fabric.version;

        FabricLoaderObj? meta1 = await FabricHelper.GetLoader(mc, version, BaseClient.Source);
        if (meta1 == null)
        {
            return (DownloadState.GetInfo, null);
        }

        File.WriteAllText($"{VersionPath.FabricDir}/{meta1.id}.json",
            JsonConvert.SerializeObject(meta1));

        foreach (var item in meta1.libraries)
        {
            var name = PathC.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelp.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                Name = name.Name,
                Local = $"{LibrariesPath.BaseDir}/{name.Path}"
            });

        }

        return (DownloadState.End, list);
    }

    public static Task<(DownloadState State, List<DownloadItem>? List)> DownloadQuilt(GameSettingObj obj)
    {
        return DownloadQuilt(obj.Version, obj.LoaderVersion);
    }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadQuilt(string mc, string? version = null)
    {
        var list = new List<DownloadItem>();
        var meta = await QuiltHelper.GetMeta(BaseClient.Source);
        if (meta == null)
        {
            return (DownloadState.Init, null);
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
            return (DownloadState.GetInfo, null);
        }

        version = quilt.version;

        QuiltLoaderObj? meta1 = await QuiltHelper.GetLoader(mc, version, BaseClient.Source);
        if (meta1 == null)
        {
            return (DownloadState.GetInfo, null);
        }

        File.WriteAllText($"{VersionPath.QuiltDir}/{meta1.id}.json",
            JsonConvert.SerializeObject(meta1));

        foreach (var item in meta1.libraries)
        {
            var name = PathC.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelp.DownloadQuilt(item.url + name.Path, BaseClient.Source),
                Name = name.Name,
                Local = $"{LibrariesPath.BaseDir}/{name.Path}"
            });

        }

        return (DownloadState.End, list);
    }
}
