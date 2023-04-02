using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class Modrinth
{
    public const string Url = "https://api.modrinth.com/v2/";

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

            var url = $"{Url}search?query={query}&index={type.Data}&offset={offset}" +
                $"&limit={limit}&facets={MFacetsObj.Build(list)}";
            var res = await BaseClient.DownloadClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ModrinthSearchObj>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
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
            pagesize, categoryId, ClassMod, loader == Loaders.Normal ? "" :
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

    ///// <summary>
    ///// 获取
    ///// </summary>
    ///// <param name = "id" ></ param >
    ///// < returns ></ returns >
    //public static async Task<ModrinthProjectObj?> Project(string id)
    //{
    //    try
    //    {
    //        var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}project/{id}");
    //        return JsonConvert.DeserializeObject<ModrinthProjectObj>(res);
    //    }
    //    catch (Exception e)
    //    {
    //        Logs.Error("get fail", e);
    //        return null;
    //    }
    //}


    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器类型</param>
    /// <returns></returns>
    public static async Task<List<ModrinthVersionObj>?> Version(string id, string? mc, Loaders loader)
    {
        try
        {
            string url = $"{Url}project/{id}/version?game_versions=[" +
                $"{(string.IsNullOrWhiteSpace(mc) ? "" : ('"' + mc + '"'))}]"
                + (loader != Loaders.Normal ? $"&loaders=[\"{loader.GetName().ToLower()}\"]" : "");
            var res = await BaseClient.DownloadClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<ModrinthVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    private static List<string>? ModrinthGameVersions;

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public static async Task<List<ModrinthGameVersionObj>?> GetGameVersions()
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}tag/game_version");
            return JsonConvert.DeserializeObject<List<ModrinthGameVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
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
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}tag/category");
            return JsonConvert.DeserializeObject<List<ModrinthCategoriesObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public static async Task<List<string>?> GetGameVersion()
    {
        if (ModrinthGameVersions != null)
        {
            return ModrinthGameVersions;
        }

        var list = await GetGameVersions();
        if (list == null)
        {
            return null;
        }

        var list1 = new List<string>
        {
            ""
        };

        list1.AddRange(from item in list select item.version);

        ModrinthGameVersions = list1;

        return list1;
    }
}
