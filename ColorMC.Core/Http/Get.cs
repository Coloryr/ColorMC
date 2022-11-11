using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Pack;
using Newtonsoft.Json;

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

    public static async Task<VersionObj?> GetVersion(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelp.Version(local));
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

    public static async Task<FabircMetaObj?> GetFabricMeta(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelp.FabricMeta(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<FabircMetaObj>(data);
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
}
