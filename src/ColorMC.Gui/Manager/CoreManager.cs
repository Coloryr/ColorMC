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
        ColorMCCore.GameLog += ColorMCCore_GameLog;
        ColorMCCore.GameExit += ColorMCCore_GameExit;
        ColorMCCore.InstanceChange += ColorMCCore_InstanceChange;
        ColorMCCore.Download += ColorMCCore_Download;
    }

    private static void ColorMCCore_Download(DownloadEventArgs obj)
    {
        obj.GuiHandel = WindowManager.ShowDownload(obj.Thread);
    }

    private static void ColorMCCore_GameLog(GameLogEventArgs obj)
    {
        if (obj.LogItem != null)
        {
            //给系统日志填充内容
            if (obj.LogItem.LogType != GameSystemLog.None)
            {
                obj.LogItem.Log = obj.LogItem.LogType.GetName(obj.Game, obj.LogItem);
            }

            GameManager.AddGameLog(obj);
        }
    }

    private static void ColorMCCore_Error(CoreErrorEventArgs arg)
    {
        string log = "";
        string title = "";
        if (arg is ConfigLoadErrorEventArgs)
        {
            log = LanguageUtils.Get("Text.ConfigError");
            title = LanguageUtils.Get("Core.Error.Log35");
        }
        else if (arg is ConfigSaveErrorEventArgs)
        {
            log = LanguageUtils.Get("Core.Error.Log1");
            title = LanguageUtils.Get("Core.Error.Log35");
        }
        else if (arg is DownloadSizeErrorEvnetArgs arg3)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log3"), arg3.File.Name, arg3.File.Url, arg3.File.AllSize, arg3.Size);
        }
        else if (arg is DownloadHashErrorEventArgs arg4)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log6"), arg4.File.Name, arg4.File.Url, arg4.Hash, arg4.Now);
        }
        else if (arg is DownloadExceptionErrorEventArgs arg5)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log34"), arg5.File.Name, arg5.File.Url);
        }
        else if (arg is GameLangLoadErrorEventArgs arg6)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log40"), arg6.Key);
        }
        else if (arg is GameServerPackErrorEventArgs arg7)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log31"), arg7.Game.Name);
        }
        else if (arg is GameLogFileErrorEventArgs arg8)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log27"), arg8.Game.Name, arg8.File);
        }
        else if (arg is GameModErrorEventArgs arg9)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log19"), arg9.Game.Name, arg9.File);
        }
        else if (arg is GameModAddErrorEventArgs arg10)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log20"), arg10.Game.Name, arg10.File);
        }
        else if (arg is GameModReadErrorEventArgs arg11)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log30"), arg11.Game.Name, arg11.File);
        }
        else if (arg is GameResourcepackReadErrorEventArgs arg12)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log12"), arg12.Game.Name, arg12.File);
        }
        else if (arg is GameSaveAddErrorEventArgs arg13)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log13"), arg13.Game.Name, arg13.File);
        }
        else if (arg is GameSaveRestoreErrorEventArgs arg14)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log25"), arg14.Game.Name, arg14.File);
        }
        else if (arg is GameSaveReadErrorEventArgs arg15)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log21"), arg15.Game.Name, arg15.File);
        }
        else if (arg is GameSchematicAddErrorEventArgs arg16)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log14"), arg16.Game.Name, arg16.File);
        }
        else if (arg is GameSchematicReadErrorEventArgs arg17)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log15"), arg17.Game.Name, arg17.File);
        }
        else if (arg is GameServerReadErrorEventArgs arg18)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log22"), arg18.Game.Name);
        }
        else if (arg is GameServerAddErrorEventArgs arg19)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log16"), arg19.Game.Name, arg19.Name, arg19.IP);
        }
        else if (arg is GameServerDeleteErrorEventArgs arg20)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log17"), arg20.Server.Game.Name, arg20.Server.Name, arg20.Server.IP);
        }
        else if (arg is GameShaderpackAddErrorEventArgs arg21)
        {
            log = string.Format(LanguageUtils.Get("Game.Error.Log18"), arg21.Game.Name, arg21.File);
        }
        else if (arg is GameDataPackReadErrorEventArgs arg22)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log32"), arg22.Save.Game.Name, arg22.Save.LevelName, arg22.File);
        }
        else if (arg is GameDataPackDeleteErrorEventArgs arg23)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log33"), arg23.Save.Game.Name, arg23.Save.LevelName);
        }
        else if (arg is InstallModPackErrorEventArgs arg24)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log8"), arg24.File);
        }
        else if (arg is CheckJavaErrorEventArgs arg25)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log11"), arg25.Java);
        }
        else if (arg is ScanJavaErrorEventArgs)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log41"));
        }
        else if (arg is InstanceLoadErrorEventArgs arg26)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log29"), arg26.Path);
        }
        else if (arg is InstanceCreateErrorEventArgs arg27)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log28"), arg27.Name);
        }
        else if (arg is InstanceReadModErrorEventArgs arg28)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log23"), arg28.Game.Name);
        }
        else if (arg is InstanceReadModErrorEventArgs arg29)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log23"), arg29.Game.Name);
        }
        else if (arg is InstanceReadCountErrorEventArgs arg30)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log24"), arg30.Game.Name);
        }
        else if (arg is JavaInstallErrorEventArgs arg31)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log10"), arg31.Name, arg31.Url);
        }
        else if (arg is ApiRequestErrorEventArgs arg32)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log7"), arg32.Url);
        }
        else if (arg is MojangGetVersionErrorEventArgs)
        {
            log = LanguageUtils.Get("Core.Error.Log12");
        }
        else if (arg is PlayerSkinGetErrorEventArgs arg33)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log4"), arg33.Login.AuthType.GetName(), arg33.Login.UUID);
        }
        else if (arg is PlayerCapeGetErrorEventArgs arg34)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log42"), arg34.Login.AuthType.GetName(), arg34.Login.UUID);
        }
        else if (arg is PlayerTexturesGetErrorEventArgs arg35)
        {
            log = string.Format(LanguageUtils.Get("Core.Error.Log5"), arg35.Login.AuthType.GetName(), arg35.Login.UUID);
        }
        else if (arg is LocalMavenErrorEventArgs)
        {
            log = LanguageUtils.Get("Core.Error.Log2");
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
