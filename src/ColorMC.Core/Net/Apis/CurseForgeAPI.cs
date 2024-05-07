using System.Collections.Concurrent;
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
    private const string CurseForgeKEY = "$2a$10$6L8AkVsaGMcZR36i8XvCr.O4INa2zvDwMhooYdLZU0bb/E78AsT0m";

    public const int GameID = 432;

    public const int ClassModPack = 4471;
    public const int ClassMod = 6;
    public const int ClassWorld = 17;
    public const int ClassResourcepack = 12;
    public const int ClassShaderpack = 6552;

    public const int CategoryIdDataPacks = 5193;

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
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeObjList>(data1);
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
    public static async Task<CurseForgeModObj?> GetMod(CurseForgePackObj.Files obj)
    {
        try
        {
            string temp = $"{UrlHelper.CurseForge}mods/{obj.projectID}/files/{obj.fileID}";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeModObj>(data1);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error2"), e);
            return null;
        }
    }

    private record Arg2
    {
        public List<CurseForgeModObj.Data> data { get; set; }
    }

    /// <summary>
    /// 查询Mod信息
    /// </summary>
    public static async Task<List<CurseForgeModObj.Data>?> GetMods(List<CurseForgePackObj.Files> obj)
    {
        try
        {
            var arg1 = new { fileIds = new List<long>() };
            obj.ForEach(a => arg1.fileIds.Add(a.fileID));
            string temp = $"{UrlHelper.CurseForge}mods/files";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
                Content = new StringContent(JsonConvert.SerializeObject(arg1), MediaTypeHeaderValue.Parse("application/json"))
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<Arg2>(data1)?.data;
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
            string temp = $"{UrlHelper.CurseForge}categories?gameId={GameID}";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeCategoriesObj>(data1);
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
            string temp = $"{UrlHelper.CurseForge}games/{GameID}/versions";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeVersion>(data1);
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
            string temp = $"{UrlHelper.CurseForge}games/{GameID}/version-types";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeVersionType>(data1);
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
            string temp = $"{UrlHelper.CurseForge}mods/{id}";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeObj>(data1);
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
    public static async Task<CurseForgeObjList?> GetModsInfo(List<CurseForgePackObj.Files> obj)
    {
        try
        {
            string temp = $"{UrlHelper.CurseForge}mods";
            var arg1 = new { modIds = new List<long>(), filterPcOnly = true };
            obj.ForEach(a => arg1.modIds.Add(a.projectID));
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
                Content = new StringContent(JsonConvert.SerializeObject(arg1), MediaTypeHeaderValue.Parse("application/json"))
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeObjList>(data1);
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
            if (loader != Loaders.Normal)
            {
                temp += $"&modLoaderType={Loader(loader)}";
            }
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeFileObj>(data1);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.CurseForge.Error6"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取Mod依赖
    /// </summary>
    /// <param name="data">Mod</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="loader">加载器</param>
    /// <returns></returns>
    public static async Task<ConcurrentBag<((string Name, string ModId, bool Opt) Info,
        List<CurseForgeModObj.Data> List)>>
       GetModDependencies(CurseForgeModObj.Data data, string mc, Loaders loader, bool dep, ConcurrentBag<long>? ids = null)
    {
        ids ??= new();
        var list = new ConcurrentBag<((string Name, string ModId, bool Opt) Info,
        List<CurseForgeModObj.Data> List)>();
        if (data.dependencies == null || data.dependencies.Count == 0)
        {
            return list;
        }
        await Parallel.ForEachAsync(data.dependencies, async (item, cancel) =>
        {
            if (ids.Contains(item.modId))
                return;
            var opt = item.relationType != 2 && dep;
            var res1 = await GetCurseForgeFiles(item.modId.ToString(), mc, loader: loader);
            if (res1 == null || res1.data.Count == 0)
                return;
            var res2 = await GetModInfo(item.modId);
            if (res2 == null)
                return;

            list.Add(((res2.Data.name, res2.Data.id.ToString(), !opt), res1.data));
            ids.Add(item.modId);

            foreach (var item3 in await GetModDependencies(res1.data[0], mc, loader, opt, ids))
            {
                list.Add(item3);
            }
        });

        return list;
    }
}
