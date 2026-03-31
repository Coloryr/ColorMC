using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Fabric网络请求
/// </summary>
public static class FabricAPI
{
    /// <summary>
    /// 支持的游戏版本
    /// </summary>
    private static readonly HashSet<string> s_supportVersion = [];

    /// <summary>
    /// 获取元数据
    /// </summary>
    public static async Task<FabricMetaObj?> GetMetaAsync(SourceLocal? source = null)
    {
        source ??= CoreHttpClient.Source;

        string url = UrlHelper.GetFabricMeta(source);
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            return JsonUtils.ToObj(stream, JsonType.FabricMetaObj);
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
    public static async Task<GetFabricLoaderRes?> GetLoaderAsync(string mc, string version, SourceLocal? source = null)
    {
        source ??= CoreHttpClient.Source;

        string url = $"{UrlHelper.GetFabricMeta(source)}/loader/{mc}/{version}/profile/json";
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
            var meta = JsonUtils.ToObj(mem, JsonType.FabricLoaderObj);
            mem.Seek(0, SeekOrigin.Begin);
            return new GetFabricLoaderRes
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

    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <param name="mc">游戏版本</param>
    public static async Task<HashSet<string>?> GetLoadersAsync(string mc, SourceLocal? source = null)
    {
        source ??= CoreHttpClient.Source;

        string url = $"{UrlHelper.GetFabricMeta(source)}/loader/{mc}";
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            var list = JsonUtils.ToObj(stream, JsonType.ListFabricLoaderVersionObj);
            if (list == null)
            {
                return null;
            }

            var list1 = new HashSet<string>();
            foreach (var item in list)
            {
                list1.Add(item.Loader.Version);
            }

            StringHelper.VersionSort(list1);

            return list1;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    public static async Task<HashSet<string>?> GetSupportVersionAsync(SourceLocal? source = null)
    {
        if (s_supportVersion.Count != 0)
        {
            return s_supportVersion;
        }

        source ??= CoreHttpClient.Source;

        string url = $"{UrlHelper.GetFabricMeta(source)}/game";

        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            var list = JsonUtils.ToObj(stream, JsonType.ListGameObj);
            if (list == null)
            {
                return null;
            }

            s_supportVersion.Clear();
            foreach (var item in list)
            {
                s_supportVersion.Add(item.Version);
            }

            return s_supportVersion;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }
}
