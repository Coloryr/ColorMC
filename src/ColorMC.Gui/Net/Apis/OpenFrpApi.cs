using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace ColorMC.Gui.Net.Apis;

public static class OpenFrpApi
{
    public const string Url = "https://of-dev-api.bfsea.xyz/api";

    public static async Task<OpenFrpChannelObj?> GetChannel(string key)
    {
        try
        {
            var data = await WebClient.LoginClient.GetStringAsync($"{Url}?action=getallproxies&user={key}");

            return JsonConvert.DeserializeObject<OpenFrpChannelObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    public static async Task<OpenFrpChannelInfoObj?> GetChannelConfig(string key, int id)
    {
        try
        {
            var data = await WebClient.LoginClient.GetStringAsync($"{Url}?action=getproxy&proxy={id}&user={key}");

            return JsonConvert.DeserializeObject<OpenFrpChannelInfoObj>(data);
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
    public static async Task<(DownloadItemObj?, string?)> BuildFrpItem()
    {
        var data = await GetDownload();
        if (data == null || data.data == null)
        {
            return (null, null);
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
        else if (SystemInfo.Os == OsType.MacOS)
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
            return (null, null);
        }

        return (new()
        {
            Name = $"OpenFrp {data1}",
            Local = FrpPath.GetOpenFrpLocal(data.data.latest_full, true) + data1,
            Url = data.data.source[0].value + data.data.latest + data1,
            Later = (stream) =>
            {
                Unzip(stream, data.data.latest_full, data1);
            }
        }, FrpPath.GetOpenFrpLocal(data.data.latest_full));
    }

    private static void Unzip(Stream stream, string version, string file)
    {
        if (file.EndsWith("tar.gz"))
        {
            using var gzipStream = new GZipInputStream(stream);
            var tarArchive = new TarInputStream(gzipStream, TarBuffer.DefaultBlockFactor, Encoding.UTF8);
            do
            {
                TarEntry entry1 = tarArchive.GetNextEntry();
                if (entry1.IsDirectory || !entry1.Name.StartsWith("frpc"))
                {
                    continue;
                }

                using var filestream = PathHelper.OpenWrite(FrpPath.GetOpenFrpLocal(version), true);
                tarArchive.CopyEntryContents(filestream);

                break;
            }
            while (true);

            tarArchive.Close();
        }
        else
        {
            using var s = new ZipFile(stream);
            foreach (ZipEntry item in s)
            {
                if (item.IsDirectory || !item.Name.StartsWith("frpc"))
                {
                    continue;
                }

                PathHelper.WriteBytes(FrpPath.GetOpenFrpLocal(version), s.GetInputStream(item));
                break;
            }
        }
    }

    private static async Task<OpenFrpDownloadObj?> GetDownload()
    {
        try
        {
            var data = await WebClient.LoginClient.GetStringAsync($"https://console.openfrp.net/web/commonQuery/get?key=software");

            return JsonConvert.DeserializeObject<OpenFrpDownloadObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }
}
