using System.Net;
using System.Net.Http.Headers;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// CurseForge网络请求
/// </summary>
public static class CurseForgeAPI
{
    public const int GameID = 432;

    public const int ClassModPack = 4471;
    public const int ClassMod = 6;
    public const int ClassWorld = 17;
    public const int ClassResourcepack = 12;
    public const int ClassShaderpack = 6552;

    public const int CategoryIdDataPacks = 5193;

    /// <summary>
    /// 添加CF KEY并发送请求
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task<Stream?> SendAsync(HttpRequestMessage req, CancellationToken token = default)
    {
        req.Headers.Add("x-api-key", ColorMCCore.CoreArg.CurseForgeKey 
            ?? throw new Exception("CurseForge key is not set"));
        var data = await CoreHttpClient.SendAsync(req, token);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            data.Dispose();
            return null;
        }
        return await data.Content.ReadAsStreamAsync(token);
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="classid">cid</param>
    /// <param name="version">版本</param>
    /// <param name="page">页数</param>
    /// <param name="sortField">排序</param>
    /// <param name="filter">过滤</param>
    /// <param name="pagesize">页大小</param>
    /// <param name="sortOrder">排序</param>
    /// <returns></returns>
    private static async Task<CurseForgeListObj?> GetListAsync(int classid, string version, int page,
        int sortField, string filter, int pagesize, int sortOrder, string categoryId,
        int modLoaderType)
    {
        string temp = $"{UrlHelper.CurseForge}mods/search?gameId={GameID}&classId={classid}&"
                + $"gameVersion={version}&index={page * pagesize}&sortField={sortField}&"
                + $"searchFilter={filter}&pageSize={pagesize}&sortOrder={sortOrder}&"
                + $"categoryId={categoryId}";
        if (modLoaderType != 0)
        {
            temp += $"&modLoaderType={modLoaderType}";
        }
        try
        {
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeListObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    public static Task<CurseForgeListObj?> GetModPackListAsync(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "")
    {
        return GetListAsync(ClassModPack, version, page, sortField, filter, pagesize, sortOrder, categoryId, 0);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<CurseForgeListObj?> GetModListAsync(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "", Loaders loader = Loaders.Normal)
    {
        return GetListAsync(ClassMod, version, page, sortField, filter, pagesize, sortOrder, categoryId, Loader(loader));
    }

    /// <summary>
    /// 加载器转类型
    /// </summary>
    /// <param name="loader">加载器</param>
    /// <returns>类型</returns>
    private static int Loader(Loaders loader)
    {
        return loader switch
        {
            Loaders.Forge => 1,
            Loaders.NeoForge => 6,
            Loaders.Fabric => 4,
            Loaders.Quilt => 5,
            _ => 0
        };
    }

    /// <summary>
    /// 获取世界列表
    /// </summary>
    public static Task<CurseForgeListObj?> GetWorldListAsync(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "")
    {
        return GetListAsync(ClassWorld, version, page, sortField, filter, pagesize, sortOrder, categoryId, 0);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<CurseForgeListObj?> GetResourcepackListAsync(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "")
    {
        return GetListAsync(ClassResourcepack, version, page, sortField, filter, pagesize, sortOrder, categoryId, 0);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<CurseForgeListObj?> GetDataPacksList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1)
    {
        return GetListAsync(ClassResourcepack, version, page, sortField, filter, pagesize, sortOrder, CategoryIdDataPacks.ToString(), 0);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<CurseForgeListObj?> GetShadersListAsync(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1)
    {
        return GetListAsync(ClassShaderpack, version, page, sortField, filter, pagesize, sortOrder, "", 0);
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeModObj?> GetModAsync(CurseForgePackObj.FilesObj obj, CancellationToken token = default)
    {
        string temp = $"{UrlHelper.CurseForge}mods/{obj.ProjectID}/files/{obj.FileID}";
        try
        {
            using var data = await SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            }, token);
            return JsonUtils.ToObj(data, JsonType.CurseForgeModObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 查询Mod信息
    /// </summary>
    public static async Task<List<CurseForgeModObj.CurseForgeDataObj>?> 
        GetFilesAsync(List<CurseForgePackObj.FilesObj> obj, CancellationToken token = default)
    {
        string temp = $"{UrlHelper.CurseForge}mods/files";
        try
        {
            var arg1 = new CurseForgeGetFilesObj { FileIds = [] };
            obj.ForEach(a => arg1.FileIds.Add(a.FileID));
            using var data = await SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
                Content = new StringContent(JsonUtils.ToString(arg1, JsonType.CurseForgeGetFilesObj), MediaTypeHeaderValue.Parse("application/json"))
            }, token);
            return JsonUtils.ToObj(data, JsonType.CurseForgeGetFilesResObj)?.Data;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取分类信息
    /// </summary>
    /// <returns>信息</returns>
    public static async Task<CurseForgeCategoriesObj?> GetCategoriesAsync()
    {
        string temp = $"{UrlHelper.CurseForge}categories?gameId={GameID}";
        try
        {
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeCategoriesObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取版本信息
    /// </summary>
    public static async Task<CurseForgeVersionObj?> GetCurseForgeVersionAsync()
    {
        string temp = $"{UrlHelper.CurseForge}games/{GameID}/versions";
        try
        {
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeVersionObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取版本类型
    /// </summary>
    public static async Task<CurseForgeVersionTypeObj?> GetCurseForgeVersionType()
    {
        string temp = $"{UrlHelper.CurseForge}games/{GameID}/version-types";
        try
        {
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeVersionTypeObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static Task<CurseForgeObj?> GetModInfoAsync(long id)
    {
        return GetModInfoAsync(id.ToString());
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeObj?> GetModInfoAsync(string id)
    {
        string temp = $"{UrlHelper.CurseForge}mods/{id}";
        try
        {
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeListObj?> GetModsInfoAsync(List<CurseForgePackObj.FilesObj> obj)
    {
        string temp = $"{UrlHelper.CurseForge}mods";
        try
        {
            var arg1 = new CurseForgeModsInfoObj()
            {
                ModIds = [],
                FilterPcOnly = true
            };
            obj.ForEach(a => arg1.ModIds.Add(a.ProjectID));
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
                Content = new StringContent(JsonUtils.ToString(arg1, JsonType.CurseForgeModsInfoObj), MediaTypeHeaderValue.Parse("application/json"))
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeListObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    public static async Task<CurseForgeFileObj?> GetCurseForgeFilesAsync(string id, string? mc, int page = 0, Loaders loader = Loaders.Normal)
    {
        mc ??= "";
        string temp = $"{UrlHelper.CurseForge}mods/{id}/files?index={page * 50}&pageSize=50&gameVersion={mc}";

        try
        {
            if (loader is not (Loaders.Normal or Loaders.Custom))
            {
                temp += $"&modLoaderType={Loader(loader)}";
            }
            using var data = await SendAsync(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            return JsonUtils.ToObj(data, JsonType.CurseForgeFileObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(temp, e));
            return null;
        }
    }
}
