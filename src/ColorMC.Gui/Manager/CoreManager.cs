using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// ColorMCCore管理器
/// </summary>
public static class CoreManager
{
    /// <summary>
    /// 初始化Core事件
    /// </summary>
    public static void Init()
    {
        ColorMCCore.Error += ColorMCCore_Error;
        ColorMCCore.GameLog += GameManager.AddGameLog;
        ColorMCCore.GameExit += ColorMCCore_GameExit;
        ColorMCCore.InstanceChange += ColorMCCore_InstanceChange;
    }

    private static void ColorMCCore_Error(CoreErrorEventArgs arg)
    {
        string log = "";
        string title = "";
        if (arg is ConfigLoadErrorEventArgs arg1)
        {
            log = LanguageUtils.Get("Core.Error1");
            title = LanguageUtils.Get("Core.Error120");
        }
        else if (arg is ConfigSaveErrorEventArgs arg2)
        {
            log = LanguageUtils.Get("Core.Error2");
            title = LanguageUtils.Get("Core.Error120");
        }
        else if (arg is DownloadSizeErrorEvnetArgs arg3)
        {
            log = string.Format(LanguageUtils.Get("Core.Error4"), arg3.File.Name, arg3.File.Url, arg3.File.AllSize, arg3.Size);
        }
        else if (arg is DownloadHashErrorEventArgs arg4)
        {
            log = string.Format(LanguageUtils.Get("Core.Error12"), arg4.File.Name, arg4.File.Url, arg4.Hash, arg4.Now);
        }
        else if (arg is DownloadExceptionErrorEventArgs arg5)
        {
            log = string.Format(LanguageUtils.Get("Core.Error119"), arg5.File.Name, arg5.File.Url);
        }

        if (arg is ExceptionErrorEventArgs error)
        {
            Logs.Error(log, error.Exception);
            if (arg.Show)
            {
                WindowManager.ShowError(title, log, error.Exception, arg.Close);
            }
        }
        else
        {
            Logs.Error(log);
            if (arg.Show)
            {
                WindowManager.ShowError(title, log, arg.Close);
            }
        }
    }

    /// <summary>
    /// 实例修改后
    /// </summary>
    private static void ColorMCCore_InstanceChange(InstanceChangeEventArgs args)
    {
        if (args.Type == InstanceChangeType.NumberChange)
        {
            WindowManager.MainWindow?.LoadGameItem();
        }
        else if (args.Type == InstanceChangeType.IconChange && args.Game != null)
        {
            ImageManager.ReloadImage(args.Game);
        }
    }

    /// <summary>
    /// 游戏退出时
    /// </summary>
    /// <param name="args"></param>
    private static void ColorMCCore_GameExit(GameExitEventArgs args)
    {
        GameManager.GameExit(args.Game);
        GameCountUtils.GameClose(args.Game);
        UserManager.UnlockUser(args.Login);
        Dispatcher.UIThread.Post(() =>
        {
            WindowManager.MainWindow?.GameClose(args.Game.UUID);
        });
        //如果不是状态0退出
        if (args.Code != 0 && !ColorMCGui.IsClose)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WindowManager.ShowGameLog(args.Game, args.Code);
            });
        }
        else
        {
            //检查云同步
            if (ColorMCCloudAPI.Connect && !ColorMCGui.IsClose)
            {
                Task.Run(() =>
                {
                    GameBinding.CheckCloudAndOpen(args.Game);
                });
            }
            else
            {
                Dispatcher.UIThread.Post(App.TestClose);
            }
        }

        GameBinding.GameStateUpdate(args.Game);
    }
}
