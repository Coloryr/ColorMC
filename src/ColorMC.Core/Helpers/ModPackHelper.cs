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
    /// <param name="data">数据</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateModPack(UpdateCurseForgeModPackArg arg)
    {
        arg.Data.FixDownloadUrl();

        var item = new DownloadItemObj()
        {
            Url = arg.Data.downloadUrl,
            Name = arg.Data.fileName,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + arg.Data.fileName),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await UpdateCurseForgeModPackAsync(new UpdateModPackArg
        {
            Game = arg.Game,
            Zip = item.Local,
            Update = arg.Update,
            Update2 = arg.Update2
        });
        if (res)
        {
            arg.Game.PID = arg.Data.modId.ToString();
            arg.Game.FID = arg.Data.id.ToString();
            arg.Game.Save();
            arg.Game.SaveModInfo();
        }

        return res;
    }

    /// <summary>
    /// 更新Modrinth整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data"></param>
    /// <returns>升级结果</returns>
    public static async Task<bool> UpdateModPack(UpdateModrinthModPackArg arg)
    {
        var file = arg.Data.files.FirstOrDefault(a => a.primary) ?? arg.Data.files[0];
        var item = new DownloadItemObj()
        {
            Url = file.url,
            Name = file.filename,
            SHA1 = file.hashes.sha1,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.filename),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await UpdateModrinthModPackAsync(new UpdateModPackArg
        { 
            Game = arg.Game,
            Zip = item.Local, 
            Update = arg.Update, 
            Update2 = arg.Update2
        });
        if (res)
        {
            arg.Game.PID = arg.Data.project_id;
            arg.Game.FID = arg.Data.id;
            arg.Game.Save();
            arg.Game.SaveModInfo();
        }

        return res;
    }

    /// <summary>
    /// 升级CurseForge整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="zip">压缩包路径</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateCurseForgeModPackAsync(UpdateModPackArg arg)
    {
        using var zFile = new ZipFile(arg.Zip);
        using var stream1 = new MemoryStream();
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

        arg.Update2?.Invoke(CoreRunState.Init);
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
                arg.Game.Loader = Loaders.Forge;
                arg.Game.LoaderVersion = item.id.Replace("forge-", "");
            }
            else if (item.id.StartsWith("neoforge"))
            {
                arg.Game.Loader = Loaders.NeoForge;
                arg.Game.LoaderVersion = item.id.Replace("neoforge-", "");
            }
            else if (item.id.StartsWith("fabric"))
            {
                arg.Game.Loader = Loaders.Fabric;
                arg.Game.LoaderVersion = item.id.Replace("fabric-", "");
            }
            else if (item.id.StartsWith("quilt"))
            {
                arg.Game.Loader = Loaders.Quilt;
                arg.Game.LoaderVersion = item.id.Replace("quilt-", "");
            }
        }

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith(info.overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(arg.Game.GetGamePath() +
                    e.Name[info.overrides.Length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        CurseForgePackObj? info3 = null;
        var json = arg.Game.GetModPackJsonFile();
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

        var path = arg.Game.GetGamePath();

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
                if (arg.Game.Mods.Remove(item.projectID.ToString(), out var mod))
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

                var path1 = await CurseForgeHelper.GetItemPath(arg.Game, res.data);
                var modid = res.data.modId.ToString();
                list1.Add(res.data.MakeModDownloadObj(path1.Item1));
                arg.Game.Mods.Add(modid, res.data.MakeModInfo(path1.Item2));
            }
        }
        else
        {
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            var obj1 = arg.Game.CopyObj();
            obj1.Mods.Clear();

            var list = await CurseForgeHelper.GetModInfo(new GetCurseForgeModInfoArg
            { 
                Game = obj1, 
                Info = info, 
                Update = arg.Update 
            });
            if (!list.State)
            {
                return false;
            }

            ModInfoObj[] temp1 = [.. arg.Game.Mods.Values];
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
                arg.Game.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                list1.Add(list.List!.First(a => a.SHA1 == item.SHA1));
                arg.Game.Mods.Add(item.ModId, item);
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
    public static async Task<GameRes> InstallCurseForgeModPackAsync(InstallModPackZipArg arg)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using ZipFile zFile = new(PathHelper.OpenRead(arg.Zip));
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
            return new();
        }

        arg.Update2?.Invoke(CoreRunState.Init);
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
            return new();
        }
        if (info == null)
        {
            return new();
        }

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

        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = $"{info.name}-{info.version}";
        }


        //创建游戏实例
        var game = new GameSettingObj()
        {
            GroupName = arg.Group,
            Name = arg.Name,
            Version = info.minecraft.version,
            ModPack = true,
            Loader = loaders,
            ModPackType = SourceType.CurseForge,
            LoaderVersion = loaderversion
        };

        game = await InstancesPath.CreateGame(new CreateGameArg
        { 
            Game = game,
            Request = arg.Request,
            Overwirte = arg.Overwirte 
        });

        if (game == null)
        {
            return new();
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

        arg.Update2?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息
        var list = await CurseForgeHelper.GetModInfo(new GetCurseForgeModInfoArg
        {
            Game = game,
            Info = info,
            Update = arg.Update
        });
        if (!list.State)
        {
            return new GameRes { Game = game };
        }

        game.SaveModInfo();

        arg.Update2?.Invoke(CoreRunState.Download);

        await DownloadManager.StartAsync([.. list.List]);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 升级Modrinth整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="zip">整合包路径</param>
    /// <returns>升级结果</returns>
    public static async Task<bool> UpdateModrinthModPackAsync(UpdateModPackArg arg)
    {
        using var zFile = new ZipFile(PathHelper.OpenRead(arg.Zip));
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

        arg.Update2?.Invoke(CoreRunState.Init);
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
            arg.Game.Loader = Loaders.Forge;
            arg.Game.LoaderVersion = version;
        }
        else if (info.dependencies.TryGetValue("neoforge", out version))
        {
            arg.Game.Loader = Loaders.NeoForge;
            arg.Game.LoaderVersion = version;
        }
        else if (info.dependencies.TryGetValue("fabric-loader", out version))
        {
            arg.Game.Loader = Loaders.Fabric;
            arg.Game.LoaderVersion = version;
        }
        else if (info.dependencies.TryGetValue("quilt-loader", out version))
        {
            arg.Game.Loader = Loaders.Quilt;
            arg.Game.LoaderVersion = version;
        }

        int length = "overrides".Length;

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith("overrides/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(arg.Game.GetGamePath() +
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
        var json = arg.Game.GetModPackJsonFile();
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

        var obj1 = arg.Game.CopyObj();
        obj1.Mods.Clear();

        //获取Mod信息
        var list = ModrinthHelper.GetModrinthModInfo(new GetModrinthModInfoArg
        { 
            Game = obj1, 
            Info = info, 
            Update = arg.Update 
        });
        var list1 = new List<DownloadItemObj>();

        string path = arg.Game.GetGamePath();

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
                    arg.Game.Mods.Remove(modid);
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

                    arg.Game.Mods.Remove(modid);
                    arg.Game.Mods.Add(modid, new()
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

            ModInfoObj[] temp1 = [.. arg.Game.Mods.Values];
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
                arg.Game.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                list1.Add(list.First(a => a.SHA1 == item.SHA1));
                arg.Game.Mods.Add(item.ModId, item);
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
    public static async Task<GameRes> InstallModrinthModPackAsync(InstallModPackZipArg arg)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using ZipFile zFile = new(PathHelper.OpenRead(arg.Zip));
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
            return new();
        }

        arg.Update2?.Invoke(CoreRunState.Init);
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
            return new();
        }
        if (info == null)
        {
            return new();
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
        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = $"{info.name}-{info.versionId}";
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGame(new CreateGameArg
        {
            Game = new GameSettingObj()
            {
                GroupName = arg.Group,
                Name = arg.Name,
                Version = info.dependencies["minecraft"],
                ModPack = true,
                ModPackType = SourceType.Modrinth,
                Loader = loaders,
                LoaderVersion = loaderversion
            },
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes { Game = game };
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

        arg.Update2?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息

        var list = ModrinthHelper.GetModrinthModInfo(new GetModrinthModInfoArg 
        { 
            Game = game, 
            Info = info, 
            Update = arg.Update 
        });

        game.SaveModInfo();

        arg.Update2?.Invoke(CoreRunState.Download);

        await DownloadManager.StartAsync([.. list]);

        return new GameRes { State = true, Game = game };
    }
}