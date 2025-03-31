using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
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

    /// <summary>
    /// 启动完成回调
    /// </summary>
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
#if Phone
            case OsType.Android:
                ColorMCGui.PhoneOpenUrl(url);
                break;
#endif
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
                var file = Path.Combine(ColorMCGui.BaseDir, $"{obj.Name}.lnk");
                var shortcut = shell.CreateShortcut(file);
                var path = Environment.ProcessPath;
                shortcut.TargetPath = path;
                shortcut.Arguments = "-game " + obj.UUID;
                shortcut.WorkingDirectory = ColorMCGui.BaseDir;
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
    /// <param name="local">导入路径</param>
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
    /// <param name="item1">远程Frp项目</param>
    /// <param name="model">本地映射项目</param>
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

    /// <summary>
    /// 获取当前音乐信息
    /// </summary>
    /// <returns></returns>
    public static (PlayState, string) GetMusicNow()
    {
        return (Media.PlayState, $"{(int)Media.NowTime.TotalMinutes:00}:{Media.NowTime.Seconds:00}" +
            $"/{(int)Media.MusicTime.TotalMinutes:00}:{Media.MusicTime.Seconds:00}");
    }

    /// <summary>
    /// 清理窗口状态
    /// </summary>
    public static void ClearWindowSetting()
    {
        WindowManager.Reset();
    }

    /// <summary>
    /// 获取当前音乐播放状态
    /// </summary>
    /// <returns></returns>
    public static PlayState GetPlayState()
    {
        return Media.PlayState;
    }

    /// <summary>
    /// 获取支持的编码
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 收藏资源
    /// </summary>
    /// <param name="model">资源</param>
    /// <returns></returns>
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

    /// <summary>
    /// 资源是否收藏
    /// </summary>
    /// <param name="type">资源类型</param>
    /// <param name="pid">项目ID</param>
    /// <returns></returns>
    public static bool IsStar(SourceType type, string pid)
    {
        return CollectUtils.IsCollect(type, pid);
    }

    /// <summary>
    /// 清理缓存
    /// </summary>
    public static void DeleteTemp()
    {
        var temp = DownloadManager.DownloadDir;
        PathHelper.MoveToTrash(temp);
        Directory.CreateDirectory(temp);
    }

    /// <summary>
    /// 给压缩包添加一个文件
    /// </summary>
    /// <param name="crc">校验</param>
    /// <param name="stream">压缩包流</param>
    /// <param name="file">文件名</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private static async Task PutFile(Crc32 crc, ZipOutputStream stream, string file, string path)
    {
        using var buffer = PathHelper.OpenRead(path)!;

        var entry = new ZipEntry(file)
        {
            DateTime = DateTime.Now,
            Size = buffer.Length
        };
        crc.Reset();
        await crc.AppendAsync(buffer);
        entry.Crc = crc.GetCurrentHashAsUInt32();
        await stream.PutNextEntryAsync(entry);
        buffer.Seek(0, SeekOrigin.Begin);
        await buffer.CopyToAsync(stream);
    }

    /// <summary>
    /// 给压缩包添加一个文件
    /// </summary>
    /// <param name="crc">校验</param>
    /// <param name="stream">压缩包流</param>
    /// <param name="file">文件名</param>
    /// <param name="data">文件内容</param>
    /// <returns></returns>
    private static async Task PutFile(Crc32 crc, ZipOutputStream stream, string file, byte[] data)
    {
        var entry = new ZipEntry(file)
        {
            DateTime = DateTime.Now,
            Size = data.Length
        };
        crc.Reset();
        crc.Append(data);
        entry.Crc = crc.GetCurrentHashAsUInt32();
        await stream.PutNextEntryAsync(entry);
        await stream.WriteAsync(data);
    }

    /// <summary>
    /// 给压缩包添加一个文件夹
    /// </summary>
    /// <param name="crc">校验</param>
    /// <param name="stream">压缩包流</param>
    /// <param name="file">文件名</param>
    /// <param name="path">路径</param>
    /// <param name="basepath">基础路径，用于替换</param>
    /// <returns></returns>
    private static async Task PutFile(Crc32 crc, ZipOutputStream stream, string file, string path, string basepath)
    {
        foreach (var item in PathHelper.GetAllFiles(path))
        {
            string tempfile = file + "/" + item.FullName[(basepath.Length + 1)..];
            tempfile = tempfile.Replace("\\", "/");
            using var buffer = PathHelper.OpenRead(item.FullName)!;

            var entry = new ZipEntry(tempfile)
            {
                DateTime = DateTime.Now,
                Size = buffer.Length
            };
            crc.Reset();
            await crc.AppendAsync(buffer);
            entry.Crc = crc.GetCurrentHashAsUInt32();
            await stream.PutNextEntryAsync(entry);
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(stream);
        }
    }

    /// <summary>
    /// 读取客户端配置
    /// </summary>
    public static void ReadBuildConfig()
    {
        var file = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameClientConfigFile);
        if (!File.Exists(file))
        {
            return;
        }

        var temp = PathHelper.ReadText(file);
        if (temp == null)
        {
            return;
        }

        var obj = JObject.Parse(temp);

        var conf = GuiConfigUtils.Config;
        var conf1 = ConfigUtils.Config;
        var conf2 = conf.ServerCustom;

        if (obj.TryGetValue(nameof(BuildPackModel.UiBg), out var value) && value is JObject obj1)
        {
            var config = obj1.ToObject<UiBackConfigObj>()!;
            conf.EnableBG = config.EnableBG;
            conf.BackImage = config.BackImage;
            conf.BackEffect = config.BackEffect;
            conf.BackTran = config.BackTran;
            conf.BackLimit = config.BackLimit;
            conf.BackLimitValue = config.BackLimitValue;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.UiColor), out value) && value is JObject obj2)
        {
            var config = obj2.ToObject<UiColorConfigObj>()!;
            conf.ColorType = config.ColorType;
            conf.ColorMain = config.ColorMain;
            conf.RGB = config.RGB;
            conf.RGBS = config.RGBS;
            conf.RGBV = config.RGBV;
            conf.WindowMode = config.WindowMode;
            conf.Simple = config.Simple;
            conf.Style = config.Style;
            conf.LogColor = config.LogColor;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.UiOther), out value) && value is JObject obj3)
        {
            var config = obj3.ToObject<UiOtherConfigObj>()!;
            conf.Head = config.Head;
            conf.CloseBeforeLaunch = config.CloseBeforeLaunch;
            conf.Card = config.Card;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.LaunchCheck), out value) && value is JObject obj4)
        {
            var config = obj4.ToObject<LaunchCheckConfigObj>()!;
            conf1.GameCheck = config.GameCheck;
            conf.LaunchCheck = config.LaunchCheck;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.LaunchArg), out value) && value is JObject obj5)
        {
            var config = obj5.ToObject<RunArgObj>()!;
            conf1.DefaultJvmArg = config;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.LaunchWindow), out value) && value is JObject obj6)
        {
            var config = obj6.ToObject<WindowSettingObj>()!;
            conf1.Window = config;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.ServerOpt), out value) && value is JObject obj7)
        {
            var config = obj7.ToObject<ServerOptConfigObj>()!;
            conf2.IP = config.IP;
            conf2.Port = config.Port;
            conf2.Motd = config.Motd;
            conf2.JoinServer = config.JoinServer;
            conf2.MotdColor = config.MotdColor;
            conf2.MotdBackColor = config.MotdBackColor;
            conf2.AdminLaunch = config.AdminLaunch;
            conf2.GameAdminLaunch = config.GameAdminLaunch;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.ServerLock), out value) && value is JObject obj8)
        {
            var config = obj8.ToObject<ServerLockConfigObj>()!;
            conf2.LockGame = config.LockGame;
            conf2.GameName = config.GameName;
            conf2.LockLogin = config.LockLogin;
            conf2.LockLogins = config.LockLogins;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.ServerUi), out value) && value is JObject obj9)
        {
            var config = obj9.ToObject<ServerUiConfigObj>()!;
            conf2.EnableUI = config.EnableUI;
            conf2.CustomIcon = config.CustomIcon;
            conf2.IconFile = config.IconFile;
            conf2.CustomStart = config.CustomStart;
            conf2.StartIconFile = config.StartIconFile;
            conf2.DisplayType = config.DisplayType;
            conf2.StartText = config.StartText;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.ServerMusic), out value) && value is JObject obj10)
        {
            var config = obj10.ToObject<ServerMusicConfigObj>()!;
            conf2.PlayMusic = config.PlayMusic;
            conf2.Music = config.Music;
            conf2.Volume = config.Volume;
            conf2.SlowVolume = config.SlowVolume;
            conf2.MusicLoop = config.MusicLoop;
            conf2.RunPause = config.RunPause;
        }

        if (obj.TryGetValue(nameof(BuildPackModel.Javas), out value) && value is JArray obj11)
        {
            var list = obj11.ToObject<List<JvmConfigObj>>()!;
            conf1.JavaList.AddRange(list);
        }

        GuiConfigUtils.SaveNow();
        ConfigUtils.SaveNow();

        File.Delete(file);
    }

    /// <summary>
    /// 打包客户端
    /// </summary>
    /// <param name="model"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static async Task<bool> BuildPack(BuildPackModel model, string output)
    {
        try
        {
            var file = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());
            var stream = new ZipOutputStream(PathHelper.OpenWrite(file, true));
            stream.SetLevel(9);
            var crc = new Crc32();

            var conf = GuiConfigUtils.Config;
            var conf1 = ConfigUtils.Config;
            var conf2 = conf.ServerCustom;

            var obj = new JObject();

            model.Model.ProgressUpdate(App.Lang("BuildPackWindow.Info3"));

            //打包配置
            if (model.UiBg && File.Exists(conf.BackImage))
            {
                var filename = Path.GetFileName(conf.BackImage);
                await PutFile(crc, stream, filename, conf.BackImage);
                obj.Add(nameof(BuildPackModel.UiBg), JObject.FromObject(new UiBackConfigObj()
                {
                    EnableBG = conf.EnableBG,
                    BackImage = conf.BackImage,
                    BackEffect = conf.BackEffect,
                    BackTran = conf.BackTran,
                    BackLimit = conf.BackLimit,
                    BackLimitValue = conf.BackLimitValue
                }));
            }

            if (model.UiColor)
            {
                obj.Add(nameof(BuildPackModel.UiColor), JObject.FromObject(new UiColorConfigObj()
                {
                    ColorType = conf.ColorType,
                    ColorMain = conf.ColorMain,
                    RGB = conf.RGB,
                    RGBS = conf.RGBS,
                    RGBV = conf.RGBV,
                    WindowMode = conf.WindowMode,
                    Simple = conf.Simple,
                    Style = conf.Style,
                    LogColor = conf.LogColor
                }));
            }

            if (model.UiOther)
            {
                obj.Add(nameof(BuildPackModel.UiOther), JObject.FromObject(new UiOtherConfigObj()
                {
                    Head = conf.Head,
                    CloseBeforeLaunch = conf.CloseBeforeLaunch,
                    Card = conf.Card
                }));
            }

            if (model.LaunchCheck)
            {
                obj.Add(nameof(BuildPackModel.LaunchCheck), JObject.FromObject(new LaunchCheckConfigObj()
                {
                    GameCheck = conf1.GameCheck,
                    LaunchCheck = conf.LaunchCheck,
                }));
            }

            if (model.LaunchArg)
            {
                obj.Add(nameof(BuildPackModel.LaunchArg), JObject.FromObject(conf1.DefaultJvmArg));
            }

            if (model.LaunchWindow)
            {
                obj.Add(nameof(BuildPackModel.LaunchWindow), JObject.FromObject(conf1.Window));
            }

            if (model.ServerOpt)
            {
                obj.Add(nameof(BuildPackModel.ServerOpt), JObject.FromObject(new ServerOptConfigObj()
                {
                    IP = conf2.IP,
                    Port = conf2.Port,
                    Motd = conf2.Motd,
                    JoinServer = conf2.JoinServer,
                    MotdColor = conf2.MotdColor,
                    MotdBackColor = conf2.MotdBackColor,
                    AdminLaunch = conf2.AdminLaunch,
                    GameAdminLaunch = conf2.GameAdminLaunch
                }));
            }

            if (model.ServerLock)
            {
                obj.Add(nameof(BuildPackModel.ServerLock), JObject.FromObject(new ServerLockConfigObj()
                {
                    LockGame = conf2.LockGame,
                    GameName = conf2.GameName,
                    LockLogin = conf2.LockLogin,
                    LockLogins = conf2.LockLogins
                }));
            }

            if (model.ServerUi)
            {
                var uiFile = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameCustomUIFile);
                if (File.Exists(uiFile))
                {
                    await PutFile(crc, stream, GuiNames.NameCustomUIFile, uiFile);
                }
                if (conf2.IconFile != null)
                {
                    uiFile = Path.Combine(ColorMCGui.BaseDir, conf2.IconFile);
                    if (File.Exists(uiFile))
                    {
                        await PutFile(crc, stream, conf2.IconFile, uiFile);
                    }
                }
                if (conf2.StartIconFile != null)
                {
                    uiFile = Path.Combine(ColorMCGui.BaseDir, conf2.StartIconFile);
                    if (File.Exists(uiFile))
                    {
                        await PutFile(crc, stream, conf2.StartIconFile, uiFile);
                    }
                }

                obj.Add(nameof(BuildPackModel.ServerUi), JObject.FromObject(new ServerUiConfigObj()
                {
                    EnableUI = conf2.EnableUI,
                    CustomIcon = conf2.CustomIcon,
                    IconFile = conf2.IconFile,
                    CustomStart = conf2.CustomStart,
                    StartIconFile = conf2.StartIconFile,
                    DisplayType = conf2.DisplayType,
                    StartText = conf2.StartText
                }));
            }

            if (model.ServerMusic)
            {
                var musicFile = conf2.Music;
                var filename = Path.GetFileName(musicFile);
                if (File.Exists(musicFile))
                {
                    await PutFile(crc, stream, filename!, musicFile);
                }

                obj.Add(nameof(BuildPackModel.ServerMusic), JObject.FromObject(new ServerMusicConfigObj()
                {
                    PlayMusic = conf2.PlayMusic,
                    Music = filename,
                    Volume = conf2.Volume,
                    SlowVolume = conf2.SlowVolume,
                    MusicLoop = conf2.MusicLoop,
                    RunPause = conf2.RunPause
                }));
            }

            if (model.PackUpdate)
            {
                if (File.Exists(LauncherUpgrade.LocalPath[0]))
                {
                    await PutFile(crc, stream, $"{GuiNames.NameDllDir}/ColorMC.Core.dll", LauncherUpgrade.LocalPath[0]);
                }
                if (File.Exists(LauncherUpgrade.LocalPath[1]))
                {
                    await PutFile(crc, stream, $"{GuiNames.NameDllDir}/ColorMC.Core.pdb", LauncherUpgrade.LocalPath[1]);
                }
                if (File.Exists(LauncherUpgrade.LocalPath[2]))
                {
                    await PutFile(crc, stream, $"{GuiNames.NameDllDir}/ColorMC.Gui.dll", LauncherUpgrade.LocalPath[2]);
                }
                if (File.Exists(LauncherUpgrade.LocalPath[3]))
                {
                    await PutFile(crc, stream, $"{GuiNames.NameDllDir}/ColorMC.Gui.pdb", LauncherUpgrade.LocalPath[3]);
                }
            }

            //打包java
            if (model.Java)
            {
                model.Model.ProgressUpdate(App.Lang("BuildPackWindow.Info4"));

                var list = new List<JvmConfigObj>();
                foreach (var item in model.Javas)
                {
                    if (!item.IsSelect)
                    {
                        continue;
                    }
                    await PutFile(crc, stream, Names.NameJavaDir, Path.Combine(JvmPath.JavaDir, item.Name), JvmPath.JavaDir);
                    list.Add(new()
                    {
                        Name = item.Name,
#if Phone
                        Local = item.Path.Replace(JvmPath.BaseDir, "")
#else
                        Local = item.Path.Replace(ColorMCGui.BaseDir, "")
#endif
                    });
                }

                obj.Add(nameof(BuildPackModel.Javas), JArray.FromObject(list));
            }

            var data = obj.ToString();
            await PutFile(crc, stream, GuiNames.NameClientConfigFile, Encoding.UTF8.GetBytes(data));

            //打包启动器
            if (model.PackLaunch)
            {
                foreach (var item in LauncherUpgrade.LaunchFiles)
                {
                    var fileitem = Path.Combine(ColorMCGui.BaseDir,
                    item);
                    if (File.Exists(fileitem))
                    {
                        await PutFile(crc, stream, item, fileitem);
                    }
                }
            }

            model.Model.ProgressUpdate(App.Lang("BuildPackWindow.Info5"));

            //打包游戏实例
            foreach (var item in model.GetSelectItems())
            {
                string tempfile = item[(ColorMCGui.BaseDir.Length)..];
                tempfile = tempfile.Replace("\\", "/");

                await PutFile(crc, stream, tempfile, item);
            }

            model.Model.ProgressUpdate(App.Lang("BuildPackWindow.Info6"));

            foreach (var item in model.Files)
            {
                await PutFile(crc, stream, item.File, item.Local);
            }

            await stream.DisposeAsync();

            PathHelper.MoveFile(file, output);

            PathBinding.OpenFileWithExplorer(output);

            return true;
        }
        catch (Exception e)
        {
            WindowManager.ShowError(App.Lang("BuildPackWindow.Error1"), e);
        }

        return false;
    }

    /// <summary>
    /// 设置窗口图标
    /// </summary>
    /// <param name="file"></param>
    public static void SetWindowIcon(string file)
    {
        var name = Path.GetExtension(file);
        PathHelper.CopyFile(file, Path.Combine(ColorMCGui.BaseDir, GuiNames.NameIconFile + name));

        GuiConfigUtils.Config.ServerCustom.IconFile = GuiNames.NameIconFile + name;
        GuiConfigUtils.Save();

        ImageManager.LoadIcon();
    }

    /// <summary>
    /// 获取窗口图标
    /// </summary>
    /// <returns></returns>
    public static Bitmap? GetWindowIcon()
    {
        return ImageManager.GetCustomIcon();
    }

    /// <summary>
    /// 获取启动图标
    /// </summary>
    /// <returns></returns>
    public static Bitmap? GetStartIcon()
    {
        return ImageManager.GetStartIcon();
    }

    /// <summary>
    /// 设置启动页图标
    /// </summary>
    /// <param name="file"></param>
    public static void SetStartIcon(string file)
    {
        var name = Path.GetExtension(file);
        PathHelper.CopyFile(file, Path.Combine(ColorMCGui.BaseDir, GuiNames.NameStartImageFile + name));

        GuiConfigUtils.Config.ServerCustom.StartIconFile = GuiNames.NameStartImageFile + name;
        GuiConfigUtils.Save();

        ImageManager.LoadStartIcon();
    }

    /// <summary>
    /// 解压客户端
    /// </summary>
    /// <param name="model"></param>
    /// <param name="item"></param>
    public static async void ReadBuildConfig(BaseModel model, IStorageItem item)
    {
        if (item.GetPath() is not { } file)
        {
            return;
        }

        using var temp = PathHelper.OpenRead(file)!;
        model.Progress(App.Lang("MainWindow.Info46"));
        await new ZipUtils().UnzipAsync(ColorMCGui.BaseDir, file, temp);
        model.ProgressClose();

        ColorMCGui.Reboot();
    }
}
