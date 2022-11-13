using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ColorMC.Core.Http.Download;

public static class GameDownload
{
    public static async Task Download(VersionObj.Versions obj)
    {
        CoreMain.DownloadState?.Invoke(CoreRunState.Init);
        CoreMain.DownloadState?.Invoke(CoreRunState.GetInfo);
        var obj1 = await Get.GetGame(obj.url);
        if (obj1 == null)
            return;
        VersionPath.AddGame(obj1);
        var obj2 = await Get.GetAssets(obj1.assetIndex.url);
        if (obj2 == null)
            return;
        AssetsPath.AddIndex(obj2, obj.id);
        DownloadItem item = new()
        {
            Name = $"{obj.id}.jar",
            Url = UrlHelp.Download(obj1.downloads.client.url, BaseClient.Source),
            Local = $"{VersionPath.BaseDir}/{obj.id}.jar",
            SHA1 = obj1.downloads.client.sha1
        };
        DownloadManager.AddItem(item);
        CoreMain.DownloadStateUpdate?.Invoke(item);
        foreach (var item1 in obj1.libraries)
        {
            bool download = CheckRule.CheckAllow(item1.rules);

            if (!download || item1.downloads.artifact == null)
                continue;

            item = new()
            {
                Name = item1.name,
                Url = UrlHelp.DownloadLibraries(item1.downloads.artifact.url, BaseClient.Source),
                Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                SHA1 = item1.downloads.artifact.sha1
            };
            DownloadManager.AddItem(item);
            CoreMain.DownloadStateUpdate?.Invoke(item);
        }

        foreach (var item1 in obj2.objects)
        {
            item = new()
            {
                Name = item1.Key,
                Url = UrlHelp.DownloadAssets(item1.Value.hash, BaseClient.Source),
                Local = $"{AssetsPath.ObjectsDir}/{item1.Value.hash[..2]}/{item1.Value.hash}",
                SHA1 = item1.Value.hash
            };
            DownloadManager.AddItem(item);
            CoreMain.DownloadStateUpdate?.Invoke(item);
        }
    }

    public static Task DownloadForge(GameSettingObj obj)
    {
        return DownloadForge(obj.Version, obj.LoaderInfo.Version);
    }

    public static async Task DownloadForge(string mc, string version)
    {
        CoreMain.DownloadState?.Invoke(CoreRunState.Init);
        await VersionPath.DownloadForgeInster(mc, version);

        CoreMain.DownloadState?.Invoke(CoreRunState.GetInfo);

        string name = $"forge-{mc}-{version}";
        using ZipFile zFile = new($"{VersionPath.ForgeDir}/{name}-installer.jar");
        using MemoryStream stream1 = new();
        bool find = false;
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "version.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find = true;
                break;
            }
        }

        if (!find)
        {
            CoreMain.DownloadState?.Invoke(CoreRunState.Error);
            return;
        }

        ForgeInstallObj? info;
        var data = Encoding.UTF8.GetString(stream1.ToArray());
        try
        {
            info = JsonConvert.DeserializeObject<ForgeInstallObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("读取forge信息错误", e);
            return;
        }
        if (info == null)
            return;

        using FileStream stream3 = new($"{VersionPath.ForgeDir}/{name}.json",
            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream1.Seek(0, SeekOrigin.Begin);
        await stream1.CopyToAsync(stream3);
        DownloadItem item;
        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith("net.minecraftforge:forge:"))
            {
                item = new()
                {
                    Url = UrlHelp.DownloadForgeJar(mc, version, BaseClient.Source),
                    Name = item1.name,
                    Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" + 
                            PathC.MakeForgeName(mc, version),
                    SHA1 = item1.downloads.artifact.sha1
                };
            }
            else
            {
                item = new()
                {
                    Url = UrlHelp.DownloadForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source),
                    Name = item1.name,
                    Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                    SHA1 = item1.downloads.artifact.sha1
                };
            }

            DownloadManager.AddItem(item);
        }

        CoreMain.DownloadState?.Invoke(CoreRunState.End);
    }

    public static Task DownloadFabric(GameSettingObj obj)
    {
        return DownloadFabric(obj.Version, obj.LoaderInfo.Version);
    }

    public static async Task DownloadFabric(string mc, string? version = null)
    {
        CoreMain.DownloadState?.Invoke(CoreRunState.Init);
        var meta = await Get.GetFabricMeta(BaseClient.Source);
        if (meta == null)
        {
            CoreMain.DownloadState?.Invoke(CoreRunState.Error);
            return;
        }

        FabircMetaObj.Loader? fabric;

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
            CoreMain.DownloadState?.Invoke(CoreRunState.Error);
            return;
        }

        version = fabric.version;

        CoreMain.DownloadState?.Invoke(CoreRunState.GetInfo);

        FabricLoaderObj? meta1 = await Get.GetFabricLoader(mc, version, BaseClient.Source);
        if (meta1 == null)
        {
            CoreMain.DownloadState?.Invoke(CoreRunState.Error);
            return;
        }

        File.WriteAllText($"{VersionPath.FabricDir}/{meta1.id}.json",
            JsonConvert.SerializeObject(meta1));

        foreach (var item in meta1.libraries)
        {
            var name = PathC.ToName(item.name);
            DownloadItem item1 = new()
            {
                Url = UrlHelp.DownloadFabric(BaseClient.Source) + name.Item1,
                Name = name.Item2,
                Local = $"{LibrariesPath.BaseDir}/{name.Item1}"
            };

            DownloadManager.AddItem(item1);
        }

        CoreMain.DownloadState?.Invoke(CoreRunState.End);
    }
}
