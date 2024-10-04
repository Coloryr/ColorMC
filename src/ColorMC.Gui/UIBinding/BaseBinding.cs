using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Frp;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.Zip;

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

    public static event Action? LoadDone;

    /// <summary>
    /// 快捷启动
    /// </summary>
    public static string[] StartLaunch { get; private set; }

    /// <summary>
    /// 是否处于添加游戏实例中
    /// </summary>
    public static bool IsAddGames
    {
        set
        {
            InstancesPath.DisableWatcher = value;
        }
    }

    /// <summary>
    /// 核心初始化完成
    /// </summary>
    public static async void Init1()
    {
        LoadDone?.Invoke();

        await GameCloudUtils.StartConnect();

        if (StartLaunch != null)
        {
            Launch(StartLaunch);
        }
    }

    /// <summary>
    /// 快捷启动实例
    /// </summary>
    /// <param name="games"></param>
    public static void Launch(string[] games)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (WindowManager.MainWindow != null)
            {
                (WindowManager.MainWindow.DataContext as IMainTop)?.Launch(games);
            }
        });
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="text">文本</param>
    public static async Task CopyTextClipboard(TopLevel top, string text)
    {
        if (top.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="file">文件列表</param>
    public static async Task CopyFileClipboard(TopLevel top, List<IStorageFile> file)
    {
        if (top.Clipboard is { } clipboard)
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
    public static FontFamily[] GetFontList()
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
    public static bool TestCustomWindow()
    {
        try
        {
            return WindowManager.ShowCustom(true);
        }
        catch (Exception ex)
        {
            var data = App.Lang("BaseBinding.Error8");
            Logs.Error(data, ex);
            WindowManager.ShowError(data, ex);

            return false;
        }
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
                PathBinding.OpenFileWithExplorer(file);
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
    public static void SetLaunch(string[] uuid)
    {
        StartLaunch = uuid;
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
                await PathHelper.WriteBytesAsync(file1, stream1);
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
    public static Task<FrpLaunchRes> StartFrp(NetFrpRemoteModel item1, NetFrpLocalModel model)
    {
        return FrpLaunch.StartFrp(item1, model);
    }
}
