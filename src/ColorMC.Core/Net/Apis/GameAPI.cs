using System.Net;
using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
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
            ColorMCCore.OnError(LanguageHelper.Get("Core.Http.Error7"),
                new Exception(url), false);
            return null;
        }

        return await data.Content.ReadAsStreamAsync();
    }

    /// <summary>
    /// 下载资源文件
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<GetAssetsRes?> GetAssets(string url)
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
            return new()
            {
                Assets = obj,
                Text = mem
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error4"), e);
            return null;
        }
    }

    /// <summary>
    /// 下载游戏数据
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<GetGameArgRes?> GetGame(string url)
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
            Logs.Error(LanguageHelper.Get("Core.Http.Error5"), e);
            return null;
        }
    }

    /// <summary>
    /// 下载版本数据
    /// </summary>
    public static async Task<GetVersionsRes?> GetVersions(SourceLocal? local = null)
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
            Logs.Error(LanguageHelper.Get("Core.Http.Error6"), e);
            return null;
        }
    }
}
