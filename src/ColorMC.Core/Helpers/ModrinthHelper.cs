using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Core.Worker;

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
    public static List<ModrinthCategoriesObj>? Categories { get; private set; }

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
            Local = Path.Combine(path, file.Filename),
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
        if (Categories == null)
        {
            var list6 = await ModrinthAPI.GetCategoriesAsync();
            if (list6 == null)
            {
                return null;
            }

            Categories = list6;
        }

        var list7 = from item2 in Categories
                    where item2.ProjectType == type switch
                    {
                        FileType.Shaderpack => ModrinthAPI.ClassShaderpack,
                        FileType.Resourcepack => ModrinthAPI.ClassResourcepack,
                        FileType.Modpack => ModrinthAPI.ClassModPack,
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
    public static MakeDownloadItemsRes GetModrinthModInfo(GameSettingObj game,
        ModrinthPackObj info, IAddGui? gui, CancellationToken token = default)
    {
        var list = new List<FileItemObj>();

        var mods = new Dictionary<string, ModInfoObj>();

        var size = info.Files.Count;
        var now = 0;
        foreach (var item in info.Files)
        {
            if (token.IsCancellationRequested)
            {
                return new MakeDownloadItemsRes();
            }

            var item11 = item.MakeDownloadObj(game);
            list.Add(item11);

            var url = item.Downloads
                .FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));

            if (item.ColorMc != null)
            {
                mods.Remove(item.ColorMc.PID);
                mods.Add(item.ColorMc.PID, new()
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
                gui?.SetNowSub(size, now);
                continue;
            }
            else if (url != null)
            {
                var modid = StringHelper.GetString(url, "data/", "/ver");
                var fileid = StringHelper.GetString(url, "versions/", "/");

                mods.Remove(modid);
                mods.Add(modid, new()
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

            gui?.SetNowSub(size, now);
        }

        return new MakeDownloadItemsRes
        {
            State = true,
            List = list,
            Mods = mods
        };
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
    public static async Task<bool> UpgradeModPackAsync(GameSettingObj game, ModrinthVersionObj data, IAddGui? gui)
    {
        var item = data.MakeDownloadObj(Path.Combine(DownloadManager.DownloadDir));

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await UpgradeAsync(game, item.Local, gui);
        if (res)
        {
            game.PID = data.ProjectId;
            game.FID = data.Id;
            game.Save();
            game.SaveModInfo();
        }

        return res;
    }

    /// <summary>
    /// 升级Modrinth整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>升级结果</returns>
    private static async Task<bool> UpgradeAsync(GameSettingObj game, string zip, IAddGui? packgui)
    {
        packgui?.SetState(AddState.ReadInfo);
        packgui?.SetNow(1, 5);

        using var work = new ModrinthWork(zip, null, packgui);

        if (!work.ReadInfo() || !await work.ReadVersion())
        {
            return false;
        }

        work.UpdateGame(game);

        packgui?.SetState(AddState.Unzip);
        packgui?.SetNow(2, 5);

        if (!await work.Unzip(null))
        {
            return false;
        }

        packgui?.SetSubText(null);
        packgui?.SetNowSub(0, 0);

        packgui?.SetState(AddState.GetInfo);
        packgui?.SetNow(3, 5);

        if (!await work.CheckUpgrade())
        {
            return false;
        }

        packgui?.SetSubText(null);
        packgui?.SetNowSub(0, 0);
        packgui?.SetState(AddState.DownloadFile);
        packgui?.SetNow(4, 5);

        await work.Download();

        packgui?.SetState(AddState.Done);
        packgui?.SetNow(5, 5);

        return true;
    }
}
