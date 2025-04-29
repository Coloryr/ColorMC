using System.Net.Http.Headers;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

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

    private static async Task<string> Send(HttpRequestMessage httpRequest)
    {
        httpRequest.Headers.Add("x-api-key", ColorMCCore.CoreArg.CurseForgeKey ?? throw new Exception("CurseForge key is empty"));
        var data = await CoreHttpClient.DownloadClient.SendAsync(httpRequest);
        return await data.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="classid">cid</param>
    /// <param name="version">版本</param>
    /// <param name="page">页数</param>
    /// <param name="sort">排序</param>
    /// <param name="filter">过滤</param>
    /// <param name="pagesize">页大小</param>
    /// <param name="sortOrder">排序</param>
    /// <returns></returns>
    private static async Task<CurseForgeObjList?> GetList(int classid, string version, int page,
        int sortField, string filter, int pagesize, int sortOrder, string categoryId,
        int modLoaderType)
    {
        try
        {
            string temp = $"{UrlHelper.CurseForge}mods/search?gameId={GameID}&classId={classid}&"
                + $"gameVersion={version}&index={page * pagesize}&sortField={sortField}&"
                + $"searchFilter={filter}&pageSize={pagesize}&sortOrder={sortOrder}&"
                + $"categoryId={categoryId}";
            if (modLoaderType != 0)
            {
                temp += $"&modLoaderType={modLoaderType}";
            }
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeObjList>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    public static Task<CurseForgeObjList?> GetModPackList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "")
    {
        return GetList(ClassModPack, version, page, sortField, filter, pagesize, sortOrder, categoryId, 0);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<CurseForgeObjList?> GetModList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "", Loaders loader = Loaders.Normal)
    {
        return GetList(ClassMod, version, page, sortField, filter, pagesize, sortOrder, categoryId, Loader(loader));
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
    public static Task<CurseForgeObjList?> GetWorldList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "")
    {
        return GetList(ClassWorld, version, page, sortField, filter, pagesize, sortOrder, categoryId, 0);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<CurseForgeObjList?> GetResourcepackList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1,
        string categoryId = "")
    {
        return GetList(ClassResourcepack, version, page, sortField, filter, pagesize, sortOrder, categoryId, 0);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<CurseForgeObjList?> GetDataPacksList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1)
    {
        return GetList(ClassResourcepack, version, page, sortField, filter, pagesize, sortOrder, CategoryIdDataPacks.ToString(), 0);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    public static Task<CurseForgeObjList?> GetShadersList(string version = "", int page = 0,
        int sortField = 2, string filter = "", int pagesize = 20, int sortOrder = 1)
    {
        return GetList(ClassShaderpack, version, page, sortField, filter, pagesize, sortOrder, "", 0);
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeModObj?> GetMod(CurseForgePackObj.FilesObj obj)
    {
        try
        {
            string temp = $"{UrlHelper.CurseForge}mods/{obj.ProjectID}/files/{obj.FileID}";
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeModObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error2"), e);
            return null;
        }
    }

    private record Arg2
    {
        [JsonProperty("data")]
        public List<CurseForgeModObj.DataObj> Data { get; set; }
    }

    /// <summary>
    /// 查询Mod信息
    /// </summary>
    public static async Task<List<CurseForgeModObj.DataObj>?> GetFiles(List<CurseForgePackObj.FilesObj> obj)
    {
        try
        {
            var arg1 = new { fileIds = new List<long>() };
            obj.ForEach(a => arg1.fileIds.Add(a.FileID));
            var data = await Send(new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{UrlHelper.CurseForge}mods/files"),
                Content = new StringContent(JsonConvert.SerializeObject(arg1), MediaTypeHeaderValue.Parse("application/json"))
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<Arg2>(data)?.Data;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error3"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取分类信息
    /// </summary>
    /// <returns>信息</returns>
    public static async Task<CurseForgeCategoriesObj?> GetCategories()
    {
        try
        {
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{UrlHelper.CurseForge}categories?gameId={GameID}")
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeCategoriesObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error7"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取版本信息
    /// </summary>
    public static async Task<CurseForgeVersion?> GetCurseForgeVersion()
    {
        try
        {
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{UrlHelper.CurseForge}games/{GameID}/versions")
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeVersion>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error4"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取版本类型
    /// </summary>
    public static async Task<CurseForgeVersionType?> GetCurseForgeVersionType()
    {
        try
        {
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{UrlHelper.CurseForge}games/{GameID}/version-types")
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeVersionType>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error5"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeObj?> GetModInfo(long id)
    {
        try
        {
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{UrlHelper.CurseForge}mods/{id}")
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error6"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeObjList?> GetModsInfo(List<CurseForgePackObj.FilesObj> obj)
    {
        try
        {
            var arg1 = new { modIds = new List<long>(), filterPcOnly = true };
            obj.ForEach(a => arg1.modIds.Add(a.ProjectID));
            var data = await Send(new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{UrlHelper.CurseForge}mods"),
                Content = new StringContent(JsonConvert.SerializeObject(arg1), MediaTypeHeaderValue.Parse("application/json"))
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeObjList>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error6"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    public static async Task<CurseForgeFileObj?> GetCurseForgeFiles(string id, string? mc, int page = 0, Loaders loader = Loaders.Normal)
    {
        try
        {
            mc ??= "";
            string temp = $"{UrlHelper.CurseForge}mods/{id}/files?index={page * 50}&pageSize=50&gameVersion={mc}";
            if (loader is not (Loaders.Normal or Loaders.Custom))
            {
                temp += $"&modLoaderType={Loader(loader)}";
            }
            var data = await Send(new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            });
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CurseForgeFileObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error6"), e);
            return null;
        }
    }
}
