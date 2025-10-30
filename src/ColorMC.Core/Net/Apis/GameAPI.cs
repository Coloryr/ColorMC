using System.Net;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// 获取游戏文件
/// </summary>
public static class GameAPI
{
    private static async Task<Stream?> SendAsync(string url)
    {
        var data = await CoreHttpClient.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Error10"),
                new Exception(url), false);
            return null;
        }

        return await data.Content.ReadAsStreamAsync();
    }

    /// <summary>
    /// 下载资源文件
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<GetAssetsRes?> GetAssetsAsync(string url)
    {
        try
        {
            using var stream = await SendAsync(url);
            if (stream == null)
            {
                return null;
            }
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var obj = JsonUtils.ToObj(mem, JsonType.AssetsObj);
            mem.Seek(0, SeekOrigin.Begin);
            return new GetAssetsRes
            {
                Assets = obj,
                Text = mem
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Error7"), e);
            return null;
        }
    }

    /// <summary>
    /// 下载游戏数据
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<GetGameArgRes?> GetGameAsync(string url)
    {
        try
        {
            using var stream = await SendAsync(url);
            if (stream == null)
            {
                return null;
            }
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var obj = JsonUtils.ToObj(mem, JsonType.GameArgObj);
            mem.Seek(0, SeekOrigin.Begin);
            return new()
            {
                Arg = obj,
                Text = mem
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Error8"), e);
            return null;
        }
    }

    /// <summary>
    /// 下载版本数据
    /// </summary>
    public static async Task<GetVersionsRes?> GetVersionsAsync(SourceLocal? local = null)
    {
        try
        {
            string url = UrlHelper.GameVersion(local);
            using var stream = await SendAsync(url);
            if (stream == null)
            {
                return null;
            }
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var obj = JsonUtils.ToObj(mem, JsonType.VersionObj);
            mem.Seek(0, SeekOrigin.Begin);
            return new()
            {
                Version = obj,
                Text = mem
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Error9"), e);
            return null;
        }
    }
}
