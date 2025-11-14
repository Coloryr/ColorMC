using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.GuiHandel;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using SharpCompress.Archives.Zip;

namespace ColorMC.Core.Helpers;

/// <summary>
/// CueseForge处理
/// </summary>
public static class CurseForgeHelper
{
    /// <summary>
    /// 分类存储
    /// </summary>
    private static CurseForgeCategoriesObj? s_categories;
    /// <summary>
    /// 支持的游戏版本
    /// </summary>
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
    /// 创建CurseForge下载项目
    /// </summary>
    /// <param name="data"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileItemObj MakeDownloadObj(this CurseForgeModObj.CurseForgeDataObj data, string path)
    {
        data.FixDownloadUrl();

        return new FileItemObj()
        {
            Url = data.DownloadUrl,
            Name = data.DisplayName,
            Local = Path.Combine(path, data.FileName),
            Sha1 = data.Hashes.Where(a => a.Algo == 1)
                    .Select(a => a.Value).FirstOrDefault()
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static FileItemObj MakeDownloadObj(this CurseForgeModObj.CurseForgeDataObj data, GameSettingObj obj)
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

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="obj">游戏实例</param>
    /// <param name="info">下载项目</param>
    /// <returns></returns>
    public static FileItemObj MakeDownloadObj(this CurseForgeModObj.CurseForgeDataObj data, GameSettingObj obj, ItemPathRes info)
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
            var list6 = await CurseForgeAPI.GetCategoriesAsync();
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
                        FileType.Save => CurseForgeAPI.ClassWorld,
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

        var list2 = await CurseForgeAPI.GetCurseForgeVersionAsync();
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
    public static async Task<MakeDownloadItemsRes> GetModPackInfoAsync(GameSettingObj game, CurseForgePackObj data, ICreateInstanceGui? gui)
    {
        var size = data.Files.Count;
        var now = 0;
        var list = new ConcurrentBag<FileItemObj>();

        //获取模组下载信息
        var mods = new ConcurrentDictionary<string, ModInfoObj>(game.Mods);

        //下载信息处理
        async Task BuildItem(CurseForgeModObj.CurseForgeDataObj item)
        {
            var path = await GetItemPathAsync(game, item);
            var item1 = item.MakeDownloadObj(game, path);
            list.Add(item1);
            var modid = item.ModId.ToString();
            mods.TryRemove(modid, out _);

            if (path.FileType == FileType.Save)
            {
                item1.Later = (test) =>
                {
                    game.AddWorldZipAsync(item1.Local, test).Wait();
                };
            }
            else
            {
                mods.TryAdd(modid, item.MakeModInfo(path.Path));
            }

            now++;
            gui?.StateUpdate(size, now);
        }

        //一次性获取
        var res = await CurseForgeAPI.GetFilesAsync(data.Files);
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
            await Parallel.ForEachAsync(data.Files, async (item, token) =>
            {
                var res = await CurseForgeAPI.GetModAsync(item);
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

        game.Mods.Clear();
        foreach (var item in mods)
        {
            game.Mods.Add(item.Key, item.Value);
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
        var info1 = await CurseForgeAPI.GetModInfoAsync(item.ModId);
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
                item1.FileType = FileType.Save;
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
            var res1 = await CurseForgeAPI.GetCurseForgeFilesAsync(item.ModId.ToString(), mc, loader: loader);
            if (res1 == null || res1.Data.Count == 0)
            {
                return;
            }
            var res2 = await CurseForgeAPI.GetModInfoAsync(item.ModId);
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

    /// <summary>
    /// 升级CurseForge整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>是否升级完成</returns>
    private static async Task<bool> UpgradeCurseForgeModPackAsync(GameSettingObj game, string zip, ICreateInstanceGui? gui)
    {
        using var stream3 = PathHelper.OpenRead(zip);
        if (stream3 == null)
        {
            return false;
        }
        using var zFile = ZipArchive.Open(stream3);
        CurseForgePackObj? info = null;
        //获取主信息
        if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameManifestFile) is { } ent)
        {
            using var stream = ent.OpenEntryStream();
            info = JsonUtils.ToObj(stream, JsonType.CurseForgePackObj);
        }
        else
        {
            return false;
        }
        if (info == null)
        {
            return false;
        }

        gui?.ModPackState(CoreRunState.Init);

        //获取版本数据
        foreach (var item in info.Minecraft.ModLoaders)
        {
            if (item.Id.StartsWith(Names.NameForgeKey))
            {
                game.Loader = Loaders.Forge;
                game.LoaderVersion = item.Id.Replace(Names.NameForgeKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameNeoForgeKey))
            {
                game.Loader = Loaders.NeoForge;
                game.LoaderVersion = item.Id.Replace(Names.NameNeoForgeKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameFabricKey))
            {
                game.Loader = Loaders.Fabric;
                game.LoaderVersion = item.Id.Replace(Names.NameFabricKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameQuiltKey))
            {
                game.Loader = Loaders.Quilt;
                game.LoaderVersion = item.Id.Replace(Names.NameQuiltKey + "-", "");
            }
        }

        //解压文件
        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            if (e.Key!.StartsWith(info.Overrides + "/"))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(game.GetGamePath() + e.Key[info.Overrides.Length..]);
                file = PathHelper.ReplacePathName(file);
                using var stream1 = PathHelper.OpenWrite(file);
                await stream.CopyToAsync(stream1);
            }
            else
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(game.GetBasePath() + "/" + e.Key);
                file = PathHelper.ReplacePathName(file);
                using var stream1 = PathHelper.OpenWrite(file);
                await stream.CopyToAsync(stream1);
            }
        }

        CurseForgePackObj? info3 = null;
        var json = Path.Combine(game.GetBasePath(), Names.NameManifestFile);
        if (File.Exists(json))
        {
            try
            {
                using var stream = PathHelper.OpenRead(json);
                info3 = JsonUtils.ToObj(stream, JsonType.CurseForgePackObj);
            }
            catch
            {

            }
        }

        var path = game.GetGamePath();

        var list1 = new List<FileItemObj>();

        int b = 0;

        //筛选需要升级的资源
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
                if (game.Mods.Remove(item.ProjectID.ToString(), out var mod))
                {
                    PathHelper.Delete(Path.Combine(path, mod.Path, mod.File));
                }
            }

            if (addlist.Count > 0)
            {
                gui?.StateUpdate(addlist.Count, 0);
            }

            foreach (var item in addlist)
            {
                var res = await CurseForgeAPI.GetModAsync(item);
                if (res == null || res.Data == null)
                {
                    return false;
                }

                var path1 = await CurseForgeHelper.GetItemPathAsync(game, res.Data);
                var modid = res.Data.ModId.ToString();
                var item1 = res.Data.MakeDownloadObj(game, path1);
                list1.Add(item1);

                game.Mods.Remove(modid, out _);
                game.Mods.TryAdd(modid, res.Data.MakeModInfo(path1.Path));

                gui?.StateUpdate(addlist.Count, ++b);
            }
        }
        else
        {
            //没有筛选信息通过sha1筛选
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            var obj1 = game.CopyObj();
            obj1.Mods.Clear();

            var list = await CurseForgeHelper.GetModPackInfoAsync(obj1, info, gui);
            if (!list.State)
            {
                return false;
            }

            ModInfoObj[] temp1 = [.. game.Mods.Values];
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
                var local = Path.Combine(path, item.Path, item.File);
                if (File.Exists(local))
                {
                    PathHelper.Delete(local);
                }
                game.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                list1.Add(list.List!.First(a => a.Sha1 == item.Sha1));
                game.Mods.Add(item.ModId, item);
            }
        }

        await DownloadManager.StartAsync(list1);

        return true;
    }

    /// <summary>
    /// 升级CurseForge整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>是否升级完成</returns>
    public static async Task<bool> UpgradeModPackAsync(GameSettingObj game, CurseForgeModObj.CurseForgeDataObj data, ICreateInstanceGui? gui)
    {
        data.FixDownloadUrl();

        var item = new FileItemObj()
        {
            Url = data.DownloadUrl,
            Name = data.FileName,
            Local = Path.Combine(DownloadManager.DownloadDir, data.FileName),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await UpgradeCurseForgeModPackAsync(game, item.Local, gui);
        if (res)
        {
            game.PID = data.ModId.ToString();
            game.FID = data.Id.ToString();
            game.Save();
            game.SaveModInfo();
        }

        return res;
    }
}
