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
    public static void FixDownloadUrl(this CurseForgeModObj.CurseForgeDataObj item)
    {
        item.DownloadUrl ??= $"{UrlHelper.CurseForgeDownload}files/{item.Id / 1000}/{item.Id % 1000}/{item.FileName}";
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="path">下载路径</param>
    /// <returns>下载项目</returns>
    public static FileItemObj MakeModDownloadObj(this CurseForgeModObj.CurseForgeDataObj data, GameSettingObj obj)
    {
        data.FixDownloadUrl();

        return new FileItemObj()
        {
            Url = data.DownloadUrl,
            Name = data.DisplayName,
            Local = Path.Combine(obj.GetModsPath(), data.FileName),
            Sha1 = data.Hashes.Where(a => a.Algo == 1)
                    .Select(a => a.Value).FirstOrDefault()
        };
    }

    public static FileItemObj MakeModDownloadObj(this CurseForgeModObj.CurseForgeDataObj data, GameSettingObj obj, ItemPathRes info)
    {
        data.FixDownloadUrl();

        return new FileItemObj()
        {
            Url = data.DownloadUrl,
            Name = data.DisplayName,
            Local = Path.Combine(obj.GetGamePath(), info.File, data.FileName),
            Sha1 = data.Hashes.Where(a => a.Algo == 1)
                    .Select(a => a.Value).FirstOrDefault()
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="path">路径名字</param>
    /// <returns>Mod信息</returns>
    public static ModInfoObj MakeModInfo(this CurseForgeModObj.CurseForgeDataObj data, string path)
    {
        data.FixDownloadUrl();

        return new ModInfoObj()
        {
            Path = path,
            Name = data.DisplayName,
            File = data.FileName,
            Sha1 = data.Hashes.Where(a => a.Algo == 1)
                    .Select(a => a.Value).FirstOrDefault()!,
            ModId = data.ModId.ToString(),
            FileId = data.Id.ToString(),
            Url = data.DownloadUrl
        };
    }

    /// <summary>
    /// 获取作者名字
    /// </summary>
    /// <param name="authors">作者列表</param>
    /// <returns>作者名字</returns>
    public static string GetString(this List<CurseForgeListObj.CurseForgeListDataObj.AuthorsObj> authors)
    {
        if (authors == null || authors.Count == 0)
        {
            return "";
        }
        var builder = new StringBuilder();
        foreach (var item in authors)
        {
            builder.Append(item.Name).Append(',');
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

        var list7 = from item2 in s_categories.Data
                    where item2.ClassId == type switch
                    {
                        FileType.Mod => CurseForgeAPI.ClassMod,
                        FileType.World => CurseForgeAPI.ClassWorld,
                        FileType.Resourcepack => CurseForgeAPI.ClassResourcepack,
                        FileType.Shaderpack => CurseForgeAPI.ClassShaderpack,
                        _ => CurseForgeAPI.ClassModPack
                    }
                    orderby item2.Name descending
                    select (item2.Name, item2.Id);

        return list7.ToDictionary(a => a.Id.ToString(), a => a.Name);
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

        list.Data.RemoveAll(a =>
        {
            return !a.Name.StartsWith("Minecraft ");
        });

        var list111 = new List<CurseForgeVersionTypeObj.CurseForgeVersionTypeDataObj>();
        list111.AddRange(from item in list.Data
                         where item.Id > 17
                         orderby item.Id descending
                         select item);
        list111.AddRange(from item in list.Data
                         where item.Id < 18
                         orderby item.Id ascending
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
            var list4 = from item1 in list2.Data
                        where item1.Type == item.Id
                        select item1.Versions;
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
    /// 获取CurseForge整合包模组信息
    /// </summary>
    /// <param name="arg">获取信息</param>
    /// <returns>项目信息</returns>
    public static async Task<MakeDownloadItemsRes> GetModInfoAsync(GetCurseForgeModInfoArg arg)
    {
        var size = arg.Info.Files.Count;
        var now = 0;
        var list = new ConcurrentBag<FileItemObj>();

        //获取模组下载信息
        var mods = new ConcurrentDictionary<string, ModInfoObj>(arg.Game.Mods);

        //下载信息处理
        async Task BuildItem(CurseForgeModObj.CurseForgeDataObj item)
        {
            var path = await GetItemPathAsync(arg.Game, item);
            var item1 = item.MakeModDownloadObj(arg.Game, path);
            list.Add(item1);
            var modid = item.ModId.ToString();
            mods.TryRemove(modid, out _);

            if (path.FileType == FileType.World)
            {
                item1.Later = (test) =>
                {
                    arg.Game.AddWorldZipAsync(item1.Local, test).Wait();
                };
            }
            else
            {
                mods.TryAdd(modid, item.MakeModInfo(path.Path));
            }

            now++;
            arg.Update?.Invoke(size, now);
        }

        //一次性获取
        var res = await CurseForgeAPI.GetFiles(arg.Info.Files);
        if (res != null)
        {
            var res1 = res.Distinct(CurseForgeDataComparer.Instance);
#if false
            await Parallel.ForEachAsync(res1, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 1
            }, async (item, cancel) =>
#else
            await Parallel.ForEachAsync(res1, async (item, cancel) =>
#endif
            {
                await BuildItem(item);
            });
        }
        else
        {
            //逐一获取
            bool done = true;
            await Parallel.ForEachAsync(arg.Info.Files, async (item, token) =>
            {
                var res = await CurseForgeAPI.GetMod(item);
                if (res == null || res.Data == null)
                {
                    done = false;
                    return;
                }

                await BuildItem(res.Data);
            });
            if (!done)
            {
                return new MakeDownloadItemsRes();
            }
        }

        arg.Game.Mods.Clear();
        foreach (var item in mods)
        {
            arg.Game.Mods.Add(item.Key, item.Value);
        }

        return new MakeDownloadItemsRes { List = list, State = true };
    }

    /// <summary>
    /// 构建CurseForge资源所在的文件夹
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="item">CurseForge资源信息</param>
    /// <returns>下载信息</returns>
    public static async Task<ItemPathRes> GetItemPathAsync(this GameSettingObj game, CurseForgeModObj.CurseForgeDataObj item)
    {
        var item1 = new ItemPathRes()
        {
            File = game.GetModsPath(),
            Path = Names.NameGameModDir,
            FileType = FileType.Mod
        };
        if (item.FileName.EndsWith(Names.NameJarExt))
        {
            return item1;
        }

        //将各类文件放在正确的文件夹
        var info1 = await CurseForgeAPI.GetModInfo(item.ModId);
        if (info1 != null)
        {
            if (info1.Data.Categories.Any(item => item.ClassId == CurseForgeAPI.ClassResourcepack)
                || info1.Data.ClassId == CurseForgeAPI.ClassResourcepack)
            {
                item1.File = game.GetResourcepacksPath();
                item1.Path = Names.NameGameResourcepackDir;
                item1.FileType = FileType.Resourcepack;
            }
            else if (info1.Data.Categories.Any(item => item.ClassId == CurseForgeAPI.ClassShaderpack)
                || info1.Data.ClassId == CurseForgeAPI.ClassShaderpack)
            {
                item1.File = game.GetShaderpacksPath();
                item1.Path = Names.NameGameShaderpackDir;
                item1.FileType = FileType.Shaderpack;
            }
            else if (info1.Data.Categories.Any(item => item.ClassId == CurseForgeAPI.ClassWorld)
                || info1.Data.ClassId == CurseForgeAPI.ClassWorld)
            {
                item1.File = game.GetSavesPath();
                item1.Path = Names.NameGameSavesDir;
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
      GetModDependenciesAsync(CurseForgeModObj.CurseForgeDataObj data, string mc, Loaders loader)
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
        GetModDependenciesAsync(CurseForgeModObj.CurseForgeDataObj data, string mc, Loaders loader, ConcurrentBag<long> ids)
    {
        if (data.Dependencies == null || data.Dependencies.Count == 0)
        {
            return [];
        }
        var list = new ConcurrentBag<GetCurseForgeModDependenciesRes>();
        await Parallel.ForEachAsync(data.Dependencies, async (item, cancel) =>
        {
            if (ids.Contains(item.ModId))
            {
                return;
            }

            var opt = item.RelationType != 2;
            var res1 = await CurseForgeAPI.GetCurseForgeFiles(item.ModId.ToString(), mc, loader: loader);
            if (res1 == null || res1.Data.Count == 0)
            {
                return;
            }
            var res2 = await CurseForgeAPI.GetModInfo(item.ModId);
            if (res2 == null)
            {
                return;
            }

            var mod = new GetCurseForgeModDependenciesRes()
            {
                Name = res2.Data.Name,
                ModId = res2.Data.Id,
                Opt = !opt,
                List = res1.Data
            };
            list.Add(mod);
            ids.Add(item.ModId);

            //获取依赖的依赖
            foreach (var item3 in data.Dependencies)
            {
                if (ids.Contains(item3.ModId))
                {
                    continue;
                }

                foreach (var item5 in await GetModDependenciesAsync(res1.Data[0], mc, loader, ids))
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
