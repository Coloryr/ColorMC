using System.Net;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Modrinth网络请求
/// </summary>
public static class ModrinthAPI
{
    public const string ClassModPack = "Modpack";
    public const string ClassMod = "mod";
    public const string ClassResourcepack = "resourcepack";
    public const string ClassShaderpack = "shader";

    public const string CategoriesDataPack = "datapack";

    private static async Task<Stream?> SendAsync(string url)
    {
        var data = await CoreHttpClient.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await data.Content.ReadAsStreamAsync();
    }

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
    private static async Task<ModrinthSearchObj?> SearchAsync(string version, string query, int sortOrder,
        int offset, int limit, string categoryId, string type2, string? type3)
    {
        var list = new List<MFacetsObj>
        {
            ModrinthHelper.BuildProjectType([type2])
        };

        if (!string.IsNullOrWhiteSpace(version))
        {
            list.Add(ModrinthHelper.BuildVersions([version]));
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
            list.Add(ModrinthHelper.BuildCategories(list1));
        }

        var type = sortOrder switch
        {
            1 => ModrinthHelper.Downloads,
            2 => ModrinthHelper.Follows,
            3 => ModrinthHelper.Newest,
            4 => ModrinthHelper.Updated,
            _ => ModrinthHelper.Relevance
        };

        var url = $"{UrlHelper.Modrinth}search?query={query}&index={type.Data}&offset={offset}" +
            $"&limit={limit}&facets={ModrinthHelper.BuildFacets(list)}";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthSearchObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModPackListAsync(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return SearchAsync(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassModPack, null);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModListAsync(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "", Loaders loader = Loaders.Normal)
    {
        return SearchAsync(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassMod, loader is Loaders.Normal or Loaders.Custom ? "" :
            loader.ToString().ToLower());
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetResourcepackListAsync(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return SearchAsync(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassResourcepack, null);
    }

    /// <summary>
    /// 获取光影包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetShaderpackListAsync(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return SearchAsync(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassShaderpack, null);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetDataPackListAsync(string version = "",
        int page = 0, string filter = "", int pagesize = 20, int sortOrder = 0,
        string categoryId = "")
    {
        return SearchAsync(version, filter, sortOrder, page * pagesize,
            pagesize, categoryId, ClassMod, CategoriesDataPack);
    }

    /// <summary>
    /// 获取指定版本号的内容
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="version">版本ID</param>
    public static async Task<ModrinthVersionObj?> GetVersionAsync(string id, string version)
    {
        string url = $"{UrlHelper.Modrinth}project/{id}/version/{version}";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthVersionObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取团队列表
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public static async Task<List<ModrinthTeamObj>?> GetTeamAsync(string pid)
    {
        string url = $"{UrlHelper.Modrinth}project/{pid}/members";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthTeamObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取指定项目的内容
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="version">版本ID</param>
    public static async Task<ModrinthProjectObj?> GetProjectAsync(string id)
    {
        string url = $"{UrlHelper.Modrinth}project/{id}";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthProjectObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
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
    public static async Task<List<ModrinthVersionObj>?> GetFileVersionsAsync(string id, string? mc, Loaders loader)
    {
        string url;
        if (string.IsNullOrWhiteSpace(mc))
        {
            url = $"{UrlHelper.Modrinth}project/{id}/version?" +
            (loader != Loaders.Normal ? $"&loaders=[\"{loader.ToString().ToLower()}\"]" : "");
        }
        else
        {
            url = $"{UrlHelper.Modrinth}project/{id}/version?game_versions=[" +
            $"{(string.IsNullOrWhiteSpace(mc) ? "" : ('"' + mc + '"'))}]"
            + (loader != Loaders.Normal ? $"&loaders=[\"{loader.ToString().ToLower()}\"]" : "");
        }
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthVersionObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public static async Task<List<ModrinthGameVersionObj>?> GetGameVersionsAsync()
    {
        string url = $"{UrlHelper.Modrinth}tag/game_version";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthGameVersionObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取所有类型
    /// </summary>
    /// <returns></returns>
    public static async Task<List<ModrinthCategoriesObj>?> GetCategoriesAsync()
    {
        string url = $"{UrlHelper.Modrinth}tag/category";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthCategoriesObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 从文件Sha1获取项目
    /// </summary>
    /// <param name="sha1"></param>
    /// <returns></returns>
    public static async Task<ModrinthVersionObj?> GetVersionFromSha1Async(string sha1)
    {
        string url = $"{UrlHelper.Modrinth}version_file/{sha1}";
        try
        {
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthVersionObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }
}
