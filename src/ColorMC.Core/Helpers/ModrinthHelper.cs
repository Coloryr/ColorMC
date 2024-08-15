using System.Collections.Concurrent;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;

namespace ColorMC.Core.Helpers;

/// <summary>
/// Modrinth处理
/// </summary>
public static class ModrinthHelper
{
    private static List<ModrinthCategoriesObj>? s_categories;
    private static List<string>? s_gameVersions;

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj MakeModDownloadObj(this ModrinthVersionObj data, GameSettingObj obj)
    {
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        return new DownloadItemObj()
        {
            Name = data.name,
            Url = file.url,
            Local = Path.GetFullPath(obj.GetModsPath() + "/" + file.filename),
            SHA1 = file.hashes.sha1
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj MakeDownloadObj(this ModrinthPackObj.File data, GameSettingObj obj)
    {
        return new DownloadItemObj()
        {
            Url = data.downloads[0],
            Name = data.path,
            Local = obj.GetGamePath() + "/" + data.path,
            SHA1 = data.hashes.sha1
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns>Mod信息</returns>
    public static ModInfoObj MakeModInfo(this ModrinthVersionObj data)
    {
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        return new ModInfoObj()
        {
            Path = "mods",
            FileId = data.id.ToString(),
            ModId = data.project_id,
            File = file.filename,
            Name = data.name,
            Url = file.url,
            SHA1 = file.hashes.sha1
        };
    }

    /// <summary>
    /// 获取MO分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>?> GetModrinthCategories(FileType type)
    {
        if (s_categories == null)
        {
            var list6 = await ModrinthAPI.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            s_categories = list6;
        }

        var list7 = from item2 in s_categories
                    where item2.project_type == type switch
                    {
                        FileType.Shaderpack => ModrinthAPI.ClassShaderpack,
                        FileType.Resourcepack => ModrinthAPI.ClassResourcepack,
                        FileType.ModPack => ModrinthAPI.ClassModPack,
                        _ => ModrinthAPI.ClassMod
                    }
                    && item2.header == "categories"
                    orderby item2.name descending
                    select item2.name;

        return list7.ToDictionary(a => a);
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public static async Task<List<string>?> GetGameVersion()
    {
        if (s_gameVersions != null)
        {
            return s_gameVersions;
        }

        var list = await ModrinthAPI.GetGameVersions();
        if (list == null)
        {
            return null;
        }

        var list1 = new List<string>
        {
            ""
        };

        list1.AddRange(from item in list select item.version);

        s_gameVersions = list1;

        return list1;
    }

    /// <summary>
    /// 获取Modrinth整合包Mod信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="info">信息</param>
    /// <param name="notify">通知</param>
    /// <returns>信息</returns>
    public static List<DownloadItemObj> GetModrinthModInfo(GetModrinthModInfoArg arg)
    {
        var list = new List<DownloadItemObj>();

        var size = arg.Info.files.Count;
        var now = 0;
        foreach (var item in arg.Info.files)
        {
            var item11 = item.MakeDownloadObj(arg.Game);
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

            now++;

            arg.Update?.Invoke(size, now);
        }

        return list;
    }

    /// <summary>
    /// 自动标记mod
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="cov"></param>
    /// <returns></returns>
    public static async Task<IntRes> AutoMark(GameSettingObj obj, bool cov)
    {
        var list = await obj.GetModsAsync();
        var mods1 = obj.Mods.Values.ToArray();

        var res = new ConcurrentBag<ModInfoObj>();

        int error = 0;
        int count = 0;

        await Parallel.ForEachAsync(list, async (item, cancel) =>
        {
            if (mods1.Any(item1 => item.Sha1 == item1.SHA1) && !cov)
            {
                return;
            }

            var data = await ModrinthAPI.GetVersionFromSha1(item.Sha1);
            if (data == null)
            {
                error++;
                return;
            }
            res.Add(new()
            {
                Path = "mods",
                Name = data.files[0].filename,
                File = Path.GetFileName(item.Local),
                SHA1 = item.Sha1,
                Url = data.files[0].url,
                ModId = data.project_id,
                FileId = data.id
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
    /// 获取Mod依赖
    /// </summary>
    /// <param name="data">mod</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <returns></returns>
    public static async Task<ConcurrentBag<GetModrinthModDependenciesRes>>
        GetModDependencies(ModrinthVersionObj data, string mc, Loaders loader)
    {
        var list = new ConcurrentBag<string>();
        return await GetModDependencies(data, mc, loader, list);
    }

    private static async Task<ConcurrentBag<GetModrinthModDependenciesRes>>
        GetModDependencies(ModrinthVersionObj data, string mc, Loaders loader,
        ConcurrentBag<string> ids)
    {
        if (data.dependencies == null || data.dependencies.Count == 0)
        {
            return [];
        }
        var list = new ConcurrentBag<GetModrinthModDependenciesRes>();
        //await Parallel.ForEachAsync(data.dependencies, new ParallelOptions()
        //{
        //    MaxDegreeOfParallelism = 1
        //}, async (item, cancel) =>
        await Parallel.ForEachAsync(data.dependencies, async (item, cancel) =>
        {
            if (ids.Contains(item.project_id))
            {
                return;
            }

            ModrinthVersionObj? res = null;
            var info = await ModrinthAPI.GetProject(item.project_id);
            if (info == null)
            {
                return;
            }
            if (item.version_id == null)
            {
                var res1 = await ModrinthAPI.GetFileVersions(item.project_id, mc, loader);
                if (res1 == null || res1.Count == 0)
                {
                    return;
                }
                res = res1[0];
            }
            else
            {
                res = await ModrinthAPI.GetVersion(item.project_id, item.version_id);
            }

            if (res == null)
            {
                return;
            }

            var mod = new GetModrinthModDependenciesRes()
            {
                Name = info.title,
                ModId = res.project_id,
                List = [res]
            };
            ids.Add(res.project_id);
            list.Add(mod);

            foreach (var item3 in data.dependencies)
            {
                if (ids.Contains(item3.project_id))
                {
                    continue;
                }
                foreach (var item5 in await GetModDependencies(res, mc, loader, ids))
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
}
