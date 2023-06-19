using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 整合包
/// </summary>
public static class ModPackHelper
{
    /// <summary>
    /// 升级游戏整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="zip">压缩包路径</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateCurseForgeModPack(GameSettingObj obj, string zip)
    {
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
            return false;
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
            return false;
        }
        if (info == null)
            return false;

        //获取版本数据
        foreach (var item in info.minecraft.modLoaders)
        {
            if (item.id.StartsWith("forge"))
            {
                obj.Loader = Loaders.Forge;
                obj.LoaderVersion = item.id.Replace("forge-", "");
            }
            else if (item.id.StartsWith("fabric"))
            {
                obj.Loader = Loaders.Fabric;
                obj.LoaderVersion = item.id.Replace("fabric-", "");
            }
        }

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith(info.overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(obj.GetGamePath() +
                    e.Name[info.overrides.Length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        File.WriteAllBytes(obj.GetModJsonFile(), array1);

        var obj1 = obj.CopyObj();
        obj1.Mods.Clear();

        //获取Mod信息
        var list = await GetCurseForgeModInfo(obj1, info, true);
        if (!list.Item1)
        {
            return false;
        }

        var addlist = new List<ModInfoObj>();
        var removelist = new List<ModInfoObj>();

        var temp1 = obj.Mods.Values.ToArray();
        var temp2 = obj1.Mods.Values.ToArray();

        foreach (var item in temp1)
        {
            for (int a = 0; a < temp2.Length; a++)
            {
                var item1 = temp2[a];
                if (item1 == null)
                    continue;
                if (item.ModId == item1.ModId)
                {
                    temp2[a] = null;
                    if (item.FileId != item1.FileId
                        || item.SHA1 != item1.SHA1)
                    {
                        addlist.Add(item1);
                        removelist.Add(item);
                        break;
                    }
                }
            }
        }

        foreach (var item in temp2)
        {
            if (item != null)
            {
                addlist.Add(item);
            }
        }

        var list1 = new List<DownloadItemObj>();

        string path = obj.GetGamePath();

        foreach (var item in removelist)
        {
            string local = Path.GetFullPath(path + "/" + item.Path + "/" + item.File);
            if (File.Exists(local))
            {
                File.Delete(local);
            }
            obj.Mods.Remove(item.ModId);
        }

        foreach (var item in addlist)
        {
            list1.Add(list.Item2.First(a => a.SHA1 == item.SHA1));
            obj.Mods.Add(item.ModId, item);
        }

        var res2 = await DownloadManager.Start(list1);
        if (!res2)
        {
            return false;
        }

        return true;
    }

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
            LoaderVersion = loaderversion
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
        var list = await GetCurseForgeModInfo(game, info, true);
        if (!list.Item1)
        {
            return (false, game);
        }

        game.SaveModInfo();

        ColorMCCore.PackState?.Invoke(CoreRunState.Download);

        var res2 = await DownloadManager.Start(list.Item2.ToList());
        if (!res2)
        {
            return (false, game);
        }

        return (true, game);
    }

    /// <summary>
    /// 获取CurseForge整合包Mod信息
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="info">整合包信息</param>
    /// <param name="notify">是否通知</param>
    /// <returns>结果</returns>
    private static async Task<(bool, ConcurrentBag<DownloadItemObj>)> GetCurseForgeModInfo(GameSettingObj game, CurseForgePackObj info, bool notify)
    {
        var size = info.files.Count;
        var now = 0;
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

                now++;
                if (notify)
                {
                    ColorMCCore.PackUpdate?.Invoke(size, now);
                }
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
                    Local = game.GetGamePath() + "/mods/" + res.data.fileName,
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

                now++;
                if (notify)
                {
                    ColorMCCore.PackUpdate?.Invoke(size, now);
                }
            });
            if (!done)
            {
                return (false, list);
            }
        }

        return (true, list);
    }

    /// <summary>
    /// 升级Modrinth整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="zip">整合包路径</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateModrinthModPack(GameSettingObj obj, string zip)
    {
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
            return false;
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
            return false;
        }
        if (info == null)
        {
            return false;
        }

        //获取版本数据
        if (!string.IsNullOrWhiteSpace(info.dependencies.forge))
        {
            obj.Loader = Loaders.Forge;
            obj.LoaderVersion = info.dependencies.forge;
        }
        else if (!string.IsNullOrWhiteSpace(info.dependencies.fabric_loader))
        {
            obj.Loader = Loaders.Fabric;
            obj.LoaderVersion = info.dependencies.fabric_loader;
        }
        else if (!string.IsNullOrWhiteSpace(info.dependencies.quilt_loader))
        {
            obj.Loader = Loaders.Quilt;
            obj.LoaderVersion = info.dependencies.quilt_loader;
        }

        int length = "overrides".Length;

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith("overrides/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(obj.GetGamePath() +
                    e.Name[length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        File.WriteAllBytes(obj.GetModJsonFile(), array1);

        var obj1 = obj.CopyObj();
        obj1.Mods.Clear();

        //获取Mod信息
        var list = GetModrinthModInfo(obj1, info, true);

        var addlist = new List<ModInfoObj>();
        var removelist = new List<ModInfoObj>();

        var temp1 = obj.Mods.Values.ToArray();
        var temp2 = obj1.Mods.Values.ToArray();

        foreach (var item in temp1)
        {
            for (int a = 0; a < temp2.Length; a++)
            {
                var item1 = temp2[a];
                if (item1 == null)
                    continue;
                if (item.ModId == item1.ModId)
                {
                    temp2[a] = null;
                    if (item.FileId != item1.FileId
                        || item.SHA1 != item1.SHA1)
                    {
                        addlist.Add(item1);
                        removelist.Add(item);
                        break;
                    }
                }
            }
        }

        foreach (var item in temp2)
        {
            if (item != null)
            {
                addlist.Add(item);
            }
        }

        var list1 = new List<DownloadItemObj>();

        string path = obj.GetGamePath();

        foreach (var item in removelist)
        {
            string local = Path.GetFullPath(path + "/" + item.File);
            if (File.Exists(local))
            {
                File.Delete(local);
            }
            obj.Mods.Remove(item.ModId);
        }

        foreach (var item in addlist)
        {
            list1.Add(list.First(a => a.SHA1 == item.SHA1));
            obj.Mods.Add(item.ModId, item);
        }

        var res2 = await DownloadManager.Start(list1);
        if (!res2)
        {
            return false;
        }

        return true;
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
            LoaderVersion = loaderversion
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

        //获取Mod信息

        var list = GetModrinthModInfo(game, info, true);

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
    /// 获取Modrinth整合包Mod信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="info">信息</param>
    /// <param name="notify">通知</param>
    /// <returns>信息</returns>
    private static List<DownloadItemObj> GetModrinthModInfo(GameSettingObj obj, ModrinthPackObj info, bool notify)
    {
        var list = new List<DownloadItemObj>();

        var size = info.files.Count;
        var now = 0;
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
                Local = obj.GetGamePath() + "/" + item.path,
                SHA1 = item.hashes.sha1
            };

            list.Add(item11);

            obj.Mods.Add(modid, new()
            {
                Path = item.path[..item.path.IndexOf('/')],
                Name = item.path,
                File = item.path,
                SHA1 = item11.SHA1!,
                ModId = modid,
                FileId = fileid,
                Url = url
            });

            now++;

            if (notify)
            {
                ColorMCCore.PackUpdate?.Invoke(size, now);
            }
        }

        return list;
    }
}
