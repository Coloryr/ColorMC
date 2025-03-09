using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.Threading.Tasks;

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
        ColorMCCore.Error += WindowManager.ShowError;
        ColorMCCore.LanguageReload += LanguageReload;
        ColorMCCore.GameLog += GameManager.AddGameLog;
        ColorMCCore.OnDownload = WindowManager.ShowDownload;
        ColorMCCore.GameExit += GameExit;
        ColorMCCore.InstanceChange += InstanceChange;
        ColorMCCore.InstanceIconChange += InstanceIconChange;
    }

    /// <summary>
    /// 实例图标修改后
    /// </summary>
    /// <param name="obj"></param>
    private static void InstanceIconChange(GameSettingObj obj)
    {
        WindowManager.MainWindow?.IconChange(obj.UUID);
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
        UserBinding.UnLockUser(obj1);
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
                WindowManager.MainWindow?.ShowMessage(App.Lang("Live2dControl.Text3"));
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
