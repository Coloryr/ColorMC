using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Net;

public static class GetHelper
{
    /// <summary>
    /// 下载资源文件
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<AssetsObj?> GetAssets(string url)
    {
        try
        {
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<AssetsObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Error4"), e);
            return null;
        }
    }

    /// <summary>
    /// 下载游戏数据
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<GameArgObj?> GetGame(string url)
    {
        try
        {
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<GameArgObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Error5"), e);
            return null;
        }
    }

    /// <summary>
    /// 下载版本数据
    /// </summary>
    public static async Task<VersionObj?> GetVersions(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelper.GameVersion(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<VersionObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Error6"), e);
            return null;
        }
    }
}
