using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 启动器更新器
/// </summary>
public static class UpdateChecker
{
    public static readonly string[] WebSha1s = ["", "", "", ""];
    public static readonly string[] Sha1s = ["", "", "", ""];
    public static readonly string[] LocalPath = ["", "", "", ""];

    public static void Init()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return;
        }

        LocalPath[0] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Core.dll");
        LocalPath[1] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Core.pdb");
        LocalPath[2] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Gui.dll");
        LocalPath[3] = Path.GetFullPath($"{ColorMCGui.RunDir}dll/ColorMC.Gui.pdb");

        for (int a = 0; a < 4; a++)
        {
            if (File.Exists(LocalPath[a]))
            {
                using var file = PathHelper.OpenRead(LocalPath[a])!;
                Sha1s[a] = HashHelper.GenSha1(file);
            }
            else
            {
                Sha1s[a] = ColorMCGui.BaseSha1[a];
            }
        }
    }

    public static async Task<(bool, bool, string?)> Check()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return (false, false, null);
        }

        try
        {
            var obj = await ColorMCAPI.GetUpdateIndex();
            if (obj == null)
            {
                App.UpdateCheckFail();
                return (false, false, null);
            }

            if (obj.TryGetValue("Version", out var temp)
                && ColorMCCore.TopVersion != temp.ToString())
            {
                return (true, true, obj["Text"]?.ToString());
            }
            var data1 = await CheckOne();
            if (data1.Item1 == true)
            {
                return (true, false, data1.Item2!);
            }
        }
        catch (Exception e)
        {
            App.UpdateCheckFail();
            Logs.Error(App.Lang("SettingWindow.Tab3.Error2"), e);
        }

        return (false, false, null);
    }

    public static async void StartUpdate()
    {
        if (ColorMCGui.BaseSha1 == null)
            return;

        var list = new List<DownloadItemObj>()
        {
            new()
            {
                Name = "ColorMC.Core.dll",
                SHA1 = WebSha1s[0],
                Url = $"{ColorMCAPI.CheckUrl}ColorMC.Core.dll",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Core.dll",
                Overwrite = true,
                UseColorMCHead = true
            },
            new()
            {
                Name = "ColorMC.Core.pdb",
                SHA1 = WebSha1s[1],
                Url = $"{ColorMCAPI.CheckUrl}ColorMC.Core.pdb",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Core.pdb",
                Overwrite = true,
                UseColorMCHead = true
            },
            new()
            {
                Name = "ColorMC.Gui.dll",
                SHA1 = WebSha1s[2],
                Url = $"{ColorMCAPI.CheckUrl}ColorMC.Gui.dll",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Gui.dll",
                Overwrite = true,
                UseColorMCHead = true
            },
            new()
            {
                Name = "ColorMC.Gui.pdb",
                SHA1 = WebSha1s[3],
                Url = $"{ColorMCAPI.CheckUrl}ColorMC.Gui.pdb",
                Local = $"{ColorMCGui.RunDir}dll/ColorMC.Gui.pdb",
                Overwrite = true,
                UseColorMCHead = true
            }
        };

        var res = await DownloadManager.StartAsync(list);
        if (res)
        {
            App.Reboot();
        }
        else
        {
            App.ShowError(App.Lang("Gui.Error22"), "");
        }
    }

    public static async Task<(bool?, string?)> CheckOne()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return (false, null);
        }

        try
        {
            var obj = await ColorMCAPI.GetUpdateSha1();
            if (obj == null || obj.TryGetValue("res", out _))
            {
                App.ShowError(App.Lang("SettingWindow.Tab3.Error2"), "Json Error");
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
                    return (true, data1?.ToString() ?? App.Lang("Gui.Info20"));
                }
            }

            return (false, null);
        }
        catch (Exception e)
        {
            App.ShowError(App.Lang("SettingWindow.Tab3.Error2"), e);
        }

        return (null, null);
    }
}
