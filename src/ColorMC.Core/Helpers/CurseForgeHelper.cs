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
    /// <param name="item"></param>
    public static void FixDownloadUrl(this CurseForgeModObj.Data item)
    {
        item.downloadUrl ??= $"{UrlHelper.CurseForgeDownload}files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="obj">游戏实例</param>
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
    /// <param name="data"></param>
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
    /// <param name="authors"></param>
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
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>?> GetCategories(FileType type)
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
    public static async Task<List<string>?> GetGameVersions()
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
    /// <param name="game">游戏实例</param>
    /// <param name="info">整合包信息</param>
    /// <param name="notify">是否通知</param>
    /// <returns>Res安装结果
    /// Game游戏实例</returns>
    public static async Task<MakeDownloadItemsRes> GetModInfo(GetCurseForgeModInfoArg arg)
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
                var path = await GetItemPath(arg.Game, item);
                var item1 = item.MakeModDownloadObj(path.Item1);
                list.Add(item1);
                var modid = item.modId.ToString();
                arg.Game.Mods.Remove(modid);

                if (path.Item3 == FileType.Mod)
                {
                    arg.Game.Mods.Add(modid, item.MakeModInfo(path.Item2));
                }
                else if (path.Item3 == FileType.World)
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

                var path = await GetItemPath(arg.Game, res.data);

                list.Add(res.data.MakeModDownloadObj(path.Item1));
                var modid = res.data.modId.ToString();
                arg.Game.Mods.Remove(modid);
                arg.Game.Mods.Add(modid, res.data.MakeModInfo(path.Item2));

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
    /// <param name="game"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task<(string, string, FileType)> GetItemPath(GameSettingObj game, CurseForgeModObj.Data item)
    {
        var path = game.GetModsPath();
        var path1 = InstancesPath.Name11;
        var type = FileType.Mod;
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
                    type = FileType.Resourcepack;
                }
                else if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassShaderpack)
                    || info1.Data.classId == CurseForgeAPI.ClassShaderpack)
                {
                    path = game.GetShaderpacksPath();
                    path1 = InstancesPath.Name9;
                    type = FileType.Shaderpack;
                }
                else if (info1.Data.categories.Any(item => item.classId == CurseForgeAPI.ClassWorld)
                    || info1.Data.classId == CurseForgeAPI.ClassWorld)
                {
                    path = game.GetSavesPath();
                    path1 = InstancesPath.Name12;
                    type = FileType.World;
                }
            }
        }

        return (path, path1, type);
    }

    /// <summary>
    /// 获取Mod依赖
    /// </summary>
    /// <param name="data">Mod</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <returns></returns>

    public static async Task<ConcurrentBag<GetCurseForgeModDependenciesRes>>
      GetModDependencies(CurseForgeModObj.Data data, string mc, Loaders loader, bool dep)
    {
        var ids = new ConcurrentBag<long>();
        return await GetModDependencies(data, mc, loader, dep, ids);
    }

    public static async Task<ConcurrentBag<GetCurseForgeModDependenciesRes>> GetModDependencies(CurseForgeModObj.Data data, string mc, Loaders loader, bool dep, ConcurrentBag<long> ids)
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

            var opt = item.relationType != 2 && dep;
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

                foreach (var item5 in await GetModDependencies(res1.data[0], mc, loader, opt, ids))
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
