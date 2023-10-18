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
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public const string DrapType = "Game";

    public readonly static Dictionary<Process, string> Games = new();
    public readonly static Dictionary<string, Process> RunGames = new();
    public readonly static Dictionary<string, StringBuilder> GameLogs = new();
    public static bool ISNewStart => ColorMCCore.NewStart;

    private static CancellationTokenSource s_launchCancel = new();

    private static string s_launch;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        ColorMCCore.OnError = ShowError;
        ColorMCCore.DownloaderUpdate = DownloaderUpdate;
        ColorMCCore.ProcessLog = PLog;
        ColorMCCore.GameLog = PLog;
        ColorMCCore.LanguageReload = ChangeLanguage;
        ColorMCCore.NoJava = NoJava;
        ColorMCCore.UpdateSelect = PackUpdate;
        ColorMCCore.LoadDone = LoadDone;

        if (ColorMCGui.RunType == RunType.Program)
        {
            GameCountUtils.Init(ColorMCGui.RunDir);

            try
            {
                Media.Init();
            }
            catch (Exception e)
            {
                Logs.Error("error", e);
            }
        }

        ImageUtils.Init(ColorMCGui.RunDir);

        FontSel.Instance.Load();
        ColorSel.Instance.Load();
        StyleSel.Instance.Load();
        LoadStyle();

        InputElement.PointerReleasedEvent.AddClassHandler<DataGridCell>((x, e) =>
        {
            LongPressed.Released();
        }, handledEventsToo: true);
    }

    private static async void LoadDone()
    {
        UpdateChecker.Init();
        GameCloudUtils.Init(ColorMCGui.RunDir);
        App.MainWindow?.LoadDone();
        App.CustomWindow?.Load1();

        await GameCloudUtils.StartConnect();

        if (s_launch != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var game = InstancesPath.GetGame(s_launch);
                var window = App.GetMainWindow();
                if (window == null)
                {
                    return;
                }
                if (game == null)
                {
                    window?.Model.Show(App.GetLanguage("Gui.Error28"));
                }
                else if (window?.Model is BaseModel model)
                {
                    window.Model.Progress(string.Format(App.GetLanguage("Gui.Info28"), game.Name));
                    var res = await GameBinding.Launch(model, game, wait: true);
                    if (!res.Item1)
                    {
                        window.Show();
                        window.Model.Show(res.Item2!);
                    }
                    else
                    {
                        window.Model.ProgressClose();
                        window.Hide();
                    }
                }
            });
        }
    }

    public static void LoadStyle()
    {
        App.PageSlide500.Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime);
        App.PageSlide500.Fade = GuiConfigUtils.Config.Style.AmFade;
    }



    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="text">文本</param>
    public static async Task CopyTextClipboard(string text)
    {
        if (App.TopLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="file">文件列表</param>
    public static async Task CopyFileClipboard(List<IStorageFile> file)
    {
        if (App.TopLevel?.Clipboard is { } clipboard)
        {
            var obj = new DataObject();
            obj.Set(DataFormats.Files, file);
            await clipboard.SetDataObjectAsync(obj);
        }
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
    public static void NoJava(int version)
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
    public static async Task<(bool, string?)> Launch(BaseModel window,
        GameSettingObj obj, LoginObj obj1, WorldObj? world = null, bool wait = false)
    {
        InfoBinding.Window = window;
        InfoBinding.Launch();

        s_launchCancel = new();

        if (Games.ContainsValue(obj.UUID))
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

        ColorMCCore.DownloaderUpdate = DownloaderUpdateOnThread;

        if (App.GameLogWindows.TryGetValue(obj.UUID, out var win))
        {
            win.ClearLog();
        }

        //清空日志
        if (GameLogs.TryGetValue(obj.UUID, out StringBuilder? value))
        {
            value.Clear();
        }
        else
        {
            GameLogs.Add(obj.UUID, new());
        }

        if (SystemInfo.Os != OsType.Android)
        {
            //锁定账户
            UserBinding.AddLockUser(obj1);
        }

        var res = await Task.Run(async () => await Launch(obj, obj1, world, s_launchCancel.Token));

        ColorMCCore.GameLaunch?.Invoke(obj, LaunchState.End);
        FuntionUtils.RunGC();

        if (SystemInfo.Os == OsType.Android && res.Item1 != null)
        {
            return (true, null);
        }

        if (s_launchCancel.IsCancellationRequested)
        {
            return (true, null);
        }

        if (res.Item1 is { } pr)
        {
            obj.LaunchData.LastTime = DateTime.Now;
            obj.SaveLaunchData();

            if (GuiConfigUtils.Config.ServerCustom.RunPause)
            {
                Media.Pause();
            }

            App.MainWindow?.ShowMessage(App.GetLanguage("Live2D.Text2"));

            //_ = Task.Run(() =>
            //{
            //    nint ptr;
            //    Task.Delay(1000);
            //    pr.WaitForInputIdle();
            //    while (true)
            //    {
            //        Task.Delay(100);
            //        ptr = pr.MainWindowHandle;
            //        if (ptr != IntPtr.Zero)
            //        {
            //            break;
            //        }
            //    }
            //    Dispatcher.UIThread.Invoke(() =>
            //    {
            //        EmbedSampleWin.WindowHandel = ptr;
            //        new GameWindow().Show();
            //        return;
            //    });
            //});

            pr.Exited += (a, b) =>
            {
                GameCountUtils.GameClose(obj);
                RunGames.Remove(obj.UUID);
                UserBinding.UnLockUser(obj1);
                Dispatcher.UIThread.Post(() =>
                {
                    if (s_launch != null)
                    {
                        App.Close();
                        return;
                    }
                    App.MainWindow?.GameClose(obj.UUID);
                });
                Games.Remove(pr);
                if (a is Process p1 && p1.ExitCode != 0 && !App.IsClose)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        App.ShowGameLog(obj, true);
                        App.MainWindow?.ShowMessage(App.GetLanguage("Live2D.Text3"));
                    });
                }
                else
                {
                    if (App.IsHide && !IsGameRuning())
                    {
                        App.Close();
                    }
                    if (GameCloudUtils.Connect && !App.IsClose)
                    {
                        Task.Run(() =>
                        {
                            GameBinding.CheckCloudAndOpen(obj);
                        });
                    }
                }
                pr.Dispose();
                GameBinding.GameStateUpdate(obj);
            };

            Games.Add(pr, obj.UUID);
            RunGames.Add(obj.UUID, pr);
            GameCountUtils.LaunchDone(obj);
            GameBinding.GameStateUpdate(obj);

            if (wait)
            {
                await Task.Run(() =>
                {
                    Task.Delay(1000);
                    try
                    {
                        pr.WaitForInputIdle();

                        if (pr.HasExited)
                        {
                            return;
                        }

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
        }
        else
        {
            GameCountUtils.LaunchError(obj);
            UserBinding.UnLockUser(obj1);
        }

        ColorMCCore.DownloaderUpdate = DownloaderUpdate;

        return (res.Item1 != null, res.Item2);
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
            if (SystemInfo.Os == OsType.Android)
            {
                return (new Process(), null);
            }
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
        {
            return;
        }
        if (Games.TryGetValue(p, out var uuid))
        {
            GameLogs[uuid].Append(d).Append(Environment.NewLine);
            if (App.GameLogWindows.TryGetValue(uuid, out var win))
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
            case OsType.Android:
                ColorMCCore.PhoneOpenUrl(url);
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

    public static (bool, string?) TestCustomWindow(string file)
    {
        if (!File.Exists(file))
        {
            file = GetRunDir() + file;
            if (!File.Exists(file))
            {
                return (false, App.GetLanguage("Gui.Error9"));
            }
        }

        try
        {
            App.ShowCustom(file);
            App.CustomWindow?.Load1();
        }
        catch (Exception ex)
        {
            var data = App.GetLanguage("SettingWindow.Tab6.Error2");
            Logs.Error(data, ex);
            App.ShowError(data, ex);

            return (false, data);
        }

        return (true, null);
    }

    public static void OpenPhoneSetting()
    {
        ColorMCGui.PhoneOpenSetting?.Invoke();
    }

    public static void CreateLaunch(GameSettingObj obj)
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic shell = Activator.CreateInstance(shellType)!;
                var file = $"{ColorMCGui.RunDir}{obj.Name}.lnk";
                var shortcut = shell.CreateShortcut(file);
                var path = Process.GetCurrentProcess()!.MainModule!.FileName;
                shortcut.TargetPath = path;
                shortcut.Arguments = "-game " + obj.UUID;
                shortcut.WorkingDirectory = ColorMCGui.RunDir;
                shortcut.Save();
                PathBinding.OpFile(file);
            }
            catch (Exception e)
            {
                Logs.Error("create error", e);
            }
        }
    }

    public static void SetLaunch(string uuid)
    {
        s_launch = uuid;
    }

    public static void SetCloudKey(string str)
    {
        GuiConfigUtils.Config.ServerKey = str[9..];
        App.ShowSetting(SettingType.Net);
    }

    public static async Task<bool> SetLive2DCore(string local)
    {
        using var stream = PathHelper.OpenRead(local);
        using var zip = new ZipFile(stream);
        string file = "";
        string file1 = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName!;
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                file = "Core/dll/windows/" + (SystemInfo.Is64Bit ? "x86_64" : "x86") + "/Live2DCubismCore.dll";
                file1 += "/Live2DCubismCore.dll";
                break;
            case OsType.MacOS:
                file = "Core/dll/macos/libLive2DCubismCore.dylib";
                file1 += "/Live2DCubismCore.dylib";
                break;
            case OsType.Linux:
                file = SystemInfo.IsArm ? "Core/dll/linux/x86_64/libLive2DCubismCore.so"
                    : "Core/dll/experimental/rpi/libLive2DCubismCore.so";
                file1 += "/Live2DCubismCore.so";
                break;
        }

        foreach (ZipEntry item in zip)
        {
            if (item.IsFile && item.Name.Contains(file))
            {
                using var stream1 = zip.GetInputStream(item);
                using var stream2 = PathHelper.OpenWrite(file1);
                await stream1.CopyToAsync(stream2);
                return true;
            }
        }

        return false;
    }
}
