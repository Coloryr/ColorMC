using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.Utils;

public static class FrpLaunchUtils
{
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        BaseDir = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameFrpFile);

        Directory.CreateDirectory(BaseDir);
    }

    /// <summary>
    /// 获取Frp文件名
    /// </summary>
    /// <returns></returns>
    public static string GetFrpcName()
    {
        return SystemInfo.Os == OsType.Windows ? GuiNames.NameFrpFile1 : GuiNames.NameFrpFile;
    }

    /// <summary>
    /// 获取SakuraFrp文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetSakuraFrpLocal(string ver)
    {
        return Path.Combine(BaseDir, GuiNames.NameSakuraFrpDir, ver, GetFrpcName());
    }

    /// <summary>
    /// 获取OpenFrp文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetOpenFrpLocal(string ver, string? filename = null)
    {
        return filename != null ? Path.Combine(BaseDir, GuiNames.NameOpenFrpDir, ver, filename)
            : Path.Combine(BaseDir, GuiNames.NameOpenFrpDir, ver, GetFrpcName());
    }

    /// <summary>
    /// 获取OpenFrp文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetFrpLocal(string ver, string? filename = null)
    {
        return filename != null ? Path.Combine(BaseDir, GuiNames.NameFrpDir, ver, filename)
            : Path.Combine(BaseDir, GuiNames.NameFrpDir, ver);
    }

    /// <summary>
    /// 启动Frp
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<FrpLaunchRes> StartFrp(NetFrpSelfItemModel item1, NetFrpLocalModel model)
    {
        var obj1 = await ColorMCCloudAPI.GetFrpListAsync();
        if (obj1 == null)
        {
            return new();
        }
        var item = obj1.FirstOrDefault();
        var obj = ColorMCCloudAPI.BuildFrpItem(item.Key, item.Value);

        if (obj.Path == null)
        {
            return new();
        }
        if (!File.Exists(obj.Path))
        {
            var res = await DownloadManager.StartAsync([obj.File!]);
            if (!res)
            {
                return new();
            }
        }
        var file = obj.Path;
        var info2 = new FileInfo(file);
        var dir = info2.DirectoryName!;

        var builder = new StringBuilder();
        builder.AppendLine($"serverAddr = \"{item1.Obj.IP}\"")
            .AppendLine($"serverPort = {item1.Obj.Port}")
            .AppendLine();
        if (!string.IsNullOrWhiteSpace(item1.Obj.User))
        {
            builder.AppendLine($"user = \"{item1.Obj.User}\"");
        }
        if (!string.IsNullOrWhiteSpace(item1.Obj.Key))
        {
            builder.AppendLine($"auth.token = \"{item1.Obj.Key}\"");
        }
        builder.AppendLine()
            .AppendLine("[[proxies]]")
            .AppendLine($"name = \"{(string.IsNullOrWhiteSpace(item1.Obj.RName) ? "minecraft" : item1.Obj.RName)}\"")
            .AppendLine("type = \"tcp\"")
            .AppendLine("localIP = \"127.0.0.1\"")
            .AppendLine($"localPort = {model.Port}")
            .AppendLine($"remotePort = {item1.Obj.NetPort}");

        PathHelper.WriteText(Path.Combine(dir, GuiNames.NameServerTomlFile), builder.ToString());

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
                    Arguments = $"-c {GuiNames.NameServerTomlFile}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            return new()
            {
                Res = true,
                Process = p,
                IP = $"{item1.Obj.IP}:{item1.NetPort}"
            };
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("BaseBinding.Error6"), e);
        }

        return new();
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
        string version = GuiNames.NameSakuraFrpVersion;
        FileItemObj? obj = null;
        string? local = "";
        if (item1.FrpType == FrpType.SakuraFrp)
        {
            var obj1 = await SakuraFrpApi.GetDownloadAsync();
            if (obj1 == null)
            {
                return new();
            }
            version = obj1.Frpc.Ver;
            obj = SakuraFrpApi.BuildFrpItem(obj1);
            local = obj?.Local;
        }
        else if (item1.FrpType == FrpType.OpenFrp)
        {
            var res = await OpenFrpApi.BuildFrpItemAsync();
            obj = res.File;
            local = res.Path;
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

        string? info = null;
        if (item1.FrpType == FrpType.SakuraFrp)
        {
            info = await SakuraFrpApi.GetChannelConfigAsync(item1.Key, item1.ID, version);
        }
        else if (item1.FrpType == FrpType.OpenFrp)
        {
            var temp = await OpenFrpApi.GetChannelConfigAsync(item1.Key, item1.ID);
            if (temp != null && temp.Proxies?.Count > 0)
            {
                info = temp.Proxies.Values.First();
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
            outip = $"{ip}:{item1.Remote}";
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
            outip = item1.Remote;
        }

        PathHelper.WriteText(Path.Combine(dir, GuiNames.NameServerIniFile), builder.ToString());

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
                    Arguments = $"-c {GuiNames.NameServerIniFile}",
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
