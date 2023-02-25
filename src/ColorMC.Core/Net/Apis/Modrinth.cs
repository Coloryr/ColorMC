using ColorMC.Core.Objs.Modrinth;
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

    public static async Task<ModrinthSearchObj?> Search(string version, string query, int sortOrder, int offset, int limit, string categoryId, string type2, string? type3)
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
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(filter, version, sortOrder, page * pagesize,
            pagesize, categoryId, ClassModPack, null);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModList(string version = "",
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(filter, version, sortOrder, page * pagesize,
            pagesize, categoryId, ClassMod, null);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetResourcepackList(string version = "",
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(filter, version, sortOrder, page * pagesize,
            pagesize, categoryId, ClassResourcepack, null);
    }

    /// <summary>
    /// 获取光影包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetShaderpackList(string version = "",
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(filter, version, sortOrder, page * pagesize,
            pagesize, categoryId, ClassShaderpack, null);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetDataPackList(string version = "",
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        return Search(filter, version, sortOrder, page * pagesize,
            pagesize, categoryId, ClassMod, CategoriesDataPack);
    }

    public static async Task<ModrinthProjectObj?> Project(string id)
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}project/{id}");
            return JsonConvert.DeserializeObject<ModrinthProjectObj>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    public static async Task<List<ModrinthVersionObj>?> Version(string id)
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}project/{id}/version");
            return JsonConvert.DeserializeObject<List<ModrinthVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    public static async Task<List<ModrinthGameVersionObj>?> GetGameVersion()
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
}
