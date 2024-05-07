using System.Collections.Concurrent;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Modrinth网络请求
/// </summary>
public static class ModrinthAPI
{
    public const string ClassModPack = "modpack";
    public const string ClassMod = "mod";
    public const string ClassResourcepack = "resourcepack";
    public const string ClassShaderpack = "shader";

    public const string CategoriesDataPack = "datapack";

    /// <summary>
    /// 搜索
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <param name="query">关键字</param>
    /// <param name="sortOrder">排序方式</param>
    /// <param name="offset">偏移</param>
    /// <param name="limit">限制</param>
    /// <param name="categoryId">类型</param>
    /// <param name="type2">类型1</param>
    /// <param name="type3">类型2</param>
    /// <returns></returns>
    private static async Task<ModrinthSearchObj?> Search(string version, string query, int sortOrder, int offset, int limit, string categoryId, string type2, string? type3)
    {
        try
        {
            var list = new List<MFacetsObj>
            {
                MFacetsObj.BuildProjectType(new() { type2 })
            };

            if (!string.IsNullOrWhiteSpace(version))
            {
                list.Add(MFacetsObj.BuildVersions(new() { version }));
            }

            var list1 = new List<string>();
            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                list1.Add(categoryId);
            }
            if (!string.IsNullOrWhiteSpace(type3))
            {
                list1.Add(type3);
            }
            if (list1.Count != 0)
            {
                list.Add(MFacetsObj.BuildCategories(list1));
            }

            var type = sortOrder switch
            {
                1 => MSortingObj.Downloads,
                2 => MSortingObj.Follows,
                3 => MSortingObj.Newest,
                4 => MSortingObj.Updated,
                _ => MSortingObj.Relevance
            };

            var url = $"{UrlHelper.Modrinth}search?query={query}&index={type.Data}&offset={offset}" +
                $"&limit={limit}&facets={MFacetsObj.Build(list)}";
            var res = await BaseClient.DownloadClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ModrinthSearchObj>(res);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModPackList(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassModPack, null);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModList(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "", Loaders loader = Loaders.Normal)
    {
        return Search(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassMod, loader is Loaders.Normal or Loaders.Custom ? "" :
            loader.GetName().ToLower());
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetResourcepackList(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassResourcepack, null);
    }

    /// <summary>
    /// 获取光影包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetShaderpackList(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassShaderpack, null);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetDataPackList(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassMod, CategoriesDataPack);
    }

    /// <summary>
    /// 获取指定版本号的内容
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="version">版本ID</param>
    public static async Task<ModrinthVersionObj?> GetVersion(string id, string version)
    {
        try
        {
            var res = await BaseClient.GetStringAsync($"{UrlHelper.Modrinth}project/{id}/version/{version}");
            if (res.Item1 == false)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<ModrinthVersionObj>(res.Item2!);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取指定项目的内容
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="version">版本ID</param>
    public static async Task<ModrinthVersionObj?> GetProject(string id)
    {
        try
        {
            var res = await BaseClient.GetStringAsync($"{UrlHelper.Modrinth}project/{id}");
            if (res.Item1 == false)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<ModrinthVersionObj>(res.Item2!);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器类型</param>
    /// <returns></returns>
    public static async Task<List<ModrinthVersionObj>?> GetFileVersions(string id, string? mc, Loaders loader)
    {
        try
        {
            string url;
            if (string.IsNullOrWhiteSpace(mc))
            {
                url = $"{UrlHelper.Modrinth}project/{id}/version?" +
                (loader != Loaders.Normal ? $"&loaders=[\"{loader.GetName().ToLower()}\"]" : "");
            }
            else
            {
                url = $"{UrlHelper.Modrinth}project/{id}/version?game_versions=[" +
                $"{(string.IsNullOrWhiteSpace(mc) ? "" : ('"' + mc + '"'))}]"
                + (loader != Loaders.Normal ? $"&loaders=[\"{loader.GetName().ToLower()}\"]" : "");
            }
            var res = await BaseClient.DownloadClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<ModrinthVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error3"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public static async Task<List<ModrinthGameVersionObj>?> GetGameVersions()
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{UrlHelper.Modrinth}tag/game_version");
            return JsonConvert.DeserializeObject<List<ModrinthGameVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error4"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取所有类型
    /// </summary>
    /// <returns></returns>
    public static async Task<List<ModrinthCategoriesObj>?> GetCategories()
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{UrlHelper.Modrinth}tag/category");
            return JsonConvert.DeserializeObject<List<ModrinthCategoriesObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error5"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取Mod依赖
    /// </summary>
    /// <param name="data">mod</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <returns></returns>
    public static async Task<ConcurrentBag<((string Name, string ModId) Info,
        List<ModrinthVersionObj> List)>>
        GetModDependencies(ModrinthVersionObj data, string mc, Loaders loader)
    {
        var list = new ConcurrentBag<((string Name, string ModId) Info, List<ModrinthVersionObj> List)>();
        if (data.dependencies == null || data.dependencies.Count == 0)
        {
            return list;
        }
        await Parallel.ForEachAsync(data.dependencies, async (item, cancel) =>
        {
            ModrinthVersionObj? res = null;
            if (item.version_id == null)
            {
                var res1 = await GetFileVersions(item.project_id, mc, loader);
                res = res1?[0];
            }
            else
            {
                res = await GetVersion(item.project_id, item.version_id);
            }

            if (res == null)
                return;

            list.Add(((res.name, res.project_id), new() { res }));

            foreach (var item3 in await GetModDependencies(res, mc, loader))
            {
                list.Add(item3);
            }
        });

        return list;
    }
}
