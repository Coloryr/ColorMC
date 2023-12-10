using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Frp;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Frp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ColorMC.Gui.Net.Apis;

public static class SakuraFrpApi
{
    public const string Url = "https://api.natfrp.com/v4/";

    public static async Task<SakuraFrpUserObj?> GetUserInfo(string key)
    {
        try
        {
            var data = await BaseClient.LoginClient.GetStringAsync($"{Url}user/info?token={key}");

            return JsonConvert.DeserializeObject<SakuraFrpUserObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    public static async Task<List<SakuraFrpChannelObj>?> GetChannel(string key)
    {
        try
        {
            var data = await BaseClient.LoginClient.GetStringAsync($"{Url}tunnels?token={key}");

            return JsonConvert.DeserializeObject<List<SakuraFrpChannelObj>>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    public static async Task<string?> GetChannelConfig(string key, int id)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { query = id }),
                MediaTypeHeaderValue.Parse("application/json"));

            var data = await BaseClient.LoginClient.PostAsync($"{Url}tunnel/config?token={key}", content);
            var str = await data.Content.ReadAsStringAsync();
            if (str.StartsWith("{"))
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
    public static async Task<DownloadItemObj?> BuildFrpItem()
    {
        var data = await GetDownload();
        if (data == null)
        {
            return null;
        }

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
            Local = FrpPath.GetSakuraFrpLocal(data.frpc.ver),
            MD5 = data1.hash,
            Url = data1.url
        };
    }

    private static async Task<SakuraFrpDownloadObj?> GetDownload()
    {
        try
        {
            var data = await BaseClient.LoginClient.GetStringAsync($"{Url}system/clients");

            return JsonConvert.DeserializeObject<SakuraFrpDownloadObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }
}
