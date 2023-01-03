using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Game;
using ColorMC.Core.Net.Downloader;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public readonly static Dictionary<GameSettingObj, Process> Games = new();

    public static void Init()
    {
        CoreMain.OnError = ShowError;
        CoreMain.NewStart = ShowNew;
        CoreMain.DownloaderUpdate = DownloaderUpdate;
        CoreMain.ProcessLog = PLog;
    }

    public static void Exit()
    {
        Logs.Stop();
        DownloadManager.Stop();
    }

    public static async Task<bool> Launch(GameSettingObj obj, LoginObj obj1, bool debug)
    {
        var res = await obj.StartGame(obj1, null);
        if (res != null)
        {
            res.Exited += (a, b) =>
            {
                Games.Remove(obj);
            };
            Games.Add(obj, res);
        }
        return res == null;
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        App.DownloaderUpdate(state);
    }

    public static void PLog(Process? p, string? d)
    {

    }

    private static void ShowError(string data, Exception e, bool close)
    {
        App.ShowError(data, e, close);
    }

    private static void ShowNew()
    {
        App.ShowNew();
    }


}
