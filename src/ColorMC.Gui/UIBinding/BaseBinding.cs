using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.BuildPack;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

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
    public static bool IsDownload => DownloadManager.State;

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
    public static void OpenUrl(string? url)
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
                Process.Start("open", '"' + url + '"');
                break;
            case OsType.Android:
                ColorMCGui.PhoneOpenUrl(url);
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
        DownloadManager.Stop();
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public static void DownloadPause()
    {
        DownloadManager.Pause();
    }

    /// <summary>
    /// 继续下载
    /// </summary>
    public static void DownloadResume()
    {
        DownloadManager.Resume();
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
    public static async Task<MusicPlayRes> MusicStart(string file, bool loop, bool slow, int volume)
    {
        Media.Loop = loop;
        return await Media.PlayMusic(file, slow, volume);
    }

    /// <summary>
    /// 音乐停止
    /// </summary>
    public static void MusicStop()
    {
        Media.PlayState = PlayState.Stop;
    }

    /// <summary>
    /// 音乐恢复
    /// </summary>
    public static void MusicPlay()
    {
        Media.PlayState = PlayState.Run;
    }

    /// <summary>
    /// 音乐暂停
    /// </summary>
    public static void MusicPause()
    {
        Media.PlayState = PlayState.Pause;
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
                var file = Path.Combine(ColorMCGui.RunDir, $"{obj.Name}.lnk");
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
    public static Task<FrpLaunchRes> StartFrp(object item1, NetFrpLocalModel model)
    {
        if (item1 is NetFrpRemoteModel model1)
        {
            return FrpLaunchUtils.StartFrp(model1, model);
        }
        else if (item1 is NetFrpSelfItemModel model2)
        {
            return FrpLaunchUtils.StartFrp(model2, model);
        }
        return Task.FromResult(new FrpLaunchRes());
    }

    public static (PlayState, string) GetMusicNow()
    {
        return (Media.PlayState, $"{(int)Media.NowTime.TotalMinutes:00}:{Media.NowTime.Seconds:00}" +
            $"/{(int)Media.MusicTime.TotalMinutes:00}:{Media.MusicTime.Seconds:00}");
    }

    public static void ClearWindowSetting()
    {
        WindowManager.Reset();
    }

    public static PlayState GetPlayState()
    {
        return Media.PlayState;
    }

    public static string[] GetEncoding()
    {
        try
        {
            _ = Encoding.GetEncoding("gbk");
            return ["UTF-8", "GBK"];
        }
        catch
        {
            return ["UTF-8"];
        }
    }

    public static bool SetStart(FileItemModel model)
    {
        var obj = new CollectItemObj()
        {
            Icon = model.Logo,
            Url = model.Url,
            FileType = model.FileType,
            Source = model.SourceType,
            Name = model.Name
        };

        if (model.SourceType == SourceType.CurseForge)
        {
            var obj1 = (model.Data as CurseForgeObjList.DataObj)!;
            obj.Pid = obj1.Id.ToString();

        }
        else if (model.SourceType == SourceType.Modrinth)
        {
            var obj1 = (model.Data as ModrinthSearchObj.HitObj)!;
            obj.Pid = obj1.ProjectId;
        }

        if (model.IsStar)
        {
            CollectUtils.RemoveItem(obj);
            return false;
        }
        else
        {
            CollectUtils.AddItem(obj);
            return true;
        }
    }

    public static bool IsStar(SourceType type, string pid)
    {
        return CollectUtils.IsCollect(type, pid);
    }

    public static string GetFolderSize(string folderPath)
    {
        return GetSizeReadable(PathHelper.GetFolderSize(folderPath));
    }

    private static string GetSizeReadable(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes /= 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }

    public static void DeleteTemp()
    {
        var temp = DownloadManager.DownloadDir;
        PathHelper.MoveToTrash(temp);
        Directory.CreateDirectory(temp);
    }

    private static async Task PutFile(Crc32 crc, ZipOutputStream stream, string file, string path)
    {
        var buffer = PathHelper.ReadByte(path)!;

        var entry = new ZipEntry(file)
        {
            DateTime = DateTime.Now,
            Size = buffer.Length
        };
        crc.Reset();
        crc.Update(buffer);
        entry.Crc = crc.Value;
        await stream.PutNextEntryAsync(entry);
        await stream.WriteAsync(buffer);

    }

    public static async Task<bool> BuildPack(BuildPackModel model)
    {
        var file = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());
        using var s = new ZipOutputStream(PathHelper.OpenWrite(file, true));
        s.SetLevel(9);
        var crc = new Crc32();

        var conf = GuiConfigUtils.Config;
        var conf1 = ConfigUtils.Config;

        var obj = new JObject();

        if (model.UiBg && File.Exists(conf.BackImage))
        {
            var filename = Path.GetFileName(conf.BackImage);
            await PutFile(crc, s, filename, conf.BackImage);
            obj.Add(nameof(GuiConfigObj.EnableBG), conf.EnableBG);
            obj.Add(nameof(GuiConfigObj.BackImage), filename);
            obj.Add(nameof(GuiConfigObj.BackEffect), conf.BackEffect);
            obj.Add(nameof(GuiConfigObj.BackTran), conf.BackTran);
            obj.Add(nameof(GuiConfigObj.BackLimit), conf.BackLimit);
            obj.Add(nameof(GuiConfigObj.BackLimitValue), conf.BackLimitValue);
        }

        if (model.UiColor)
        {
            obj.Add(nameof(GuiConfigObj.ColorType), (int)conf.ColorType);
            obj.Add(nameof(GuiConfigObj.ColorMain), conf.ColorMain);
            obj.Add(nameof(GuiConfigObj.RGB), conf.RGB);
            obj.Add(nameof(GuiConfigObj.RGBS), conf.RGBS);
            obj.Add(nameof(GuiConfigObj.RGBV), conf.RGBV);
            obj.Add(nameof(GuiConfigObj.WindowMode), conf.WindowMode);
            obj.Add(nameof(GuiConfigObj.Simple), conf.Simple);
            obj.Add(nameof(GuiConfigObj.Style), new JObject(conf.Style));
            obj.Add(nameof(GuiConfigObj.LogColor), new JObject(conf.LogColor));
        }

        if (model.UiOther)
        {
            obj.Add(nameof(GuiConfigObj.Head), new JObject(conf.Head));
            obj.Add(nameof(GuiConfigObj.CloseBeforeLaunch), conf.CloseBeforeLaunch);
            obj.Add(nameof(GuiConfigObj.Card), new JObject(conf.Card));
        }

        if (model.LaunchCheck)
        {
            obj.Add(nameof(ConfigObj.GameCheck), new JObject(conf1.GameCheck));
            obj.Add(nameof(GuiConfigObj.LaunchCheck), new JObject(conf.LaunchCheck));
        }

        if (model.LaunchArg)
        {
            obj.Add(nameof(ConfigObj.DefaultJvmArg), new JObject(conf1.DefaultJvmArg));
        }

        if (model.LaunchWindow)
        {
            obj.Add(nameof(ConfigObj.Window), new JObject(conf1.Window));
        }

        var obj1 = new JObject();
        var conf2 = conf.ServerCustom;

        if (model.ServerOpt)
        {
            obj1.Add(nameof(GuiConfigObj.ServerCustom.IP), conf2.IP);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.Port), conf2.Port);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.Motd), conf2.Motd);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.JoinServer), conf2.JoinServer);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.MotdColor), conf2.MotdColor);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.MotdBackColor), conf2.MotdBackColor);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.AdminLaunch), conf2.AdminLaunch);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.GameAdminLaunch), conf2.GameAdminLaunch);
        }

        if (model.ServerLock)
        {
            obj1.Add(nameof(GuiConfigObj.ServerCustom.LockGame), conf2.LockGame);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.GameName), conf2.GameName);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.LockLogin), conf2.LockLogin);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.LockLogins), new JArray(conf2.LockLogins));
        }

        var uiFile = Path.Combine(ColorMCGui.RunDir, GuiNames.NameCustomUIFile);

        if (model.ServerUi && File.Exists(uiFile))
        {
            await PutFile(crc, s, GuiNames.NameCustomUIFile, uiFile);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.EnableUI), conf2.EnableUI);
        }

        var musicFile = conf2.Music;

        if (model.ServerMusic && File.Exists(musicFile))
        {
            await PutFile(crc, s, Path.GetFileName(musicFile), musicFile);
            obj1.Add(nameof(GuiConfigObj.ServerCustom.PlayMusic), conf2.PlayMusic);
        }

        obj.Add(nameof(GuiConfigObj.ServerCustom), obj1);

        //foreach (var item in zipList)
        //{
        //    string tempfile = item[(path.Length + 1)..];
        //    if (Directory.Exists(item))
        //    {
        //        var entry = new ZipEntry(tempfile + "/")
        //        {
        //            DateTime = DateTime.Now
        //        };
        //        await s.PutNextEntryAsync(entry);
        //    }
        //    else
        //    {
        //        var buffer = PathHelper.ReadByte(item)!;

        //        var entry = new ZipEntry(tempfile)
        //        {
        //            DateTime = DateTime.Now,
        //            Size = buffer.Length
        //        };
        //        crc.Reset();
        //        crc.Update(buffer);
        //        entry.Crc = crc.Value;
        //        await s.PutNextEntryAsync(entry);
        //        await s.WriteAsync(buffer);
        //    }
        //}

        return false;
    }
}
