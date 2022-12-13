using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Pack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ColorMC.Core.Http;

public static class Get
{
    private const string CurseForgeKEY = "$2a$10$6L8AkVsaGMcZR36i8XvCr.O4INa2zvDwMhooYdLZU0bb/E78AsT0m";
    private const string CurseForgeUrl = "https://api.curseforge.com/";

    public static async Task<AssetsObj?> GetAssets(string url)
    {
        try
        {
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<AssetsObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取游戏资源信息发生错误", e);
            return null;
        }
    }

    public static async Task<GameArgObj?> GetGame(string url)
    {
        try
        {
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<GameArgObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取游戏信息发生错误", e);
            return null;
        }
    }
    public static async Task<CurseForgeObj?> GetCurseForge(string version = "", int index = 0, SortField sort = SortField.Popularity, string filter = "")
    {
        try
        {
            string temp = CurseForgeUrl + "v1/mods/search?gameId=432&classId=4471&"
                + $"gameVersion={version}&index={index}&sortOrder={(int)sort}&searchFilter={filter}";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.Client.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeObj>(data1);
        }
        catch (Exception e)
        {
            Logs.Error("获取CurseForge_Pack信息发生错误", e);
            return null;
        }
    }

    public static async Task<CurseForgeModObj?> GetCurseForgeMod(CurseForgePackObj.Files obj)
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
            var data = await BaseClient.Client.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<CurseForgeModObj>(data1);
        }
        catch (Exception e)
        {
            Logs.Error("获取CurseForge_Mod信息发生错误", e);
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

    public static async Task<List<CurseForgeModObj.Data>?> GetCurseForgeMods(List<CurseForgePackObj.Files> obj)
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
            var data = await BaseClient.Client.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<Arg2>(data1).data;
        }
        catch (Exception e)
        {
            Logs.Error("获取CurseForge_Mod信息发生错误", e);
            return null;
        }
    }

    public static async Task<JObject?> GetCurseForgeModPage(CurseForgePackObj.Files obj)
    {
        try
        {
            string temp = CurseForgeUrl + $"v1/mods/{obj.projectID}";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(temp)
            };
            httpRequest.Headers.Add("x-api-key", CurseForgeKEY);
            var data = await BaseClient.Client.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            return JsonConvert.DeserializeObject<JObject>(data1);
        }
        catch (Exception e)
        {
            Logs.Error("获取CurseForge_Mod信息发生错误", e);
            return null;
        }
    }

    public static async Task<VersionObj?> GetVersions(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelp.GameVersion(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<VersionObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取版本信息发生错误", e);
            return null;
        }
    }

    public static async Task<FabricMetaObj?> GetFabricMeta(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelp.FabricMeta(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<FabricMetaObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取版本信息发生错误", e);
            return null;
        }
    }

    public static async Task<FabricLoaderObj?> GetFabricLoader(string mc, string version, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelp.FabricMeta(local)}/loader/{mc}/{version}/profile/json";
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<FabricLoaderObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取版本信息发生错误", e);
            return null;
        }
    }

    public static async Task<QuiltMetaObj?> GetQuiltMeta(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelp.QuiltMeta(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<QuiltMetaObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取版本信息发生错误", e);
            return null;
        }
    }

    public static async Task<QuiltLoaderObj?> GetQuiltLoader(string mc, string version, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelp.QuiltMeta(local)}/loader/{mc}/{version}/profile/json";
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<QuiltLoaderObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取版本信息发生错误", e);
            return null;
        }
    }

    public static async Task<List<string>?> GetForgeVersions(string mc, SourceLocal? local = null)
    {

        return null;
    }
}
