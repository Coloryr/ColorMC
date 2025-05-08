using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Mclo;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Mc log上传
/// </summary>
public static class McloAPI
{
    public const string Url = $"https://api.mclo.gs/1/log";

    /// <summary>
    /// 上传游戏日志
    /// </summary>
    /// <param name="arg">日志内容</param>
    /// <returns>返回的网址</returns>
    public static async Task<string?> Push(string arg)
    {
        try
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Url),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "content", arg }
                })
            };

            using var data = await CoreHttpClient.SendAsync(httpRequest);
            using var stream = await data.Content.ReadAsStreamAsync();
            var obj = JsonUtils.ToObj(stream, JsonType.McloResObj);
            if (obj?.Success != true)
            {
                return null;
            }

            return obj.Url;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Mclo.Error1"), e);
            return null;
        }
    }
}
