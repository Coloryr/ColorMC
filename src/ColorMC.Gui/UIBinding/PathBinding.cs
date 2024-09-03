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
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SkiaSharp;

namespace ColorMC.Gui.UIBinding;

public static class PathBinding
{
    private static readonly string[] EXE = ["*.exe"];
    private static readonly string[] ZIP = ["*.zip", "*.tar.xz", "*.tar.gz"];
    private static readonly string[] JSON = ["*.json"];
    private static readonly string[] MODPACK = ["*.zip", "*.mrpack"];
    private static readonly string[] PICFILE = ["*.png", "*.jpg", "*.bmp"];
    private static readonly string[] AUDIO = ["*.mp3", "*.wav"];
    private static readonly string[] MODEL = ["*.model3.json"];
    private static readonly string[] HEADFILE = ["*.png"];
    private static readonly string[] ZIPFILE = ["*.zip"];
    private static readonly string[] JARFILE = ["*.jar"];

    /// <summary>
    /// 提升权限
    /// </summary>
    /// <param name="path">文件</param>
    public static void Chmod(string path)
    {
        try
        {
            using var p = new Process();
            p.StartInfo.FileName = "sh";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine("chmod a+x " + path);

            p.StandardInput.WriteLine("exit");
            p.WaitForExit();
        }
        catch (Exception e)
        {
            Logs.Error("chmod error", e);
        }
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
            case PathType.ShaderpacksPath:
                OpenPathWithExplorer(obj.GetShaderpacksPath());
                break;
            case PathType.ResourcepackPath:
                OpenPathWithExplorer(obj.GetResourcepacksPath());
                break;
            case PathType.WorldBackPath:
                OpenPathWithExplorer(obj.GetWorldBackupPath());
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
                OpenPathWithExplorer(ColorMCCore.BaseDir);
                break;
            case PathType.RunPath:
                OpenPathWithExplorer(AppContext.BaseDirectory);
                break;
            case PathType.DownloadPath:
                OpenPathWithExplorer(DownloadManager.DownloadDir);
                break;
            case PathType.JavaPath:
                OpenPathWithExplorer(JvmPath.BaseDir + JvmPath.Name1);
                break;
            case PathType.PicPath:
                OpenPathWithExplorer(ImageManager.Local);
                break;
        }
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    /// <param name="obj">世界存储</param>
    public static void OpenPath(WorldObj obj)
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
    public static async Task<string?> SelectPath(TopLevel top, PathType type)
    {
        switch (type)
        {
            case PathType.ServerPackPath:
                var res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.Lang("PathBinding.Text2")
                });
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case PathType.GamePath:
                res = await top.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.Lang("PathBinding.Text6")
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
    public static async Task<bool?> SaveFile(TopLevel top, FileType type, object[]? arg)
    {
        switch (type)
        {
            case FileType.User:
                var file = await SaveFile(top,
                    App.Lang("PathBinding.Text41"), ".json", "user.json");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    PathHelper.WriteText(file.GetPath()!,
                    JsonConvert.SerializeObject(AuthDatabase.Auths.Values, Formatting.Indented));

                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("PathBinding.Errro2"), e);
                    return false;
                }
            case FileType.World:
                file = await SaveFile(top,
                    App.Lang("PathBinding.Text18"), ".zip", "world.zip");
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
            case FileType.Skin:
                file = await SaveFile(top,
                    App.Lang("PathBinding.Text1"), ".png", "skin.png");
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
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("PathBinding.Errro2"), e);
                    return false;
                }
            case FileType.Text:
                file = await SaveFile(top,
                    App.Lang("PathBinding.Text3"), ".txt", "log.txt");
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
                    Logs.Error(App.Lang("PathBinding.Errro3"), e);
                    return false;
                }
            case FileType.InputConfig:
                file = await SaveFile(top,
                    App.Lang("PathBinding.Text15"), ".json", arg![0] + ".json");
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
                    Logs.Error(App.Lang("PathBinding.Errro4"), e);
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
    private static async Task<IReadOnlyList<IStorageFile>?> SelectFile(TopLevel? top, string title,
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
                     Patterns = new List<string>(ext)
                }
            }
        });
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="window">窗口</param>
    /// <param name="type">类型</param>
    /// <returns>路径</returns>
    public static async Task<(string?, string?)> SelectFile(TopLevel top, FileType type)
    {
        switch (type)
        {
            case FileType.Java:
                var res = await SelectFile(top,
                    App.Lang("PathBinding.Text26"),
                    SystemInfo.Os == OsType.Windows ? EXE : null,
                    App.Lang("PathBinding.Text27"),
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
                    App.Lang("PathBinding.Text28"),
                    ZIP,
                    App.Lang("PathBinding.Text29"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Config:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text30"),
                    JSON,
                    App.Lang("PathBinding.Text31"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.AuthConfig:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text32"),
                    JSON,
                    App.Lang("PathBinding.Text33"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.ModPack:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text4"),
                    MODPACK,
                    App.Lang("PathBinding.Text5"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Pic:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text34"),
                    PICFILE,
                    App.Lang("PathBinding.Text35"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Music:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text11"),
                    AUDIO,
                    App.Lang("PathBinding.Text12"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Live2D:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text36"),
                    MODEL,
                    App.Lang("PathBinding.Text37"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Icon:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text7"),
                    PICFILE,
                    App.Lang("PathBinding.Text8"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Head:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text9"),
                    HEADFILE,
                    App.Lang("PathBinding.Text10"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Live2DCore:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text38"),
                    ZIPFILE,
                    App.Lang("PathBinding.Text39"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.Loader:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text24"),
                    JARFILE,
                    App.Lang("PathBinding.Text25"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
            case FileType.InputConfig:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text13"),
                    JSON,
                    App.Lang("PathBinding.Text14"));
                if (res?.Any() == true)
                {
                    return (res[0].GetPath(), res[0].Name);
                }
                break;
        }

        return (null, null);
    }

    public static async Task<bool?> AddFile(TopLevel top, GameSettingObj obj, FileType type)
    {
        switch (type)
        {
            case FileType.Schematic:
                var res = await SelectFile(top,
                      App.Lang("GameEditWindow.Tab12.Text1"),
                      ["*" + Schematic.Name1, "*" + Schematic.Name2],
                      App.Lang("PathBinding.Text23"), true);
                if (res?.Any() == true)
                {
                    return GameBinding.AddSchematic(obj, res);
                }
                return null;
            case FileType.Shaderpack:
                res = await SelectFile(top,
                    App.Lang("GameEditWindow.Tab11.Text1"),
                    ZIPFILE,
                    App.Lang("PathBinding.Text22"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddShaderpack(obj, res);
                }
                return null;
            case FileType.Mod:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text16"),
                    JARFILE,
                    App.Lang("PathBinding.Text17"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddMods(obj, res);
                }
                return null;
            case FileType.World:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text18"),
                    ZIPFILE,
                    App.Lang("PathBinding.Text19"));
                if (res?.Any() == true)
                {
                    return await GameBinding.AddWorld(obj, res[0].GetPath());
                }
                return null;
            case FileType.Resourcepack:
                res = await SelectFile(top,
                    App.Lang("PathBinding.Text20"),
                    ZIPFILE,
                    App.Lang("PathBinding.Text21"), true);
                if (res?.Any() == true)
                {
                    return await GameBinding.AddResourcepack(obj, res);
                }
                return null;
        }

        return null;
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="top"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool?> Export(TopLevel top, GameExportModel model)
    {
        if (model.Type == PackType.ColorMC)
        {
            var file = await SaveFile(top, App.Lang("PathBinding.Text40"),
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

                using var s = new ZipFile(PathHelper.OpenWrite(name, false));
                var data = JsonConvert.SerializeObject(obj1);
                var data1 = Encoding.UTF8.GetBytes(data);
                using var stream = new ZipFileStream(data1);
                s.BeginUpdate();
                s.Add(stream, InstancesPath.Name14);
                s.CommitUpdate();

                OpenFileWithExplorer(name);
                return true;
            }
            catch (Exception e)
            {
                string temp = App.Lang("PathBinding.Errro5");
                WindowManager.ShowError(temp, e);
                Logs.Error(temp, e);
                return false;
            }
        }
        else if (model.Type == PackType.CurseForge)
        {
            var file = await SaveFile(top, App.Lang("PathBinding.Text40"),
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
                string temp = App.Lang("PathBinding.Errro5");
                WindowManager.ShowError(temp, e);
                Logs.Error(temp, e);
                return false;
            }
        }
        else if (model.Type == PackType.Modrinth)
        {
            var file = await SaveFile(top, App.Lang("PathBinding.Text40"),
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
                string temp = App.Lang("PathBinding.Errro5");
                WindowManager.ShowError(temp, e);
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
            Logs.Error(App.Lang("PathBinding.Error1"), e);
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