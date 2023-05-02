using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

public static class UpdateChecker
{
    //private const string url = "http://localhost/colormc/A17/";
    private const string url = "https://colormc.coloryr.com/colormc/A17/";

    public static readonly string[] WebSha1s = new string[4] { "", "", "", "" };
    private static readonly string[] Sha1s = new string[4] { "", "", "", "" };
    private static readonly string[] Local = new string[4] { "", "", "", "" };
    public static void Init()
    {
        Local[0] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Core.dll");
        Local[1] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Core.pdb");
        Local[2] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Gui.dll");
        Local[3] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Gui.pdb");

        for (int a = 0; a < 4; a++)
        {
            if (File.Exists(Local[a]))
            {
                using var file = File.OpenRead(Local[a]);
                Sha1s[a] = Funtcions.GenSha1(file);
            }
            else
            {
                Sha1s[a] = ColorMCGui.BaseSha1[a];
            }
        }
    }

    public static async void Check()
    {
        try
        {
            var data = await CheckOne();
            if (data.Item1 == true)
            {
                var res = await App.HaveUpdate(data.Item2!);
                if (!res)
                {
                    StartUpdate();
                }
            }
        }
        catch (Exception e)
        {
            App.ShowError(App.GetLanguage("Gui.Error21"), e);
        }

    }

    public static async void StartUpdate()
    {
        var list = new List<DownloadItemObj>()
        {
            new DownloadItemObj()
            {
                Name = "ColorMC.Core.dll",
                SHA1 = WebSha1s[0],
                Url = $"{url}ColorMC.Core.dll",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Core.dll",
                Overwrite = true
            },
            new DownloadItemObj()
            {
                Name = "ColorMC.Core.pdb",
                SHA1 = WebSha1s[1],
                Url = $"{url}ColorMC.Core.pdb",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Core.pdb",
                Overwrite = true
            },
            new DownloadItemObj()
            {
                Name = "ColorMC.Gui.dll",
                SHA1 = WebSha1s[2],
                Url = $"{url}ColorMC.Gui.dll",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Gui.dll",
                Overwrite = true
            },
            new DownloadItemObj()
            {
                Name = "ColorMC.Gui.pdb",
                SHA1 = WebSha1s[3],
                Url = $"{url}ColorMC.Gui.pdb",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Gui.pdb",
                Overwrite = true
            }
        };

        var res = await DownloadManager.Start(list);
        if (res)
        {
            Process.Start($"{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                    "ColorMC.Launcher.exe" : "ColorMC.Launcher")}");
            App.Close();
        }
        else
        {
            App.ShowError(App.GetLanguage("Gui.Error22"), "");
        }
    }

    public static async Task<(bool?, string?)> CheckOne()
    {
        try
        {
            var data = await BaseClient.DownloadClient.GetStringAsync(url + "sha1.json");
            var obj = JObject.Parse(data);
            if (obj == null)
            {
                App.ShowError(App.GetLanguage("Gui.Error21"), "Json Error");
                return (false, null);
            }

            WebSha1s[0] = obj["core.dll"]!.ToString();
            WebSha1s[1] = obj["core.pdb"]!.ToString();
            WebSha1s[2] = obj["gui.dll"]!.ToString();
            WebSha1s[3] = obj["gui.pdb"]!.ToString();

            Logs.Info($"ColorMC.Core.dll:{Sha1s[0]} Web:{WebSha1s[0]}");
            Logs.Info($"ColorMC.Core.pdb:{Sha1s[1]} Web:{WebSha1s[1]}");
            Logs.Info($"ColorMC.Gui.dll:{Sha1s[2]} Web:{WebSha1s[2]}");
            Logs.Info($"ColorMC.Gui.pdb:{Sha1s[3]} Web:{WebSha1s[3]}");

            for (int a = 0; a < 4; a++)
            {
                if (WebSha1s[a] != Sha1s[a])
                {
                    obj.TryGetValue("text", out var data1);
                    return (true, data1?.ToString() ?? App.GetLanguage("Gui.Info20"));
                }
            }

            return (false, null);
        }
        catch (Exception e)
        {
            App.ShowError(App.GetLanguage("Gui.Error21"), e);
        }

        return (null, null);
    }
}
