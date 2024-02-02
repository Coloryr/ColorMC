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
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.Hook;
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
using X11;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public const string DrapType = "Game";

    public readonly static Dictionary<Process, string> Games = [];
    public readonly static Dictionary<string, Process> RunGames = [];
    public readonly static Dictionary<string, StringBuilder> GameLogs = [];

    /// <summary>
    /// 是否为第一次启动
    /// </summary>
    public static bool NewStart => ColorMCCore.NewStart;
    /// <summary>
    /// 是否正在下载
    /// </summary>
    public static bool IsDownload => DownloadManager.State != DownloadState.End;

    private static CancellationTokenSource s_launchCancel = new();

    private static string s_launch;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        ColorMCCore.OnError += App.ShowError;
        ColorMCCore.OnLanguageReload += LanguageReload;
        ColorMCCore.OnProcessLog += (p, d) =>
        {
            if (p != null && Games.TryGetValue(p, out var uuid))
            {
                GameLogs[uuid].Append(d).Append(Environment.NewLine);
                if (App.GameLogWindows.TryGetValue(uuid, out var win))
                {
                    win.Log(d);
                }
            }
        };
        ColorMCCore.OnGameLog += (obj, d) =>
        {
            if (GameLogs.TryGetValue(obj.UUID, out var log))
            {
                log.Append(d).Append(Environment.NewLine);
            }

            if (App.GameLogWindows.TryGetValue(obj.UUID, out var win))
            {
                win.Log(d);
            }
        };
        ColorMCCore.OnStartDownload = App.StartDownload;

        if (ColorMCGui.RunType == RunType.Program && SystemInfo.Os != OsType.Android)
        {
            try
            {
                Media.Init();
                InputControlUtils.Init();
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Gui.Error32"), e);
            }
        }

        FrpConfigUtils.Init(ColorMCGui.RunDir);
        ImageUtils.Init(ColorMCGui.RunDir);
        InputConfigUtils.Init(ColorMCGui.RunDir);

        FontSel.Load();
        ColorSel.Load();
        StyleSel.Load();
        LoadStyle();

        InputElement.PointerReleasedEvent.AddClassHandler<DataGridCell>((x, e) =>
        {
            LongPressed.Released();
        }, handledEventsToo: true);
    }

    public static void LanguageReload(LanguageType type)
    {
        App.LoadLanguage(type);
        LangSel.Reload();

        App.Reboot();
    }

    public static async void LoadDone()
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
                    window?.Model.Show(App.Lang("Gui.Error28"));
                }
                else if (window?.Model is BaseModel model)
                {
                    window.Model.Progress(string.Format(App.Lang("Gui.Info28"), game.Name));
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

    // 执行 Bash 命令的辅助方法
    private static void ExecuteBashCommand(string command)
    {
        var proc = new Process
        {
            StartInfo = new()
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        proc.Start();
        proc.WaitForExit();
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="obj1">保存的账户</param>
    /// <returns>结果</returns>
    public static async Task<(bool, string?)> Launch(BaseModel model, GameSettingObj obj,
        LoginObj obj1, WorldObj? world = null, bool wait = false)
    {
        if (SystemInfo.Os == OsType.Android)
        {
            wait = false;
        }

        s_launchCancel = new();

        if (Games.ContainsValue(obj.UUID))
        {
            return (false, App.Lang("Gui.Error42"));
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

        //清空日志
        if (GameLogs.TryGetValue(obj.UUID, out StringBuilder? value))
        {
            value.Clear();
        }
        else
        {
            GameLogs.Add(obj.UUID, new());
        }

        //锁定账户
        UserBinding.AddLockUser(obj1);

        var res = await Task.Run(async () =>
            await Launch(obj, (a) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowWait(a);
                });
            }, (pre) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                    model.ShowWait(pre ? App.Lang("MainWindow.Info29") : App.Lang("MainWindow.Info30")));
            }, (text) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (text == null)
                    {
                        model.ProgressClose();
                    }
                    else
                    {
                        model.Progress(text);
                    }
                });
            }, (text) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowTextWait(App.Lang("Gui.Info5"), text ?? "");
                });
            }, () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    App.ShowSetting(SettingType.SetJava);
                });
            }, (login) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowWait(string.Format(
                        App.Lang("MainWindow.Info21"), login.UserName));
                });
            }, (obj, state) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (GuiConfigUtils.Config.CloseBeforeLaunch)
                    {
                        if (state == LaunchState.End)
                        {
                            model.ProgressClose();
                        }
                        model.ProgressUpdate(App.Lang(state switch
                        {
                            LaunchState.Login => "MainWindow.Info8",
                            LaunchState.Check => "MainWindow.Info9",
                            LaunchState.CheckVersion => "MainWindow.Info10",
                            LaunchState.CheckLib => "MainWindow.Info11",
                            LaunchState.CheckAssets => "MainWindow.Info12",
                            LaunchState.CheckLoader => "MainWindow.Info13",
                            LaunchState.CheckLoginCore => "MainWindow.Info14",
                            LaunchState.CheckMods => "MainWindow.Info17",
                            LaunchState.Download => "MainWindow.Info15",
                            LaunchState.JvmPrepare => "MainWindow.Info16",
                            LaunchState.LaunchPre => "MainWindow.Info31",
                            LaunchState.LaunchPost => "MainWindow.Info32",
                            LaunchState.InstallForge => "MainWindow.Info38",
                            _ => ""
                        }));
                    }
                    else
                    {
                        model.Title1 = App.Lang(state switch
                        {
                            LaunchState.Login => "MainWindow.Info8",
                            LaunchState.Check => "MainWindow.Info9",
                            LaunchState.CheckVersion => "MainWindow.Info10",
                            LaunchState.CheckLib => "MainWindow.Info11",
                            LaunchState.CheckAssets => "MainWindow.Info12",
                            LaunchState.CheckLoader => "MainWindow.Info13",
                            LaunchState.CheckLoginCore => "MainWindow.Info14",
                            LaunchState.CheckMods => "MainWindow.Info17",
                            LaunchState.Download => "MainWindow.Info15",
                            LaunchState.JvmPrepare => "MainWindow.Info16",
                            LaunchState.LaunchPre => "MainWindow.Info31",
                            LaunchState.LaunchPost => "MainWindow.Info32",
                            LaunchState.InstallForge => "MainWindow.Info38",
                            _ => ""
                        });
                    }
                });
            }, obj1, world, s_launchCancel.Token));

        model.ProgressClose();
        model.Title1 = "";
        FuntionUtils.RunGC();

        if (s_launchCancel.IsCancellationRequested)
        {
            return (true, null);
        }

        if (res.Item1 is { } pr)
        {
            if (obj.LaunchData.LastTime == new DateTime(0))
            {
                if (obj.Loader == Loaders.Forge
                    && new Version(obj.Version) > new Version(1, 20, 1))
                {
                    model.Show(App.Lang("Gui.Info41"));
                }
            }
            obj.LaunchData.LastTime = DateTime.Now;
            obj.SaveLaunchData();

            if (GuiConfigUtils.Config.ServerCustom.RunPause)
            {
                Media.Pause();
            }

            App.MainWindow?.ShowMessage(App.Lang("Live2D.Text2"));

            pr.Exited += (a, b) =>
            {
                GameCount.GameClose(obj);
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
                        App.ShowGameLog(obj);
                        App.MainWindow?.ShowMessage(App.Lang("Live2D.Text3"));
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
            GameCount.LaunchDone(obj);
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
            else
            {
                await Task.Run(() =>
                {
                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        pr.WaitForInputIdle();
                        while (!pr.HasExited)
                        {
                            Task.Delay(100);
                            ptr = pr.MainWindowHandle;
                            if (ptr != IntPtr.Zero)
                            {
                                break;
                            }
                        }

                        if (pr.HasExited)
                        {
                            return;
                        }

                        if (obj.Window?.GameTitle is { } title)
                        {
                            if (SystemInfo.Os == OsType.Windows)
                            {
                                Win32Native.Win32.SetWindowText(ptr, title);
                            }
                            else if (SystemInfo.Os == OsType.Linux)
                            {
                                IntPtr display = Xlib.XOpenDisplay(null);
                                if (display == IntPtr.Zero)
                                {
                                    return;
                                }

                                var window = new IntPtr(pr.Id);
                                Xlib.XStoreName(display, window, title);
                                Xlib.XCloseDisplay(display);
                            }
                            else if (SystemInfo.Os == OsType.MacOS)
                            {
                                string cmd = $"osascript -e 'tell application \"System Events\" " +
                                $"to set title of windows of process \"{pr.ProcessName}\" to \"{title}\"'";
                                ExecuteBashCommand(cmd);
                            }
                        }

                        if (SystemInfo.Os == OsType.Windows && GuiConfigUtils.Config.Input.Enable)
                        {
                            var run = true;
                            var uuid = GuiConfigUtils.Config.Input.NowConfig;
                            Dispatcher.UIThread.Invoke(async () =>
                            {
                                if (string.IsNullOrWhiteSpace(uuid) || !InputConfigUtils.Configs.ContainsKey(uuid))
                                {
                                    run = await model.ShowWait(App.Lang("Gui.Error51"));
                                }
                                if (run)
                                {
                                    App.ShowGameWindow(obj, pr, ptr);
                                }
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                });
            }
        }
        else
        {
            GameCount.LaunchError(obj);
            UserBinding.UnLockUser(obj1);
        }

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
        ColorMCCore.Request request, ColorMCCore.LaunchP pre,
        ColorMCCore.UpdateState state, ColorMCCore.UpdateSelect select,
        ColorMCCore.NoJava nojava, ColorMCCore.LoginFail loginfail,
        ColorMCCore.GameLaunch update2,
        LoginObj obj1, WorldObj? world, CancellationToken cancel)
    {
        string? temp = null;
        try
        {
            //启动
            var p = await obj.StartGameAsync(obj1, world, request, pre, state, select, nojava,
                loginfail, update2, cancel);
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
            temp = App.Lang("Gui.Error6");
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
            temp = App.Lang("Gui.Error6");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return (null, temp);
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
        return [.. FontManager.Current.SystemFonts];
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
                return (false, App.Lang("Gui.Error9"));
            }
        }

        try
        {
            App.ShowCustom(file);
            App.CustomWindow?.Load1();
        }
        catch (Exception ex)
        {
            var data = App.Lang("SettingWindow.Tab6.Error2");
            Logs.Error(data, ex);
            App.ShowError(data, ex);

            return (false, data);
        }

        return (true, null);
    }

    public static void CreateLaunch(GameSettingObj obj)
    {
#pragma warning disable CA1416 // 验证平台兼容性
        if (SystemInfo.Os == OsType.Windows)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic shell = Activator.CreateInstance(shellType)!;
                var file = $"{ColorMCGui.RunDir}{obj.Name}.lnk";
                var shortcut = shell.CreateShortcut(file);
                var path = Environment.ProcessPath;
                shortcut.TargetPath = path;
                shortcut.Arguments = "-game " + obj.UUID;
                shortcut.WorkingDirectory = ColorMCGui.RunDir;
                shortcut.Save();
                PathBinding.OpFile(file);
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Gui.Error33"), e);
            }
        }
#pragma warning restore CA1416 // 验证平台兼容性
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

    public static async Task<(bool, Process?, string?)> StartFrp(NetFrpRemoteModel item1, NetFrpLocalModel model)
    {
        string file;
        string dir;
        if (SystemInfo.Os == OsType.Android)
        {
            file = ColorMCGui.PhoneGetFrp.Invoke(item1.FrpType);
            dir = FrpPath.BaseDir;
        }
        else
        {
            DownloadItemObj? obj = null;
            string? local = "";
            if (item1.FrpType == FrpType.SakuraFrp)
            {
                obj = await SakuraFrpApi.BuildFrpItem();
                local = obj?.Local;
            }
            else if (item1.FrpType == FrpType.OpenFrp)
            {
                (obj, local) = await OpenFrpApi.BuildFrpItem();
            }
            if (obj == null)
            {
                return (false, null, null);
            }
            if (!File.Exists(obj.Local))
            {
                var res = await App.StartDownload([obj]);
                if (!res)
                {
                    return (false, null, null);
                }
            }
            file = local!;
            var info2 = new FileInfo(file);
            dir = info2.DirectoryName!;
        }
        string? info = null;
        if (item1.FrpType == FrpType.SakuraFrp)
        {
            info = await SakuraFrpApi.GetChannelConfig(item1.Key, item1.ID);
        }
        else if (item1.FrpType == FrpType.OpenFrp)
        {
            var temp = await OpenFrpApi.GetChannelConfig(item1.Key, item1.ID);
            if (temp != null && temp.proxies?.Count > 0)
            {
                info = temp.proxies.Values.First();
            }
        }
        if (info == null)
        {
            return (false, null, null);
        }

        var lines = info.Split("\n");
        var builder = new StringBuilder();
        string outip = "";
        if (item1.FrpType == FrpType.SakuraFrp)
        {
            string ip = "";

            foreach (var item2 in lines)
            {
                var item3 = item2.Trim();
                if (item3.StartsWith("login_fail_exit"))
                {
                    builder.AppendLine("login_fail_exit = true");
                }
                else if (item3.StartsWith("server_addr"))
                {
                    ip = item3.Split("=")[1].Trim();
                    builder.AppendLine(item3);
                }
                else if (item3.StartsWith("local_port"))
                {
                    builder.AppendLine($"local_port = {model.Port}");
                }
                else
                {
                    builder.AppendLine(item3);
                }
            }

            File.WriteAllText(dir + "/server.ini", builder.ToString());

            outip = ip + ":" + item1.Remote;
        }
        else if (item1.FrpType == FrpType.OpenFrp)
        {
            foreach (var item2 in lines)
            {
                var item3 = item2.Trim();
                if (item3.StartsWith("local_port"))
                {
                    builder.AppendLine($"local_port = {model.Port}");
                }
                else
                {
                    builder.AppendLine(item3);
                }
            }

            File.WriteAllText(dir + "/server.ini", builder.ToString());

            outip = item1.Remote;
        }

        try
        {
            if (SystemInfo.Os != OsType.Windows)
            {
                PathUtils.Chmod(file);
            }

            var p = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = file,
                    WorkingDirectory = dir,
                    Arguments = "-c server.ini",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            return (true, p, outip);
        }
        catch (Exception e)
        {
            Logs.Error("frp start error", e);
        }

        return (false, null, null);
    }

    public static void Clear()
    {
        ColorSel.Remove();
        FontSel.Remove();
        LangSel.Remove();
        StyleSel.Remove();
    }
}
