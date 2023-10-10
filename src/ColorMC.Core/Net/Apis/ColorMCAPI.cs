using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Net.Apis;

public static class ColorMCAPI
{
    private static async Task<Dictionary<string, McModSearchItemObj>?> GetList(int type, List<string> ids)
    {
        try
        {
            string temp = $"https://mc1.coloryr.com:8081/findmod";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
            };
            httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(new { type, ids }));

            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            var obj = JObject.Parse(data1);
            if (obj.TryGetValue("res", out var res) && ((int)res) != 100)
            {
                return null;
            }

            var data2 = obj.GetValue("data")!;
            return data2.ToObject<Dictionary<string, McModSearchItemObj>>();
        } 
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.McMod.Error1"), e);
            return null;
        }
    }

    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromCF(List<string> ids)
    {
        return GetList(0, ids);
    }

    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromMO(List<string> ids)
    {
        return GetList(1, ids);
    }

    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromName(string name, int page)
    {
        return GetList(2, new() { name, page.ToString() });
    }
}
