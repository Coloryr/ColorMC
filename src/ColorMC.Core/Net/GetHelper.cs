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
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            return JsonConvert.DeserializeObject<AssetsObj>(data.Item2!);
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
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            return JsonConvert.DeserializeObject<GameArgObj>(data.Item2!);
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
            string url = UrlHelper.GameVersion(local);
            var data = await BaseClient.GetString(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            return JsonConvert.DeserializeObject<VersionObj>(data.Item2!);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Error6"), e);
            return null;
        }
    }
}
