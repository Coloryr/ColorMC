using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Pack;
using ColorMC.Core.Path;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Text;

namespace ColorMC.Core.Http.Download;

public static class PackDownload
{
    public static string PackName { get; private set; }
    public static int Size { get; private set; }
    public static int Now { get; private set; }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadCurseForge(CurseForgeObj.Data obj)
    {
        var obj1 = obj.latestFiles.First();
        DownloadItem item = new()
        {
            Url = obj1.downloadUrl,
            Name = obj1.fileName,
            Local = InstancesPath.BaseDir + "/" + obj1.fileName,
        };

        await DownloadThread.Download(item);

        return await DownloadCurseForge(item.Local);
    }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadCurseForge(string zip)
    {
        var list = new List<DownloadItem>();

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
            return (DownloadState.Init, null);
        }
        CurseForgePackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<CurseForgePackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error("读取压缩包错误", e);
            return (DownloadState.GetInfo, null);
        }
        if (info == null)
            return (DownloadState.GetInfo, null);

        Loaders loaders = Loaders.Normal;
        LoaderInfoObj? info1 = null;
        foreach (var item in info.minecraft.modLoaders)
        {
            if (item.id.StartsWith("forge"))
            {
                loaders = Loaders.Forge;
                info1 = new LoaderInfoObj()
                {
                    Name = "forge",
                    Version = item.id.Replace("forge-", "")
                };
            }
            else if (item.id.StartsWith("fabric"))
            {
                loaders = Loaders.Fabric;
                info1 = new LoaderInfoObj()
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
                return (DownloadState.GetInfo, null);
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

        File.WriteAllBytes(game.Dir + "/manifest.json", array1);

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

                list.Add(new()
                {
                    Url = item.downloadUrl,
                    Name = item.fileName,
                    Local = InstancesPath.GetDir(game) + "/mods/" + item.fileName,
                    SHA1 = item.hashes.Where(a => a.algo == 1)
                            .Select(a => a.value).FirstOrDefault()
                });

                Now++;

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

                    list.Add(new()
                    {
                        Url = res.data.downloadUrl,
                        Name = res.data.displayName,
                        Local = InstancesPath.GetDir(game) + "/mods/" + res.data.fileName,
                        SHA1 = res.data.hashes.Where(a => a.algo == 1)
                            .Select(a => a.value).FirstOrDefault()
                    });

                    Now++;

                    CoreMain.PackUpdate?.Invoke(Size, Now);

                    break;
                } while (a > 0);
            });
        }
        var version = VersionPath.Versions?.versions
            .Where(a => a.id == info.minecraft.version).FirstOrDefault();

        (DownloadState State, List<DownloadItem>? List) list1;

        if (version != null)
        {
            list1 = await GameDownload.Download(version);
            if (list1.State != DownloadState.End)
            {
                return (DownloadState.GetInfo, null);
            }

            list.AddRange(list1.List!);
        }

        if (loaders == Loaders.Forge)
        {
            list1 = await GameDownload.DownloadForge(game.Version, game.LoaderInfo.Version);
            if (list1.State != DownloadState.End)
            {
                return (DownloadState.GetInfo, null);
            }

            list.AddRange(list1.List!);
        }
        else if (loaders == Loaders.Fabric)
        {
            list1 = await GameDownload.DownloadFabric(game.Version, game.LoaderInfo.Version);
            if (list1.State != DownloadState.End)
            {
                return (DownloadState.GetInfo, null);
            }

            list.AddRange(list1.List!);
        }

        return (DownloadState.End, list);
    }
}
