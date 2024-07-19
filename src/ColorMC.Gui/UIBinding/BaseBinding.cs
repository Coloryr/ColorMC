using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using ColorMC.Gui.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.Hook;
using ICSharpCode.SharpZipLib.Zip;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public const string DrapType = "Game";

    /// <summary>
    /// 是否为第一次启动
    /// </summary>
    public static bool NewStart => ColorMCCore.NewStart;
    /// <summary>
    /// 是否正在下载
    /// </summary>
    public static bool IsDownload => DownloadManager.State != DownloadState.End;

    public static bool SdlInit { get; private set; }

    public static event Action? LoadDone;

    /// <summary>
    /// 快捷启动
    /// </summary>
    private static string s_launch;

    public static bool IsAddGames
    { 
        set 
        { 
            InstancesPath.AddGames = value; 
        } 
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        ColorMCCore.Error += WindowManager.ShowError;
        ColorMCCore.LanguageReload += LanguageReload;
        ColorMCCore.GameLog += (obj, d) =>
        {
            GameManager.AddGameLog(obj.UUID, d);
        };
        ColorMCCore.OnDownload = WindowManager.ShowDownload;
        ColorMCCore.GameExit += GameExit;
        ColorMCCore.InstanceChange += InstanceChange;
        ColorMCCore.InstanceIconChange += InstanceIconChange;

        if (ColorMCGui.RunType == RunType.Program && SystemInfo.Os != OsType.Android)
        {
            try
            {
                var sdl = Sdl.GetApi();
                if (sdl.Init(Sdl.InitGamecontroller | Sdl.InitAudio) == 0)
                {
                    InputControl.Init(sdl);
                    Media.Init(sdl);
                    SdlInit = true;
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("BaseBinding.Error1"), e);
            }
        }

        InputElement.PointerReleasedEvent.AddClassHandler<DataGridCell>((x, e) =>
        {
            LongPressed.Released();
        }, handledEventsToo: true);
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
                WindowManager.ShowGameLog(obj);
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
                App.TestClose();
            }
        }

        GameBinding.GameStateUpdate(obj);
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
    /// 核心初始化完成
    /// </summary>
    public static async void Init1()
    {
        LoadDone?.Invoke();

        await GameCloudUtils.StartConnect();

        if (s_launch != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var game = InstancesPath.GetGame(s_launch);
                var window = WindowManager.GetMainWindow();
                if (window == null)
                {
                    return;
                }
                if (window?.Model is { } model)
                {
                    if (game == null)
                    {
                        model.Show(App.Lang("BaseBinding.Error2"));
                        return;
                    }
                    model.Progress(string.Format(App.Lang("BaseBinding.Info1"), game.Name));
                    var res = await GameBinding.Launch(model, game, hide: true);
                    if (!res.Item1)
                    {
                        window.Show();
                        model.Show(res.Item2!);
                    }
                    else
                    {
                        model.ProgressClose();
                        window.Hide();
                    }
                }
            });
        }
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
    /// 停止下载DownloadStop
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
    public static void MusicStart()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (config == null)
        {
            return;
        }
        var file = config.Music;
        if (file == null)
            return;

        Media.Loop = config.MusicLoop;
        Media.PlayMusic(file, config.SlowVolume, config.Volume);
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

    /// <summary>
    /// 测试启动自定义窗口
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static (bool, string?) TestCustomWindow()
    {
        try
        {
            WindowManager.ShowCustom(true);
        }
        catch (Exception ex)
        {
            var data = App.Lang("BaseBinding.Error8");
            Logs.Error(data, ex);
            WindowManager.ShowError(data, ex);

            return (false, data);
        }

        return (true, null);
    }

    /// <summary>
    /// 创建快捷方式
    /// </summary>
    /// <param name="obj"></param>
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
                Logs.Error(App.Lang("BaseBinding.Error5"), e);
            }
        }
#pragma warning restore CA1416 // 验证平台兼容性
    }

    /// <summary>
    /// 设置快捷启动
    /// </summary>
    /// <param name="uuid"></param>
    public static void SetLaunch(string uuid)
    {
        s_launch = uuid;
    }

    /// <summary>
    /// 设置服务器密钥
    /// </summary>
    /// <param name="str"></param>
    public static void SetCloudKey(string str)
    {
        GuiConfigUtils.Config.ServerKey = str[9..];
        WindowManager.ShowSetting(SettingType.Net);
    }

    /// <summary>
    /// 导入Live2D核心
    /// </summary>
    /// <param name="local"></param>
    /// <returns></returns>
    public static async Task<bool> SetLive2DCore(string local)
    {
        using var stream = PathHelper.OpenRead(local);
        using var zip = new ZipFile(stream);
        string file = "";
        string file1 = Directory.GetCurrentDirectory();
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

        file1 = Path.GetFullPath(file1);

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

    /// <summary>
    /// 启动Frp
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<(bool, Process?, string?)> StartFrp(NetFrpRemoteModel item1, NetFrpLocalModel model)
    {
        string file;
        string dir;
        string version = "0.51.0-sakura-7.2";
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
                var obj1 = await SakuraFrpApi.GetDownload();
                if (obj1 == null)
                {
                    return (false, null, null);
                }
                version = obj1.frpc.ver;
                obj = SakuraFrpApi.BuildFrpItem(obj1);
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
                var res = await DownloadManager.StartAsync([obj]);
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
            info = await SakuraFrpApi.GetChannelConfig(item1.Key, item1.ID, version);
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
                PathBinding.Chmod(file);
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
            Logs.Error(App.Lang("BaseBinding.Error6"), e);
        }

        return (false, null, null);
    }
}
