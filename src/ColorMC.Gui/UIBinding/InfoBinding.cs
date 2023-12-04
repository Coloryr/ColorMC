using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class InfoBinding
{
    public static BaseModel? Window { get; set; }

    public static void Init()
    {
        ColorMCCore.OfflineLaunch = (login) =>
        {
            if (Window == null)
            {
                return Task.Run(() => { return false; });
            }
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                return Window.ShowWait(string.Format(
                    App.Lang("MainWindow.Info21"), login.UserName));
            });
        };
        ColorMCCore.GameLaunch = GameLunch;
        ColorMCCore.GameRequest = GameRequest;
        ColorMCCore.LaunchP = (pre) =>
        {
            if (Window == null)
            {
                return Task.Run(() => { return false; });
            }

            return Dispatcher.UIThread.InvokeAsync(() =>
                Window.ShowWait(pre ? App.Lang("MainWindow.Info29")
                : App.Lang("MainWindow.Info30")));
        };
        ColorMCCore.UpdateState = (info) =>
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
        };
    }

    public static void Launch()
    {
        ColorMCCore.GameRequest = GameRequest;
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

    private static void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Window == null)
            {
                return;
            }
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                switch (state)
                {
                    case LaunchState.Login:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info8"));
                        break;
                    case LaunchState.Check:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info9"));
                        break;
                    case LaunchState.CheckVersion:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info10"));
                        break;
                    case LaunchState.CheckLib:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info11"));
                        break;
                    case LaunchState.CheckAssets:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info12"));
                        break;
                    case LaunchState.CheckLoader:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info13"));
                        break;
                    case LaunchState.CheckLoginCore:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info14"));
                        break;
                    case LaunchState.CheckMods:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info17"));
                        break;
                    case LaunchState.Download:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info15"));
                        break;
                    case LaunchState.JvmPrepare:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info16"));
                        break;
                    case LaunchState.LaunchPre:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info31"));
                        break;
                    case LaunchState.LaunchPost:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info32"));
                        break;
                    case LaunchState.InstallForge:
                        Window.ProgressUpdate(App.Lang("MainWindow.Info38"));
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
                        Window.Title1 = App.Lang("MainWindow.Info8");
                        break;
                    case LaunchState.Check:
                        Window.Title1 = App.Lang("MainWindow.Info9");
                        break;
                    case LaunchState.CheckVersion:
                        Window.Title1 = App.Lang("MainWindow.Info10");
                        break;
                    case LaunchState.CheckLib:
                        Window.Title1 = App.Lang("MainWindow.Info11");
                        break;
                    case LaunchState.CheckAssets:
                        Window.Title1 = App.Lang("MainWindow.Info12");
                        break;
                    case LaunchState.CheckLoader:
                        Window.Title1 = App.Lang("MainWindow.Info13");
                        break;
                    case LaunchState.CheckLoginCore:
                        Window.Title1 = App.Lang("MainWindow.Info14");
                        break;
                    case LaunchState.CheckMods:
                        Window.Title1 = App.Lang("MainWindow.Info17");
                        break;
                    case LaunchState.Download:
                        Window.Title1 = App.Lang("MainWindow.Info15");
                        break;
                    case LaunchState.JvmPrepare:
                        Window.Title1 = App.Lang("MainWindow.Info16");
                        break;
                    case LaunchState.LaunchPre:
                        Window.Title1 = App.Lang("MainWindow.Info31");
                        break;
                    case LaunchState.LaunchPost:
                        Window.Title1 = App.Lang("MainWindow.Info32");
                        break;
                    case LaunchState.InstallForge:
                        Window.Title1 = App.Lang("MainWindow.Info38");
                        break;
                    case LaunchState.End:
                        Window.Title1 = "";
                        break;
                }
            }
        });
    }
}
