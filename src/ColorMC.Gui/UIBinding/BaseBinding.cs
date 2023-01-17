using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public readonly static Dictionary<Process, GameSettingObj> Games = new();
    public static bool ISNewStart { get; private set; } = false;

    public static void Init()
    {
        CoreMain.OnError = ShowError;
        CoreMain.NewStart = ShowNew;
        CoreMain.DownloaderUpdate = DownloaderUpdate;
        CoreMain.ProcessLog = PLog;
        CoreMain.GameLog = PLog;
        CoreMain.LanguageReload = Change;

        GuiConfigUtils.Init(AppContext.BaseDirectory);
        CoreMain.Init(AppContext.BaseDirectory);
    }

    public static void Exit()
    {
        Colors.Instance.Stop();
        Logs.Stop();
        DownloadManager.Stop();
    }

    public static async Task<bool> Launch(GameSettingObj obj, LoginObj obj1)
    {
        if (Games.ContainsValue(obj))
        {
            return false;
        }
        if (App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.ClearLog();
        }
        var res = await Task.Run(() => 
        {
            try
            {
                return obj.StartGame(obj1, null).Result;
            }
            catch (Exception e)
            {
                Logs.Error("游戏启动失败", e);
                CoreMain.OnError?.Invoke("游戏启动失败", e, false);
                return null;
            }
        });
        if (res != null)
        {
            res.Exited += (a, b) =>
            {
                if (Games.Remove(res, out var obj1))
                { 
                    App.MainWindow?.GameClose(obj1);
                }
            };
            Games.Add(res, obj);
        }
        return res != null;
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        App.DownloaderUpdate(state);
    }

    public static void PLog(Process? p, string? d)
    {
        if (Games.TryGetValue(p, out var obj)
            && App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.Log(d);
        }
    }

    public static void PLog(GameSettingObj obj, string? d)
    {
        if (App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.Log(d);
        }
    }

    private static void ShowError(string data, Exception e, bool close)
    {
        App.ShowError(data, e, close);
    }

    private static void ShowNew()
    {
        ISNewStart = true;
    }

    private static void Change(LanguageType type)
    {
        App.LoadLanguage(type);
        Localizer.Instance.Reload();
    }
}
