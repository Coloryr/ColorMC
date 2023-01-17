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
        if (App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.ClearLog();
        }
        var res = await obj.StartGame(obj1, null);
        if (res != null)
        {
            res.Exited += (a, b) =>
            {
                Games.Remove(res);
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
        App.ShowHello();
    }

    private static void Change(LanguageType type)
    {
        App.LoadLanguage(type);
        Localizer.Instance.Reload();
    }
}
