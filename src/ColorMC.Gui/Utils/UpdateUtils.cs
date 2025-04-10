using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 启动器更新器
/// </summary>
public static class UpdateUtils
{
    public static readonly string[] WebSha1s = ["", ""];
    public static readonly string[] Sha1s = ["", ""];
    public static readonly string[] LocalPath = ["", ""];

    public static readonly string[] LaunchFiles =
    ["av_libglesv2.dll", "ColorMC.Launcher.exe",
    "libHarfBuzzSharp.dll", "libSkiaSharp.dll", "SDL2.dll"];

    /// <summary>
    /// 初始化更新器
    /// </summary>
    public static void Init()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return;
        }

        LocalPath[0] = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameDllDir, "ColorMC.Core.dll");
        LocalPath[1] = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameDllDir, "ColorMC.Gui.dll");

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

    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool, bool, string?)> Check()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return (false, false, null);
        }

        try
        {
            var obj = await ColorMCCloudAPI.GetUpdateIndex();
            if (obj == null)
            {
                UpdateCheckFail();
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
            UpdateCheckFail();
            Logs.Error(App.Lang("SettingWindow.Tab3.Error2"), e);
        }

        return (false, false, null);
    }

    /// <summary>
    /// 开始更新
    /// </summary>
    public static async void StartUpdate()
    {
        if (ColorMCGui.BaseSha1 == null)
            return;

        var list = new List<FileItemObj>()
        {
            new()
            {
                Name = "ColorMC.Core.dll",
                Sha1 = WebSha1s[0],
                Url = $"{ColorMCCloudAPI.CheckUrl}ColorMC.Core.dll",
                Local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameDllDir, "ColorMC.Core.dll"),
                Overwrite = true
            },
            new()
            {
                Name = "ColorMC.Gui.dll",
                Sha1 = WebSha1s[1],
                Url = $"{ColorMCCloudAPI.CheckUrl}ColorMC.Gui.dll",
                Local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameDllDir, "ColorMC.Gui.dll"),
                Overwrite = true
            }
        };

        var res = await DownloadManager.StartAsync(list);
        if (res)
        {
            ColorMCGui.Reboot();
        }
        else
        {
            WindowManager.ShowError(App.Lang("UpdateChecker.Error1"), "");
        }
    }

    /// <summary>
    /// 检测更新
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool?, string?)> CheckOne()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return (false, null);
        }

        try
        {
            var obj = await ColorMCCloudAPI.GetUpdateSha1();
            if (obj == null || obj.TryGetValue("res", out _))
            {
                WindowManager.ShowError(App.Lang("SettingWindow.Tab3.Error2"), "Json Error");
                return (false, null);
            }

            WebSha1s[0] = obj["core.dll"]!.ToString();
            WebSha1s[1] = obj["gui.dll"]!.ToString();

            Logs.Info($"ColorMC.Core.dll:{Sha1s[0]} Web:{WebSha1s[0]}");
            Logs.Info($"ColorMC.Gui.dll:{Sha1s[1]} Web:{WebSha1s[1]}");

            for (int a = 0; a < 2; a++)
            {
                if (WebSha1s[a] != Sha1s[a])
                {
                    obj.TryGetValue("text", out var data1);
                    return (true, data1?.ToString() ?? App.Lang("UpdateChecker.Info1"));
                }
            }

            return (false, null);
        }
        catch (Exception e)
        {
            WindowManager.ShowError(App.Lang("SettingWindow.Tab3.Error2"), e);
        }

        return (null, null);
    }

    /// <summary>
    /// 检测更新失败
    /// </summary>
    public static void UpdateCheckFail()
    {
        var window = WindowManager.GetMainWindow();
        if (window == null)
        {
            return;
        }
        window.Model.Notify(App.Lang("SettingWindow.Tab3.Error2"));
    }
}
