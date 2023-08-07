using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public const string DrapType = "Game";

    public readonly static Dictionary<Process, GameSettingObj> Games = new();
    public readonly static Dictionary<string, Process> RunGames = new();
    public readonly static Dictionary<string, StringBuilder> GameLogs = new();
    public static bool ISNewStart => ColorMCCore.NewStart;

    private static IBaseWindow? s_window;
    private static CancellationTokenSource s_launchCancel = new();

    /// <summary>
    /// 初始化
    /// </summary>
    public static async void Init()
    {
        ColorMCCore.OnError = ShowError;
        ColorMCCore.DownloaderUpdate = DownloaderUpdate;
        ColorMCCore.ProcessLog = PLog;
        ColorMCCore.GameLog = PLog;
        ColorMCCore.LanguageReload = ChangeLanguage;
        ColorMCCore.NoJava = NoJava;
        ColorMCCore.UpdateSelect = PackUpdate;
        ColorMCCore.UpdateState = UpdateState;
        ColorMCCore.OfflineLaunch = OfflineLaunch;
        ColorMCCore.GameLaunch = GameLunch;
        ColorMCCore.GameDownload = GameDownload;
        ColorMCCore.LaunchP = LaunchP;

        if (ColorMCGui.RunType == RunType.Program)
        {
            GameCountUtils.Init(ColorMCGui.RunDir);
            ImageUtils.Init(ColorMCGui.RunDir);
            UpdateChecker.Init();
            GameCloudUtils.Init(ColorMCGui.RunDir);
            await GameCloudUtils.StartConnect();

            try
            {
                Media.Init();
            }
            catch (Exception e)
            {
                Logs.Error("error", e);
            }
        }
        FontSel.Instance.Load();
        ColorSel.Instance.Load();
        StyleSel.Instance.Load();
        LoadStyle();
    }

    public static void LoadStyle()
    {
        App.PageSlide500.Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime);
        App.PageSlide500.Fade = GuiConfigUtils.Config.Style.AmFade;
    }

    private static Task<bool> LaunchP(bool pre)
    {
        if (s_window == null)
        {
            return Task.Run(() => { return false; });
        }

        return Dispatcher.UIThread.InvokeAsync(() =>
            s_window.OkInfo.ShowWait(pre ? App.GetLanguage("MainWindow.Info29")
            : App.GetLanguage("MainWindow.Info30")));
    }

    private static Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        if (s_window == null)
        {
            return Task.Run(() => { return false; });
        }

        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return state switch
            {
                LaunchState.LostLib => await s_window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info5")),
                LaunchState.LostLoader => await s_window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info6")),
                LaunchState.LostLoginCore => await s_window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info7")),
                _ => await s_window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info4")),
            };
        });
    }

    private static Task<bool> OfflineLaunch(LoginObj login)
    {
        if (s_window == null)
        {
            return Task.Run(() => { return false; });
        }
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return s_window.OkInfo.ShowWait(string.Format(
                App.GetLanguage("MainWindow.Info21"), login.UserName));
        });
    }

    private static void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (s_window == null)
                return;
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                switch (state)
                {
                    case LaunchState.Login:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info8"));
                        break;
                    case LaunchState.Check:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info9"));
                        break;
                    case LaunchState.CheckVersion:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info10"));
                        break;
                    case LaunchState.CheckLib:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info11"));
                        break;
                    case LaunchState.CheckAssets:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info12"));
                        break;
                    case LaunchState.CheckLoader:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info13"));
                        break;
                    case LaunchState.CheckLoginCore:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info14"));
                        break;
                    case LaunchState.CheckMods:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info17"));
                        break;
                    case LaunchState.Download:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info15"));
                        break;
                    case LaunchState.JvmPrepare:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info16"));
                        break;
                    case LaunchState.LaunchPre:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info31"));
                        break;
                    case LaunchState.LaunchPost:
                        s_window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info32"));
                        break;
                    case LaunchState.End:
                        s_window.ProgressInfo.Close();
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case LaunchState.Login:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info8");
                        break;
                    case LaunchState.Check:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info9");
                        break;
                    case LaunchState.CheckVersion:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info10");
                        break;
                    case LaunchState.CheckLib:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info11");
                        break;
                    case LaunchState.CheckAssets:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info12");
                        break;
                    case LaunchState.CheckLoader:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info13");
                        break;
                    case LaunchState.CheckLoginCore:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info14");
                        break;
                    case LaunchState.CheckMods:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info17");
                        break;
                    case LaunchState.Download:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info15");
                        break;
                    case LaunchState.JvmPrepare:
                        s_window.Head.Title1 = App.GetLanguage("MainWindow.Info16");
                        break;
                    case LaunchState.End:
                        s_window.Head.Title1 = "";
                        break;
                }
            }
        });
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="text">文本</param>
    public static async Task CopyTextClipboard(TopLevel? level, string text)
    {
        if (level?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(text);
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="file">文件列表</param>
    public static async Task CopyFileClipboard(TopLevel? level, List<IStorageFile> file)
    {
        if (level?.Clipboard is { } clipboard)
        {
            var obj = new DataObject();
            obj.Set(DataFormats.Files, file);
            await clipboard.SetDataObjectAsync(obj);
        }
    }

    /// <summary>
    /// 更新状态回调
    /// </summary>
    /// <param name="info">信息</param>
    public static void UpdateState(string info)
    {
        var window = App.GetMainWindow();
        if (window == null)
        {
            return;
        }
        Dispatcher.UIThread.Post(() =>
        {
            window.ProgressInfo.Show(info);
        });
    }

    /// <summary>
    /// 更新整合包状态回调
    /// </summary>
    /// <param name="info">状态</param>
    public static Task<bool> PackUpdate(string info)
    {
        return Dispatcher.UIThread.InvokeAsync(() => App.HaveUpdate(info));
    }

    /// <summary>
    /// 找不到Java回调
    /// </summary>
    public static void NoJava()
    {
        Dispatcher.UIThread.Post(() =>
        {
            App.ShowSetting(SettingType.SetJava);
        });
    }

    /// <summary>
    /// 游戏实例是否在运行
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static bool IsGameRun(GameSettingObj obj)
    {
        return RunGames.ContainsKey(obj.UUID);
    }

    /// <summary>
    /// 是否有游戏在运行
    /// </summary>
    public static bool IsGameRuning()
    {
        return RunGames.Count > 0;
    }

    /// <summary>
    /// 强制停止游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void StopGame(GameSettingObj obj)
    {
        s_launchCancel.Cancel();
        if (RunGames.TryGetValue(obj.UUID, out var item))
        {
            Task.Run(() => item.Kill(true));
        }
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="obj1">保存的账户</param>
    /// <returns>结果</returns>
    public static async Task<(bool, string?)> Launch(IBaseWindow window,
        GameSettingObj obj, LoginObj obj1, WorldObj? world = null)
    {
        s_window = window;
        s_launchCancel = new();

        if (Games.ContainsValue(obj))
        {
            return (false, App.GetLanguage("GameBinding.Error4"));
        }
        //设置自动加入服务器
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfo(GuiConfigUtils.Config.ServerCustom.IP,
                GuiConfigUtils.Config.ServerCustom.Port);

            obj = obj.CopyObj();
            obj.StartServer ??= new();
            obj.StartServer.IP = server.ServerAddress;
            obj.StartServer.Port = server.ServerPort;
        }
        if (App.GameLogWindows.TryGetValue(obj.UUID, out var win))
        {
            win.ClearLog();
        }

        ColorMCCore.DownloaderUpdate = DownloaderUpdateOnThread;

        //清空日志
        if (GameLogs.ContainsKey(obj.UUID))
        {
            GameLogs[obj.UUID].Clear();
        }
        else
        {
            GameLogs.Add(obj.UUID, new());
        }
        //锁定账户
        UserBinding.AddLockUser(obj1);

        var res = await Task.Run(async () => await Launch(obj, obj1, world, s_launchCancel.Token));

        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.End);
        Funtions.RunGC();

        if (s_launchCancel.IsCancellationRequested)
            return (true, null);

        var p = res.Item1;

        if (p != null)
        {
            obj.LaunchData.LastTime = DateTime.Now;
            obj.SaveLaunchData();
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                _ = Task.Run(() =>
                {
                    Task.Delay(1000);
                    try
                    {
                        p.WaitForInputIdle();

                        if (p.HasExited)
                            return;

                        Dispatcher.UIThread.Post(() =>
                        {
                            App.Hide();
                        });
                    }
                    catch
                    {

                    }
                });
            }

            if (GuiConfigUtils.Config.ServerCustom.RunPause)
            {
                Media.Pause();
            }

            App.MainWindow?.ShowMessage(App.GetLanguage("Live2D.Text2"));

            p.Exited += (a, b) =>
            {
                GameCountUtils.GameClose(obj.UUID);
                RunGames.Remove(obj.UUID);
                UserBinding.UnLockUser(obj1);
                App.MainWindow?.GameClose(obj.UUID);
                Games.Remove(p);
                if (a is Process p1 && p1.ExitCode != 0)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        App.ShowGameLog(obj);
                        GameLogs.Remove(obj.UUID);
                    });
                    App.MainWindow?.ShowMessage(App.GetLanguage("Live2D.Text3"));
                }
                else
                {
                    if (App.IsHide && !IsGameRuning())
                    {
                        App.Close();
                    }
                }
                p.Dispose();
                GameBinding.GameStateUpdate(obj);
            };

            Games.Add(p, obj);
            RunGames.Add(obj.UUID, p);
            GameCountUtils.LaunchDone(obj.UUID);
            GameBinding.GameStateUpdate(obj);
        }
        else
        {
            UserBinding.UnLockUser(obj1);
        }

        ColorMCCore.DownloaderUpdate = DownloaderUpdate;

        return (p != null, res.Item2);
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="obj1">登陆的账户</param>
    /// <param name="world">需要运行的世界</param>
    /// <param name="cancel">取消启动</param>
    /// <returns>进程信息</returns>
    private static async Task<(Process?, string?)> Launch(GameSettingObj obj,
        LoginObj obj1, WorldObj? world, CancellationToken cancel)
    {
        string? temp = null;
        try
        {
            //启动
            var p = await obj.StartGame(obj1, world, cancel);
            if (cancel.IsCancellationRequested)
            {
                if (p != null && p.Id != 0)
                {
                    p.Kill(true);
                }
            }
            else
            {
                return (p, null);
            }
        }
        catch (LaunchException e1)
        {
            temp = App.GetLanguage("Gui.Error6");
            if (!string.IsNullOrWhiteSpace(e1.Message))
            {
                temp = e1.Message;
            }
            if (e1.Ex != null)
            {
                Logs.Error(temp, e1.Ex);
                App.ShowError(temp, e1.Ex);
            }
        }
        catch (Exception e)
        {
            temp = App.GetLanguage("Gui.Error6");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return (null, temp);
    }

    /// <summary>
    /// 下载器状态更新回调
    /// </summary>
    /// <param name="state">状态</param>
    public static void DownloaderUpdateOnThread(CoreRunState state)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            App.DownloaderUpdate(state);
        }).Wait();
    }

    /// <summary>
    /// 下载器状态更新回调
    /// </summary>
    /// <param name="state">状态</param>
    public static void DownloaderUpdate(CoreRunState state)
    {
        App.DownloaderUpdate(state);
    }

    /// <summary>
    /// 获取下载状态
    /// </summary>
    /// <returns>状态</returns>
    public static (int, int) GetDownloadSize()
    {
        return (DownloadManager.AllSize, DownloadManager.DoneSize);
    }

    /// <summary>
    /// 是否正在下载
    /// </summary>
    public static bool IsDownload
        => DownloadManager.State != CoreRunState.End;

    /// <summary>
    /// 进程日志回调
    /// </summary>
    /// <param name="p">进程</param>
    /// <param name="d">日志</param>
    public static void PLog(Process? p, string? d)
    {
        if (p == null)
            return;
        if (Games.TryGetValue(p, out var obj))
        {
            GameLogs[obj.UUID].Append(d).Append(Environment.NewLine);
            if (App.GameLogWindows.TryGetValue(obj.UUID, out var win))
            {
                win.Log(d);
            }
        }
    }

    /// <summary>
    /// 游戏实例日志回调
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="d">日志</param>
    public static void PLog(GameSettingObj obj, string? d)
    {
        if (GameLogs.TryGetValue(obj.UUID, out var log))
        {
            log.Append(d).Append(Environment.NewLine);
        }

        if (App.GameLogWindows.TryGetValue(obj.UUID, out var win))
        {
            win.Log(d);
        }
    }

    /// <summary>
    /// 错误显示回调
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="e">错误</param>
    /// <param name="close">是否关闭</param>
    private static void ShowError(string? data, Exception? e, bool close)
    {
        App.ShowError(data, e, close);
    }

    /// <summary>
    /// 语言调整回调
    /// </summary>
    /// <param name="type">语言类型</param>
    private static void ChangeLanguage(LanguageType type)
    {
        App.LoadLanguage(type);
        Localizer.Instance.Reload();
    }

    /// <summary>
    /// 在浏览器打开网址
    /// </summary>
    /// <param name="url">网址</param>
    public static void OpUrl(string? url)
    {
        url = url?.Replace(" ", "%20");
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                var ps = Process.Start(new ProcessStartInfo()
                {
                    FileName = "cmd",
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                });
                if (ps != null)
                {
                    ps.StandardInput.WriteLine($"start {url}");
                    ps.Close();
                }
                break;
            case OsType.Linux:
                Process.Start("xdg-open",
                    '"' + url + '"');
                break;
            case OsType.MacOS:
                Process.Start("open", "-a Safari " +
                    '"' + url + '"');
                break;
        }
    }

    /// <summary>
    /// 获取字体列表
    /// </summary>
    /// <returns></returns>
    public static List<FontFamily> GetFontList()
    {
        return FontManager.Current.SystemFonts.ToList();
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public static void DownloadStop()
    {
        DownloadManager.DownloadStop();
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public static void DownloadPause()
    {
        DownloadManager.DownloadPause();
    }

    /// <summary>
    /// 继续下载
    /// </summary>
    public static void DownloadResume()
    {
        DownloadManager.DownloadResume();
    }

    /// <summary>
    /// 服务器包检查
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static async void ServerPackCheck(GameSettingObj obj)
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (config.ServerPack)
        {
            if (string.IsNullOrWhiteSpace(config.ServerUrl))
                return;

            var window = App.GetMainWindow();
            if (window == null)
            {
                return;
            }

            try
            {
                var (Res, Obj) = await obj.ServerPackCheck(config.ServerUrl);
                if (Res && !string.IsNullOrWhiteSpace(Obj?.UI))
                {
                    GuiConfigUtils.Config.ServerCustom.UIFile = Obj.UI;
                    GuiConfigUtils.Save();
                }

                if (Res)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        window.OkInfo.ShowOk(App.GetLanguage("Gui.Info13"), App.Close);
                    });
                }
                else if (Obj != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        window.OkInfo.Show(App.GetLanguage("Gui.Error18"));
                    });
                }

            }
            catch (Exception e)
            {
                string text = App.GetLanguage("Gui.Error19");
                Logs.Error(text, e);
                Dispatcher.UIThread.Post(() =>
                {
                    window.ProgressInfo.Close();
                    window.OkInfo.Show(text);
                });
            }
        }
    }

    /// <summary>
    /// 转网址
    /// </summary>
    /// <param name="item">项目</param>
    /// <param name="type">类型</param>
    /// <param name="url">网址</param>
    /// <returns>网址</returns>
    public static string MakeUrl(ServerModItemObj item, FileType type, string url)
    {
        return UrlHelper.MakeUrl(item, type, url);
    }

    /// <summary>
    /// 获取基础运行路径
    /// </summary>
    /// <returns>路径</returns>
    public static string GetRunDir()
    {
        return ColorMCCore.BaseDir;
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="value">音量</param>
    public static void SetVolume(int value)
    {
        if (value > 100 || value < 0)
            return;

        Media.Volume = (float)value / 100;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    public static async void MusicStart()
    {
        bool play = false;
        Media.Volume = 0;
        var file = GuiConfigUtils.Config.ServerCustom.Music;
        if (file == null)
            return;

        if (file.StartsWith("http://") || file.StartsWith("https://"))
        {
            await Media.PlayUrl(file);
            play = true;
        }
        else
        {
            file = Path.GetFullPath(file);
            if (File.Exists(file))
            {
                if (file.EndsWith(".mp3"))
                {
                    await Media.PlayMp3(file);
                    play = true;
                }
                else if (file.EndsWith(".wav"))
                {
                    await Media.PlayWAV(file);
                    play = true;
                }
            }
        }
        if (play)
        {
            if (GuiConfigUtils.Config.ServerCustom.SlowVolume)
            {
                await Task.Run(() =>
                {
                    for (int a = 0; a < GuiConfigUtils.Config.ServerCustom.Volume; a++)
                    {
                        Media.Volume = (float)a / 100;
                        Thread.Sleep(50);
                    }
                });
            }
            else
            {
                Media.Volume = (float)GuiConfigUtils.Config.ServerCustom.Volume / 100;
            }
        }
    }

    /// <summary>
    /// 启动后音乐播放
    /// </summary>
    public static void LoadMusic()
    {
        if (GuiConfigUtils.Config.ServerCustom.PlayMusic)
        {
            MusicStart();
        }
    }

    /// <summary>
    /// 音乐停止
    /// </summary>
    public static void MusicStop()
    {
        Media.Stop();
    }

    /// <summary>
    /// 音乐恢复
    /// </summary>
    public static void MusicPlay()
    {
        Media.Play();
    }

    /// <summary>
    /// 音乐暂停
    /// </summary>
    public static void MusicPause()
    {
        Media.Pause();
    }

    public static void OpenLive2DCore()
    {
        OpUrl("https://www.live2d.com/download/cubism-sdk/download-native/");
    }
}
