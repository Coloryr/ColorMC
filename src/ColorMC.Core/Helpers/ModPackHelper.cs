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
    /// <param name="arg">参数</param>
    /// <returns>是否升级完成</returns>
    public static async Task<bool> UpgradeModPack(UpdateCurseForgeModPackArg arg)
    {
        arg.Data.FixDownloadUrl();

        var item = new DownloadItemObj()
        {
            Url = arg.Data.DownloadUrl,
            Name = arg.Data.FileName,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + arg.Data.FileName),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await UpgradeCurseForgeModPackAsync(new UpdateModPackArg
        {
            Game = arg.Game,
            Zip = item.Local,
            Update = arg.Update,
            Update2 = arg.Update2
        });
        if (res)
        {
            arg.Game.PID = arg.Data.ModId.ToString();
            arg.Game.FID = arg.Data.Id.ToString();
            arg.Game.Save();
            arg.Game.SaveModInfo();
        }

        return res;
    }

    /// <summary>
    /// 更新Modrinth整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>是否升级完成</returns>
    public static async Task<bool> UpgradeModPack(UpdateModrinthModPackArg arg)
    {
        var file = arg.Data.Files.FirstOrDefault(a => a.Primary) ?? arg.Data.Files[0];
        var item = new DownloadItemObj()
        {
            Url = file.Url,
            Name = file.Filename,
            SHA1 = file.Hashes.Sha1,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.Filename),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await UpgradeModrinthModPackAsync(new UpdateModPackArg
        {
            Game = arg.Game,
            Zip = item.Local,
            Update = arg.Update,
            Update2 = arg.Update2
        });
        if (res)
        {
            arg.Game.PID = arg.Data.ProjectId;
            arg.Game.FID = arg.Data.Id;
            arg.Game.Save();
            arg.Game.SaveModInfo();
        }

        return res;
    }

    /// <summary>
    /// 升级CurseForge整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>是否升级完成</returns>
    private static async Task<bool> UpgradeCurseForgeModPackAsync(UpdateModPackArg arg)
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
        foreach (var item in info.Minecraft.ModLoaders)
        {
            if (item.Id.StartsWith("forge"))
            {
                arg.Game.Loader = Loaders.Forge;
                arg.Game.LoaderVersion = item.Id.Replace("forge-", "");
            }
            else if (item.Id.StartsWith("neoforge"))
            {
                arg.Game.Loader = Loaders.NeoForge;
                arg.Game.LoaderVersion = item.Id.Replace("neoforge-", "");
            }
            else if (item.Id.StartsWith("fabric"))
            {
                arg.Game.Loader = Loaders.Fabric;
                arg.Game.LoaderVersion = item.Id.Replace("fabric-", "");
            }
            else if (item.Id.StartsWith("quilt"))
            {
                arg.Game.Loader = Loaders.Quilt;
                arg.Game.LoaderVersion = item.Id.Replace("quilt-", "");
            }
        }

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith(info.Overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(arg.Game.GetGamePath() +
                    e.Name[info.Overrides.Length..]);
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

        int b = 0;

        if (info3 != null)
        {
            var addlist = new List<CurseForgePackObj.FilesObj>();
            var removelist = new List<CurseForgePackObj.FilesObj>();

            CurseForgePackObj.FilesObj?[] temp1 = [.. info.Files];
            CurseForgePackObj.FilesObj?[] temp2 = [.. info3.Files];

            for (int index1 = 0; index1 < temp1.Length; index1++)
            {
                var item = temp1[index1];
                for (int index2 = 0; index2 < temp2.Length; index2++)
                {
                    var item1 = temp2[index2];
                    if (item1 == null)
                        continue;
                    if (item!.ProjectID == item1.ProjectID)
                    {
                        temp1[index1] = null;
                        temp2[index2] = null;
                        if (item.FileID != item1.FileID)
                        {
                            addlist.Add(item1);
                            removelist.Add(item);
                            break;
                        }
                    }
                }
            }

            foreach (var item in temp1)
            {
                if (item != null)
                {
                    addlist.Add(item);
                }
            }

            foreach (var item in temp2)
            {
                if (item != null)
                {
                    removelist.Add(item);
                }
            }

            foreach (var item in removelist)
            {
                if (arg.Game.Mods.Remove(item.ProjectID.ToString(), out var mod))
                {
                    PathHelper.Delete(Path.GetFullPath($"{path}/{mod.Path}/{mod.File}"));
                }
            }

            if (addlist.Count > 0)
            {
                arg.Update?.Invoke(addlist.Count, 0);
            }

            foreach (var item in addlist)
            {
                var res = await CurseForgeAPI.GetMod(item);
                if (res == null || res.Data == null)
                {
                    return false;
                }

                var path1 = await CurseForgeHelper.GetItemPathAsync(arg.Game, res.Data);
                var modid = res.Data.ModId.ToString();
                var item1 = res.Data.MakeModDownloadObj(path1.File);
                list1.Add(item1);
                if (path1.FileType == FileType.Mod)
                {
                    arg.Game.Mods.TryAdd(modid, res.Data.MakeModInfo(path1.Name));
                }

                arg.Update?.Invoke(addlist.Count, ++b);
            }
        }
        else
        {
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            var obj1 = arg.Game.CopyObj();
            obj1.Mods.Clear();

            var list = await CurseForgeHelper.GetModInfoAsync(new GetCurseForgeModInfoArg
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
    /// <param name="arg">参数</param>
    /// <returns>安装结果</returns>
    public static async Task<GameRes> InstallCurseForgeModPackAsync(InstallModPackZipArg arg)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
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
        foreach (var item in info.Minecraft.ModLoaders)
        {
            if (item.Id.StartsWith("forge"))
            {
                loaders = Loaders.Forge;
                loaderversion = item.Id.Replace("forge-", "");
            }
            else if (item.Id.StartsWith("neoforge"))
            {
                loaders = Loaders.NeoForge;
                loaderversion = item.Id.Replace("neoforge-", "");
            }
            else if (item.Id.StartsWith("fabric"))
            {
                loaders = Loaders.Fabric;
                loaderversion = item.Id.Replace("fabric-", "");
            }
            else if (item.Id.StartsWith("quilt"))
            {
                loaders = Loaders.Quilt;
                loaderversion = item.Id.Replace("quilt-", "");
            }
        }

        if (loaderversion.StartsWith(info.Minecraft.Version + "-")
            && loaderversion.Length > info.Minecraft.Version.Length + 1)
        {
            loaderversion = loaderversion[(info.Minecraft.Version.Length + 1)..];
        }

        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = $"{info.Name}-{info.Version}";
        }

        var gameversion = info.Minecraft.Version;
        if (VersionPath.CheckUpdateAsync(gameversion) == null)
        {
            await VersionPath.GetFromWebAsync();
            if (VersionPath.CheckUpdateAsync(gameversion) == null)
            {
                return new();
            }
        }

        //创建游戏实例
        var game = new GameSettingObj()
        {
            GroupName = arg.Group,
            Name = arg.Name,
            Version = gameversion,
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
            if (e.IsFile && e.Name.StartsWith(info.Overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(game.GetGamePath() +
                    e.Name[info.Overrides.Length..]);
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
        var list = await CurseForgeHelper.GetModInfoAsync(new GetCurseForgeModInfoArg
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

        await DownloadManager.StartAsync([.. list.List!]);

        arg.Update2?.Invoke(CoreRunState.DownloadDone);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 升级Modrinth整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>升级结果</returns>
    private static async Task<bool> UpgradeModrinthModPackAsync(UpdateModPackArg arg)
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

        if (info.Dependencies.TryGetValue("forge", out var version))
        {
            arg.Game.Loader = Loaders.Forge;
            arg.Game.LoaderVersion = version;
        }
        else if (info.Dependencies.TryGetValue("neoforge", out version))
        {
            arg.Game.Loader = Loaders.NeoForge;
            arg.Game.LoaderVersion = version;
        }
        else if (info.Dependencies.TryGetValue("fabric-loader", out version))
        {
            arg.Game.Loader = Loaders.Fabric;
            arg.Game.LoaderVersion = version;
        }
        else if (info.Dependencies.TryGetValue("quilt-loader", out version))
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
            var addlist = new List<ModrinthPackObj.FileObj>();
            var removelist = new List<ModrinthPackObj.FileObj>();

            ModrinthPackObj.FileObj?[] temp1 = [.. info.Files];
            ModrinthPackObj.FileObj?[] temp2 = [.. info3.Files];

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
                    if (item.Hashes.Sha1 == item1.Hashes.Sha1)
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
                PathHelper.Delete(Path.GetFullPath($"{path}/{item.Path}"));

                var url = item.Downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url is { })
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    arg.Game.Mods.Remove(modid);
                }
            }

            foreach (var item in addlist)
            {
                var item11 = list.First(a => a.SHA1 == item.Hashes.Sha1);
                list1.Add(item11);
                var url = item.Downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url != null)
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    var fileid = StringHelper.GetString(url, "versions/", "/");

                    arg.Game.Mods.Remove(modid);
                    arg.Game.Mods.Add(modid, new()
                    {
                        Path = item.Path[..item.Path.IndexOf('/')],
                        Name = item.Path,
                        File = item.Path,
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
    /// <param name="arg">参数</param>
    /// <returns>安装结果</returns>
    public static async Task<GameRes> InstallModrinthModPackAsync(InstallModPackZipArg arg)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using var zFile = new ZipFile(arg.Zip);
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
        if (info.Dependencies.TryGetValue("forge", out var version))
        {
            loaders = Loaders.Forge;
            loaderversion = version;
        }
        else if (info.Dependencies.TryGetValue("neoforge", out version))
        {
            loaders = Loaders.NeoForge;
            loaderversion = version;
        }
        else if (info.Dependencies.TryGetValue("fabric-loader", out version))
        {
            loaders = Loaders.Fabric;
            loaderversion = version;
        }
        else if (info.Dependencies.TryGetValue("quilt-loader", out version))
        {
            loaders = Loaders.Quilt;
            loaderversion = version;
        }
        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = $"{info.Name}-{info.VersionId}";
        }

        var gameversion = info.Dependencies["minecraft"];
        if (VersionPath.CheckUpdateAsync(gameversion) == null)
        {
            await VersionPath.GetFromWebAsync();
            if (VersionPath.CheckUpdateAsync(gameversion) == null)
            {
                return new();
            }
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGame(new CreateGameArg
        {
            Game = new GameSettingObj()
            {
                GroupName = arg.Group,
                Name = arg.Name,
                Version = gameversion,
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