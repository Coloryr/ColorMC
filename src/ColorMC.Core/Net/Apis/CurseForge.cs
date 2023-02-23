using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ColorMC.Core.Net.Apis;

public static class CurseForge
{
    private const string CurseForgeKEY = "$2a$10$6L8AkVsaGMcZR36i8XvCr.O4INa2zvDwMhooYdLZU0bb/E78AsT0m";
    private const string CurseForgeUrl = "https://api.curseforge.com/v1/";

    public const int GameID = 432;

    public const int ClassModPack = 4471;
    public const int ClassMod = 6;
    public const int ClassWorld = 441771;
    public const int ClassResourcepack = 12;

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
    private static async Task<CurseForgeObj?> GetList(int classid, string version, int page,
        int sort, string filter, int pagesize, int sortOrder, string categoryId)
    {
        try
        {
            string temp = $"{CurseForgeUrl}mods/search?gameId={GameID}&classId={classid}&"
                + $"gameVersion={version}&index={page * pagesize}&sortField={sort}&"
                + $"searchFilter={filter}&pageSize={pagesize}&sortOrder={sortOrder}&" 
                + $"categoryId={categoryId}";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error6"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    public static Task<CurseForgeObj?> GetModPackList(string version = "", int page = 0,
        int sort = 2, string filter = "", int pagesize = 50, int sortOrder = 1, 
        string categoryId = "")
    {
        return GetList(ClassModPack, version, page, sort, filter, pagesize, sortOrder, categoryId);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<CurseForgeObj?> GetModList(string version = "", int page = 0,
        int sort = 2, string filter = "", int pagesize = 50, int sortOrder = 1,
        string categoryId = "")
    {
        return GetList(ClassMod, version, page, sort, filter, pagesize, sortOrder, categoryId);
    }

    /// <summary>
    /// 获取世界列表
    /// </summary>
    public static Task<CurseForgeObj?> GetWorldList(string version = "", int page = 0,
        int sort = 2, string filter = "", int pagesize = 50, int sortOrder = 1,
        string categoryId = "")
    {
        return GetList(ClassWorld, version, page, sort, filter, pagesize, sortOrder, categoryId);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<CurseForgeObj?> GetResourcepackList(string version = "", int page = 0,
        int sort = 2, string filter = "", int pagesize = 50, int sortOrder = 1,
        string categoryId = "")
    {
        return GetList(ClassResourcepack, version, page, sort, filter, pagesize, sortOrder, categoryId);
    }

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    public static async Task<CurseForgeModObj?> GetMod(CurseForgePackObj.Files obj)
    {
        try
        {
            string temp = $"{CurseForgeUrl}mods/{obj.projectID}/files/{obj.fileID}";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error7"), e);
            return null;
        }
    }

    private record Arg1
    {
        public List<long> fileIds { get; set; } = new();
    }

    private record Arg2
    {
        public List<CurseForgeModObj.Data> data { get; set; } = new();
    }

    /// <summary>
    /// 查询Mod信息
    /// </summary>
    public static async Task<List<CurseForgeModObj.Data>?> GetMods(List<CurseForgePackObj.Files> obj)
    {
        try
        {
            Arg1 arg1 = new();
            obj.ForEach(a => arg1.fileIds.Add(a.fileID));
            string temp =  $"{CurseForgeUrl}mods/files";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error8"), e);
            return null;
        }
    }

    //public static async Task<JObject?> GetCurseForgeModPage(CurseForgePackObj.Files obj)
    //{
    //    try
    //    {
    //        string temp = CurseForgeUrl + $"v1/mods/{obj.projectID}";
    //        HttpRequestMessage httpRequest = new()
    //        {
    //            Method = HttpMethod.Get,
    //            RequestUri = new Uri(temp)
    //        };
    //        httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
    //        var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
    //        var data1 = await data.Content.ReadAsStringAsync();
    //        if (string.IsNullOrWhiteSpace(data1))
    //            return null;
    //        return JsonConvert.DeserializeObject<JObject>(data1);
    //    }
    //    catch (Exception e)
    //    {
    //        Logs.Error("获取CurseForge_Mod信息发生错误", e);
    //        return null;
    //    }
    //}


    public static async Task<CurseForgeCategoriesObj?> GetCategories()
    {
        try
        {
            string temp = $"{CurseForgeUrl}categories?gameId={GameID}";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error9"), e);
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
            string temp = $"{CurseForgeUrl}games/{GameID}/versions";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error9"), e);
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
            string temp = $"{CurseForgeUrl}games/{GameID}/version-types";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error10"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    public static async Task<CurseForgeFileObj?> GetCurseForgeFiles(string id, int page = 0)
    {
        try
        {
            string temp = $"{CurseForgeUrl}mods/{id}/files?index={page * 50}&pageSize=50";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Error11"), e);
            return null;
        }
    }
}
