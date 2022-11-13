using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Pack;
using ColorMC.Core.Path;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ColorMC.Core.Http.Download;

public static class PackDownload
{
    public static string PackName { get; private set; }
    public static int Size { get; private set; }
    public static int Now { get; private set; }

    public static async Task DownloadCurseForge(CurseForgeObj.Data obj)
    {
        var obj1 = obj.latestFiles.First();
        DownloadItem item = new()
        {
            Url = obj1.downloadUrl,
            Name = obj1.fileName,
            Local = InstancesPath.BaseDir + "/" + obj1.fileName,
        };

        await DownloadThread.Download(item, CancellationToken.None);

        await DownloadCurseForge(item.Local);
    }

    public static async Task DownloadCurseForge(string zip)
    {
        CoreMain.PackState?.Invoke(CoreRunState.Init);
        using ZipFile zFile = new(zip);
        using MemoryStream stream1 = new();
        bool find = false;
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "manifest.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find = true;
                break;
            }
        }

        if (!find)
        {
            return;
        }
        CurseForgePackObj? info;
        var data = Encoding.UTF8.GetString(stream1.ToArray());
        try
        {
            info = JsonConvert.DeserializeObject<CurseForgePackObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("读取压缩包错误", e);
            return;
        }
        if (info == null)
            return;
        Loaders loaders = Loaders.Normal;
        LoaderInfo info1 = null;
        foreach (var item in info.minecraft.modLoaders)
        {
            if (item.id.StartsWith("forge"))
            {
                loaders = Loaders.Forge;
                info1 = new LoaderInfo()
                {
                    Name = "forge",
                    Version = item.id.Replace("forge-", "")
                };
            }
            else if (item.id.StartsWith("fabric"))
            {
                loaders = Loaders.Fabric;
                info1 = new LoaderInfo()
                {
                    Name = "fabric",
                    Version = item.id.Replace("fabric-", "")
                };
            }
        }
        string name = $"{info.name}-{info.version}";
        var game = InstancesPath.CreateVersion(name, info.minecraft.version, loaders, info1);
        if (game == null)
        {
            game = InstancesPath.GetGame(name);
            if (game == null || !CoreMain.GameOverwirte(game))
            {
                return;
            }
        }
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith(info.overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = InstancesPath.GetDir(game) +
                    e.Name.Replace(info.overrides, "");
                FileInfo info2 = new(file);
                info2.Directory.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        using FileStream stream3 = new(game.Dir + "/manifest.json", FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
        stream1.Seek(0, SeekOrigin.Begin);
        await stream1.CopyToAsync(stream3);

        PackName = name;

        CoreMain.PackState?.Invoke(CoreRunState.GetInfo);
        Size = info.files.Count;
        Now = 0;
        var res = await Get.GetCurseForgeMods(info.files);
        if (res != null)
        {
            foreach (var item in res)
            {
                item.downloadUrl ??= $"https://edge.forgecdn.net/files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";

                var info2 = new DownloadItem()
                {
                    Url = item.downloadUrl,
                    Name = item.fileName,
                    Local = InstancesPath.GetDir(game) + "/mods/" + item.fileName,
                    SHA1 = item.hashes.Where(a => a.algo == 1)
                            .Select(a => a.value).FirstOrDefault()
                };

                DownloadManager.AddItem(info2);
                Now++;

                CoreMain.DownloadStateUpdate?.Invoke(info2);
                CoreMain.PackUpdate?.Invoke(Size, Now);
            }
        }
        else
        {
            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = 5
            };
            Parallel.ForEach(info.files, options, item =>
            {
                int a = 5;
                do
                {
                    var res = Get.GetCurseForgeMod(item).Result;
                    if (res == null)
                    {
                        a--;
                        continue;
                    }

                    var info2 = new DownloadItem()
                    {
                        Url = res.data.downloadUrl,
                        Name = res.data.displayName,
                        Local = InstancesPath.GetDir(game) + "/mods/" + res.data.fileName,
                        SHA1 = res.data.hashes.Where(a => a.algo == 1)
                            .Select(a => a.value).FirstOrDefault()
                    };

                    DownloadManager.AddItem(info2);
                    Now++;

                    CoreMain.DownloadStateUpdate?.Invoke(info2);
                    CoreMain.PackUpdate?.Invoke(Size, Now);

                    break;
                } while (a > 0);
            });
        }
        var version = VersionPath.Versions.versions
            .Where(a => a.id == info.minecraft.version).FirstOrDefault();

        if (version != null)
            await GameDownload.Download(version);

        if (loaders != Loaders.Normal)
        {
            if (loaders == Loaders.Forge)
            {
                await GameDownload.DownloadForge(game.Version, game.LoaderInfo.Version);
            }
            else if (loaders == Loaders.Fabric)
            {
                await GameDownload.DownloadFabric(game.Version, game.LoaderInfo.Version);
            }
        }

        CoreMain.PackState?.Invoke(CoreRunState.End);
    }
}
