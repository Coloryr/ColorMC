using System;
using System.Threading.Tasks;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Net.Apis;

/// <summary>
/// Openfrp相关API
/// </summary>
public static class OpenFrpApi
{
    public const string Url = "https://of-dev-api.bfsea.com/api";

    /// <summary>
    /// 获取通道列表
    /// </summary>
    /// <param name="key">API KEY</param>
    /// <returns>通道列表</returns>
    public static async Task<OpenFrpChannelObj?> GetChannelAsync(string key)
    {
        try
        {
            string url = $"{Url}?action=getallproxies&user={key}";
            using var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data, JsonGuiType.OpenFrpChannelObj);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    /// <summary>
    /// 获取通道配置
    /// </summary>
    /// <param name="key">API KEY</param>
    /// <param name="id">通道ID</param>
    /// <returns>通道配置</returns>
    public static async Task<OpenFrpChannelInfoObj?> GetChannelConfigAsync(string key, int id)
    {
        try
        {
            string url = $"{Url}?action=getproxy&proxy={id}&user={key}";
            using var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data, JsonGuiType.OpenFrpChannelInfoObj);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    /// <summary>
    /// 创建Frp下载项目
    /// </summary>
    /// <returns></returns>
    public static async Task<FileItemRes> BuildFrpItemAsync()
    {
        var data = await GetDownloadAsync();
        if (data == null || data.Data == null)
        {
            return new FileItemRes();
        }

        string data1;

        if (SystemInfo.Os == OsType.Windows)
        {
            if (SystemInfo.IsArm)
            {
                data1 = "frpc_windows_arm64.zip";
            }
            else
            {
                data1 = "frpc_windows_amd64.zip";
            }
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            if (SystemInfo.IsArm)
            {
                data1 = "frpc_linux_arm64.tar.gz";
            }
            else
            {
                data1 = "frpc_linux_amd64.tar.gz";
            }
        }
        else if (SystemInfo.Os == OsType.MacOs)
        {
            if (SystemInfo.IsArm)
            {
                data1 = "frpc_darwin_arm64.tar.gz";
            }
            else
            {
                data1 = "frpc_darwin_amd64.tar.gz";
            }
        }
        else
        {
            return new FileItemRes();
        }

        return new FileItemRes
        {
            Path = FrpLaunchUtils.GetOpenFrpLocal(data.Data.LatestFull),
            File = new()
            {
                Name = $"OpenFrp {data1}",
                Local = FrpLaunchUtils.GetOpenFrpLocal(data.Data.LatestFull, data1),
                Url = data.Data.Source[0].Value + data.Data.Latest + data1,
                Later = (stream) =>
                {
                    ToolUtils.Unzip(stream, FrpLaunchUtils.GetOpenFrpLocal(data.Data.LatestFull), data1);
                }
            }
        };
    }

    /// <summary>
    /// 获取下载列表
    /// </summary>
    /// <returns></returns>
    private static async Task<OpenFrpDownloadObj?> GetDownloadAsync()
    {
        try
        {
            string url = "https://console.openfrp.net/web/commonQuery/get?key=software";
            using var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data, JsonGuiType.OpenFrpDownloadObj);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }
}
