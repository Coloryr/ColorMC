using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.BuildPack;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    /// <summary>
    /// 快捷启动
    /// </summary>
    private static string[]? s_startLaunch;

    public static readonly DataFormat<string> DrapType = DataFormat.CreateStringApplicationFormat("Game");

    /// <summary>
    /// 核心初始化完成
    /// </summary>
    public static async void Init1()
    {
        WindowManager.MainWindow?.LoadDone();

        await GameCloudUtils.StartConnect();

        if (s_startLaunch != null)
        {
            Launch(s_startLaunch);
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
                WindowManager.MainWindow.Window?.WindowActivate();
                var list = new List<Guid>();
                foreach (var item in games)
                {
                    if (Guid.TryParse(item, out var uuid))
                    {
                        list.Add(uuid);
                    }
                }
                (WindowManager.MainWindow.DataContext as IMainTop)?.Launch(list);
            }
        });
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="text">文本</param>
    public static async void CopyTextClipboard(TopLevel top, string text)
    {
        if (top.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    /// <param name="top"></param>
    /// <param name="file">文件列表</param>
    public static async Task CopyFileClipboardAsync(TopLevel top, List<IStorageFile> file)
    {
        if (top.Clipboard is { } clipboard)
        {
            var obj = new DataTransfer();
            foreach (var item in file)
            {
                obj.Add(DataTransferItem.CreateFile(item));
            }
            await clipboard.SetDataAsync(obj);
        }
    }

    /// <summary>
    /// 在浏览器打开网址
    /// </summary>
    /// <param name="url">网址</param>
    public static void OpenUrl(string? url)
    {
        if (url == null)
        {
            return;
        }
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
                    ps.StandardInput.WriteLine($"start \"\" \"{url}\"");
                    ps.Close();
                }
                break;
            case OsType.Linux:
                Process.Start("xdg-open",
                    '"' + url + '"');
                break;
            case OsType.MacOs:
                Process.Start("open", '"' + url + '"');
                break;
        }
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="value">音量</param>
    public static void SetVolume(int value)
    {
        if (value > 100 || value < 0)
        {
            return;
        }

        Media.Volume = (float)value / 100;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    public static async Task<MusicPlayRes> MusicStartAsync(string file, bool loop, bool slow, int volume)
    {
        Media.Loop = loop;
        return await Media.PlayMusicAsync(file, slow, volume);
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
            var data = LangUtils.Get("App.Text112");
            Logs.Error(data, ex);
            WindowManager.ShowError(data, ex);

            return false;
        }
    }

    /// <summary>
    /// 设置快捷启动
    /// </summary>
    /// <param name="uuid"></param>
    public static void SetLaunch(string[] uuid)
    {
        s_startLaunch = uuid;
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
    /// 启动Frp
    /// </summary>
    /// <param name="item1">远程Frp项目</param>
    /// <param name="model">本地映射项目</param>
    /// <returns></returns>
    public static Task<FrpLaunchRes> StartFrpAsync(object item1, NetFrpLocalModel model)
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
    public static bool SetStar(FileItemModel model)
    {
        var obj = new CollectItemObj
        {
            Icon = model.Logo,
            Url = model.Url,
            FileType = model.Obj.Type,
            Source = model.Obj.Source,
            Name = model.Name,
            Pid = model.Obj.Pid
        };

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
    /// 清理缓存
    /// </summary>
    public static void DeleteTemp()
    {
        var temp = DownloadManager.DownloadDir;
        PathHelper.MoveToTrashAsync(temp);
        Directory.CreateDirectory(temp);
    }

    /// <summary>
    /// 给压缩包添加一个文件
    /// </summary>
    /// <param name="zip">压缩包</param>
    /// <param name="file">文件名</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private static async Task PutFileAsync(ZipWriter zip, string file, string path)
    {
        using var buffer = PathHelper.OpenRead(path)!;
        using var stream = zip.WriteToStream(file, new ZipWriterEntryOptions());
        buffer.Seek(0, SeekOrigin.Begin);
        await buffer.CopyToAsync(stream);
    }

    /// <summary>
    /// 给压缩包添加一个文件
    /// </summary>
    /// <param name="zip">压缩包</param>
    /// <param name="file">文件名</param>
    /// <param name="data">文件内容</param>
    /// <returns></returns>
    private static async Task PutFileAsync(ZipWriter zip, string file, byte[] data)
    {
        using var stream = zip.WriteToStream(file, new ZipWriterEntryOptions());
        await stream.WriteAsync(data);
    }

    /// <summary>
    /// 给压缩包添加一个文件夹
    /// </summary>
    /// <param name="zip">压缩包</param>
    /// <param name="file">文件名</param>
    /// <param name="path">路径</param>
    /// <param name="basepath">基础路径，用于替换</param>
    /// <returns></returns>
    private static async Task PutFileAsync(ZipWriter zip, string file, string path, string basepath)
    {
        foreach (var item in PathHelper.GetAllFiles(path))
        {
            string tempfile = file + "/" + item.FullName[(basepath.Length + 1)..];
            tempfile = tempfile.Replace("\\", "/");
            using var buffer = PathHelper.OpenRead(item.FullName)!;
            using var stream = zip.WriteToStream(file, new ZipWriterEntryOptions());
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
        using var stream = PathHelper.OpenRead(file);
        if (stream == null)
        {
            return;
        }
        var obj = JsonDocument.Parse(stream);
        var json = obj.RootElement;
        var conf = GuiConfigUtils.Config;
        var conf1 = ConfigLoad.Config;
        var conf2 = conf.ServerCustom;
        var conf3 = conf.LauncherFunction;

        if (json.TryGetProperty(nameof(BuildPackModel.UiBg), out var value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.UiBackConfigObj)!;
            conf.EnableBG = config.EnableBG;
            conf.BackImage = config.BackImage;
            conf.BackEffect = config.BackEffect;
            conf.BackTran = config.BackTran;
            conf.BackLimit = config.BackLimit;
            conf.BackLimitValue = config.BackLimitValue;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.UiColor), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.UiColorConfigObj)!;
            conf.ColorType = config.ColorType;
            conf.ColorMain = config.ColorMain;
            conf.RGB = config.RGB;
            conf.RGBS = config.RGBS;
            conf.RGBV = config.RGBV;
            conf.WindowMode = config.WindowMode;
            conf.Style = config.Style;
            conf.LogColor = config.LogColor;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.UiOther), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.UiOtherConfigObj)!;
            conf.Head = config.Head;
            conf.CloseBeforeLaunch = config.CloseBeforeLaunch;
            conf.Card = config.Card;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.LaunchCheck), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.LaunchCheckConfigObj)!;
            conf1.GameCheck = config.GameCheck;
            conf.LaunchCheck = config.LaunchCheck;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.LaunchArg), out value)
             && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.RunArgObj)!;
            conf1.DefaultJvmArg = config;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.LaunchWindow), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.WindowSettingObj)!;
            conf1.Window = config;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.ServerOpt), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.ServerOptConfigObj)!;
            conf2.IP = config.IP;
            conf2.Port = config.Port;
            conf2.Motd = config.Motd;
            conf2.JoinServer = config.JoinServer;
            conf2.MotdColor = config.MotdColor;
            conf2.MotdBackColor = config.MotdBackColor;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.ServerLock), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.ServerLockConfigObj)!;
            conf2.LockGame = config.LockGame;
            conf2.GameName = config.GameName;
            conf2.LockLogin = config.LockLogin;
            conf2.LockLogins = config.LockLogins;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.ServerUi), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.ServerUiConfigObj)!;
            conf2.EnableUI = config.EnableUI;
            conf2.CustomIcon = config.CustomIcon;
            conf2.IconFile = config.IconFile;
            conf2.CustomStart = config.CustomStart;
            conf2.StartIconFile = config.StartIconFile;
            conf2.DisplayType = config.DisplayType;
            conf2.StartText = config.StartText;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.ServerMusic), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.ServerMusicConfigObj)!;
            conf2.PlayMusic = config.PlayMusic;
            conf2.Music = config.Music;
            conf2.Volume = config.Volume;
            conf2.SlowVolume = config.SlowVolume;
            conf2.MusicLoop = config.MusicLoop;
            conf2.RunPause = config.RunPause;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.Function), out value)
            && value.ValueKind is JsonValueKind.Object)
        {
            var config = value.Deserialize(JsonGuiType.LauncherFunctionConfigObj)!;
            conf3.FastLaunch = config.FastLaunch;
            conf3.FastModrinth = config.FastModrinth;
            conf3.AdminLaunch = config.AdminLaunch;
            conf3.GameAdminLaunch = config.GameAdminLaunch;
        }

        if (json.TryGetProperty(nameof(BuildPackModel.Javas), out value) && value.ValueKind is JsonValueKind.Array)
        {
            var list = value.Deserialize(JsonGuiType.ListJvmConfigObj)!;
            conf1.JavaList.AddRange(list);
        }

        GuiConfigUtils.SaveNow();
        ConfigLoad.SaveNow();

        File.Delete(file);
    }

    /// <summary>
    /// 打包客户端
    /// </summary>
    /// <param name="model"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static async Task<bool> BuildPackAsync(BuildPackModel model, ProgressModel dialog, string output)
    {
        try
        {
            var file = Path.Combine(DownloadManager.DownloadDir, FunctionUtils.NewUUID());
            var stream = PathHelper.OpenWrite(file);
            var zip = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));
            var conf = GuiConfigUtils.Config;
            var conf1 = ConfigLoad.Config;
            var conf2 = conf.ServerCustom;
            var conf3 = conf.LauncherFunction;

            var obj = new JsonObject();

            dialog.Text = LangUtils.Get("BuildPackWindow.Text3");

            //打包配置
            if (model.UiBg && File.Exists(conf.BackImage))
            {
                var filename = Path.GetFileName(conf.BackImage);
                await PutFileAsync(zip, filename, conf.BackImage);
                obj.Add(nameof(BuildPackModel.UiBg), JsonSerializer.SerializeToNode(new UiBackConfigObj()
                {
                    EnableBG = conf.EnableBG,
                    BackImage = conf.BackImage,
                    BackEffect = conf.BackEffect,
                    BackTran = conf.BackTran,
                    BackLimit = conf.BackLimit,
                    BackLimitValue = conf.BackLimitValue
                }, JsonGuiType.UiBackConfigObj));
            }

            if (model.UiColor)
            {
                obj.Add(nameof(BuildPackModel.UiColor), JsonSerializer.SerializeToNode(new UiColorConfigObj()
                {
                    ColorType = conf.ColorType,
                    ColorMain = conf.ColorMain,
                    RGB = conf.RGB,
                    RGBS = conf.RGBS,
                    RGBV = conf.RGBV,
                    WindowMode = conf.WindowMode,
                    Style = conf.Style,
                    LogColor = conf.LogColor
                }, JsonGuiType.UiColorConfigObj));
            }

            if (model.UiOther)
            {
                obj.Add(nameof(BuildPackModel.UiOther), JsonSerializer.SerializeToNode(new UiOtherConfigObj()
                {
                    Head = conf.Head,
                    CloseBeforeLaunch = conf.CloseBeforeLaunch,
                    Card = conf.Card
                }, JsonGuiType.UiOtherConfigObj));
            }

            if (model.LaunchCheck)
            {
                obj.Add(nameof(BuildPackModel.LaunchCheck), JsonSerializer.SerializeToNode(new LaunchCheckConfigObj()
                {
                    GameCheck = conf1.GameCheck,
                    LaunchCheck = conf.LaunchCheck,
                }, JsonGuiType.LaunchCheckConfigObj));
            }

            if (model.LaunchArg)
            {
                obj.Add(nameof(BuildPackModel.LaunchArg), JsonSerializer.SerializeToNode(conf1.DefaultJvmArg, JsonGuiType.RunArgObj));
            }

            if (model.LaunchWindow)
            {
                obj.Add(nameof(BuildPackModel.LaunchWindow), JsonSerializer.SerializeToNode(conf1.Window, JsonGuiType.WindowSettingObj));
            }

            if (model.ServerOpt)
            {
                obj.Add(nameof(BuildPackModel.ServerOpt), JsonSerializer.SerializeToNode(new ServerOptConfigObj()
                {
                    IP = conf2.IP,
                    Port = conf2.Port,
                    Motd = conf2.Motd,
                    JoinServer = conf2.JoinServer,
                    MotdColor = conf2.MotdColor,
                    MotdBackColor = conf2.MotdBackColor,
                }, JsonGuiType.ServerOptConfigObj));
            }

            if (model.ServerLock)
            {
                obj.Add(nameof(BuildPackModel.ServerLock), JsonSerializer.SerializeToNode(new ServerLockConfigObj()
                {
                    LockGame = conf2.LockGame,
                    GameName = conf2.GameName,
                    LockLogin = conf2.LockLogin,
                    LockLogins = conf2.LockLogins
                }, JsonGuiType.ServerLockConfigObj));
            }

            if (model.Function)
            {
                obj.Add(nameof(BuildPackModel.Function), JsonSerializer.SerializeToNode(new LauncherFunctionConfigObj()
                {
                    FastLaunch = conf3.FastLaunch,
                    FastModrinth = conf3.FastModrinth,
                    AdminLaunch = conf3.AdminLaunch,
                    GameAdminLaunch = conf3.GameAdminLaunch
                }, JsonGuiType.LauncherFunctionConfigObj));
            }

            if (model.ServerUi)
            {
                var uiFile = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameCustomUIFile);
                if (File.Exists(uiFile))
                {
                    await PutFileAsync(zip, GuiNames.NameCustomUIFile, uiFile);
                }
                uiFile = Path.Combine(ColorMCGui.BaseDir, conf2.IconFile);
                if (File.Exists(uiFile))
                {
                    await PutFileAsync(zip, conf2.IconFile, uiFile);
                }
                uiFile = Path.Combine(ColorMCGui.BaseDir, conf2.StartIconFile);
                if (File.Exists(uiFile))
                {
                    await PutFileAsync(zip, conf2.StartIconFile, uiFile);
                }

                obj.Add(nameof(BuildPackModel.ServerUi), JsonSerializer.SerializeToNode(new ServerUiConfigObj()
                {
                    EnableUI = conf2.EnableUI,
                    CustomIcon = conf2.CustomIcon,
                    IconFile = conf2.IconFile,
                    CustomStart = conf2.CustomStart,
                    StartIconFile = conf2.StartIconFile,
                    DisplayType = conf2.DisplayType,
                    StartText = conf2.StartText
                }, JsonGuiType.ServerUiConfigObj));
            }

            if (model.ServerMusic)
            {
                var musicFile = conf2.Music;
                var filename = Path.GetFileName(musicFile);
                if (File.Exists(musicFile))
                {
                    await PutFileAsync(zip, filename!, musicFile);
                }

                obj.Add(nameof(BuildPackModel.ServerMusic), JsonSerializer.SerializeToNode(new ServerMusicConfigObj()
                {
                    PlayMusic = conf2.PlayMusic,
                    Music = filename,
                    Volume = conf2.Volume,
                    SlowVolume = conf2.SlowVolume,
                    MusicLoop = conf2.MusicLoop,
                    RunPause = conf2.RunPause
                }, JsonGuiType.ServerMusicConfigObj));
            }

            if (model.PackUpdate)
            {
                if (File.Exists(UpdateUtils.LocalPath[0]))
                {
                    await PutFileAsync(zip, $"{GuiNames.NameDllDir}/ColorMC.Core.dll", UpdateUtils.LocalPath[0]);
                }
                if (File.Exists(UpdateUtils.LocalPath[1]))
                {
                    await PutFileAsync(zip, $"{GuiNames.NameDllDir}/ColorMC.Gui.dll", UpdateUtils.LocalPath[2]);
                }
            }

            //打包java
            if (model.Java)
            {
                dialog.Text = LangUtils.Get("BuildPackWindow.Text4");

                var list = new List<JvmConfigObj>();
                foreach (var item in model.Javas)
                {
                    if (!item.IsSelect)
                    {
                        continue;
                    }
                    await PutFileAsync(zip, Names.NameJavaDir, Path.Combine(JvmPath.JavaDir, item.Name), JvmPath.JavaDir);
                    list.Add(new()
                    {
                        Name = item.Name,
                        Local = item.Path.Replace(ColorMCGui.BaseDir, "")
                    });
                }

                obj.Add(nameof(BuildPackModel.Javas), JsonSerializer.SerializeToNode(list, JsonGuiType.ListJvmConfigObj));
            }

            var data = obj.ToJsonString();
            await PutFileAsync(zip, GuiNames.NameClientConfigFile, Encoding.UTF8.GetBytes(data));

            //打包启动器
            if (model.PackLaunch)
            {
                foreach (var item in UpdateUtils.LaunchFiles)
                {
                    var fileitem = Path.Combine(ColorMCGui.BaseDir,
                    item);
                    if (File.Exists(fileitem))
                    {
                        await PutFileAsync(zip, item, fileitem);
                    }
                }
            }

            dialog.Text = LangUtils.Get("BuildPackWindow.Text5");

            //打包游戏实例
            foreach (var item in model.GetSelectItems())
            {
                string tempfile = item[(ColorMCGui.BaseDir.Length)..];
                tempfile = tempfile.Replace("\\", "/");

                await PutFileAsync(zip, tempfile, item);
            }

            dialog.Text = LangUtils.Get("BuildPackWindow.Text6");

            foreach (var item in model.Files)
            {
                await PutFileAsync(zip, item.File, item.Local);
            }

            zip.Dispose();
            stream.Dispose();

            PathHelper.MoveFile(file, output);

            PathBinding.OpenFileWithExplorer(output);

            return true;
        }
        catch (Exception e)
        {
            WindowManager.ShowError(LangUtils.Get("BuildPackWindow.Text8"), e);
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
    public static async void ReadBuildConfig(WindowModel model, IStorageItem item)
    {
        if (item.GetPath() is not { } file)
        {
            return;
        }

        using var temp = PathHelper.OpenRead(file)!;
        var dialog = model.ShowProgress(LangUtils.Get("App.Text26"));
        await new ZipProcess().UnzipAsync(ColorMCGui.BaseDir, file, temp);
        model.CloseDialog(dialog);

        ColorMCGui.Reboot();
    }

    /// <summary>
    /// 读取语言文件
    /// </summary>
    /// <returns>语言</returns>
    public static async Task<JsonDocument?> ReadLang()
    {
        var version = BlockTexUtils.Blocks.Id;
        if (version == null)
        {
            return null;
        }
        var obj = VersionPath.GetVersion(version);
        var ass = obj?.AssetIndex?.GetIndex();
        string lang = "zh_cn.json";
        if (GuiConfigUtils.Config.Language == LanguageType.en_us)
        {
            lang = "en_us.json";
        }
        if (ass == null || !ass.Objects.TryGetValue("minecraft/lang/" + lang, out var zh))
        {
            return null;
        }
        var file = GameDownloadHelper.BuildAssetsItem("minecraft/lang/" + lang, zh.Hash);
        if (!File.Exists(file.Local))
        {
            await DownloadManager.StartAsync([file]);
        }
        using var stream = AssetsPath.ReadAssets(zh.Hash);
        if (stream == null)
        {
            return null;
        }
        return JsonDocument.Parse(stream);
    }

    /// <summary>
    /// 获取解锁的方块列表
    /// </summary>
    /// <returns>方块列表</returns>
    public static async Task<List<BlockItemModel>?> BuildUnlockItems()
    {
        var lang = await ReadLang();
        if (lang == null)
        {
            return null;
        }
        var list = new List<BlockItemModel>();

        foreach (var item in BlockTexUtils.Unlocks.List)
        {
            if (!BlockTexUtils.Blocks.Tex.TryGetValue(item, out var tex))
            {
                continue;
            }

            list.Add(new BlockItemModel(item, GetBlockName(lang, item), item,
                ImageManager.GetBlockIcon(item, tex), 0));
        }

        return list;
    }

    public static string? GetBlockName(JsonDocument? document, string key)
    {
        if (document == null)
        {
            return null;
        }

        var keys = key.Split(":");
        var key1 = keys.Length == 2 ? keys[1] : keys[0];
        if (document.RootElement.TryGetProperty("block.minecraft." + key1, out var name)
                && name.ValueKind == JsonValueKind.String)
        {
            return name.GetString();
        }
        else if (document.RootElement.TryGetProperty("block.minecraft." + key1.Replace("_powered", ""), out var name1)
            && name.ValueKind == JsonValueKind.String)
        {
            return name1.GetString();
        }

        return null;
    }

    /// <summary>
    /// 获取今日方块
    /// </summary>
    /// <returns>方块</returns>
    public static async Task<BlockItemModel?> GetBlock()
    {
        if (!BlockTexUtils.IsGet())
        {
            return null;
        }

        var lang = await ReadLang();
        if (lang == null)
        {
            return null;
        }
        var item = BlockTexUtils.Unlocks.Today;

        if (!BlockTexUtils.Blocks.Tex.TryGetValue(item, out var tex))
        {
            return null;
        }
        return new BlockItemModel(item, GetBlockName(lang, item), item,
                ImageManager.GetBlockIcon(item, tex), 0);
    }

    /// <summary>
    /// 生成幸运方块列表
    /// </summary>
    /// <returns>方块列表</returns>
    public static async Task<List<BlockItemModel>?> BuildLotteryItems()
    {
        var lang = await ReadLang();
        if (lang == null)
        {
            return null;
        }

        var list = new List<BlockItemModel>();

        foreach (var item in BlockTexUtils.Blocks.Tex)
        {
            list.Add(new BlockItemModel(item.Key, GetBlockName(lang, item.Key), item.Key,
                    ImageManager.GetBlockIcon(item.Key, item.Value), 0));
        }

        var random = new Random();
        return [.. list.OrderBy(x => random.Next())];
    }

    /// <summary>
    /// 开始加载方块列表
    /// </summary>
    /// <returns>加载结果</returns>
    public static async Task<StringRes> StartLoadBlock()
    {
        var temp = TaskManager.StartMutexTask(GuiNames.NameKeyLoadBlock);
        if (temp != null)
        {
            var res = await temp;
            if (res is StringRes res1)
            {
                return res1;
            }

            return new StringRes();
        }

        var res2 = await BlockTexUtils.LoadNow();
        TaskManager.StopMutexTask(GuiNames.NameKeyLoadBlock, res2);

        return res2;
    }
}
