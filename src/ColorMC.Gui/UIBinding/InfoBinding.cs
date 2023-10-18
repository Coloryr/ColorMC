using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class InfoBinding
{
    public static BaseModel? Window { get; set; }

    public static void Init()
    {
        ColorMCCore.OfflineLaunch = OfflineLaunch;
        ColorMCCore.GameLaunch = GameLunch;
        ColorMCCore.GameRequest = GameRequest;
        ColorMCCore.LaunchP = LaunchP;
        ColorMCCore.UpdateState = UpdateState;
    }

    public static void Launch()
    {
        ColorMCCore.GameRequest = GameRequest;
    }

    /// <summary>
    /// 更新状态回调
    /// </summary>
    /// <param name="info">信息</param>
    private static void UpdateState(string? info)
    {
        if (Window is { } win)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (info == null)
                {
                    win.ProgressClose();
                }
                else
                {
                    win.Progress(info);
                }
            });
        }
    }

    private static Task<bool> LaunchP(bool pre)
    {
        if (Window == null)
        {
            return Task.Run(() => { return false; });
        }

        return Dispatcher.UIThread.InvokeAsync(() =>
            Window.ShowWait(pre ? App.GetLanguage("MainWindow.Info29")
            : App.GetLanguage("MainWindow.Info30")));
    }

    private static Task<bool> GameRequest(string state)
    {
        if (Window == null)
        {
            return Task.Run(() => { return false; });
        }

        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return Window.ShowWait(state);
        });
    }

    private static Task<bool> OfflineLaunch(LoginObj login)
    {
        if (Window == null)
        {
            return Task.Run(() => { return false; });
        }
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return Window.ShowWait(string.Format(
                App.GetLanguage("MainWindow.Info21"), login.UserName));
        });
    }

    private static void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Window == null)
                return;
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                switch (state)
                {
                    case LaunchState.Login:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info8"));
                        break;
                    case LaunchState.Check:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info9"));
                        break;
                    case LaunchState.CheckVersion:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info10"));
                        break;
                    case LaunchState.CheckLib:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info11"));
                        break;
                    case LaunchState.CheckAssets:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info12"));
                        break;
                    case LaunchState.CheckLoader:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info13"));
                        break;
                    case LaunchState.CheckLoginCore:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info14"));
                        break;
                    case LaunchState.CheckMods:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info17"));
                        break;
                    case LaunchState.Download:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info15"));
                        break;
                    case LaunchState.JvmPrepare:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info16"));
                        break;
                    case LaunchState.LaunchPre:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info31"));
                        break;
                    case LaunchState.LaunchPost:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info32"));
                        break;
                    case LaunchState.InstallForge:
                        Window.ProgressUpdate(App.GetLanguage("MainWindow.Info38"));
                        break;
                    case LaunchState.End:
                        Window.ProgressClose();
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case LaunchState.Login:
                        Window.Title1 = App.GetLanguage("MainWindow.Info8");
                        break;
                    case LaunchState.Check:
                        Window.Title1 = App.GetLanguage("MainWindow.Info9");
                        break;
                    case LaunchState.CheckVersion:
                        Window.Title1 = App.GetLanguage("MainWindow.Info10");
                        break;
                    case LaunchState.CheckLib:
                        Window.Title1 = App.GetLanguage("MainWindow.Info11");
                        break;
                    case LaunchState.CheckAssets:
                        Window.Title1 = App.GetLanguage("MainWindow.Info12");
                        break;
                    case LaunchState.CheckLoader:
                        Window.Title1 = App.GetLanguage("MainWindow.Info13");
                        break;
                    case LaunchState.CheckLoginCore:
                        Window.Title1 = App.GetLanguage("MainWindow.Info14");
                        break;
                    case LaunchState.CheckMods:
                        Window.Title1 = App.GetLanguage("MainWindow.Info17");
                        break;
                    case LaunchState.Download:
                        Window.Title1 = App.GetLanguage("MainWindow.Info15");
                        break;
                    case LaunchState.JvmPrepare:
                        Window.Title1 = App.GetLanguage("MainWindow.Info16");
                        break;
                    case LaunchState.LaunchPre:
                        Window.Title1 = App.GetLanguage("MainWindow.Info31");
                        break;
                    case LaunchState.LaunchPost:
                        Window.Title1 = App.GetLanguage("MainWindow.Info32");
                        break;
                    case LaunchState.InstallForge:
                        Window.Title1 = App.GetLanguage("MainWindow.Info38");
                        break;
                    case LaunchState.End:
                        Window.Title1 = "";
                        break;
                }
            }
        });
    }
}
