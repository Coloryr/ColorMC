using System.Net;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Quilt网络请求
/// </summary>
public static class QuiltAPI
{
    /// <summary>
    /// 支持的游戏版本
    /// </summary>
    private static List<string>? s_supportVersion;

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    public static async Task<List<string>?> GetSupportVersionAsync(SourceLocal? local = null)
    {
        if (s_supportVersion != null)
        {
            return s_supportVersion;
        }

        string url = $"{UrlHelper.GetQuiltMeta(local)}/game";
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            if (stream == null)
            {
                return null;
            }
            var list = JsonUtils.ToObj(stream, JsonType.ListQuiltGameObj);
            if (list == null)
            {
                return null;
            }

            var list1 = new List<string>();
            foreach (var item in list)
            {
                list1.Add(item.Version);
            }

            s_supportVersion = list1;

            return list1;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取元数据
    /// </summary>
    public static async Task<QuiltMetaObj?> GetMetaAsync(SourceLocal? local = null)
    {
        string url = UrlHelper.GetQuiltMeta(local);

        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            return JsonUtils.ToObj(stream, JsonType.QuiltMetaObj);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <param name="mc">游戏版本</param>
    public static async Task<List<string>?> GetLoadersAsync(string mc, SourceLocal? local = null)
    {
        string url = $"{UrlHelper.GetQuiltMeta(local)}/loader/{mc}";

        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            var list = JsonUtils.ToObj(stream, JsonType.ListFabricLoaderVersionObj);
            if (list == null)
            {
                return null;
            }

            var list1 = new List<string>();
            foreach (var item in list)
            {
                list1.Add(item.Loader.Version);
            }
            return list1;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取加载器数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    public static async Task<GetQuiltLoaderRes?> GetLoaderAsync(string mc, string version, SourceLocal? local = null)
    {
        string url = $"{UrlHelper.GetQuiltMeta(local)}/loader/{mc}/{version}/profile/json";
        try
        {
            var stream = await CoreHttpClient.GetStreamAsync(url);
            if (stream == null)
            {
                return null;
            }

            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var meta = JsonUtils.ToObj(mem, JsonType.QuiltLoaderObj);
            mem.Seek(0, SeekOrigin.Begin);

            return new GetQuiltLoaderRes
            {
                Meta = meta,
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
