using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.FTB;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;

namespace ColorMC.Core.Net.Download;

public static class PackDownload
{
    public static int Size { get; private set; }
    public static int Now { get; private set; }
    /// <summary>
    /// 安装CurseForge整合包
    /// </summary>
    /// <param name="zip">压缩包路径</param>
    public static async Task<(bool State, GameSettingObj? Game)> DownloadCurseForgeModPack(string zip, string? name, string? group)
    {
        ColorMCCore.PackState?.Invoke(CoreRunState.Read);
        using ZipFile zFile = new(zip);
        using MemoryStream stream1 = new();
        bool find = false;
        //获取主信息
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
            return (false, null);
        }

        ColorMCCore.PackState?.Invoke(CoreRunState.Init);
        CurseForgePackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<CurseForgePackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Pack.Error1"), e);
            return (false, null);
        }
        if (info == null)
            return (false, null);

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        foreach (var item in info.minecraft.modLoaders)
        {
            if (item.id.StartsWith("forge"))
            {
                loaders = Loaders.Forge;
                loaderversion = item.id.Replace("forge-", "");
            }
            else if (item.id.StartsWith("fabric"))
            {
                loaders = Loaders.Fabric;
                loaderversion = item.id.Replace("fabric-", "");
            }
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{info.name}-{info.version}";
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGame(new()
        {
            GroupName = group,
            Name = name,
            Version = info.minecraft.version,
            ModPack = true,
            Loader = loaders,
            ModPackType = SourceType.CurseForge,
            LoaderVersion = loaderversion,
            Mods = new()
        });

        if (game == null)
        {
            return (false, game);
        }

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith(info.overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(game.GetGamePath() +
                    e.Name[info.overrides.Length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        File.WriteAllBytes(game.GetModJsonFile(), array1);

        ColorMCCore.PackState?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息
        Size = info.files.Count;
        Now = 0;
        var list = new ConcurrentBag<DownloadItemObj>();
        var res = await CurseForgeAPI.GetMods(info.files);
        if (res != null)
        {
            var res1 = res.Distinct(CurseDataComparer.Instance);
            foreach (var item in res1)
            {
                item.downloadUrl ??= $"https://edge.forgecdn.net/files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";

                var item11 = new DownloadItemObj()
                {
                    Url = item.downloadUrl,
                    Name = item.fileName,
                    Local = game.GetGamePath() + "/mods/" + item.fileName,
                    SHA1 = item.hashes.Where(a => a.algo == 1)
                            .Select(a => a.value).FirstOrDefault()
                };

                list.Add(item11);

                game.Mods.Add(item.modId.ToString(), new()
                {
                    Path = "mods",
                    Name = item.displayName,
                    File = item.fileName,
                    SHA1 = item11.SHA1!,
                    ModId = item.modId.ToString(),
                    FileId = item.id.ToString(),
                    Url = item.downloadUrl
                });

                Now++;

                ColorMCCore.PackUpdate?.Invoke(Size, Now);
            }
        }
        else
        {
            bool done = true;
            await Parallel.ForEachAsync(info.files, async (item, token) =>
            {
                var res = await CurseForgeAPI.GetMod(item);
                if (res == null)
                {
                    done = false;
                    return;
                }

                res.data.downloadUrl ??= $"https://edge.forgecdn.net/files/{res.data.id / 1000}/{res.data.id % 1000}/{res.data.fileName}";

                var item11 = new DownloadItemObj()
                {
                    Url = res.data.downloadUrl,
                    Name = res.data.displayName,
                    Local = InstancesPath.GetGamePath(game) + "/mods/" + res.data.fileName,
                    SHA1 = res.data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault()
                };

                list.Add(item11);

                game.Mods.Add(res.data.modId.ToString(), new()
                {
                    Path = "mods",
                    Name = res.data.displayName,
                    File = res.data.fileName,
                    SHA1 = item11.SHA1!,
                    ModId = res.data.modId.ToString(),
                    FileId = res.data.id.ToString(),
                    Url = res.data.downloadUrl
                });

                Now++;

                ColorMCCore.PackUpdate?.Invoke(Size, Now);
            });
            if (!done)
            {
                return (false, game);
            }
        }

        game.SaveModInfo();

        ColorMCCore.PackState?.Invoke(CoreRunState.Download);

        var res2 = await DownloadManager.Start(list.ToList());
        if (!res2)
        {
            return (false, game);
        }

        return (true, game);
    }

    /// <summary>
    /// 安装Modrinth整合包
    /// </summary>
    /// <param name="zip">文件路径</param>
    /// <param name="name">名字</param>
    /// <param name="group">群组</param>
    /// <returns></returns>
    public static async Task<(bool State, GameSettingObj? Game)> DownloadModrinthModPack(string zip, string? name, string? group)
    {
        ColorMCCore.PackState?.Invoke(CoreRunState.Read);
        using ZipFile zFile = new(zip);
        using MemoryStream stream1 = new();
        bool find = false;
        //获取主信息
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "modrinth.index.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find = true;
                break;
            }
        }

        if (!find)
        {
            return (false, null);
        }

        ColorMCCore.PackState?.Invoke(CoreRunState.Init);
        ModrinthPackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<ModrinthPackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Pack.Error1"), e);
            return (false, null);
        }
        if (info == null)
        {
            return (false, null);
        }

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        if (!string.IsNullOrWhiteSpace(info.dependencies.forge))
        {
            loaders = Loaders.Forge;
            loaderversion = info.dependencies.forge;
        }
        else if (!string.IsNullOrWhiteSpace(info.dependencies.fabric_loader))
        {
            loaders = Loaders.Fabric;
            loaderversion = info.dependencies.fabric_loader;
        }
        else if (!string.IsNullOrWhiteSpace(info.dependencies.quilt_loader))
        {
            loaders = Loaders.Quilt;
            loaderversion = info.dependencies.quilt_loader;
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{info.name}-{info.versionId}";
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGame(new()
        {
            GroupName = group,
            Name = name,
            Version = info.dependencies.minecraft,
            ModPack = true,
            ModPackType = SourceType.Modrinth,
            Loader = loaders,
            LoaderVersion = loaderversion,
            Mods = new()
        });

        if (game == null)
        {
            return (false, game);
        }

        int length = "overrides".Length;

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith("overrides/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(game.GetGamePath() +
                    e.Name[length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        File.WriteAllBytes(game.GetModJsonFile(), array1);

        ColorMCCore.PackState?.Invoke(CoreRunState.GetInfo);

        var list = new List<DownloadItemObj>();

        //获取Mod信息
        Size = info.files.Count;
        Now = 0;
        foreach (var item in info.files)
        {
            string? modid, fileid;
            var url = item.downloads.FirstOrDefault(a => a.StartsWith("https://cdn.modrinth.com/data/"));
            if (url == null)
            {
                url = item.downloads[0];
                modid = "";
                fileid = "";
            }
            else
            {
                modid = Funtcions.GetString(url, "data/", "/ver");
                fileid = Funtcions.GetString(url, "versions/", "/");
            }

            var item11 = new DownloadItemObj()
            {
                Url = item.downloads[0],
                Name = item.path,
                Local = game.GetGamePath() + "/" + item.path,
                SHA1 = item.hashes.sha1
            };

            list.Add(item11);

            game.Mods.Add(modid, new()
            {
                Path = item.path[..item.path.IndexOf('/')],
                Name = item.path,
                File = item.path,
                SHA1 = item11.SHA1!,
                ModId = modid,
                FileId = fileid,
                Url = url
            });

            Now++;

            ColorMCCore.PackUpdate?.Invoke(Size, Now);
        }

        game.SaveModInfo();

        ColorMCCore.PackState?.Invoke(CoreRunState.Download);

        var res = await DownloadManager.Start(list.ToList());
        if (!res)
        {
            return (false, game);
        }

        return (true, game);
    }

    /// <summary>
    /// 安装FTB整合包
    /// </summary>
    /// <param name="obj">FTB整合包数据</param>
    /// <param name="file">数据</param>
    /// <param name="name">名字</param>
    /// <param name="group">群组</param>
    /// <returns></returns>
    public static async Task<(bool, GameSettingObj?)> InstallFTB(FTBModpackObj obj, FTBModpackObj.Versions file, string? name, string? group)
    {
        ColorMCCore.PackState?.Invoke(CoreRunState.Read);

        var data = await FTBAPI.GetFiles(obj.id, file.id);
        if (data == null)
            return (false, null);

        ColorMCCore.PackState?.Invoke(CoreRunState.Init);

        //获取版本数据
        string mc = "";
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        foreach (var item in file.targets)
        {
            if (item.type == "modloader")
            {
                if (item.name == "forge")
                {
                    loaders = Loaders.Forge;
                    loaderversion = item.version;
                }
                else if (item.name == "fabric")
                {
                    loaders = Loaders.Fabric;
                    loaderversion = item.version;
                }
            }
            else if (item.type == "game")
            {
                mc = item.version;
            }
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{obj.name}-{file.name}-{file.type}";
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGame(new()
        {
            GroupName = group,
            Name = name,
            Version = mc,
            ModPack = true,
            Loader = loaders,
            LoaderVersion = loaderversion,
            ModPackType = SourceType.FTB,
            Mods = new()
        });

        if (game == null)
        {
            return (false, game);
        }

        var list = new ConcurrentBag<DownloadItemObj>();
        bool error = false;
        await Parallel.ForEachAsync(data.files, async (item, cancel) =>
        {
            if (error)
                return;
            if (item.type == "mod")
            {
                var res1 = await CurseForgeAPI.GetMod(new()
                {
                    projectID = item.curseforge.project,
                    fileID = item.curseforge.file
                });
                if (res1 == null)
                {
                    error = true;
                    return;
                }

                res1.data.downloadUrl ??= $"https://edge.forgecdn.net/files/{res1.data.id / 1000}/{res1.data.id % 1000}/{res1.data.fileName}";

                var item11 = new DownloadItemObj()
                {
                    Url = res1.data.downloadUrl,
                    Name = item.path[1..] + item.name,
                    Local = game.GetGamePath() + item.path[1..] + item.name,
                    SHA1 = item.sha1
                };

                game.Mods.Add(item.curseforge.project.ToString(), new()
                {
                    Path = item.path[2..^1],
                    Name = res1.data.displayName,
                    File = item.name,
                    SHA1 = item.sha1!,
                    ModId = item.curseforge.project.ToString(),
                    FileId = item.curseforge.file.ToString(),
                    Url = res1.data.downloadUrl
                });

                list.Add(item11);
            }
            else
            {
                var item11 = new DownloadItemObj()
                {
                    Url = item.url,
                    Name = item.path[1..] + item.name,
                    Local = game.GetGamePath() + item.path[1..] + item.name,
                    SHA1 = item.sha1
                };

                list.Add(item11);
            }
        });
        if (error)
        {
            return (false, game);
        }

        var file1 = game.GetModPackJsonFile();
        File.WriteAllText(file1, JsonConvert.SerializeObject(data));

        game.SaveModInfo();

        ColorMCCore.PackState?.Invoke(CoreRunState.Download);

        var res = await DownloadManager.Start(list.ToList());
        if (!res)
        {
            return (false, game);
        }

        ColorMCCore.PackState?.Invoke(CoreRunState.End);

        return (true, game);
    }
}
