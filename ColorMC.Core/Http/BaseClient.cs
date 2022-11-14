using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ColorMC.Core.Http;

public enum SourceLocal
{
    Offical, BMCLAPI, MCBBS
}

public static class BaseClient
{
    public static SourceLocal Source { get; set; }

    public static HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public static async Task<string> GetString(string url, Dictionary<string, string> arg = null)
    {
        if (arg == null)
        {
            return await Client.GetStringAsync(url);
        }
        else
        {
            string temp = url;
            foreach (var item in arg)
            {
                temp += $"{item.Key}={item.Value}&";
            }
            temp = temp[..^1];
            return await Client.GetStringAsync(temp);
        }
    }

    public static async Task<byte[]> GetBytes(string url, Dictionary<string, string> arg = null)
    {
        if (arg == null)
        {
            return await Client.GetByteArrayAsync(url);
        }
        else
        {
            string temp = url;
            foreach (var item in arg)
            {
                temp += $"{item.Key}={item.Value}&";
            }
            temp = temp[..^1];
            return await Client.GetByteArrayAsync(temp);
        }
    }

    public static async Task<string> PostString(string url, Dictionary<string, string> arg)
    {
        FormUrlEncodedContent content = new(arg);
        var message = await Client.PostAsync(url, content);

        return await message.Content.ReadAsStringAsync();
    }

    public static async Task<JObject> PostObj(string url, object arg)
    {
        var data1 = JsonConvert.SerializeObject(arg);
        StringContent content = new(data1, MediaTypeHeaderValue.Parse("application/json"));
        var message = await Client.PostAsync(url, content);
        var data = await message.Content.ReadAsStringAsync();
        return JObject.Parse(data);
    }
}
