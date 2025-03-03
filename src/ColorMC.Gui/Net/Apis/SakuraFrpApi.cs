using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ColorMC.Gui.Net.Apis;

/// <summary>
/// SakuraFrp请求
/// </summary>
public static class SakuraFrpApi
{
    public const string Url = "https://api.natfrp.com/v4/";

    ///// <summary>
    ///// 获取用户信息
    ///// </summary>
    ///// <param name="key">API KEY</param>
    ///// <returns>用户信息</returns>
    //public static async Task<SakuraFrpUserObj?> GetUserInfo(string key)
    //{
    //    try
    //    {
    //        var data = await CoreHttpClient.LoginClient.GetStringAsync($"{Url}user/info?token={key}");

    //        return JsonConvert.DeserializeObject<SakuraFrpUserObj>(data);
    //    }
    //    catch (Exception e)
    //    {
    //        Logs.Error("frp", e);
    //    }

    //    return null;
    //}

    /// <summary>
    /// 获取通道列表
    /// </summary>
    /// <param name="key">API KEY</param>
    /// <returns></returns>
    public static async Task<List<SakuraFrpChannelObj>?> GetChannel(string key)
    {
        try
        {
            var data = await CoreHttpClient.LoginClient.GetStringAsync($"{Url}tunnels?token={key}");

            return JsonConvert.DeserializeObject<List<SakuraFrpChannelObj>>(data);
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
    /// <param name="version">版本号</param>
    /// <returns>通道配置</returns>
    public static async Task<string?> GetChannelConfig(string key, int id, string version)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { query = id }),
                MediaTypeHeaderValue.Parse("application/json"));

            var data = await CoreHttpClient.LoginClient.PostAsync($"{Url}tunnel/config?token={key}&frpc={version}", content);
            var str = await data.Content.ReadAsStringAsync();
            if (str.StartsWith('{'))
            {
                return null;
            }

            return str;
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
    public static DownloadItemObj? BuildFrpItem(SakuraFrpDownloadObj data)
    {
        SakuraFrpDownloadObj.DownloadItemObj.Arch.ArchItem data1;

        if (SystemInfo.Os == OsType.Windows)
        {
            if (SystemInfo.IsArm)
            {
                data1 = data.frpc.archs.windows_arm64;
            }
            else
            {
                data1 = data.frpc.archs.windows_amd64;
            }
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            if (SystemInfo.IsArm)
            {
                data1 = data.frpc.archs.linux_arm64;
            }
            else
            {
                data1 = data.frpc.archs.linux_amd64;
            }
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            if (SystemInfo.IsArm)
            {
                data1 = data.frpc.archs.darwin_arm64;
            }
            else
            {
                data1 = data.frpc.archs.darwin_amd64;
            }
        }
        else
        {
            return null;
        }

        return new()
        {
            Name = $"SakuraFrp {data1.title}",
            Local = FrpLaunchUtils.GetSakuraFrpLocal(data.frpc.ver),
            Md5 = data1.hash,
            Url = data1.url
        };
    }

    /// <summary>
    /// 获取下载列表
    /// </summary>
    /// <returns></returns>
    public static async Task<SakuraFrpDownloadObj?> GetDownload()
    {
        try
        {
            var data = await CoreHttpClient.LoginClient.GetStringAsync($"{Url}system/clients");

            return JsonConvert.DeserializeObject<SakuraFrpDownloadObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }
}
