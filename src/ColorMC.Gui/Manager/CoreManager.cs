using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

public static class CoreManager
{
    public static void Init()
    {
        ColorMCCore.Error += WindowManager.ShowError;
        ColorMCCore.LanguageReload += LanguageReload;
        ColorMCCore.GameLog += GameManager.AddGameLog;
        ColorMCCore.OnDownload = WindowManager.ShowDownload;
        ColorMCCore.GameExit += GameExit;
        ColorMCCore.InstanceChange += InstanceChange;
        ColorMCCore.InstanceIconChange += InstanceIconChange;
    }

    private static void InstanceIconChange(GameSettingObj obj)
    {
        WindowManager.MainWindow?.IconChange(obj.UUID);
    }

    private static void InstanceChange()
    {
        WindowManager.MainWindow?.LoadMain();
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
        GameManager.GameExit(obj.UUID);
        GameCount.GameClose(obj);
        UserBinding.UnLockUser(obj1);
        Dispatcher.UIThread.Post(() =>
        {
            WindowManager.MainWindow?.GameClose(obj.UUID);
        });
        if (code != 0 && !ColorMCGui.IsClose)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WindowManager.ShowGameLog(obj, code);
                WindowManager.MainWindow?.ShowMessage(App.Lang("Live2dControl.Text3"));
            });
        }
        else
        {
            if (GameCloudUtils.Connect && !ColorMCGui.IsClose)
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
