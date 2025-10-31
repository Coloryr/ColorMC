using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
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
        ColorMCCore.LanguageReload += LanguageReload;
        ColorMCCore.GameLog += GameManager.AddGameLog;
        ColorMCCore.OnDownload = WindowManager.ShowDownload;
        ColorMCCore.GameExit += GameExit;
        ColorMCCore.InstanceChange += InstanceChange;
        ColorMCCore.InstanceIconChange += InstanceIconChange;
    }

    private static void ColorMCCore_Error(CoreErrorEventArgs arg)
    {
        string log = "";
        if (arg is ConfigLoadErrorEventArgs arg1)
        {
            log = LanguageUtils.Get("Core.Error1");
        }
        else if (arg is ConfigSaveErrorEventArgs arg2)
        {
            log = LanguageUtils.Get("Core.Error2");
        }
        else if (arg is DownloadSizeErrorEvnetArgs arg3)
        {
            log = string.Format(LanguageUtils.Get("Core.Error4"), arg3.File.Name, arg3.File.Url, arg3.File.AllSize, arg3.Size);
        }
        else if (arg is DownloadHashErrorEventArgs arg4)
        {
            log = string.Format(LanguageUtils.Get("Core.Error12"), arg4.File.Name, arg4.File.Url, arg4.Hash, arg4.Now);
        }


            switch (arg.Type)
            {
                case ErrorType.ConfigLoadError:

                    break;
                case ErrorType.ConfigSaveError:

                    break;
                case ErrorType.DownloadError:
                    log = string.Format(LanguageUtils.Get("Core.Error119"))
                    break;
                case ErrorType.DownloadSizeError:

                    break;
                case ErrorType.DownloadCheckError:
                    
                    break;
            }
        Logs.Error(log, arg.Exception);
        if (arg.Show)
        {
            WindowManager.ShowError(log, arg.Exception, arg.Close);
        }
    }

    /// <summary>
    /// 实例图标修改后
    /// </summary>
    /// <param name="obj"></param>
    private static void InstanceIconChange(GameSettingObj obj)
    {
        ImageManager.ReloadImage(obj);
    }

    /// <summary>
    /// 实例修改后
    /// </summary>
    private static void InstanceChange()
    {
        WindowManager.MainWindow?.LoadGameItem();
    }

    /// <summary>
    /// 语言重载
    /// </summary>
    /// <param name="type"></param>
    private static void LanguageReload(LanguageType type)
    {
        App.LoadLanguage(type);
        LangMananger.Reload();

        ColorMCGui.Reboot();
    }

    /// <summary>
    /// 游戏退出时
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    /// <param name="code"></param>
    private static void GameExit(GameSettingObj obj, LoginObj obj1, int code)
    {
        GameManager.GameExit(obj);
        GameCountUtils.GameClose(obj);
        UserManager.UnlockUser(obj1);
        Dispatcher.UIThread.Post(() =>
        {
            WindowManager.MainWindow?.GameClose(obj.UUID);
        });
        //如果不是状态0退出
        if (code != 0 && !ColorMCGui.IsClose)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WindowManager.ShowGameLog(obj, code);
            });
        }
        else
        {
            //检查云同步
            if (ColorMCCloudAPI.Connect && !ColorMCGui.IsClose)
            {
                Task.Run(() =>
                {
                    GameBinding.CheckCloudAndOpen(obj);
                });
            }
            else
            {
                Dispatcher.UIThread.Post(App.TestClose);
            }
        }

        GameBinding.GameStateUpdate(obj);
    }
}
