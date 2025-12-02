using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// 获取游戏文件
/// </summary>
public static class GameAPI
{
    /// <summary>
    /// 下载资源文件
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<GetAssetsRes?> GetAssetsAsync(string url)
    {
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
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
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
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
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            if (stream == null)
            {
                return null;
            }
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var obj = JsonUtils.ToObj(mem, JsonType.GameArgObj);
            mem.Seek(0, SeekOrigin.Begin);
            return new GetGameArgRes
            {
                Arg = obj,
                Text = mem
            };
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 下载版本数据
    /// </summary>
    public static async Task<GetVersionsRes?> GetVersionsAsync(SourceLocal? local = null)
    {
        string url = UrlHelper.GameVersion(local);
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            if (stream == null)
            {
                return null;
            }
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var obj = JsonUtils.ToObj(mem, JsonType.VersionObj);
            mem.Seek(0, SeekOrigin.Begin);
            return new GetVersionsRes
            {
                Version = obj,
                Text = mem
            };
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }
}
