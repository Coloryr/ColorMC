using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SkiaSharp;

namespace ColorMC.Gui.UIBinding;

public static class PathBinding
{
    /// <summary>
    /// 在资源管理器打开路径
    /// </summary>
    /// <param name="item">路径</param>
    private static void OpPath(string item)
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

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="type">路径类型</param>
    public static void OpPath(PathType type)
    {
        switch (type)
        {
            case PathType.BasePath:
                OpPath(ColorMCCore.BaseDir);
                break;
            case PathType.RunPath:
                OpPath(AppContext.BaseDirectory);
                break;
            case PathType.DownloadPath:
                OpPath(DownloadManager.DownloadDir);
                break;
            case PathType.JavaPath:
                OpPath(JvmPath.BaseDir + JvmPath.Name1);
                break;
            case PathType.PicPath:
                OpPath(ImageUtils.Local);
                break;
        }
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="obj">世界存储</param>
    public static void OpPath(WorldObj obj)
    {
        OpPath(obj.Local);
    }

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
    /// 选择路径
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<string?> SelectPath(PathType type)
    {
        var top = App.TopLevel;
        if (top == null)
        {
            return null;
        }
        switch (type)
        {
            case PathType.ServerPackPath:
                var res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.Lang("Gui.Info11")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case PathType.GamePath:
                res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.Lang("Gui.Info24")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case PathType.RunDir:
                res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.Lang("SettingWindow.Tab1.Info14")
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
    private static Task<IStorageFile?> SaveFile(TopLevel window, string title, string ext, string name)
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
    public static async Task<bool?> SaveFile(FileType type, object[]? arg)
    {
        var top = App.TopLevel;
        if (top == null)
        {
            return false;
        }

        switch (type)
        {
            case FileType.World:
                var file = await SaveFile(top,
                    App.Lang("GameEditWindow.Tab5.Info2"), ".zip", "world.zip");
                if (file == null)
                    break;

                try
                {
                    await GameBinding.ExportWorld((arg![0] as WorldObj)!,
                        file.GetPath());
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("GameEditWindow.Tab5.Error1"), e);
                    return false;
                }
            case FileType.UI:
                file = await SaveFile(top,
                    App.Lang("SettingWindow.Tab6.Info1"), ".axaml", "ui.axaml");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return null;
                    if (File.Exists(name))
                    {
                        PathHelper.Delete(name);
                    }

                    File.WriteAllBytes(name, PathUtils.GetFile("ColorMC.Gui.Resource.UI.UI.axaml"));
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("SettingWindow.Tab6.Error3"), e);
                    return false;
                }
            case FileType.Skin:
                file = await SaveFile(top,
                    App.Lang("Gui.Info9"), ".png", "skin.png");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    var data = UserBinding.SkinImage?.Encode(SKEncodedImageFormat.Png, 100);
                    if (data?.AsSpan().ToArray() is { } data1)
                    {
                        PathHelper.WriteBytes(name!, data1);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("SettingWindow.Tab6.Error3"), e);
                    return false;
                }
            case FileType.Text:
                file = await SaveFile(top,
                    App.Lang("Gui.Info21"), ".txt", "log.txt");
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
                    Logs.Error(App.Lang("SettingWindow.Tab6.Error3"), e);
                    return false;
                }
            case FileType.InputConfig:
                file = await SaveFile(top,
                    App.Lang("SettingWindow.Tab8.Info13"), ".json", arg![0] + ".json");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (name == null)
                        return false;
                    await File.WriteAllTextAsync(name, JsonConvert.SerializeObject(arg![1]));
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("SettingWindow.Tab8.Error3"), e);
                    return false;
                }
        }

        return null;
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
    private static async Task<IReadOnlyList<IStorageFile>?> SelectFile(TopLevel? window, string title,
        string[]? ext, string name, bool multiple = false, DirectoryInfo? storage = null)
    {
        if (window == null)
            return null;

        var defaultFolder = storage == null ? null : await window.StorageProvider.TryGetFolderFromPathAsync(storage.FullName);

        return await window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = title,
            AllowMultiple = multiple,
            SuggestedStartLocation = defaultFolder,
            FileTypeFilter = ext == null ? null : new List<FilePickerFileType>()
            {
                new(name)
                {
                     Patterns = new List<string>(ext)
                }
            }
        });
    }

    private static readonly string[] EXE = ["*.exe"];
    private static readonly string[] ZIP = ["*.zip", "*.tar.xz", "*.tar.gz"];
    private static readonly string[] JSON = ["*.json"];
    private static readonly string[] MODPACK = ["*.zip", "*.mrpack"];
    private static readonly string[] PICFILE = ["*.png", "*.jpg", "*.bmp"];
    private static readonly string[] UIFILE = ["*.axaml"];
    private static readonly string[] AUDIO = ["*.mp3", "*.wav"];
    private static readonly string[] MODEL = ["*.model3.json"];
    private static readonly string[] HEADFILE = ["*.png"];
    private static readonly string[] ZIPFILE = ["*.zip"];
    private static readonly string[] JARFILE = ["*.jar"];

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<(string?, string?)> SelectFile(FileType type)
    {
        var top = App.TopLevel;
        if (top == null)
        {
            return (null, null);
        }

        switch (type)
        {
            case FileType.Java:
                var res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab5.Info2"),
                    SystemInfo.Os == OsType.Windows ? EXE : null,
                    App.Lang("SettingWindow.Tab5.Info2"),
                    storage: JavaBinding.GetSuggestedStartLocation());
                if (res?.Any() == true)
                {
                    var file = res[0].GetPath();
                    if (file == null)
                        return (null, null);
                    if (SystemInfo.Os == OsType.Windows && file.EndsWith("java.exe"))
                    {
                        var file1 = file[..^4] + "w.exe";
                        if (File.Exists(file1))
                            return (file1, "javaw.exe");
                    }

                    return (file, res[0].Name);
                }
                break;
            case FileType.JavaZip:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab5.Info5"),
                    ZIP,
                    App.Lang("SettingWindow.Tab5.Info5"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Config:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab1.Info7"),
                    JSON,
                    App.Lang("SettingWindow.Tab1.Info11"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.AuthConfig:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab1.Info10"),
                    JSON,
                    App.Lang("SettingWindow.Tab1.Info12"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.ModPack:
                res = await SelectFile(top,
                    App.Lang("Gui.Info22"),
                    MODPACK,
                    App.Lang("Gui.Info23"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Pic:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab2.Info3"),
                    PICFILE,
                    App.Lang("SettingWindow.Tab2.Info6"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.UI:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab6.Info2"),
                    UIFILE,
                    App.Lang("SettingWindow.Tab6.Info3"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Music:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab6.Info5"),
                    AUDIO,
                    App.Lang("SettingWindow.Tab6.Info6"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Live2D:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab2.Info7"),
                    MODEL,
                    App.Lang("SettingWindow.Tab2.Info8"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Icon:
                res = await SelectFile(top,
                    App.Lang("Gui.Info37"),
                    PICFILE,
                    App.Lang("Gui.Info38"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Head:
                res = await SelectFile(top,
                    App.Lang("Gui.Info39"),
                    HEADFILE,
                    App.Lang("Gui.Info40"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Live2DCore:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab2.Info9"),
                    ZIPFILE,
                    App.Lang("SettingWindow.Tab2.Info10"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Loader:
                res = await SelectFile(top,
                    App.Lang("AddGameWindow.Tab1.Info17"),
                    JARFILE,
                    App.Lang("AddGameWindow.Tab1.Info18"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.InputConfig:
                res = await SelectFile(top,
                    App.Lang("SettingWindow.Tab8.Info11"),
                    JSON,
                    App.Lang("SettingWindow.Tab8.Info12"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
        }

        return (null, null);
    }

    public static async Task<bool?> AddFile(GameSettingObj obj, FileType type)
    {
        var top = App.TopLevel;
        if (top == null)
        {
            return false;
        }

        switch (type)
        {
            case FileType.Schematic:
                var res = await SelectFile(top,
                      App.Lang("GameEditWindow.Tab12.Text1"),
                      ["*" + Schematic.Name1, "*" + Schematic.Name2],
                      App.Lang("GameEditWindow.Tab12.Info2"), true);
                if (res?.Any() == true)
                {
                    return GameBinding.AddSchematic(obj, res);
                }
                return null;
            case FileType.Shaderpack:
                res = await SelectFile(top,
                    App.Lang("GameEditWindow.Tab11.Info1"),
                    ZIPFILE,
                    App.Lang("GameEditWindow.Tab11.Info2"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddShaderpack(obj, res);
                }
                return null;
            case FileType.Mod:
                res = await SelectFile(top,
                    App.Lang("GameEditWindow.Tab4.Info7"),
                    JARFILE,
                    App.Lang("GameEditWindow.Tab4.Info8"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddMods(obj, res);
                }
                return null;
            case FileType.World:
                res = await SelectFile(top,
                    App.Lang("GameEditWindow.Tab5.Info2"),
                    ZIPFILE,
                    App.Lang("GameEditWindow.Tab5.Info6"));
                if (res?.Any() == true)
                {
                    return await GameBinding.AddWorld(obj, res[0].GetPath());
                }
                return null;
            case FileType.Resourcepack:
                res = await SelectFile(top,
                    App.Lang("GameEditWindow.Tab8.Info2"),
                    ZIPFILE,
                    App.Lang("GameEditWindow.Tab8.Info5"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddResourcepack(obj, res);
                }
                return null;
        }

        return null;
    }

    public static async Task<bool?> Export(GameExportModel model)
    {
        var top = App.TopLevel;
        if (top == null)
        {
            return false;
        }

        if (model.Type == PackType.ColorMC)
        {
            var file = await SaveFile(top,
                   App.Lang("GameEditWindow.Tab6.Info1"),
                   ".zip", $"{model.Obj.Name}.zip");
            if (file == null)
                return null;

            var name = file.GetPath();
            if (name == null)
                return null;

            try
            {
                var list = model.Files.GetUnSelectItems();
                list.Add(model.Obj.GetModInfoJsonFile());

                var obj1 = new Dictionary<string, ModInfoObj>();
                foreach (var item in model.Mods)
                {
                    if (item.Export && item.Obj1 != null)
                    {
                        obj1.Add(item.Obj1.ModId, item.Obj1);
                    }
                }

                await new ZipUtils().ZipFileAsync(model.Obj.GetBasePath(),
                    name, list);

                using var s = new ZipFile(PathHelper.OpenRead(name));
                var data = JsonConvert.SerializeObject(obj1);
                var data1 = Encoding.UTF8.GetBytes(data);
                using var stream = new ZipFileStream(data1);
                s.BeginUpdate();
                s.Add(stream, InstancesPath.Name14);
                s.CommitUpdate();

                OpFile(name);
                return true;
            }
            catch (Exception e)
            {
                string temp = App.Lang("GameEditWindow.Tab6.Error1");
                App.ShowError(temp, e);
                Logs.Error(temp, e);
                return false;
            }
        }
        else if (model.Type == PackType.CurseForge)
        {
            var file = await SaveFile(top,
               App.Lang("GameEditWindow.Tab6.Info1"),
               ".zip", $"{model.Name}-{model.Version}.zip");
            if (file == null)
                return null;

            var name = file.GetPath();
            if (name == null)
                return null;

            var obj = new CurseForgePackObj()
            {
                name = model.Name,
                author = model.Author,
                version = model.Version,
                manifestType = "minecraftModpack",
                manifestVersion = 1,
                overrides = "overrides",
                minecraft = new()
                {
                    version = model.Obj.Version,
                    modLoaders = []
                },
                files = []
            };

            if (model.Obj.Loader != Loaders.Normal)
            {
                obj.minecraft.modLoaders.Add(new()
                {
                    id = $"{model.Obj.Loader.ToString().ToLower()}-{model.Obj.LoaderVersion}",
                    primary = true
                });
            }

            foreach (var item in model.Mods)
            {
                if (item.Export)
                {
                    obj.files.Add(new()
                    {
                        fileID = int.Parse(item.FID!),
                        projectID = int.Parse(item.PID!),
                        required = true
                    });
                }
            }

            var data = await CurseForgeAPI.GetModsInfo(obj.files);
            StringBuilder html = new();
            html.AppendLine("<ul>");
            if (data != null)
            {
                foreach (var item in data.data)
                {
                    html.AppendLine($"<li><a href=\"{item.links.websiteUrl}\">{item.name} (by {item.authors.GetString()})</a></li>");
                }
            }
            html.AppendLine("</ul>");

            var crc = new Crc32();

            try
            {
                using var s = new ZipOutputStream(File.Create(name));
                s.SetLevel(9);

                //manifest.json
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented));
                    var entry = new ZipEntry("manifest.json")
                    {
                        DateTime = DateTime.Now,
                        Size = buffer.Length
                    };
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    await s.PutNextEntryAsync(entry);
                    await s.WriteAsync(buffer);
                }

                //modlist.html
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());
                    var entry = new ZipEntry("modlist.html")
                    {
                        DateTime = DateTime.Now,
                        Size = buffer.Length
                    };
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    await s.PutNextEntryAsync(entry);
                    await s.WriteAsync(buffer);
                }

                {
                    var path = model.Obj.GetGamePath();

                    foreach (var item in model.Files.GetSelectItems())
                    {
                        var name1 = item[path.Length..];
                        name1 = name1.Replace("\\", "/");
                        if (!name1.StartsWith('/'))
                        {
                            name1 = '/' + name1;
                        }
                        name1 = "overrides/" + name1;
                        byte[] buffer = File.ReadAllBytes(item);
                        var entry = new ZipEntry(name1)
                        {
                            DateTime = DateTime.Now,
                            Size = buffer.Length
                        };
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        await s.PutNextEntryAsync(entry);
                        await s.WriteAsync(buffer);
                    }
                }

                await s.FinishAsync(CancellationToken.None);
                s.Close();
            }
            catch (Exception e)
            {
                string temp = App.Lang("GameEditWindow.Tab6.Error1");
                App.ShowError(temp, e);
                Logs.Error(temp, e);
                return false;
            }
        }
        else if (model.Type == PackType.Modrinth)
        {
            var file = await SaveFile(top,
               App.Lang("GameEditWindow.Tab6.Info1"),
               ".zip", $"{model.Name}-{model.Version}.mrpack");
            if (file == null)
                return null;

            var name = file.GetPath();
            if (name == null)
                return null;

            var obj = new ModrinthPackObj()
            {
                formatVersion = 1,
                name = model.Name,
                versionId = model.Version,
                summary = model.Summary,
                files = [],
                dependencies = []
            };

            obj.dependencies.Add("minecraft", model.Obj.Version);
            switch (model.Obj.Loader)
            {
                case Loaders.Forge:
                case Loaders.NeoForge:
                    obj.dependencies.Add("forge", model.Obj.LoaderVersion!);
                    break;
                case Loaders.Fabric:
                    obj.dependencies.Add("fabric-loader", model.Obj.LoaderVersion!);
                    break;
                case Loaders.Quilt:
                    obj.dependencies.Add("quilt-loader", model.Obj.LoaderVersion!);
                    break;
            }

            foreach (var item in model.Mods)
            {
                if (item.Source != null)
                {
                    obj.files.Add(new()
                    {
                        path = item.Obj1!.Name,
                        hashes = new()
                        {
                            sha1 = item.Sha1,
                            sha512 = item.Sha512
                        },
                        downloads = [item.Url],
                        fileSize = item.FileSize
                    });
                }
            }

            foreach (var item in model.OtherFiles)
            {
                obj.files.Add(new()
                {
                    path = item.Path,
                    hashes = new()
                    {
                        sha1 = item.Sha1,
                        sha512 = item.Sha512
                    },
                    downloads = [item.Url],
                    fileSize = item.FileSize
                });
            }

            var crc = new Crc32();

            try
            {
                using var s = new ZipOutputStream(File.Create(name));
                s.SetLevel(9);

                //manifest.json
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented));
                    var entry = new ZipEntry("modrinth.index.json")
                    {
                        DateTime = DateTime.Now,
                        Size = buffer.Length
                    };
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    await s.PutNextEntryAsync(entry);
                    await s.WriteAsync(buffer);
                }

                {
                    var path = model.Obj.GetGamePath();

                    foreach (var item in model.Files.GetSelectItems())
                    {
                        var name1 = item[path.Length..];
                        name1 = name1.Replace("\\", "/");
                        if (!name1.StartsWith('/'))
                        {
                            name1 = '/' + name1;
                        }
                        name1 = "overrides/" + name1;
                        byte[] buffer = File.ReadAllBytes(item);
                        var entry = new ZipEntry(name1)
                        {
                            DateTime = DateTime.Now,
                            Size = buffer.Length
                        };
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        await s.PutNextEntryAsync(entry);
                        await s.WriteAsync(buffer);
                    }
                }

                await s.FinishAsync(CancellationToken.None);
                s.Close();
            }
            catch (Exception e)
            {
                string temp = App.Lang("GameEditWindow.Tab6.Error1");
                App.ShowError(temp, e);
                Logs.Error(temp, e);
                return false;
            }
        }
        return true;
    }

    public static async Task CopyBG(string pic)
    {
        try
        {
            using var stream = ColorMCCore.PhoneReadFile(pic);
            if (stream == null)
                return;
            string file = ColorMCGui.RunDir + "BG";
            PathHelper.Delete(file);
            using var temp = File.Create(file);
            await stream.CopyToAsync(temp);
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("SettingWindow.Tab2.Error5"), e);
        }
    }

    public static void OpenPicFile(string screenshot)
    {
        screenshot = Path.GetFullPath(screenshot);
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                {
                    var proc = new Process();
                    proc.StartInfo.WorkingDirectory = ColorMCCore.BaseDir;
                    proc.StartInfo.FileName = "rundll32.exe";
                    proc.StartInfo.Arguments = @"C:\WINDOWS\system32\shimgvw.dll,ImageView_Fullscreen " + screenshot;
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