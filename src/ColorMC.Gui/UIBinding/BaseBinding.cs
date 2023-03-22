using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils.LaunchSetting;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public const string DrapType = "Game";

    private readonly static Dictionary<Process, GameSettingObj> Games = new();
    private readonly static Dictionary<string, Process> RunGames = new();
    private readonly static Dictionary<string, string> GameLogs = new();
    private static Mutex mutex1;
    public static bool ISNewStart => ColorMCCore.NewStart;

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

        try
        {
            Media.Init();
        }
        catch (Exception e)
        {
            Logs.Error("openal error", e);
        }
        FontSel.Instance.Load();
        ColorSel.Instance.Load();
    }

    public static void UpdateState(string info)
    {
        var window = App.GetMainWindow();
        if (window == null)
        {
            return;
        }
        Dispatcher.UIThread.Post(() =>
        {
            window.Info1.Show(info);
        });
    }

    public static Task<bool> PackUpdate(string info)
    {
        return Dispatcher.UIThread.InvokeAsync(() => App.HaveUpdate(info));
    }

    public static void NoJava()
    {
        Dispatcher.UIThread.Post(() =>
        {
            App.ShowSetting(SettingType.SetJava);
        });
    }

    public static Task<IReadOnlyList<IStorageFile>> OpFile(IBaseWindow? window, string title,
        string[] ext, string name, bool multiple = false, IStorageFolder? storage = null)
    {
        return OpFile(window as TopLevel, title, ext, name, multiple, storage);
    }

    public static Task<IReadOnlyList<IStorageFile>> OpFile(TopLevel? window, string title,
        string[] ext, string name, bool multiple = false, IStorageFolder? storage = null)
    {
        if (window == null)
            return null;

        return window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = title,
            AllowMultiple = multiple,
            SuggestedStartLocation = storage,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new(name)
                {
                     Patterns = new List<string>(ext)
                }
            }
        });
    }

    public static List<string> GetFilterName() => new()
    {
        App.GetLanguage("BaseBinding.Filter.Item1"),
        App.GetLanguage("BaseBinding.Filter.Item2"),
        App.GetLanguage("BaseBinding.Filter.Item3")
    };

    public static List<string> GetSkinRotateName() => new()
    {
        App.GetLanguage("BaseBinding.SkinRotate.Item1"),
        App.GetLanguage("BaseBinding.SkinRotate.Item2"),
        App.GetLanguage("BaseBinding.SkinRotate.Item3")
    };

    public static Task<IStorageFile?> OpSave(TopLevel window, string title, string ext, string name)
    {
        return window.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = title,
            DefaultExtension = ext,
            SuggestedFileName = name
        });
    }

    public static void Exit()
    {
        ColorMCCore.Close();
        ColorSel.Instance.Stop();
#if !DEBUG
        mutex1.Dispose();
#endif
    }

    public static bool IsGameRun(GameSettingObj obj)
    {
        return RunGames.ContainsKey(obj.UUID);
    }

    public static void StopGame(GameSettingObj obj)
    {
        if (RunGames.TryGetValue(obj.UUID, out var item))
        {
            Task.Run(item.Kill);
        }
    }

    public static void OpenDownloadPath()
    {
        OpPath(DownloadManager.DownloadDir);
    }

    public static void OpenDownloadJavaPath()
    {
        OpPath(Path.GetFullPath(JvmPath.BaseDir + JvmPath.Name1));
    }

    public static async Task<(bool, string?)> Launch(GameSettingObj obj, LoginObj obj1)
    {
        if (Games.ContainsValue(obj))
        {
            return (false, App.GetLanguage("GameBinding.Error4"));
        }
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfo(GuiConfigUtils.Config.ServerCustom.IP,
                GuiConfigUtils.Config.ServerCustom.Port);

            obj = obj.CopyObj();
            obj.StartServer.IP = server.ServerAddress;
            obj.StartServer.Port = server.ServerPort;
        }
        if (App.GameEditWindows.TryGetValue(obj.UUID, out var win))
        {
            win?.ClearLog();
        }

        ColorMCCore.DownloaderUpdate = DownloaderUpdateOnThread;

        string? temp = null;

        if (GameLogs.ContainsKey(obj.UUID))
        {
            GameLogs[obj.UUID] = "";
        }
        else
        {
            GameLogs.Add(obj.UUID, "");
        }

        var res = await Task.Run(() =>
        {
            try
            {
                return obj.StartGame(obj1).Result;
            }
            catch (Exception e)
            {
                UserBinding.RemoveLockUser(obj1);
                if (e.InnerException is LaunchException launch)
                {
                    temp = string.IsNullOrWhiteSpace(launch.Message)
                        ? App.GetLanguage("Error6") : launch.Message;
                    if (launch.Ex != null)
                    {
                        Logs.Error(temp, launch.Ex);
                        App.ShowError(temp, launch.Ex);
                    }
                    else
                    {
                        temp = launch.Message;
                    }
                }
                else
                {
                    temp = App.GetLanguage("Error6");
                    Logs.Error(temp, e);
                    App.ShowError(temp, e);
                }
                return null;
            }
        });
        Funtcions.RunGC();
        if (res != null)
        {
            UserBinding.AddLockUser(obj1);

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                _ = Task.Run(() =>
                {
                    Task.Delay(1000);
                    try
                    {
                        res.WaitForInputIdle();

                        if (res.HasExited)
                            return;

                        Dispatcher.UIThread.Post(() =>
                        {
                            App.Close();
                        });
                    }
                    catch
                    {

                    }
                });
            }

            res.Exited += (a, b) =>
            {
                RunGames.Remove(obj.UUID);
                UserBinding.RemoveLockUser(obj1);
                if (Games.Remove(res, out var obj2))
                {
                    App.MainWindow?.GameClose(obj2);
                }
                GameLogs.Remove(obj.UUID, out var log);
                if (a is Process p)
                {
                    if (p.ExitCode == 0)
                    {
                        return;
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        App.ShowError(App.GetLanguage("UserBinding.Error2"), log ?? "");
                    });
                }
                res.Dispose();
            };
            Games.Add(res, obj);
            RunGames.Add(obj.UUID, res);
        }

        ColorMCCore.DownloaderUpdate = DownloaderUpdate;

        return (res != null, temp);
    }

    public static void DownloaderUpdateOnThread(CoreRunState state)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            App.DownloaderUpdate(state);
        }).Wait();
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        App.DownloaderUpdate(state);
    }

    public static void PLog(Process? p, string? d)
    {
        if (p == null)
            return;
        if (Games.TryGetValue(p, out var obj))
        {
            GameLogs[obj.UUID] += d + Environment.NewLine;
            if (App.GameEditWindows.TryGetValue(obj.UUID, out var win))
            {
                win?.Log(d);
            }
        }
    }

    public static void PLog(GameSettingObj obj, string? d)
    {
        GameLogs[obj.UUID] += d + Environment.NewLine;
        if (App.GameEditWindows.TryGetValue(obj.UUID, out var win))
        {
            win?.Log(d);
        }
    }

    private static void ShowError(string data, Exception e, bool close)
    {
        App.ShowError(data, e, close);
    }

    private static void Change(LanguageType type)
    {
        App.LoadLanguage(type);
        Localizer.Instance.Reload();
    }

    public static (int, int) GetDownloadSize()
    {
        return (DownloadManager.AllSize, DownloadManager.DoneSize);
    }

    public static CoreRunState DownloadState
        => DownloadManager.State;

    public static bool IsDownload
        => DownloadManager.State != CoreRunState.End;

    public static List<string> GetDownloadSources()
    {
        var list = new List<string>
        {
            SourceLocal.Offical.GetName(),
            SourceLocal.BMCLAPI.GetName(),
            SourceLocal.MCBBS.GetName()
        };

        return list;
    }

    public static List<string> GetWindowTranTypes()
    {
        return new()
        {
            App.GetLanguage("TranTypes.Item1"),
            App.GetLanguage("TranTypes.Item2"),
            App.GetLanguage("TranTypes.Item3"),
            App.GetLanguage("TranTypes.Item4"),
            App.GetLanguage("TranTypes.Item5")
        };
    }

    public static List<string> GetLanguages()
    {
        var list = new List<string>
        {
            LanguageType.zh_cn.GetName(),
            LanguageType.en_us.GetName()
        };

        return list;
    }

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

    public static byte[] GetUIJson()
    {
        return App.GetFile("ColorMC.Gui.Resource.UI.CustomUI.json");
    }

    public static void OpPath(string item)
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

    public static async Task<string?> OpPath(IBaseWindow window, FileType type)
    {
        if (window is TopLevel top)
        {
            return await OpPath(top, type);
        }

        return null;
    }

    public static async Task<string?> OpPath(TopLevel window, FileType type)
    {
        switch (type)
        {
            case FileType.ServerPack:
                var res = await window.StorageProvider.OpenFolderPickerAsync(new()
                {
                    Title = App.GetLanguage("Gui.Info11")
                });
                if (res.Any())
                {
                    return res.First().GetPath();
                }

                break;
        }

        return null;
    }

    public static void OpUrl(string url)
    {
        url = url.Replace(" ", "%20");
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

    public static string GetPath(this IStorageFolder file)
    {
        file.TryGetUri(out var uri);
        return uri!.LocalPath;
    }

    public static string GetPath(this IStorageFile file)
    {
        file.TryGetUri(out var uri);
        return uri!.LocalPath;
    }

    public static List<string> GetFontList()
    {
        return FontManager.Current.GetInstalledFontFamilyNames().ToList();
    }

    public static void OpenBaseDir()
    {
        OpPath(ColorMCCore.BaseDir);
    }

    public static bool IsLaunch()
    {
#if !DEBUG
        mutex1 = new Mutex(true, "ColorMC-lock" +
            ColorMCCore.BaseDir.Replace("\\", "_").Replace("/", "_"), out var isnew);

        return !isnew;
#else
        return false;
#endif
    }

    public static async Task<bool?> AddFile(Window? window, GameSettingObj obj, FileType type)
    {
        if (window == null)
            return false;
        switch (type)
        {
            case FileType.Schematic:
                var res = await OpFile(window,
                      App.GetLanguage("GameEditWindow.Tab12.Info1"),
                      new string[] { "*" + Schematic.Name1, "*" + Schematic.Name2 },
                      App.GetLanguage("GameEditWindow.Tab12.Info2"), true);
                if (!res.Any())
                {
                    return null;
                }
                return await GameBinding.AddSchematic(obj, res);
            case FileType.Shaderpack:
                res = await OpFile(window,
                    App.GetLanguage("GameEditWindow.Tab11.Info1"),
                    new string[] { "*.zip" },
                    App.GetLanguage("GameEditWindow.Tab11.Info2"), true);
                if (!res.Any())
                {
                    return null;
                }
                return await GameBinding.AddShaderpack(obj, res);
            case FileType.Mod:
                res = await OpFile(window,
                    App.GetLanguage("GameEditWindow.Tab4.Info7"),
                    new string[] { "*.jar" },
                    App.GetLanguage("GameEditWindow.Tab4.Info8"), true);
                if (!res.Any())
                {
                    return null;
                }
                return await GameBinding.AddMods(obj, res);
            case FileType.World:
                res = await OpFile(window!,
                    App.GetLanguage("GameEditWindow.Tab5.Info2"),
                    new string[] { "*.zip" },
                    App.GetLanguage("GameEditWindow.Tab5.Info8"));
                if (!res.Any())
                {
                    return null;
                }
                return await GameBinding.AddWorld(obj, res[0].GetPath());
            case FileType.Resourcepack:
                res = await OpFile(window,
                    App.GetLanguage("GameEditWindow.Tab8.Info2"),
                    new string[] { "*.zip" },
                    App.GetLanguage("GameEditWindow.Tab8.Info7"), true);
                if (!res.Any())
                {
                    return null;
                }
                return await GameBinding.AddResourcepack(obj, res);
        }

        return null;
    }

    public static Task<bool?> SaveFile(IBaseWindow? window, FileType type, object[]? arg)
    {
        return SaveFile(window as TopLevel, type, arg);
    }

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
                    await GameBinding.ExportWorld((arg![0] as WorldDisplayObj)!.World, file.GetPath());
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.GetLanguage("GameEditWindow.Tab5.Error1"), e);
                    return false;
                }
            case FileType.Game:
                file = await OpSave(window,
                    App.GetLanguage("GameEditWindow.Tab6.Info1"), ".zip", "game.zip");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    await GameBinding.ExportGame((arg![0] as GameSettingObj)!, name,
                        (arg[1] as List<string>)!, (PackType)arg[2]);
                    OpFile(name);
                    return true;
                }
                catch (Exception e)
                {
                    Logs.Error(App.GetLanguage("GameEditWindow.Tab6.Error1"), e);
                    return false;
                }
            case FileType.UI:
                file = await OpSave(window,
                    App.GetLanguage("SettingWindow.Tab6.Info1"), ".json", "ui.json");
                if (file == null)
                    break;

                try
                {
                    var name = file.GetPath();
                    if (File.Exists(name))
                    {
                        File.Delete(name);
                    }

                    File.WriteAllBytes(name, GetUIJson());
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
        }

        return null;
    }

    public static async Task<string?> OpFile(IBaseWindow window, FileType type)
    {
        if (window is TopLevel top)
        {
            return await OpFile(top, type);
        }

        return null;
    }

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
                    if (file.EndsWith("java.exe"))
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
                    App.GetLanguage("HelloWindow.Tab2.Info3"),
                    new string[] { "*.json" },
                    App.GetLanguage("HelloWindow.Tab2.Info7"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.AuthConfig:
                res = await OpFile(window,
                    App.GetLanguage("HelloWindow.Tab2.Info6"),
                    new string[] { "*.json" },
                    App.GetLanguage("HelloWindow.Tab2.Info8"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
            case FileType.ModPack:
                res = await OpFile(window,
                    App.GetLanguage("AddGameWindow.Info13"),
                    new string[] { "*.zip", "*.mrpack" },
                    App.GetLanguage("AddGameWindow.Info15"));
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
                    new string[] { "*.json" },
                    App.GetLanguage("SettingWindow.Tab6.Info3"));
                if (res?.Any() == true)
                {
                    return res[0].GetPath();
                }
                break;
        }

        return null;
    }

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
        }
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

    public static void OpenPicPath()
    {
        OpPath(ImageTemp.Local);
    }

    public static void DownloadStop()
    {
        DownloadManager.DownloadStop();
    }

    public static void DownloadPause()
    {
        DownloadManager.DownloadPause();
    }

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
                if (res.Item1 && !string.IsNullOrWhiteSpace(res.Item2?.UI))
                {
                    GuiConfigUtils.Config.ServerCustom.UIFile = res.Item2.UI;
                    GuiConfigUtils.Save();
                }

                if (res.Item1)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await window.Info.ShowOk(App.GetLanguage("Gui.Info13"));
                        App.Close();
                    });
                }
                else if (res.Item2 != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        window.Info.Show(App.GetLanguage("Gui.Error18"));
                    });
                }

            }
            catch (Exception e)
            {
                string text = App.GetLanguage("Gui.Error19");
                Logs.Error(text, e);
                Dispatcher.UIThread.Post(() =>
                {
                    window.Info1.Close();
                    window.Info.Show(text);
                });
            }
        }
    }

    public static string MakeUrl(ServerModItemObj item, FileType type, string url)
    {
        return UrlHelper.MakeUrl(item, type, url);
    }

    public static string GetRunDir()
    {
        return ColorMCCore.BaseDir;
    }

    public static void SetVolume(int value)
    {
        if (value > 100 || value < 0)
            return;

        Media.Volume = (float)value / 100;
    }

    public static void MusicStart()
    {
        bool play = false;
        Media.Volume = 0;
        string file = GuiConfigUtils.Config.ServerCustom.Music;
        if (file.StartsWith("http://") || file.StartsWith("https://"))
        {
            Media.PlayUrl(file);
            play = true;
        }
        else
        {
            file = Path.GetFullPath(file);
            if (File.Exists(file))
            {
                if (file.EndsWith(".mp3"))
                {
                    Media.PlayMp3(file);
                    play = true;
                }
                else if (file.EndsWith(".wav"))
                {
                    Media.PlayWAV(file);
                    play = true;
                }
            }
        }
        if (play)
        {
            if (GuiConfigUtils.Config.ServerCustom.SlowVolume)
            {
                Task.Run(() =>
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

    public static void MusicStop()
    {
        Media.Stop();
    }

    public static void MusicPlay()
    {
        Media.Play();
    }

    public static void MusicPause()
    {
        Media.Pause();
    }
}
