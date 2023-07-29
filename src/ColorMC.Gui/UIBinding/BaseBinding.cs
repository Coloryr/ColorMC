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
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ColorMC.Core.Game.Launch;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public const string DrapType = "Game";

    public readonly static Dictionary<Process, GameSettingObj> Games = new();
    public readonly static Dictionary<string, Process> RunGames = new();
    public readonly static Dictionary<string, StringBuilder> GameLogs = new();
    public static bool ISNewStart => ColorMCCore.NewStart;

    private static IBaseWindow? _window;
    private static CancellationTokenSource s_launchCancel = new();

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        ColorMCCore.OnError = ShowError;
        ColorMCCore.DownloaderUpdate = DownloaderUpdate;
        ColorMCCore.ProcessLog = PLog;
        ColorMCCore.GameLog = PLog;
        ColorMCCore.LanguageReload = Change;
        ColorMCCore.NoJava = NoJava;
        ColorMCCore.UpdateSelect = PackUpdate;
        ColorMCCore.UpdateState = UpdateState;
        ColorMCCore.OfflineLaunch = OfflineLaunch;
        ColorMCCore.GameLaunch = GameLunch;
        ColorMCCore.GameDownload = GameDownload;
        ColorMCCore.LaunchP = LaunchP;

        if (ColorMCGui.RunType == RunType.Program)
        {
            GameCount.Init(ColorMCGui.RunDir);
            ImageUtils.Init(ColorMCGui.RunDir);
            UpdateChecker.Init();

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
        if (_window == null)
        {
            return Task.Run(() => { return false; });
        }

        return Dispatcher.UIThread.InvokeAsync(() =>
            _window.OkInfo.ShowWait(pre ? App.GetLanguage("MainWindow.Info29")
            : App.GetLanguage("MainWindow.Info30")));
    }

    private static Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        if (_window == null)
        {
            return Task.Run(() => { return false; });
        }

        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return state switch
            {
                LaunchState.LostLib => await _window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info5")),
                LaunchState.LostLoader => await _window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info6")),
                LaunchState.LostLoginCore => await _window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info7")),
                _ => await _window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info4")),
            };
        });
    }

    private static Task<bool> OfflineLaunch(LoginObj login)
    {
        if (_window == null)
        {
            return Task.Run(() => { return false; });
        }
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return _window.OkInfo.ShowWait(string.Format(
                App.GetLanguage("MainWindow.Info21"), login.UserName));
        });
    }

    private static void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_window == null)
                return;
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                switch (state)
                {
                    case LaunchState.Login:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info8"));
                        break;
                    case LaunchState.Check:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info9"));
                        break;
                    case LaunchState.CheckVersion:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info10"));
                        break;
                    case LaunchState.CheckLib:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info11"));
                        break;
                    case LaunchState.CheckAssets:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info12"));
                        break;
                    case LaunchState.CheckLoader:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info13"));
                        break;
                    case LaunchState.CheckLoginCore:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info14"));
                        break;
                    case LaunchState.CheckMods:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info17"));
                        break;
                    case LaunchState.Download:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info15"));
                        break;
                    case LaunchState.JvmPrepare:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info16"));
                        break;
                    case LaunchState.LaunchPre:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info31"));
                        break;
                    case LaunchState.LaunchPost:
                        _window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info32"));
                        break;
                    case LaunchState.End:
                        _window.ProgressInfo.Close();
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case LaunchState.Login:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info8");
                        break;
                    case LaunchState.Check:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info9");
                        break;
                    case LaunchState.CheckVersion:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info10");
                        break;
                    case LaunchState.CheckLib:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info11");
                        break;
                    case LaunchState.CheckAssets:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info12");
                        break;
                    case LaunchState.CheckLoader:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info13");
                        break;
                    case LaunchState.CheckLoginCore:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info14");
                        break;
                    case LaunchState.CheckMods:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info17");
                        break;
                    case LaunchState.Download:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info15");
                        break;
                    case LaunchState.JvmPrepare:
                        _window.Head.Title1 = App.GetLanguage("MainWindow.Info16");
                        break;
                    case LaunchState.End:
                        _window.Head.Title1 = "";
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
    /// 打开文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="title">标题</param>
    /// <param name="ext">后缀</param>
    /// <param name="name">名字</param>
    /// <param name="multiple">多选</param>
    /// <param name="storage">首选路径</param>
    /// <returns></returns>
    public static async Task<IReadOnlyList<IStorageFile>?> OpFile(TopLevel? window, string title,
        string[] ext, string name, bool multiple = false, DirectoryInfo? storage = null)
    {
        if (window == null)
            return null;

        var defaultFolder = storage == null ? null : await window.StorageProvider.TryGetFolderFromPathAsync(storage.FullName);

        return await window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = title,
            AllowMultiple = multiple,
            SuggestedStartLocation = defaultFolder,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new(name)
                {
                     Patterns = new List<string>(ext)
                }
            }
        });
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="title">标题</param>
    /// <param name="ext">后缀</param>
    /// <param name="name">名字</param>
    /// <returns>文件路径</returns>
    public static Task<IStorageFile?> OpSave(TopLevel window, string title, string ext, string name)
    {
        return window.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = title,
            DefaultExtension = ext,
            SuggestedFileName = name
        });
    }

    /// <summary>
    /// 游戏实例是否在运行
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>状态</returns>
    public static bool IsGameRun(GameSettingObj obj)
    {
        return RunGames.ContainsKey(obj.UUID);
    }

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
    /// 打开二进制运行路径
    /// </summary>
    public static void OpenRunDir()
    {
        OpPath(AppContext.BaseDirectory);
    }

    /// <summary>
    /// 打开基础运行路径
    /// </summary>
    public static void OpenBaseDir()
    {
        OpPath(ColorMCCore.BaseDir);
    }

    /// <summary>
    /// 打开下载路径
    /// </summary>
    public static void OpenDownloadPath()
    {
        OpPath(DownloadManager.DownloadDir);
    }

    /// <summary>
    /// 打开Java下载路径
    /// </summary>
    public static void OpenDownloadJavaPath()
    {
        OpPath(Path.GetFullPath(JvmPath.BaseDir + JvmPath.Name1));
    }

    /// <summary>
    /// 打开图片路径
    /// </summary>
    public static void OpenPicPath()
    {
        OpPath(ImageUtils.Local);
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
        _window = window;
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
        Funtcions.RunGC();

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
                GameCount.GameClose(obj.UUID);
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
            GameCount.LaunchDone(obj.UUID);
            GameBinding.GameStateUpdate(obj);
        }
        else
        {
            UserBinding.UnLockUser(obj1);
        }

        ColorMCCore.DownloaderUpdate = DownloaderUpdate;

        return (p != null, res.Item2);
    }

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
                    p.Kill();
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
        GameLogs[obj.UUID].Append(d).Append(Environment.NewLine);
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
    private static void Change(LanguageType type)
    {
        App.LoadLanguage(type);
        Localizer.Instance.Reload();
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
    /// 在资源管理器打开文件
    /// </summary>
    /// <param name="item">文件</param>
    public static void OpFile(string item)
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                Process.Start("explorer",
                    $@"/select,{item}");
                break;
            case OsType.Linux:
                try
                {
                    Process.Start("nautilus",
                        '"' + item + '"');
                }
                catch
                {
                    Process.Start("dolphin",
                        '"' + item + '"');
                }
                break;
            case OsType.MacOS:
                var file1 = new FileInfo(item);
                Process.Start("open", '"' + file1.Directory?.FullName + '"');
                break;
        }
    }
    /// <summary>
    /// 在资源管理器打开路径
    /// </summary>
    /// <param name="item">路径</param>
    private static void OpPath(string item)
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                Process.Start("explorer",
                    $"{item}");
                break;
            case OsType.Linux:
                Process.Start("xdg-open",
                    '"' + item + '"');
                break;
            case OsType.MacOS:
                Process.Start("open",
                    '"' + item + '"');
                break;
        }
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">路径类型</param>
    public static void OpPath(GameSettingObj obj, PathType type)
    {
        switch (type)
        {
            case PathType.ShaderpacksPath:
                OpPath(obj.GetShaderpacksPath());
                break;
            case PathType.ResourcepackPath:
                OpPath(obj.GetResourcepacksPath());
                break;
            case PathType.WorldBackPath:
                OpPath(obj.GetWorldBackupPath());
                break;
            case PathType.SavePath:
                OpPath(obj.GetSavesPath());
                break;
            case PathType.GamePath:
                OpPath(obj.GetGamePath());
                break;
            case PathType.SchematicsPath:
                OpPath(obj.GetSchematicsPath());
                break;
            case PathType.ScreenshotsPath:
                OpPath(obj.GetScreenshotsPath());
                break;
            case PathType.ModPath:
                OpPath(obj.GetModsPath());
                break;
            case PathType.BasePath:
                OpPath(obj.GetBasePath());
                break;
        }
    }

    public static void OpPath(WorldObj obj)
    {
        OpPath(obj.Local);
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<string?> OpPath(IBaseWindow window, FileType type)
    {
        var top = window.TopLevel;
        if (top != null)
        {
            return await OpPath(top, type);
        }

        return null;
    }

    /// <summary>
    /// 选择路径
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<string?> OpPath(TopLevel window, FileType type)
    {
        switch (type)
        {
            case FileType.ServerPack:
                var res = await window.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.GetLanguage("Gui.Info11")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.Game:
                res = await window.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.GetLanguage("Gui.Info24")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
        }

        return null;
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
    /// 目录转字符串
    /// </summary>
    /// <param name="file">路径</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageFolder file)
    {
        return file.TryGetLocalPath();
    }
    /// <summary>
    /// 文件转字符串
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageFile file)
    {
        return file.TryGetLocalPath();
    }
    /// <summary>
    /// 文件转字符串
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageItem file)
    {
        return file.TryGetLocalPath();
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
    /// 保存文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <param name="arg">参数</param>
    /// <returns>结果</returns>
    public static Task<bool?> SaveFile(IBaseWindow? window, FileType type, object[]? arg)
    {
        return SaveFile(window as TopLevel, type, arg);
    }

    public static byte[] GetFile(string name)
    {
        var assm = Assembly.GetExecutingAssembly();
        var item = assm.GetManifestResourceStream(name);
        using MemoryStream stream = new();
        item!.CopyTo(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <param name="arg">参数</param>
    /// <returns>结果</returns>
    public static async Task<bool?> SaveFile(TopLevel? window, FileType type, object[]? arg)
    {
        if (window == null)
            return false;

        switch (type)
        {
            case FileType.World:
                var file = await OpSave(window,
                    App.GetLanguage("GameEditWindow.Tab5.Info2"), ".zip", "world.zip");
                if (file == null)
                    break;

                try
                {
                    await GameBinding.ExportWorld((arg![0] as WorldDisplayObj)!.World,
                        file.GetPath());
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.GetLanguage("GameEditWindow.Tab5.Error1"), e);
                    return false;
                }
            case FileType.UI:
                file = await OpSave(window,
                    App.GetLanguage("SettingWindow.Tab6.Info1"), ".axaml", "ui.axaml");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return null;
                    if (File.Exists(name))
                    {
                        File.Delete(name);
                    }

                    File.WriteAllBytes(name, GetFile("ColorMC.Gui.Resource.UI.UI.axaml"));
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.GetLanguage("SettingWindow.Tab6.Error3"), e);
                    return false;
                }
            case FileType.Skin:
                file = await OpSave(window,
                    App.GetLanguage("Gui.Info9"), ".png", "skin.png");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    await UserBinding.SkinImage.SaveAsPngAsync(name);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.GetLanguage("SettingWindow.Tab6.Error3"), e);
                    return false;
                }
            case FileType.Text:
                file = await OpSave(window,
                    App.GetLanguage("Gui.Info21"), ".txt", "log.txt");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return false;
                    await File.WriteAllTextAsync(name, arg![0] as string);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.GetLanguage("SettingWindow.Tab6.Error3"), e);
                    return false;
                }
        }

        return null;
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<string?> OpFile(IBaseWindow window, FileType type)
    {
        var top = window.TopLevel;
        if (top != null)
        {
            return await OpFile(top, type);
        }

        return null;
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<string?> OpFile(TopLevel? window, FileType type)
    {
        switch (type)
        {
            case FileType.Java:
                var res = await OpFile(window,
                    App.GetLanguage("SettingWindow.Tab5.Info2"),
                    new string[] { SystemInfo.Os == OsType.Windows ? "*.exe" : "" },
                    App.GetLanguage("SettingWindow.Tab5.Info2"),
                    storage: JavaBinding.GetSuggestedStartLocation());
                if (res?.Any() == true)
                {
                    var file = res[0].GetPath();
                    if (file == null)
                        return null;
                    if (SystemInfo.Os == OsType.Windows && file.EndsWith("java.exe"))
                    {
                        var file1 = file[..^4] + "w.exe";
                        if (File.Exists(file1))
                            return file1;
                    }

                    return file;
                }
                break;
            case FileType.Config:
                res = await OpFile(window,
                    App.GetLanguage("SettingWindow.Tab1.Info7"),
                    new string[] { "*.json" },
                    App.GetLanguage("SettingWindow.Tab1.Info11"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.AuthConfig:
                res = await OpFile(window,
                    App.GetLanguage("SettingWindow.Tab1.Info10"),
                    new string[] { "*.json" },
                    App.GetLanguage("SettingWindow.Tab1.Info12"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.ModPack:
                res = await OpFile(window,
                    App.GetLanguage("Gui.Info22"),
                    new string[] { "*.zip", "*.mrpack" },
                    App.GetLanguage("Gui.Info23"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.Pic:
                res = await OpFile(window, App.GetLanguage("SettingWindow.Tab2.Info3"),
                    new string[] { "*.png", "*.jpg", "*.bmp" },
                    App.GetLanguage("SettingWindow.Tab2.Info6"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.UI:
                res = await OpFile(window,
                    App.GetLanguage("SettingWindow.Tab6.Info2"),
                    new string[] { "*.axaml" },
                    App.GetLanguage("SettingWindow.Tab6.Info3"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.Music:
                res = await OpFile(window,
                    App.GetLanguage("SettingWindow.Tab6.Info5"),
                    new string[] { "*.mp3", "*.wav" },
                    App.GetLanguage("SettingWindow.Tab6.Info6"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.Live2D:
                res = await OpFile(window,
                    App.GetLanguage("SettingWindow.Tab2.Info7"),
                    new string[] { "*.model3.json" },
                    App.GetLanguage("SettingWindow.Tab2.Info8"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
        }

        return null;
    }

    /// <summary>
    /// A14到A15路径处理
    /// </summary>
    public static bool CheckOldDir()
    {
        if (SystemInfo.Os == OsType.Linux)
        {
            if (Directory.Exists($"{AppContext.BaseDirectory}minecraft/"))
            {
                OpPath(AppContext.BaseDirectory);
                OpPath(InstancesPath.BaseDir);
                return true;
            }
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            if (Directory.Exists("/Users/ColorMC/"))
            {
                OpPath("/Users/ColorMC/");
                OpPath(InstancesPath.BaseDir);
                return true;
            }
        }

        return false;
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
                var res = await obj.ServerPackCheck(config.ServerUrl);
                if (res.Res && !string.IsNullOrWhiteSpace(res.Obj?.UI))
                {
                    GuiConfigUtils.Config.ServerCustom.UIFile = res.Obj.UI;
                    GuiConfigUtils.Save();
                }

                if (res.Res)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        window.OkInfo.ShowOk(App.GetLanguage("Gui.Info13"), App.Close);
                    });
                }
                else if (res.Obj != null)
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
