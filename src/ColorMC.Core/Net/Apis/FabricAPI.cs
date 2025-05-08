using System.Net;
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
    private static List<string>? s_supportVersion;

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
    /// 获取元数据
    /// </summary>
    public static async Task<FabricMetaObj?> GetMeta(SourceLocal? local = null)
    {
        try
        {
            string url = UrlHelper.GetFabricMeta(local);
            using var stream = await SendAsync(url);
            return JsonUtils.ToObj(stream, JsonType.FabricMetaObj);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Download.Fabric.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取加载器数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    public static async Task<Stream?> GetLoader(string mc, string version, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.GetFabricMeta(local)}/loader/{mc}/{version}/profile/json";
            return await SendAsync(url);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Fabric.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <param name="mc">游戏版本</param>
    public static async Task<List<string>?> GetLoaders(string mc, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.GetFabricMeta(local)}/loader/{mc}";
            using var stream = await SendAsync(url);
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

            StringHelper.VersionSort(list1);

            return list1;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Fabric.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    public static async Task<List<string>?> GetSupportVersion(SourceLocal? local = null)
    {
        try
        {
            if (s_supportVersion != null)
                return s_supportVersion;

            string url = $"{UrlHelper.GetFabricMeta(local)}/game";
            using var stream = await SendAsync(url);
            var list = JsonUtils.ToObj(stream, JsonType.ListGameObj);
            if (list == null)
                return null;

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
            Logs.Error(LanguageHelper.Get("Core.Http.Fabric.Error3"), e);
            return null;
        }
    }
}
