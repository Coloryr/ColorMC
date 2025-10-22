using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using SharpCompress.Archives.Zip;

namespace ColorMC.Core.Helpers;

/// <summary>
/// Modrinth处理
/// </summary>
public static class ModrinthHelper
{
    public static readonly MSortingObj Relevance = new()
    {
        Data = "relevance"
    };

    public static readonly MSortingObj Downloads = new()
    {
        Data = "downloads"
    };

    public static readonly MSortingObj Follows = new()
    {
        Data = "follows"
    };

    public static readonly MSortingObj Newest = new()
    {
        Data = "newest"
    };

    public static readonly MSortingObj Updated = new()
    {
        Data = "updated"
    };

    public static MFacetsObj BuildCategories(List<string> values) => new()
    {
        Data = "categories",
        Values = values
    };

    public static MFacetsObj BuildVersions(List<string> values) => new()
    {
        Data = "versions",
        Values = values
    };

    public static MFacetsObj BuildProjectType(List<string> values) => new()
    {
        Data = "project_type",
        Values = values
    };

    /// <summary>
    /// 分类列表
    /// </summary>
    private static List<ModrinthCategoriesObj>? s_categories;

    /// <summary>
    /// 游戏版本列表
    /// </summary>
    private static List<string>? s_gameVersions;

    /// <summary>
    /// 构建一个字符串
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string BuildFacets(List<MFacetsObj> list)
    {
        var builder = new StringBuilder();
        builder.Append('[');
        foreach (var item in list)
        {
            if (item.Values.Count == 0)
                continue;

            foreach (var item1 in item.Values)
            {
                builder.Append($"[\"{item.Data}:{item1}\"],");
            }

        }
        builder.Remove(builder.Length - 1, 1);
        builder.Append(']');
        return builder.ToString();
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="path">文件路径</param>
    /// <returns>下载项目</returns>
    public static FileItemObj MakeDownloadObj(this ModrinthVersionObj data, string path)
    {
        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
        return new FileItemObj()
        {
            Name = data.Name,
            Url = file.Url,
            Local = path,
            Sha1 = file.Hashes.Sha1
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static FileItemObj MakeDownloadObj(this ModrinthVersionObj data, GameSettingObj obj)
    {
        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
        return new FileItemObj()
        {
            Name = data.Name,
            Url = file.Url,
            Local = Path.Combine(obj.GetModsPath(), file.Filename),
            Sha1 = file.Hashes.Sha1
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static FileItemObj MakeDownloadObj(this ModrinthPackObj.ModrinthPackFileObj data, GameSettingObj obj)
    {
        return new FileItemObj()
        {
            Url = data.Downloads[0],
            Name = data.Path,
            Local = Path.Combine(obj.GetGamePath(), data.Path),
            Sha1 = data.Hashes.Sha1
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns>Mod信息</returns>
    public static ModInfoObj MakeModInfo(this ModrinthVersionObj data, string name)
    {
        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
        return new ModInfoObj()
        {
            Path = name,
            FileId = data.Id.ToString(),
            ModId = data.ProjectId,
            File = file.Filename,
            Name = data.Name,
            Url = file.Url,
            Sha1 = file.Hashes.Sha1
        };
    }

    /// <summary>
    /// 获取MO分组
    /// </summary>
    /// <param name="type">文件类型</param>
    /// <returns>分组列表</returns>
    public static async Task<Dictionary<string, string>?> GetCategoriesAsync(FileType type)
    {
        if (s_categories == null)
        {
            var list6 = await ModrinthAPI.GetCategoriesAsync();
            if (list6 == null)
            {
                return null;
            }

            s_categories = list6;
        }

        var list7 = from item2 in s_categories
                    where item2.ProjectType == type switch
                    {
                        FileType.Shaderpack => ModrinthAPI.ClassShaderpack,
                        FileType.Resourcepack => ModrinthAPI.ClassResourcepack,
                        FileType.ModPack => ModrinthAPI.ClassModPack,
                        _ => ModrinthAPI.ClassMod
                    }
                    && item2.Header == "categories"
                    orderby item2.Name descending
                    select item2.Name;

        return list7.ToDictionary(a => a);
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    /// <returns>版本列表</returns>
    public static async Task<List<string>?> GetGameVersionAsync()
    {
        if (s_gameVersions != null)
        {
            return s_gameVersions;
        }

        var list = await ModrinthAPI.GetGameVersionsAsync();
        if (list == null)
        {
            return null;
        }

        var list1 = new List<string>
        {
            ""
        };

        list1.AddRange(from item in list select item.Version);

        s_gameVersions = list1;

        return list1;
    }

    /// <summary>
    /// 获取Modrinth整合包模组信息
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>下载列表</returns>
    public static List<FileItemObj> GetModrinthModInfo(GetModrinthModInfoArg arg)
    {
        var list = new List<FileItemObj>();

        var size = arg.Info.Files.Count;
        var now = 0;
        foreach (var item in arg.Info.Files)
        {
            var item11 = item.MakeDownloadObj(arg.Game);
            list.Add(item11);

            var url = item.Downloads
                .FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));

            if (item.ColorMc != null)
            {
                arg.Game.Mods.Remove(item.ColorMc.PID);
                arg.Game.Mods.Add(item.ColorMc.PID, new()
                {
                    Path = item.Path[..item.Path.IndexOf('/')],
                    Name = item.Path,
                    File = item.Path,
                    Sha1 = item11.Sha1!,
                    ModId = item.ColorMc.PID,
                    FileId = item.ColorMc.FID,
                    Url = url ?? item.Downloads[0]
                });

                now++;
                arg.Update?.Invoke(size, now);
                continue;
            }
            else if (url != null)
            {
                var modid = StringHelper.GetString(url, "data/", "/ver");
                var fileid = StringHelper.GetString(url, "versions/", "/");

                arg.Game.Mods.Remove(modid);
                arg.Game.Mods.Add(modid, new()
                {
                    Path = item.Path[..item.Path.IndexOf('/')],
                    Name = item.Path,
                    File = item.Path,
                    Sha1 = item11.Sha1!,
                    ModId = modid,
                    FileId = fileid,
                    Url = url
                });
            }
            else
            {
                url = item.Downloads[0];
            }

            now++;

            arg.Update?.Invoke(size, now);
        }

        return list;
    }

    /// <summary>
    /// 自动标记模组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cov">是否覆盖之前的标记</param>
    /// <returns>标记结果</returns>
    public static async Task<IntRes> AutoMarkAsync(GameSettingObj obj, bool cov)
    {
        var list = await obj.GetModsAsync();
        var mods1 = obj.Mods.Values.ToArray();

        var res = new ConcurrentBag<ModInfoObj>();

        int error = 0;
        int count = 0;

        await Parallel.ForEachAsync(list, async (item, cancel) =>
        {
            if (mods1.Any(item1 => item.Sha1 == item1.Sha1) && !cov)
            {
                return;
            }

            var data = await ModrinthAPI.GetVersionFromSha1Async(item.Sha1);
            if (data == null)
            {
                error++;
                return;
            }
            res.Add(new()
            {
                Path = Names.NameGameModDir,
                Name = data.Files[0].Filename,
                File = Path.GetFileName(item.Local),
                Sha1 = item.Sha1,
                Url = data.Files[0].Url,
                ModId = data.ProjectId,
                FileId = data.Id
            });
            count++;
        });

        foreach (var item in res)
        {
            if (!obj.Mods.TryAdd(item.ModId, item))
            {
                obj.Mods[item.ModId] = item;
            }
        }

        obj.SaveModInfo();
        if (error != 0)
        {
            return new() { Data = error, State = false };
        }
        return new() { Data = count, State = true };
    }

    /// <summary>
    /// 获取模组依赖
    /// </summary>
    /// <param name="data">模组数据</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <returns>模组列表</returns>
    public static async Task<ConcurrentBag<GetModrinthModDependenciesRes>>
        GetModDependenciesAsync(ModrinthVersionObj data, string mc, Loaders loader)
    {
        var list = new ConcurrentBag<string>();
        return await GetModDependenciesAsync(data, mc, loader, list);
    }

    /// <summary>
    /// 获取模组依赖
    /// </summary>
    /// <param name="data">模组数据</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <param name="ids">已经标记的列表</param>
    /// <returns>模组列表</returns>
    private static async Task<ConcurrentBag<GetModrinthModDependenciesRes>>
        GetModDependenciesAsync(ModrinthVersionObj data, string mc, Loaders loader, ConcurrentBag<string> ids)
    {
        if (data.Dependencies == null || data.Dependencies.Count == 0)
        {
            return [];
        }
        var list = new ConcurrentBag<GetModrinthModDependenciesRes>();
#if false
        await Parallel.ForEachAsync(data.Dependencies, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 1
        }, async (item, cancel) =>
#else
        await Parallel.ForEachAsync(data.Dependencies, async (item, cancel) =>
#endif
        {
            if (ids.Contains(item.ProjectId))
            {
                return;
            }

            ModrinthVersionObj? res = null;
            var info = await ModrinthAPI.GetProjectAsync(item.ProjectId);
            if (info == null)
            {
                return;
            }
            if (item.VersionId == null)
            {
                var res1 = await ModrinthAPI.GetFileVersionsAsync(item.ProjectId, mc, loader);
                if (res1 == null || res1.Count == 0)
                {
                    return;
                }
                res = res1[0];
            }
            else
            {
                res = await ModrinthAPI.GetVersionAsync(item.ProjectId, item.VersionId);
            }

            if (res == null)
            {
                return;
            }

            var mod = new GetModrinthModDependenciesRes()
            {
                Name = info.Title,
                ModId = res.ProjectId,
                List = [res]
            };
            ids.Add(res.ProjectId);
            list.Add(mod);

            foreach (var item3 in data.Dependencies)
            {
                if (ids.Contains(item3.ProjectId))
                {
                    continue;
                }
                foreach (var item5 in await GetModDependenciesAsync(res, mc, loader, ids))
                {
                    if (ids.Contains(item5.ModId))
                    {
                        continue;
                    }
                    ids.Add(item5.ModId);
                    list.Add(item5);
                }
            }
        });

        return list;
    }

    /// <summary>
    /// 更新Modrinth整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>是否升级完成</returns>
    public static async Task<bool> UpgradeModPackAsync(UpdateModrinthModPackArg arg)
    {
        var file = arg.Data.Files.FirstOrDefault(a => a.Primary) ?? arg.Data.Files[0];
        var item = new FileItemObj()
        {
            Url = file.Url,
            Name = file.Filename,
            Sha1 = file.Hashes.Sha1,
            Local = Path.Combine(DownloadManager.DownloadDir, file.Filename),
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
    /// 升级Modrinth整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>升级结果</returns>
    private static async Task<bool> UpgradeModrinthModPackAsync(UpdateModPackArg arg)
    {
        using var stream3 = PathHelper.OpenRead(arg.Zip);
        if (stream3 == null)
        {
            return false;
        }
        using var zFile = ZipArchive.Open(stream3);
        ModrinthPackObj? info = null;
        //获取主信息
        if (zFile.Entries.FirstOrDefault(item => item.Key
         == Names.NameModrinthFile) is { } ent)
        {
            try
            {
                using var stream = ent.OpenEntryStream();
                info = JsonUtils.ToObj(stream, JsonType.ModrinthPackObj);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Pack.Error1"), e);
                return false;
            }
        }
        else
        {
            return false;
        }
        if (info == null)
        {
            return false;
        }

        arg.Update2?.Invoke(CoreRunState.Init);
        //获取版本数据

        if (info.Dependencies.TryGetValue(Names.NameForgeKey, out var version))
        {
            arg.Game.Loader = Loaders.Forge;
            arg.Game.LoaderVersion = version;
        }
        else if (info.Dependencies.TryGetValue(Names.NameNeoForgeKey, out version))
        {
            arg.Game.Loader = Loaders.NeoForge;
            arg.Game.LoaderVersion = version;
        }
        else if (info.Dependencies.TryGetValue(Names.NameFabricLoaderKey, out version))
        {
            arg.Game.Loader = Loaders.Fabric;
            arg.Game.LoaderVersion = version;
        }
        else if (info.Dependencies.TryGetValue(Names.NameQuiltLoaderKey, out version))
        {
            arg.Game.Loader = Loaders.Quilt;
            arg.Game.LoaderVersion = version;
        }

        int length = Names.NameOverrideDir.Length;
        string dir = Names.NameOverrideDir + "/";

        //解压文件
        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            if (e.Key!.StartsWith(dir))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(arg.Game.GetGamePath() + e.Key[length..]);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
            else
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(arg.Game.GetBasePath() + "/" + e.Key);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
        }

        //获取当前整合包数据
        ModrinthPackObj? info3 = null;
        var json = Path.Combine(arg.Game.GetBasePath(), Names.NameModrinthFile);
        if (File.Exists(json))
        {
            try
            {
                using var stream = PathHelper.OpenRead(json);
                info3 = JsonUtils.ToObj(stream, JsonType.ModrinthPackObj);
            }
            catch
            {

            }
        }

        var obj1 = arg.Game.CopyObj();
        obj1.Mods.Clear();

        //获取Mod信息
        var list = GetModrinthModInfo(new GetModrinthModInfoArg
        {
            Game = obj1,
            Info = info,
            Update = arg.Update
        });
        var list1 = new List<FileItemObj>();

        string path = arg.Game.GetGamePath();

        if (info3 != null)
        {
            //筛选新旧整合包文件差距
            var addlist = new List<ModrinthPackObj.ModrinthPackFileObj>();
            var removelist = new List<ModrinthPackObj.ModrinthPackFileObj>();

            ModrinthPackObj.ModrinthPackFileObj?[] temp1 = [.. info.Files];
            ModrinthPackObj.ModrinthPackFileObj?[] temp2 = [.. info3.Files];

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
                PathHelper.Delete(Path.Combine(path, item.Path));

                var url = item.Downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url is { })
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    arg.Game.Mods.Remove(modid);
                }
            }

            foreach (var item in addlist)
            {
                var item11 = list.First(a => a.Sha1 == item.Hashes.Sha1);
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
                        Sha1 = item11.Sha1!,
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
                            || item.Sha1 != item1.Sha1)
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
                list1.Add(list.First(a => a.Sha1 == item.Sha1));
                arg.Game.Mods.Add(item.ModId, item);
            }
        }

        using var stream1 = ent.OpenEntryStream();
        PathHelper.WriteBytes(json, stream1);

        await DownloadManager.StartAsync(list1);

        return true;
    }
}
