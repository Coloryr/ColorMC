using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Mclo;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class McloAPI
{
    public static async Task<string?> Push(string arg)
    {
        try
        {
            string temp = $"https://api.mclo.gs/1/log";
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
            };
            httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);
            httpRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                { "content", arg }
            });

            var data = await CoreHttpClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            var obj = JsonConvert.DeserializeObject<McloResObj>(data1)!;
            if (!obj.success)
            {
                return null;
            }

            return obj.url;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Mclo.Error1"), e);
            return null;
        }
    }
}
