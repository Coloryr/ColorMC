using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using Heijden.DNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ColorMC.Core.Net.Apis;

public static class CurseForge
{
    private const string CurseForgeKEY = "$2a$10$6L8AkVsaGMcZR36i8XvCr.O4INa2zvDwMhooYdLZU0bb/E78AsT0m";
    private const string CurseForgeUrl = "https://api.curseforge.com/";

    private static async Task<CurseForgeObj?> GetList(int classid, string version, int page,
        int sort, string filter, int pagesize, int sortOrder)
    {
        try
        {
            string temp = CurseForgeUrl + $"v1/mods/search?gameId=432&classId={classid}&"
                + $"gameVersion={version}&index={page * pagesize}&sortField={sort}&" 
                + $"searchFilter={filter}&pageSize={pagesize}&sortOrder={sortOrder}";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.CurseForge.Error1"), e);
            return null;
        }
    }

    public static Task<CurseForgeObj?> GetPackList(string version = "", int page = 0,
        int sort = 2, string filter = "", int pagesize = 20, int sortOrder = 1)
    {
        return GetList(4471, version, page, sort, filter, pagesize, sortOrder);
    }

    public static Task<CurseForgeObj?> GetModList(string version = "", int page = 0,
        int sort = 2, string filter = "", int pagesize = 20, int sortOrder = 1)
    {
        return GetList(6, version, page, sort, filter, pagesize, sortOrder);
    }

    public static async Task<CurseForgeModObj?> GetMod(CurseForgePackObj.Files obj)
    {
        try
        {
            string temp = CurseForgeUrl + $"v1/mods/{obj.projectID}/files/{obj.fileID}";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.CurseForge.Error2"), e);
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

    public static async Task<List<CurseForgeModObj.Data>?> GetMods(List<CurseForgePackObj.Files> obj)
    {
        try
        {
            Arg1 arg1 = new();
            obj.ForEach(a => arg1.fileIds.Add(a.fileID));
            string temp = CurseForgeUrl + $"v1/mods/files";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.CurseForge.Error3"), e);
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

    public static async Task<CurseForgeVersion?> GetCurseForgeVersion()
    {
        try
        {
            string temp = CurseForgeUrl + $"v1/games/432/versions";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.CurseForge.Error4"), e);
            return null;
        }
    }

    public static async Task<CurseForgeVersionType?> GetCurseForgeVersionType()
    {
        try
        {
            string temp = CurseForgeUrl + $"v1/games/432/version-types";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.CurseForge.Error5"), e);
            return null;
        }
    }

    public static async Task<CurseForgeFileObj?> GetCurseForgeFiles(long id, int page = 0)
    {
        try
        {
            string temp = CurseForgeUrl + $"v1/mods/{id}/files?index={page * 50}";
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.CurseForge.Error6"), e);
            return null;
        }
    }
}
