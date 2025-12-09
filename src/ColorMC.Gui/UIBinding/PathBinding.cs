using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.Utils;
using SkiaSharp;

namespace ColorMC.Gui.UIBinding;

/// <summary>
/// 路径相关
/// </summary>
public static class PathBinding
{
    private static readonly string[] EXE = ["*.exe"];
    private static readonly string[] ZIP = ["*.zip", "*.tar.xz", "*.tar.gz"];
    private static readonly string[] JSON = ["*.json"];
    private static readonly string[] Modpack = ["*.zip", "*.mrpack"];
    private static readonly string[] PICFILE = ["*.png", "*.jpg", "*.bmp"];
    private static readonly string[] AUDIO = ["*.mp3", "*.wav", "*.flac"];
    private static readonly string[] HEADFILE = ["*.png"];
    private static readonly string[] ZIPFILE = ["*.zip"];
    private static readonly string[] JARFILE = ["*.jar"];

    /// <summary>
    /// 获取文件夹占用大小
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public static string GetFolderSize(string folderPath)
    {
        return GetSizeReadable(PathHelper.GetFolderSize(folderPath));
    }

    /// <summary>
    /// 获取文件夹占用大小
    /// </summary>
    /// <param name="bytes">大小</param>
    /// <returns></returns>
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

    /// <summary>
    /// 在资源管理器打开路径
    /// </summary>
    /// <param name="item">路径</param>
    private static void OpenPathWithExplorer(string item)
    {
        item = Path.GetFullPath(item);
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                Process.Start("explorer", $"{item}");
                break;
            case OsType.Linux:
                Process.Start("xdg-open", '"' + item + '"');
                break;
            case OsType.MacOS:
                Process.Start("open", '"' + item + '"');
                break;
        }
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">路径类型</param>
    public static void OpenPath(GameSettingObj obj, PathType type)
    {
        switch (type)
        {
            case PathType.JsonDir:
                OpenPathWithExplorer(obj.GetGameJsonPath());
                break;
            case PathType.ShaderpacksPath:
                OpenPathWithExplorer(obj.GetShaderpacksPath());
                break;
            case PathType.ResourcepackPath:
                OpenPathWithExplorer(obj.GetResourcepacksPath());
                break;
            case PathType.WorldBackPath:
                OpenPathWithExplorer(obj.GetSaveBackupPath());
                break;
            case PathType.SavePath:
                OpenPathWithExplorer(obj.GetSavesPath());
                break;
            case PathType.GamePath:
                OpenPathWithExplorer(obj.GetGamePath());
                break;
            case PathType.SchematicsPath:
                OpenPathWithExplorer(obj.GetSchematicsPath());
                break;
            case PathType.ScreenshotsPath:
                OpenPathWithExplorer(obj.GetScreenshotsPath());
                break;
            case PathType.ModPath:
                OpenPathWithExplorer(obj.GetModsPath());
                break;
            case PathType.BasePath:
                OpenPathWithExplorer(obj.GetBasePath());
                break;
        }
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="type">路径类型</param>
    public static void OpenPath(PathType type)
    {
        switch (type)
        {
            case PathType.BasePath:
                OpenPathWithExplorer(ColorMCGui.BaseDir);
                break;
            case PathType.RunPath:
                OpenPathWithExplorer(AppContext.BaseDirectory);
                break;
            case PathType.DownloadPath:
                OpenPathWithExplorer(DownloadManager.DownloadDir);
                break;
            case PathType.JavaPath:
                OpenPathWithExplorer(JvmPath.JavaDir);
                break;
            case PathType.PicPath:
                OpenPathWithExplorer(ImageManager.GetImagePath());
                break;
        }
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="obj">世界存储</param>
    public static void OpenPath(SaveObj obj)
    {
        OpenPathWithExplorer(obj.Local);
    }

    /// <summary>
    /// 在资源管理器打开文件
    /// </summary>
    /// <param name="item">文件</param>
    public static void OpenFileWithExplorer(string item)
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
    /// 选择路径
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<string?> SelectPathAsync(TopLevel top, PathType type)
    {
        switch (type)
        {
            case PathType.ServerPackPath:
                var res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = LangUtils.Get("App.Select.Text2")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case PathType.GamePath:
                res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = LangUtils.Get("App.Select.Text5")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case PathType.RunDir:
                res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = LangUtils.Get("SettingWindow.Tab1.Text31")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case PathType.JavaPath:
                res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = LangUtils.Get("SettingWindow.Tab5.Text19")
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
    /// 保存文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="title">标题</param>
    /// <param name="ext">后缀</param>
    /// <param name="name">名字</param>
    /// <returns>文件路径</returns>
    public static Task<IStorageFile?> SaveFileAsync(TopLevel window, string title, string ext, string name)
    {
        return window.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = title,
            DefaultExtension = ext,
            SuggestedFileName = name
        });
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <param name="arg">参数</param>
    /// <returns>结果</returns>
    public static async Task<bool?> SaveFileAsync(TopLevel top, FileType type, object[]? arg)
    {
        switch (type)
        {
            case FileType.User:
                var file = await SaveFileAsync(top,
                    LangUtils.Get("App.Select.Text25"), ".json", "user.json");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    PathHelper.WriteText(name!,
                    JsonUtils.ToString([.. AuthDatabase.Auths.Values], JsonType.ListLoginObj));
                    OpenFileWithExplorer(name!);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(LangUtils.Get("App.Error.Log7"), e);
                    return false;
                }
            case FileType.Save:
                file = await SaveFileAsync(top,
                    LangUtils.Get("App.Select.Text15"), ".zip", "world.zip");
                if (file == null)
                    break;

                try
                {
                    await (arg![0] as SaveObj)!.ExportSaveZip(file.GetPath()!);
                    OpenFileWithExplorer(file.GetPath()!);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(LangUtils.Get("GameEditWindow.Tab5.Text22"), e);
                    return false;
                }
            case FileType.Skin:
                file = await SaveFileAsync(top,
                    LangUtils.Get("App.Select.Text1"), ".png", "skin.png");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    var data = ImageManager.SkinBitmap?.Encode(SKEncodedImageFormat.Png, 100);
                    if (data?.AsSpan().ToArray() is { } data1)
                    {
                        PathHelper.WriteBytes(name!, data1);
                    }
                    OpenFileWithExplorer(name!);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(LangUtils.Get("App.Error.Log7"), e);
                    return false;
                }
            case FileType.Text:
                file = await SaveFileAsync(top,
                    LangUtils.Get("App.Select.Text3"), ".txt", "log.txt");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return false;
                    await File.WriteAllTextAsync(name, arg![0] as string);
                    OpenFileWithExplorer(name);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(LangUtils.Get("App.Error.Log8"), e);
                    return false;
                }
            case FileType.InputConfig:
                file = await SaveFileAsync(top,
                    LangUtils.Get("App.Select.Text13"), ".json", arg![0] + ".json");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return false;
                    await PathHelper.WriteTextAsync(name, JsonUtils.ToString((InputControlObj)arg![1], JsonGuiType.InputControlObj));
                    OpenFileWithExplorer(name);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(LangUtils.Get("App.Error.Log9"), e);
                    return false;
                }
            case FileType.Cmd:
                file = await SaveFileAsync(top,
                    LangUtils.Get("MainWindow.Text63"), (string)arg![0], (string)arg![1]);
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return false;
                    await File.WriteAllTextAsync(name, (string)arg![2]);
                    OpenFileWithExplorer(name);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(LangUtils.Get("App.Error.Log11"), e);
                    return false;
                }
        }

        return null;
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="top">窗口</param>
    /// <param name="title">标题</param>
    /// <param name="ext">后缀</param>
    /// <param name="name">名字</param>
    /// <param name="multiple">多选</param>
    /// <param name="storage">首选路径</param>
    /// <returns></returns>
    private static async Task<IReadOnlyList<IStorageFile>?> SelectFileAsync(TopLevel? top, string title,
        string[]? ext, string name, bool multiple = false, DirectoryInfo? storage = null)
    {
        if (top == null)
            return null;

        var defaultFolder = storage == null ? null : await top.StorageProvider.TryGetFolderFromPathAsync(storage.FullName);

        return await top.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = title,
            AllowMultiple = multiple,
            SuggestedStartLocation = defaultFolder,
            FileTypeFilter = ext == null ? null : new List<FilePickerFileType>()
            {
                new(name)
                {
                     Patterns = [.. ext]
                }
            }
        });
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="top">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<SelectRes> SelectFileAsync(TopLevel top, FileType type)
    {
        IReadOnlyList<IStorageFile>? res = null;
        switch (type)
        {
            case FileType.File:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text33"),
                    null,
                    LangUtils.Get("App.Select.Text34"));
                break;
            case FileType.Java:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text20"),
                    SystemInfo.Os == OsType.Windows ? EXE : null,
                    LangUtils.Get("App.Select.Text21"),
                    storage: JavaBinding.GetSuggestedStartLocation());
                if (res?.Any() == true)
                {
                    var file = res[0].GetPath()!;
                    if (SystemInfo.Os == OsType.Windows && file.EndsWith("java.exe"))
                    {
                        var file1 = file[..^4] + "w.exe";
                        if (File.Exists(file1))
                        {
                            return new SelectRes
                            {
                                Path = file1,
                                FileName = "javaw.exe"
                            };
                        }
                    }
                }
                break;
            case FileType.JavaZip:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text22"),
                    ZIP,
                    LangUtils.Get("App.Select.Text23"));
                break;
            case FileType.Config:
                res = await SelectFileAsync(top,
                    LangUtils.Get("BuildPackWindow.Tab1.Text1"),
                    JSON,
                    LangUtils.Get("Text.Config"));
                break;
            case FileType.AuthConfig:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text24"),
                    JSON,
                    LangUtils.Get("App.Select.Text25"));
                break;
            case FileType.Modpack:
                res = await SelectFileAsync(top,
                    LangUtils.Get("AddGameWindow.Tab2.Text3"),
                    Modpack,
                    LangUtils.Get("App.Select.Text4"));
                break;
            case FileType.Pic:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text26"),
                    PICFILE,
                    LangUtils.Get("App.Select.Text27"));
                break;
            case FileType.Icon:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text29"),
                    PICFILE,
                    LangUtils.Get("App.Select.Text30"));
                break;
            case FileType.Music:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text10"),
                    AUDIO,
                    LangUtils.Get("App.Select.Text11"));
                break;
            case FileType.GameIcon:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text6"),
                    PICFILE,
                    LangUtils.Get("App.Select.Text7"));
                break;
            case FileType.StartIcon:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text31"),
                    PICFILE,
                    LangUtils.Get("App.Select.Text32"));
                break;
            case FileType.Head:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text8"),
                    HEADFILE,
                    LangUtils.Get("App.Select.Text9"));
                break;
            case FileType.Loader:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text18"),
                    JARFILE,
                    LangUtils.Get("App.Select.Text19"));
                break;
            case FileType.InputConfig:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text12"),
                    JSON,
                    LangUtils.Get("SettingWindow.Tab8.Text5"));
                break;
        }

        if (res?.Any() == true)
        {
            return new SelectRes
            {
                Path = res[0].GetPath(),
                FileName = res[0].Name
            };
        }

        return new SelectRes();
    }

    /// <summary>
    /// 导入文件
    /// </summary>
    /// <param name="top">窗口</param>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">文件类型</param>
    /// <returns></returns>
    public static async Task<bool?> AddFileAsync(TopLevel top, GameSettingObj obj, FileType type)
    {
        switch (type)
        {
            case FileType.Schematic:
                var res = await SelectFileAsync(top,
                      LangUtils.Get("GameEditWindow.Tab12.Text1"),
                      [$"*{Names.NameLitematicExt}", $"*{Names.NameSchematicExt}"],
                      LangUtils.Get("Text.Schematic"), true);
                if (res?.Any() == true)
                {
                    return GameBinding.AddSchematic(obj, res);
                }
                return null;
            case FileType.Shaderpack:
                res = await SelectFileAsync(top,
                    LangUtils.Get("GameEditWindow.Tab11.Text1"),
                    ZIPFILE,
                    LangUtils.Get("Type.FileType.Shaderpack"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddShaderpackAsync(obj, res);
                }
                return null;
            case FileType.Mod:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text14"),
                    JARFILE,
                    LangUtils.Get("Type.FileType.Mod"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddModsAsync(obj, res);
                }
                return null;
            case FileType.Save:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text15"),
                    ZIPFILE,
                    LangUtils.Get("App.Select.Text16"));
                if (res?.Any() == true)
                {
                    return await GameBinding.AddWorldAsync(obj, res[0].GetPath());
                }
                return null;
            case FileType.Resourcepack:
                res = await SelectFileAsync(top,
                    LangUtils.Get("App.Select.Text17"),
                    ZIPFILE,
                    LangUtils.Get("Type.FileType.Resourcepack"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddResourcepackAsync(obj, res);
                }
                return null;
        }

        return null;
    }

    /// <summary>
    /// 导出整合包
    /// </summary>
    /// <param name="top">窗口</param>
    /// <param name="model">导出信息</param>
    /// <returns></returns>
    public static async Task<bool?> ExportAsync(TopLevel top, GameExportModel model)
    {
        string? name = "";
        if (model.Type == PackType.ColorMC)
        {
            var file = await SaveFileAsync(top, LangUtils.Get("App.Select.Text28"),
                  ".zip", $"{model.Obj.Name}.zip");
            if (file == null)
                return null;

            name = file.GetPath();
            if (name == null)
                return null;
        }
        else if (model.Type == PackType.CurseForge)
        {
            var file = await SaveFileAsync(top, LangUtils.Get("App.Select.Text28"),
               ".zip", $"{model.Name}-{model.Version}.zip");
            if (file == null)
                return null;

            name = file.GetPath();
            if (name == null)
                return null;
        }
        else if (model.Type == PackType.Modrinth)
        {
            var file = await SaveFileAsync(top, LangUtils.Get("App.Select.Text28"),
               ".zip", $"{model.Name}-{model.Version}.mrpack");
            if (file == null)
                return null;

            name = file.GetPath();
            if (name == null)
                return null;
        }

        var arg = new GameExportArg
        {
            File = name,
            Type = model.Type,
            Mods = model.Mods.Select(item => new ModExportObj()
            {
                Export = item.Export,
                FID = item.FID,
                FileSize = item.FileSize,
                Obj = item.Obj,
                Obj1 = item.Obj1,
                PID = item.PID,
                Sha1 = item.Sha1,
                Sha512 = item.Sha512,
                Type = item.Type,
                Url = item.Url,
                Source = item.Source
            }),
            Obj = model.Obj,
            UnSelectItems = model.Files.GetUnSelectItems(),
            SelectItems = model.Files.GetSelectItems(),
            OtherFiles = model.OtherFiles.Select(item => new ModExportBaseObj()
            {
                Path = item.Path,
                FileSize = item.FileSize,
                Sha1 = item.Sha1,
                Sha512 = item.Sha512,
                Type = item.Type,
                Url = item.Url
            }),
            Author = model.Author,
            Name = model.Name,
            Version = model.Version,
            Summary = model.Summary
        };

        try
        {
            var res = await GameExport.ExportAsync(arg);
            OpenFileWithExplorer(arg.File);
            return true;
        }
        catch (Exception e)
        {
            string temp = LangUtils.Get("App.Error.Log10");
            WindowManager.ShowError(temp, e);
            Logs.Error(temp, e);
            return false;
        }
    }

    /// <summary>
    /// 打开图片
    /// </summary>
    /// <param name="screenshot">图片路径</param>
    public static void OpenPicFile(string screenshot)
    {
        screenshot = Path.GetFullPath(screenshot);
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                {
                    var proc = new Process();
                    proc.StartInfo.WorkingDirectory = ColorMCCore.BaseDir;
                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.Arguments = $"/c start \"\" \"{screenshot}\"";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    break;
                }
            case OsType.Linux:
                Process.Start("xdg-open",
                    '"' + screenshot + '"');
                break;
            case OsType.MacOS:
                Process.Start("open", "-a Preview " +
                    '"' + screenshot + '"');
                break;
        }
    }
}