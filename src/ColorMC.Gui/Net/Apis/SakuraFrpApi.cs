using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.Utils;

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
            string url = $"{Url}tunnels?token={key}";
            var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data, JsonGuiType.ListSakuraFrpChannelObj);
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
            var obj = new SakuraFrpGetChannelObj()
            {
                Query = id
            };

            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Url}tunnel/config?token={key}&frpc={version}"),
                Content = new StringContent(JsonUtils.ToString(obj, JsonGuiType.SakuraFrpGetChannelObj),
                MediaTypeHeaderValue.Parse("application/json"))
            };
            using var data = await ColorMCAPI.SendAsync(message);
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
    public static FileItemObj? BuildFrpItem(SakuraFrpDownloadObj data)
    {
        SakuraFrpDownloadObj.SakuraFrpDownloadItemObj.Arch.ArchItem data1;

        if (SystemInfo.Os == OsType.Windows)
        {
            if (SystemInfo.IsArm)
            {
                data1 = data.Frpc.Archs.WindowsArm64;
            }
            else
            {
                data1 = data.Frpc.Archs.WindowsAmd64;
            }
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            if (SystemInfo.IsArm)
            {
                data1 = data.Frpc.Archs.LinuxArm64;
            }
            else
            {
                data1 = data.Frpc.Archs.LinuxAmd64;
            }
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            if (SystemInfo.IsArm)
            {
                data1 = data.Frpc.Archs.DarwinArm64;
            }
            else
            {
                data1 = data.Frpc.Archs.DarwinAmd64;
            }
        }
        else
        {
            return null;
        }

        return new()
        {
            Name = $"SakuraFrp {data1.Title}",
            Local = FrpLaunchUtils.GetSakuraFrpLocal(data.Frpc.Ver),
            Md5 = data1.Hash,
            Url = data1.Url
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
            string url = $"{Url}system/clients";
            using var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data,JsonGuiType.SakuraFrpDownloadObj);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }
}
