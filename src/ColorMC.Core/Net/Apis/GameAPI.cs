using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System.Text;

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
    public static async Task<(AssetsObj?, string?)> GetAssets(string url)
    {
        try
        {
            var data = await BaseClient.GetStringAsync(url);
            if (data.Item1 == false)
            {
                ColorMCCore.Error(LanguageHelper.Get("Core.Http.Error7"),
                    new Exception(url), false);
                return (null, null);
            }
            return (JsonConvert.DeserializeObject<AssetsObj>(data.Item2!), data.Item2!);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error4"), e);
            return (null, null);
        }
    }

    /// <summary>
    /// 下载游戏数据
    /// </summary>
    /// <param name="url">网址</param>
    public static async Task<(GameArgObj?, byte[]?)> GetGame(string url)
    {
        try
        {
            var data = await BaseClient.GetBytesAsync(url);
            if (data.Item1 == false)
            {
                ColorMCCore.Error(LanguageHelper.Get("Core.Http.Error7"),
                    new Exception(url), false);
                return (null, null);
            }
            return (JsonConvert
                .DeserializeObject<GameArgObj>(Encoding.UTF8.GetString(data.Item2!)),
                data.Item2);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error5"), e);
            return (null, null);
        }
    }

    /// <summary>
    /// 下载版本数据
    /// </summary>
    public static async Task<(VersionObj?, string?)> GetVersions(SourceLocal? local = null)
    {
        try
        {
            string url = UrlHelper.GameVersion(local);
            var data = await BaseClient.GetStringAsync(url);
            if (data.Item1 == false)
            {
                ColorMCCore.Error(LanguageHelper.Get("Core.Http.Error7"),
                    new Exception(url), false);
                return (null, null);
            }
            return (JsonConvert.DeserializeObject<VersionObj>(data.Item2!), data.Item2);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error6"), e);
            return (null, null);
        }
    }
}
