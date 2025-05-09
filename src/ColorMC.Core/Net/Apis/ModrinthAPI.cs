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
    public const string ClassModPack = "modpack";
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
    private static async Task<ModrinthSearchObj?> Search(string version, string query, int sortOrder,
        int offset, int limit, string categoryId, string type2, string? type3)
    {
        try
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
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthSearchObj);
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
            string url = $"{UrlHelper.Modrinth}project/{id}/version/{version}";
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthVersionObj);
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
    public static async Task<ModrinthProjectObj?> GetProject(string id)
    {
        try
        {
            string url = $"{UrlHelper.Modrinth}project/{id}";
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthProjectObj);
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
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthVersionObj);
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
            string url = $"{UrlHelper.Modrinth}tag/game_version";
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthGameVersionObj);
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
            string url = $"{UrlHelper.Modrinth}tag/category";
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ListModrinthCategoriesObj);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error5"), e);
            return null;
        }
    }

    /// <summary>
    /// 从文件Sha1获取项目
    /// </summary>
    /// <param name="sha1"></param>
    /// <returns></returns>
    public static async Task<ModrinthVersionObj?> GetVersionFromSha1(string sha1)
    {
        try
        {
            string url = $"{UrlHelper.Modrinth}version_file/{sha1}";
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.ModrinthVersionObj);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Modrinth.Error5"), e);
            return null;
        }
    }
}
