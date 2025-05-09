using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 启动器更新器
/// </summary>
public static class UpdateUtils
{
    public static readonly string[] WebSha1s = ["", ""];
    public static readonly string[] LocalSha1s = ["", ""];
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

        for (int a = 0; a < 2; a++)
        {
            if (File.Exists(LocalPath[a]))
            {
                using var file = PathHelper.OpenRead(LocalPath[a])!;
                LocalSha1s[a] = HashHelper.GenSha1(file);
            }
            else
            {
                LocalSha1s[a] = ColorMCGui.BaseSha1[a];
            }
        }
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool, bool, string?)> CheckMain()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return (false, false, null);
        }

        try
        {
            var obj = await ColorMCCloudAPI.GetMainIndex();
            if (obj == null)
            {
                UpdateCheckFail();
                return (false, false, null);
            }
            var json = obj.RootElement;

            if (json.TryGetProperty("Version", out var temp)
                && ColorMCCore.TopVersion != temp.GetString())
            {
                return (true, true, json.GetProperty("Text").GetString());
            }
            var data1 = await CheckNowVersion();
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
    public static void StartUpdate(BaseModel model)
    {
        if (ColorMCGui.BaseSha1 == null)
            return;

        if (!File.Exists(LocalPath[0]) || !File.Exists(LocalPath[1]))
        {
            StartDownloadUpdate(model);
        }
        else
        {
            StartPatchUpdate(model);
        }
    }

    /// <summary>
    /// 开始修补更新
    /// </summary>
    private static async void StartPatchUpdate(BaseModel model)
    {
        if (ColorMCGui.BaseSha1 == null)
            return;

        model.Progress(App.Lang("UpdateChecker.Info2"));
        var obj = await ColorMCCloudAPI.GetUpdateIndex();
        if (obj == null)
        {
            model.Show(App.Lang("UpdateChecker.Error3"));
            return;
        }
        var json = obj.RootElement;

        if (!json.TryGetProperty("update", out var list) 
            || list.ValueKind is not JsonValueKind.Array
            || list.Deserialize(JsonGuiType.ListUpdateObj) is not { } update)
        {
            model.Show(App.Lang("UpdateChecker.Error3"));
            return;
        }
        bool find = false;
        var diffs = new List<string>();
        var down = new List<FileItemObj>();
        foreach (var item in update)
        {
            if (find || item.Core == LocalSha1s[0] && item.Gui == LocalSha1s[1])
            {
                find = true;
                string file = Path.Combine(DownloadManager.DownloadDir, item.Diff);
                diffs.Add(file);
                down.Add(new()
                {
                    Name = "colormc_" + item.Diff,
                    Local = file,
                    Sha1 = item.Sha1,
                    Url = ColorMCCloudAPI.UpdateUrl + item.Diff
                });
            }
        }
        if (find)
        {
            var info = await ToolUtils.InitHdiff();
            if (info.Item1 == null)
            {
                model.ProgressClose();
                model.Show(App.Lang("UpdateChecker.Error1"));
                return;
            }
            if (info.Item2 != null)
            {
                down.Add(info.Item2);
            }
            var res = await DownloadManager.StartAsync(down);
            if (res)
            {
                model.ProgressUpdate(App.Lang("UpdateChecker.Info3"));
                res = await StartPatch(info.Item1, diffs);
                model.ProgressClose();
                if (res)
                {
                    ColorMCGui.Reboot();
                }
                else
                {
                    model.Show(App.Lang("UpdateChecker.Error2"));
                }
            }
            else
            {
                model.ProgressClose();
                model.Show(App.Lang("UpdateChecker.Error1"));
            }
        }
        else
        {
            StartDownloadUpdate(model);
        }
    }

    /// <summary>
    /// 开始基文件更新
    /// </summary>
    private static async void StartDownloadUpdate(BaseModel model)
    {
        if (ColorMCGui.BaseSha1 == null)
            return;

        model.Progress(App.Lang("UpdateChecker.Info2"));
        var list = new List<FileItemObj>()
        {
            new()
            {
                Name = "ColorMC.Core.dll",
                Sha1 = WebSha1s[0],
                Url = $"{ColorMCCloudAPI.UpdateUrl}ColorMC.Core.dll",
                Local = LocalPath[0],
                Overwrite = true
            },
            new()
            {
                Name = "ColorMC.Gui.dll",
                Sha1 = WebSha1s[1],
                Url = $"{ColorMCCloudAPI.UpdateUrl}ColorMC.Gui.dll",
                Local = LocalPath[1],
                Overwrite = true
            }
        };

        var res = await DownloadManager.StartAsync(list);
        model.ProgressClose();
        if (res)
        {
            ColorMCGui.Reboot();
        }
        else
        {
            model.Show(App.Lang("UpdateChecker.Error1"));
        }
    }

    /// <summary>
    /// 检测更新
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool?, string?)> CheckNowVersion()
    {
        if (ColorMCGui.BaseSha1 == null)
        {
            return (false, null);
        }

        try
        {
            var obj = await ColorMCCloudAPI.GetUpdateIndex();
            if (obj == null)
            {
                WindowManager.ShowError(App.Lang("SettingWindow.Tab3.Error2"), "Json Error");
                return (false, null);
            }
            var json = obj.RootElement;
            if (!json.TryGetProperty("index", out var index)
                || index.ValueKind is not JsonValueKind.Object)
            {
                return (false, null);
            }

            WebSha1s[0] = index.GetProperty("core").GetString()!;
            WebSha1s[1] = index.GetProperty("gui").GetString()!;

            Logs.Info($"ColorMC.Core.dll:{LocalSha1s[0]} Web:{WebSha1s[0]}");
            Logs.Info($"ColorMC.Gui.dll:{LocalSha1s[1]} Web:{WebSha1s[1]}");

            for (int a = 0; a < 2; a++)
            {
                if (WebSha1s[a] != LocalSha1s[a])
                {
                    index.TryGetProperty("info", out var data1);
                    return (true, data1.GetString() ?? App.Lang("UpdateChecker.Info1"));
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
    private static void UpdateCheckFail()
    {
        var window = WindowManager.GetMainWindow();
        if (window == null)
        {
            return;
        }
        window.Model.Notify(App.Lang("SettingWindow.Tab3.Error2"));
    }

    private static async Task<bool> StartPatch(string file, List<string> files)
    {
        return await Task.Run(() =>
        {
            try
            {
                var dir1 = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameDllDir);
                var dir2 = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameDllNewDir);
                foreach (var item in files)
                {
                    var patcher = new ProcessStartInfo(file);
                    patcher.ArgumentList.Add(dir1);
                    patcher.ArgumentList.Add(item);
                    patcher.ArgumentList.Add(dir2);
                    var p = Process.Start(patcher);
                    if (p == null)
                    {
                        return false;
                    }
                    p?.WaitForExit();
                    if (p?.ExitCode != 0)
                    {
                        return false;
                    }
                    Directory.Delete(dir1, true);
                    Directory.Move(dir2, dir1);
                }
                return true;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("UpdateChecker.Error1"), e);
                return false;
            }
        });
    }
}
