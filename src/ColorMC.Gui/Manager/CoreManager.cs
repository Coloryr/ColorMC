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
            log = LanguageUtils.Get("Core.Error1");
            title = LanguageUtils.Get("Core.Error120");
        }
        else if (arg is ConfigSaveErrorEventArgs)
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
        else if (arg is GameLangLoadErrorEventArgs arg6)
        {
            log = string.Format(LanguageUtils.Get("Core.Error125"), arg6.Key);
        }
        else if (arg is GameServerPackErrorEventArgs arg7)
        {
            log = string.Format(LanguageUtils.Get("Core.Error101"), arg7.Game.Name);
        }
        else if (arg is GameLogFileErrorEventArgs arg8)
        {
            log = string.Format(LanguageUtils.Get("Core.Error94"), arg8.Game.Name, arg8.File);
        }
        else if (arg is GameModErrorEventArgs arg9)
        {
            log = string.Format(LanguageUtils.Get("Core.Error85"), arg9.Game.Name, arg9.File);
        }
        else if (arg is GameModAddErrorEventArgs arg10)
        {
            log = string.Format(LanguageUtils.Get("Core.Error87"), arg10.Game.Name, arg10.File);
        }
        else if (arg is GameModReadErrorEventArgs arg11)
        {
            log = string.Format(LanguageUtils.Get("Core.Error100"), arg11.Game.Name, arg11.File);
        }
        else if (arg is GameResourcepackReadErrorEventArgs arg12)
        {
            log = string.Format(LanguageUtils.Get("Core.Error127"), arg12.Game.Name, arg12.File);
        }
        else if (arg is GameSaveAddErrorEventArgs arg13)
        {
            log = string.Format(LanguageUtils.Get("Core.Error128"), arg13.Game.Name, arg13.File);
        }
        else if (arg is GameSaveRestoreErrorEventArgs arg14)
        {
            log = string.Format(LanguageUtils.Get("Core.Error92"), arg14.Game.Name, arg14.File);
        }
        else if (arg is GameSaveReadErrorEventArgs arg15)
        {
            log = string.Format(LanguageUtils.Get("Core.Error88"), arg15.Game.Name, arg15.File);
        }
        else if (arg is GameSchematicAddErrorEventArgs arg16)
        {
            log = string.Format(LanguageUtils.Get("Core.Error129"), arg16.Game.Name, arg16.File);
        }
        else if (arg is GameSchematicReadErrorEventArgs arg17)
        {
            log = string.Format(LanguageUtils.Get("Core.Error130"), arg17.Game.Name, arg17.File);
        }
        else if (arg is GameServerReadErrorEventArgs arg18)
        {
            log = string.Format(LanguageUtils.Get("Core.Error89"), arg18.Game.Name);
        }
        else if (arg is GameServerAddErrorEventArgs arg19)
        {
            log = string.Format(LanguageUtils.Get("Core.Error131"), arg19.Game.Name, arg19.Name, arg19.IP);
        }
        else if (arg is GameServerDeleteErrorEventArgs arg20)
        {
            log = string.Format(LanguageUtils.Get("Core.Error132"), arg20.Server.Game.Name, arg20.Server.Name, arg20.Server.IP);
        }
        else if (arg is GameShaderpackAddErrorEventArgs arg21)
        {
            log = string.Format(LanguageUtils.Get("Core.Error133"), arg21.Game.Name, arg21.File);
        }
        else if (arg is GameDataPackReadErrorEventArgs arg22)
        {
            log = string.Format(LanguageUtils.Get("Core.Error117"), arg22.Save.Game.Name, arg22.Save.LevelName, arg22.File);
        }
        else if (arg is GameDataPackDeleteErrorEventArgs arg23)
        {
            log = string.Format(LanguageUtils.Get("Core.Error118"), arg23.Save.Game.Name, arg23.Save.LevelName);
        }
        else if (arg is InstallModPackErrorEventArgs arg24)
        {
            log = string.Format(LanguageUtils.Get("Core.Error50"), arg24.File);
        }
        else if (arg is CheckJavaErrorEventArgs arg25)
        {
            log = string.Format(LanguageUtils.Get("Core.Error57"), arg25.Java);
        }
        else if (arg is ScanJavaErrorEventArgs)
        {
            log = string.Format(LanguageUtils.Get("Core.Error134"));
        }
        else if (arg is InstanceLoadErrorEventArgs arg26)
        {
            log = string.Format(LanguageUtils.Get("Core.Error98"), arg26.Path);
        }
        else if (arg is InstanceCreateErrorEventArgs arg27)
        {
            log = string.Format(LanguageUtils.Get("Core.Error95"), arg27.Name);
        }
        else if (arg is InstanceReadModErrorEventArgs arg28)
        {
            log = string.Format(LanguageUtils.Get("Core.Error90"), arg28.Game.Name);
        }
        else if (arg is InstanceReadModErrorEventArgs arg29)
        {
            log = string.Format(LanguageUtils.Get("Core.Error90"), arg29.Game.Name);
        }
        else if (arg is InstanceReadCountErrorEventArgs arg30)
        {
            log = string.Format(LanguageUtils.Get("Core.Error91"), arg30.Game.Name);
        }
        else if (arg is JavaInstallErrorEventArgs arg31)
        {
            log = string.Format(LanguageUtils.Get("Core.Error54"), arg31.Name, arg31.Url);
        }
        else if (arg is ApiRequestErrorEventArgs arg32)
        {
            log = string.Format(LanguageUtils.Get("Core.Error44"), arg32.Url);
        }
        else if (arg is MojangGetVersionErrorEventArgs)
        {
            log = LanguageUtils.Get("Core.Error61");
        }
        else if (arg is PlayerSkinGetErrorEventArgs arg33)
        {
            log = string.Format(LanguageUtils.Get("Core.Error5"), arg33.Login.AuthType.GetName(), arg33.Login.UUID);
        }
        else if (arg is PlayerCapeGetErrorEventArgs arg34)
        {
            log = string.Format(LanguageUtils.Get("Core.Error135"), arg34.Login.AuthType.GetName(), arg34.Login.UUID);
        }
        else if (arg is PlayerTexturesGetErrorEventArgs arg35)
        {
            log = string.Format(LanguageUtils.Get("Core.Error6"), arg35.Login.AuthType.GetName(), arg35.Login.UUID);
        }
        else if (arg is LocalMavenErrorEventArgs)
        {
            log = LanguageUtils.Get("Core.Error3");
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
