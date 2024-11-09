using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// CueseForge处理
/// </summary>
public static class CurseForgeHelper
{
    private static CurseForgeCategoriesObj? s_categories;
    private static List<string>? s_supportVersion;
    /// <summary>
    /// 修正下载地址
    /// </summary>
    /// <param name="item">CurseForge数据</param>
    public static void FixDownloadUrl(this CurseForgeModObj.Data item)
    {
        item.downloadUrl ??= $"{UrlHelper.CurseForgeDownload}files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="path">下载路径</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj MakeModDownloadObj(this CurseForgeModObj.Data data, string path)
    {
        data.FixDownloadUrl();

        return new DownloadItemObj()
        {
            Url = data.downloadUrl,
            Name = data.displayName,
            Local = path + "/" + data.fileName,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                    .Select(a => a.value).FirstOrDefault()
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="path">路径名字</param>
    /// <returns>Mod信息</returns>
    public static ModInfoObj MakeModInfo(this CurseForgeModObj.Data data, string path)
    {
        data.FixDownloadUrl();

        return new ModInfoObj()
        {
            Path = path,
            Name = data.displayName,
            File = data.fileName,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                    .Select(a => a.value).FirstOrDefault(),
            ModId = data.modId.ToString(),
            FileId = data.id.ToString(),
            Url = data.downloadUrl
        };
    }

    /// <summary>
    /// 获取作者名字
    /// </summary>
    /// <param name="authors">作者列表</param>
    /// <returns>作者名字</returns>
    public static string GetString(this List<CurseForgeObjList.Data.Authors> authors)
    {
        if (authors == null || authors.Count == 0)
        {
            return "";
        }
        var builder = new StringBuilder();
        foreach (var item in authors)
        {
            builder.Append(item.name).Append(',');
        }

        return builder.ToString()[..^1];
    }

    /// <summary>
    /// 获取CF分组
    /// </summary>
    /// <param name="type">文件类型</param>
    /// <returns>分组列表</returns>
    public static async Task<Dictionary<string, string>?> GetCategoriesAsync(FileType type)
    {
        if (s_categories == null)
        {
            var list6 = await CurseForgeAPI.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            s_categories = list6;
        }

        var list7 = from item2 in s_categories.data
                    where item2.classId == type switch
                    {
                        FileType.Mod => CurseForgeAPI.ClassMod,
                        FileType.World => CurseForgeAPI.ClassWorld,
                        FileType.Resourcepack => CurseForgeAPI.ClassResourcepack,
                        FileType.Shaderpack => CurseForgeAPI.ClassShaderpack,
                        _ => CurseForgeAPI.ClassModPack
                    }
                    orderby item2.name descending
                    select (item2.name, item2.id);

        return list7.ToDictionary(a => a.id.ToString(), a => a.name);
    }

    /// <summary>
    /// 获取CurseForge支持的游戏版本
    /// </summary>
    /// <returns>游戏版本</returns>
    public static async Task<List<string>?> GetGameVersionsAsync()
    {
        if (s_supportVersion != null)
        {
            return s_supportVersion;
        }
        var list = await CurseForgeAPI.GetCurseForgeVersionType();
        if (list == null)
        {
            return null;
        }

        list.data.RemoveAll(a =>
        {
            return !a.name.StartsWith("Minecraft ");
        });

        var list111 = new List<CurseForgeVersionType.Item>();
        list111.AddRange(from item in list.data
                         where item.id > 17
                         orderby item.id descending
                         select item);
        list111.AddRange(from item in list.data
                         where item.id < 18
                         orderby item.id ascending
                         select item);

        var list2 = await CurseForgeAPI.GetCurseForgeVersion();
        if (list2 == null)
        {
            return null;
        }

        var list3 = new List<string>
        {
            ""
        };
        foreach (var item in list111)
        {
            var list4 = from item1 in list2.data
                        where item1.type == item.id
                        select item1.versions;
            var list5 = list4.FirstOrDefault();
            if (list5 != null)
            {
                list3.AddRange(list5);
            }
        }

        s_supportVersion = list3;

        return list3;
    }

    /// <summary>
    /// 获取CurseForge整合包Mod信息
    /// </summary>
    /// <param name="arg">获取信息</param>
    /// <returns>项目信息</returns>
    public static async Task<MakeDownloadItemsRes> GetModInfoAsync(GetCurseForgeModInfoArg arg)
    {
        var size = arg.Info.files.Count;
        var now = 0;
        var list = new ConcurrentBag<DownloadItemObj>();

        //获取Mod信息
        var res = await CurseForgeAPI.GetMods(arg.Info.files);
        if (res != null)
        {
            var res1 = res.Distinct(CurseForgeDataComparer.Instance);

            foreach (var item in res1)
            {
                var path = await GetItemPathAsync(arg.Game, item);
                var item1 = item.MakeModDownloadObj(path.File);
                list.Add(item1);
                var modid = item.modId.ToString();
                arg.Game.Mods.Remove(modid);

                if (path.FileType == FileType.Mod)
                {
                    arg.Game.Mods.Add(modid, item.MakeModInfo(path.Name));
                }
                else if (path.FileType == FileType.World)
                {
                    item1.Later = (test) =>
                    {
                        arg.Game.AddWorldZipAsync(item1.Local, test).Wait();
                    };
                }

                now++;
                arg.Update?.Invoke(size, now);
            }
        }
        else
        {
            //一个个获取
            bool done = true;
            await Parallel.ForEachAsync(arg.Info.files, async (item, token) =>
            {
                var res = await CurseForgeAPI.GetMod(item);
                if (res == null || res.data == null)
                {
                    done = false;
                    return;
                }

                var path = await GetItemPathAsync(arg.Game, res.data);

                list.Add(res.data.MakeModDownloadObj(path.File));
                var modid = res.data.modId.ToString();
                arg.Game.Mods.Remove(modid);
                arg.Game.Mods.Add(modid, res.data.MakeModInfo(path.Name));

                now++;
                arg.Update?.Invoke(size, now);
            });
            if (!done)
            {
                return new MakeDownloadItemsRes();
            }
        }

        return new MakeDownloadItemsRes { List = list, State = true };
    }

    /// <summary>
    /// 构建CurseForge资源所在的文件夹
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="item">CurseForge资源信息</param>
    /// <returns>下载信息</returns>
    public static async Task<ItemPathRes> GetItemPathAsync(this GameSettingObj game, CurseForgeModObj.Data item)
    {
        var item1 = new ItemPathRes()
        {
            File = game.GetModsPath(),
            Name = InstancesPath.Name11,
            FileType = FileType.Mod
        };

        var info1 = await CurseForgeAPI.GetModInfo(item.modId);
        if (info1 != null)
        {
            if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassResourcepack)
                || info1.Data.classId == CurseForgeAPI.ClassResourcepack)
            {
                item1.File = game.GetResourcepacksPath();
                item1.Name = InstancesPath.Name8;
                item1.FileType = FileType.Resourcepack;
            }
            else if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassShaderpack)
                || info1.Data.classId == CurseForgeAPI.ClassShaderpack)
            {
                item1.File = game.GetShaderpacksPath();
                item1.Name = InstancesPath.Name9;
                item1.FileType = FileType.Shaderpack;
            }
            else if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassWorld)
                || info1.Data.classId == CurseForgeAPI.ClassWorld)
            {
                item1.File = game.GetSavesPath();
                item1.Name = InstancesPath.Name12;
                item1.FileType = FileType.World;
            }
        }

        return item1;
    }

    /// <summary>
    /// 获取Mod依赖
    /// </summary>
    /// <param name="data">CurseForge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <returns>模组列表</returns>

    public static async Task<ConcurrentBag<GetCurseForgeModDependenciesRes>>
      GetModDependenciesAsync(CurseForgeModObj.Data data, string mc, Loaders loader)
    {
        var ids = new ConcurrentBag<long>();
        return await GetModDependenciesAsync(data, mc, loader, ids);
    }

    /// <summary>
    /// 获取Mod依赖
    /// </summary>
    /// <param name="data">CurseForge信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <param name="ids">已经获取的列表</param>
    /// <returns>模组列表</returns>
    private static async Task<ConcurrentBag<GetCurseForgeModDependenciesRes>> 
        GetModDependenciesAsync(CurseForgeModObj.Data data, string mc, Loaders loader, ConcurrentBag<long> ids)
    {
        if (data.dependencies == null || data.dependencies.Count == 0)
        {
            return [];
        }
        var list = new ConcurrentBag<GetCurseForgeModDependenciesRes>();
        await Parallel.ForEachAsync(data.dependencies, async (item, cancel) =>
        {
            if (ids.Contains(item.modId))
            {
                return;
            }

            var opt = item.relationType != 2;
            var res1 = await CurseForgeAPI.GetCurseForgeFiles(item.modId.ToString(), mc, loader: loader);
            if (res1 == null || res1.data.Count == 0)
            {
                return;
            }
            var res2 = await CurseForgeAPI.GetModInfo(item.modId);
            if (res2 == null)
            {
                return;
            }

            var mod = new GetCurseForgeModDependenciesRes()
            {
                Name = res2.Data.name,
                ModId = res2.Data.id,
                Opt = !opt,
                List = res1.data
            };
            list.Add(mod);
            ids.Add(item.modId);

            foreach (var item3 in data.dependencies)
            {
                if (ids.Contains(item3.modId))
                {
                    continue;
                }

                foreach (var item5 in await GetModDependenciesAsync(res1.data[0], mc, loader, ids))
                {
                    if (ids.Contains(item5.ModId))
                    {
                        continue;
                    }
                    list.Add(item5);
                    ids.Add(item5.ModId);
                }
            }
        });

        return list;
    }
}
