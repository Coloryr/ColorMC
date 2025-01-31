using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.Utils;

public static class FrpLaunchUtils
{
    public const string Name1 = "frp";
    public const string Name2 = "0.51.0-sakura-9.2";

    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;

        if (!BaseDir.EndsWith('/') && !BaseDir.EndsWith('\\'))
        {
            BaseDir += "/";
        }

        BaseDir += Name1;

        Directory.CreateDirectory(BaseDir);
    }

    private static string GetFrpcName()
    {
        return SystemInfo.Os == OsType.Windows ? "frpc.exe" : "frpc";
    }

    /// <summary>
    /// 获取SakuraFrp文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetSakuraFrpLocal(string ver)
    {
        return $"{BaseDir}/SakuraFrp/{ver}/{GetFrpcName()}";
    }

    /// <summary>
    /// 获取SakuraFrp文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetOpenFrpLocal(string ver, bool dir = false)
    {
        return dir ? $"{BaseDir}/OpenFrp/{ver}/" : $"{BaseDir}/OpenFrp/{ver}/{GetFrpcName()}";
    }

    /// <summary>
    /// 启动Frp
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<FrpLaunchRes> StartFrp(NetFrpRemoteModel item1, NetFrpLocalModel model)
    {
        string file;
        string dir;
        string version = Name2;
        if (SystemInfo.Os == OsType.Android)
        {
            file = ColorMCGui.PhoneGetFrp!(item1.FrpType);
            dir = BaseDir;
        }
        else
        {
            DownloadItemObj? obj = null;
            string? local = "";
            if (item1.FrpType == FrpType.SakuraFrp)
            {
                var obj1 = await SakuraFrpApi.GetDownload();
                if (obj1 == null)
                {
                    return new();
                }
                version = obj1.frpc.ver;
                obj = SakuraFrpApi.BuildFrpItem(obj1);
                local = obj?.Local;
            }
            else if (item1.FrpType == FrpType.OpenFrp)
            {
                (obj, local) = await OpenFrpApi.BuildFrpItem();
            }
            if (obj == null)
            {
                return new();
            }
            if (!File.Exists(obj.Local))
            {
                var res = await DownloadManager.StartAsync([obj]);
                if (!res)
                {
                    return new();
                }
            }
            file = local!;
            var info2 = new FileInfo(file);
            dir = info2.DirectoryName!;
        }
        string? info = null;
        if (item1.FrpType == FrpType.SakuraFrp)
        {
            info = await SakuraFrpApi.GetChannelConfig(item1.Key, item1.ID, version);
        }
        else if (item1.FrpType == FrpType.OpenFrp)
        {
            var temp = await OpenFrpApi.GetChannelConfig(item1.Key, item1.ID);
            if (temp != null && temp.proxies?.Count > 0)
            {
                info = temp.proxies.Values.First();
            }
        }
        if (info == null)
        {
            return new();
        }

        var lines = info.Split("\n");
        var builder = new StringBuilder();
        string outip = "";
        if (item1.FrpType == FrpType.SakuraFrp)
        {
            string ip = "";

            foreach (var item2 in lines)
            {
                var item3 = item2.Trim();
                if (item3.StartsWith("login_fail_exit"))
                {
                    builder.AppendLine("login_fail_exit = true");
                }
                else if (item3.StartsWith("server_addr"))
                {
                    ip = item3.Split("=")[1].Trim();
                    builder.AppendLine(item3);
                }
                else if (item3.StartsWith("local_port"))
                {
                    builder.AppendLine($"local_port = {model.Port}");
                }
                else
                {
                    builder.AppendLine(item3);
                }
            }

            File.WriteAllText(dir + "/server.ini", builder.ToString());

            outip = ip + ":" + item1.Remote;
        }
        else if (item1.FrpType == FrpType.OpenFrp)
        {
            foreach (var item2 in lines)
            {
                var item3 = item2.Trim();
                if (item3.StartsWith("local_port"))
                {
                    builder.AppendLine($"local_port = {model.Port}");
                }
                else
                {
                    builder.AppendLine(item3);
                }
            }

            File.WriteAllText(dir + "/server.ini", builder.ToString());

            outip = item1.Remote;
        }

        try
        {
            if (SystemInfo.Os != OsType.Windows)
            {
                PathBinding.Chmod(file);
            }

            var p = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = file,
                    WorkingDirectory = dir,
                    Arguments = "-c server.ini",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            return new()
            {
                Res = true,
                Process = p,
                IP = outip
            };
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("BaseBinding.Error6"), e);
        }

        return new();
    }
}
