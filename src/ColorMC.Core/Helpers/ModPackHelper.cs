using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 整合包处理
/// </summary>
public static class ModPackHelper
{
    /// <summary>
    /// 升级CurseForge整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="zip">压缩包路径</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateCurseForgeModPackAsync(GameSettingObj obj, string zip,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
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

        update2(CoreRunState.Init);
        CurseForgePackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<CurseForgePackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Pack.Error1"), e);
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
            else if (item.id.StartsWith("neoforge"))
            {
                obj.Loader = Loaders.NeoForge;
                obj.LoaderVersion = item.id.Replace("neoforge-", "");
            }
            else if (item.id.StartsWith("fabric"))
            {
                obj.Loader = Loaders.Fabric;
                obj.LoaderVersion = item.id.Replace("fabric-", "");
            }
            else if (item.id.StartsWith("quilt"))
            {
                obj.Loader = Loaders.Quilt;
                obj.LoaderVersion = item.id.Replace("quilt-", "");
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

        CurseForgePackObj? info3 = null;
        var json = obj.GetModPackJsonFile();
        if (File.Exists(json))
        {
            try
            {
                info3 = JsonConvert.DeserializeObject<CurseForgePackObj>(PathHelper.ReadText(json)!);
            }
            catch
            {

            }
        }

        var path = obj.GetGamePath();

        var list1 = new List<DownloadItemObj>();

        if (info3 != null)
        {
            var addlist = new List<CurseForgePackObj.Files>();
            var removelist = new List<CurseForgePackObj.Files>();

            CurseForgePackObj.Files[] temp1 = [.. info.files];
            CurseForgePackObj.Files?[] temp2 = [.. info3.files];

            foreach (var item in temp1)
            {
                for (int a = 0; a < temp2.Length; a++)
                {
                    var item1 = temp2[a];
                    if (item1 == null)
                        continue;
                    if (item.projectID == item1.projectID)
                    {
                        temp2[a] = null;
                        if (item.fileID != item1.fileID)
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

            foreach (var item in removelist)
            {
                if (obj.Mods.Remove(item.projectID.ToString(), out var mod))
                {
                    PathHelper.Delete(Path.GetFullPath($"{path}/{mod.Path}/{mod.File}"));
                }
            }

            foreach (var item in addlist)
            {
                var res = await CurseForgeAPI.GetMod(item);
                if (res == null || res.data == null)
                {
                    return false;
                }

                var path1 = await GetCurseForgeItemPath(obj, res.data);
                var modid = res.data.modId.ToString();
                list1.Add(res.data.MakeModDownloadObj(path1.Item1));
                obj.Mods.Add(modid, res.data.MakeModInfo(path1.Item2));
            }
        }
        else
        {
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            var obj1 = obj.CopyObj();
            obj1.Mods.Clear();

            var list = await GetCurseForgeModInfo(obj1, info, true, update);
            if (!list.Res)
            {
                return false;
            }

            ModInfoObj[] temp1 = [.. obj.Mods.Values];
            ModInfoObj?[] temp2 = [.. obj1.Mods.Values];

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

            foreach (var item in removelist)
            {
                var local = Path.GetFullPath($"{path}/{item.Path}/{item.File}");
                if (File.Exists(local))
                {
                    PathHelper.Delete(local);
                }
                obj.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                list1.Add(list.List.First(a => a.SHA1 == item.SHA1));
                obj.Mods.Add(item.ModId, item);
            }
        }

        PathHelper.WriteBytes(json, array1);

        await DownloadManager.StartAsync(list1);

        return true;
    }

    /// <summary>
    /// 安装CurseForge整合包
    /// </summary>
    /// <param name="zip">压缩包文件</param>
    /// <param name="name">实例名字</param>
    /// <param name="group">实例组</param>
    /// <returns>Res安装结果
    /// Game游戏实例</returns>
    public static async Task<(bool Res, GameSettingObj? Game)>
        DownloadCurseForgeModPackAsync(string zip, string? name, string? group,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        update2(CoreRunState.Read);
        using ZipFile zFile = new(PathHelper.OpenRead(zip));
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

        update2(CoreRunState.Init);
        CurseForgePackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<CurseForgePackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Pack.Error1"), e);
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
            else if (item.id.StartsWith("neoforge"))
            {
                loaders = Loaders.NeoForge;
                loaderversion = item.id.Replace("neoforge-", "");
            }
            else if (item.id.StartsWith("fabric"))
            {
                loaders = Loaders.Fabric;
                loaderversion = item.id.Replace("fabric-", "");
            }
            else if (item.id.StartsWith("quilt"))
            {
                loaders = Loaders.Quilt;
                loaderversion = item.id.Replace("quilt-", "");
            }
        }

        if (loaderversion.StartsWith(info.minecraft.version + "-")
            && loaderversion.Length > info.minecraft.version.Length + 1)
        {
            loaderversion = loaderversion[(info.minecraft.version.Length + 1)..];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{info.name}-{info.version}";
        }


        //创建游戏实例
        var game = new GameSettingObj()
        {
            GroupName = group,
            Name = name,
            Version = info.minecraft.version,
            ModPack = true,
            Loader = loaders,
            ModPackType = SourceType.CurseForge,
            LoaderVersion = loaderversion
        };

        game = await InstancesPath.CreateGame(game, request, overwirte);

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

        PathHelper.WriteBytes(game.GetModPackJsonFile(), array1);

        update2(CoreRunState.GetInfo);

        //获取Mod信息
        var list = await GetCurseForgeModInfo(game, info, true, update);
        if (!list.Res)
        {
            return (false, game);
        }

        game.SaveModInfo();

        update2(CoreRunState.Download);

        await DownloadManager.StartAsync([.. list.List]);

        return (true, game);
    }

    /// <summary>
    /// 获取CurseForge整合包Mod信息
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="info">整合包信息</param>
    /// <param name="notify">是否通知</param>
    /// <returns>Res安装结果
    /// Game游戏实例</returns>
    private static async Task<(bool Res, ConcurrentBag<DownloadItemObj> List)>
        GetCurseForgeModInfo(GameSettingObj game, CurseForgePackObj info, bool notify,
        ColorMCCore.PackUpdate update)
    {
        var size = info.files.Count;
        var now = 0;
        var list = new ConcurrentBag<DownloadItemObj>();

        //获取Mod信息
        var res = await CurseForgeAPI.GetMods(info.files);
        if (res != null)
        {
            var res1 = res.Distinct(CurseDataComparer.Instance);

            foreach (var item in res1)
            {
                var path = await GetCurseForgeItemPath(game, item);
                list.Add(item.MakeModDownloadObj(game, path.Item1));
                var modid = item.modId.ToString();
                game.Mods.Remove(modid);
                game.Mods.Add(modid, item.MakeModInfo(path.Item2));

                now++;
                if (notify)
                {
                    update(size, now);
                }
            }
        }
        else
        {
            //一个个获取
            bool done = true;
            await Parallel.ForEachAsync(info.files, async (item, token) =>
            {
                var res = await CurseForgeAPI.GetMod(item);
                if (res == null || res.data == null)
                {
                    done = false;
                    return;
                }

                var path = await GetCurseForgeItemPath(game, res.data);

                list.Add(res.data.MakeModDownloadObj(game, path.Item1));
                var modid = res.data.modId.ToString();
                game.Mods.Remove(modid);
                game.Mods.Add(modid, res.data.MakeModInfo(path.Item2));

                now++;
                if (notify)
                {
                    update(size, now);
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
    /// 构建CurseForge资源所在的文件夹
    /// </summary>
    /// <param name="game"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private static async Task<(string, string)> GetCurseForgeItemPath(GameSettingObj game, CurseForgeModObj.Data item)
    {
        var path = game.GetModsPath();
        var path1 = InstancesPath.Name11;
        if (!item.fileName.EndsWith(".jar"))
        {
            var info1 = await CurseForgeAPI.GetModInfo(item.modId);
            if (info1 != null)
            {
                if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassResourcepack)
                    || info1.Data.classId == CurseForgeAPI.ClassResourcepack)
                {
                    path = game.GetResourcepacksPath();
                    path1 = InstancesPath.Name8;
                }
                else if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassShaderpack)
                    || info1.Data.classId == CurseForgeAPI.ClassShaderpack)
                {
                    path = game.GetShaderpacksPath();
                    path1 = InstancesPath.Name9;
                }
            }
        }

        return (path, path1);
    }

    /// <summary>
    /// 升级Modrinth整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="zip">整合包路径</param>
    /// <returns>升级结果</returns>
    public static async Task<bool> UpdateModrinthModPackAsync(GameSettingObj obj, string zip,
        ColorMCCore.PackUpdate state, ColorMCCore.PackState update2)
    {
        using var zFile = new ZipFile(PathHelper.OpenRead(zip));
        using var stream1 = new MemoryStream();
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

        update2(CoreRunState.Init);
        ModrinthPackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<ModrinthPackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Pack.Error1"), e);
            return false;
        }
        if (info == null)
        {
            return false;
        }

        //获取版本数据

        if (info.dependencies.TryGetValue("forge", out var version))
        {
            obj.Loader = Loaders.Forge;
            obj.LoaderVersion = version;
        }
        else if (info.dependencies.TryGetValue("neoforge", out version))
        {
            obj.Loader = Loaders.NeoForge;
            obj.LoaderVersion = version;
        }
        else if (info.dependencies.TryGetValue("fabric-loader", out version))
        {
            obj.Loader = Loaders.Fabric;
            obj.LoaderVersion = version;
        }
        else if (info.dependencies.TryGetValue("quilt-loader", out version))
        {
            obj.Loader = Loaders.Quilt;
            obj.LoaderVersion = version;
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

        //获取当前整合包数据
        ModrinthPackObj? info3 = null;
        var json = obj.GetModPackJsonFile();
        if (File.Exists(json))
        {
            try
            {
                info3 = JsonConvert.DeserializeObject<ModrinthPackObj>(PathHelper.ReadText(json)!);
            }
            catch
            {

            }
        }

        var obj1 = obj.CopyObj();
        obj1.Mods.Clear();

        //获取Mod信息
        var list = GetModrinthModInfo(obj1, info, true, state);
        var list1 = new List<DownloadItemObj>();

        string path = obj.GetGamePath();

        if (info3 != null)
        {
            //筛选新旧整合包文件差距
            var addlist = new List<ModrinthPackObj.File>();
            var removelist = new List<ModrinthPackObj.File>();

            ModrinthPackObj.File?[] temp1 = [.. info.files];
            ModrinthPackObj.File?[] temp2 = [.. info3.files];

            for (int b = 0; b < temp1.Length; b++)
            {
                var item = temp1[b];
                if (item == null)
                    continue;
                for (int a = 0; a < temp2.Length; a++)
                {
                    var item1 = temp2[a];
                    if (item1 == null)
                        continue;
                    if (item.hashes.sha1 == item1.hashes.sha1)
                    {
                        temp1[b] = null;
                        temp2[a] = null;
                    }
                }
            }

            foreach (var item in temp1)
            {
                if (item != null)
                {
                    removelist.Add(item);
                }
            }

            foreach (var item in temp2)
            {
                if (item != null)
                {
                    addlist.Add(item);
                }
            }

            foreach (var item in removelist)
            {
                PathHelper.Delete(Path.GetFullPath($"{path}/{item.path}"));

                var url = item.downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url is { })
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    obj.Mods.Remove(modid);
                }
            }

            foreach (var item in addlist)
            {
                var item11 = list.First(a => a.SHA1 == item.hashes.sha1);
                list1.Add(item11);
                var url = item.downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url != null)
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    var fileid = StringHelper.GetString(url, "versions/", "/");

                    obj.Mods.Remove(modid);
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
                }
            }
        }
        else
        {
            //没有整合包信息
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            ModInfoObj[] temp1 = [.. obj.Mods.Values];
            ModInfoObj?[] temp2 = [.. obj1.Mods.Values];

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

            foreach (var item in removelist)
            {
                PathHelper.Delete(Path.GetFullPath($"{path}/{item.File}"));
                obj.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                list1.Add(list.First(a => a.SHA1 == item.SHA1));
                obj.Mods.Add(item.ModId, item);
            }
        }

        PathHelper.WriteBytes(json, array1);

        await DownloadManager.StartAsync(list1);

        return true;
    }

    /// <summary>
    /// 安装Modrinth整合包
    /// </summary>
    /// <param name="zip">文件路径</param>
    /// <param name="name">实例名字</param>
    /// <param name="group">实例组</param>
    /// <returns>Res安装结果
    /// Game游戏实例</returns>
    public static async Task<(bool Res, GameSettingObj? Game)>
        DownloadModrinthModPackAsync(string zip, string? name, string? group,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate state,
        ColorMCCore.PackState update2)
    {
        update2(CoreRunState.Read);
        using ZipFile zFile = new(PathHelper.OpenRead(zip));
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

        update2(CoreRunState.Init);
        ModrinthPackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<ModrinthPackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Pack.Error1"), e);
            return (false, null);
        }
        if (info == null)
        {
            return (false, null);
        }

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        if (info.dependencies.TryGetValue("forge", out var version))
        {
            loaders = Loaders.Forge;
            loaderversion = version;
        }
        else if (info.dependencies.TryGetValue("neoforge", out version))
        {
            loaders = Loaders.NeoForge;
            loaderversion = version;
        }
        else if (info.dependencies.TryGetValue("fabric-loader", out version))
        {
            loaders = Loaders.Fabric;
            loaderversion = version;
        }
        else if (info.dependencies.TryGetValue("quilt-loader", out version))
        {
            loaders = Loaders.Quilt;
            loaderversion = version;
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{info.name}-{info.versionId}";
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGame(new GameSettingObj()
        {
            GroupName = group,
            Name = name,
            Version = info.dependencies["minecraft"],
            ModPack = true,
            ModPackType = SourceType.Modrinth,
            Loader = loaders,
            LoaderVersion = loaderversion
        }, request, overwirte);

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

        PathHelper.WriteBytes(game.GetModPackJsonFile(), array1);

        update2(CoreRunState.GetInfo);

        //获取Mod信息

        var list = GetModrinthModInfo(game, info, true, state);

        game.SaveModInfo();

        update2(CoreRunState.Download);

        await DownloadManager.StartAsync([.. list]);

        return (true, game);
    }

    /// <summary>
    /// 获取Modrinth整合包Mod信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="info">信息</param>
    /// <param name="notify">通知</param>
    /// <returns>信息</returns>
    private static List<DownloadItemObj> GetModrinthModInfo(GameSettingObj obj, ModrinthPackObj info,
        bool notify, ColorMCCore.PackUpdate update)
    {
        var list = new List<DownloadItemObj>();

        var size = info.files.Count;
        var now = 0;
        foreach (var item in info.files)
        {
            var item11 = item.MakeDownloadObj(obj);
            list.Add(item11);

            var url = item.downloads
                .FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
            if (url == null)
            {
                url = item.downloads[0];
            }
            else
            {
                var modid = StringHelper.GetString(url, "data/", "/ver");
                var fileid = StringHelper.GetString(url, "versions/", "/");

                obj.Mods.Remove(modid);
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
            }

            now++;

            if (notify)
            {
                update(size, now);
            }
        }

        return list;
    }
}