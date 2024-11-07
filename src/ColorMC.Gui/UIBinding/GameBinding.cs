using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Chunk;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Chunk;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SkiaSharp;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    /// <summary>
    /// 是否不存在游戏
    /// </summary>
    public static bool IsNotGame => InstancesPath.IsNotGame;

    /// <summary>
    /// 停止启动游戏
    /// </summary>
    private static CancellationTokenSource s_launchCancel = new();

    /// <summary>
    /// 获取游戏实例列表
    /// </summary>
    /// <returns></returns>
    public static List<GameSettingObj> GetGames()
    {
        return InstancesPath.Games;
    }

    /// <summary>
    /// 获取游戏版本号
    /// </summary>
    /// <param name="type">发布类型</param>
    /// <returns></returns>
    public static Task<List<string>> GetGameVersions(GameType type)
    {
        return GameHelper.GetGameVersions(type);
    }

    /// <summary>
    /// 添加游戏实例
    /// </summary>
    /// <param name="game"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<bool> AddGame(GameSettingObj game, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte)
    {
        var game1 = await InstancesPath.CreateGame(new CreateGameArg
        {
            Game = game,
            Request = request,
            Overwirte = overwirte
        });
        if (game1 != null)
        {
            ConfigBinding.SetLastLaunch(game1.UUID);
        }

        return game1 != null;
    }

    /// <summary>
    /// 导入文件夹
    /// </summary>
    /// <param name="name"></param>
    /// <param name="local"></param>
    /// <param name="unselect"></param>
    /// <param name="group"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<GameRes> AddGame(string? name, string local, List<string>? unselect,
        string? group, ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.ZipUpdate state, bool open)
    {
        var res = await AddGameHelper.AddGameFolder(new()
        {
            Local = local,
            Name = name,
            Unselect = unselect,
            Group = group,
            Request = request,
            Overwirte = overwirte,
            State = state
        });

        if (!res.State)
        {
            return res;
        }

        if (res.Game != null && open)
        {
            WindowManager.ShowGameEdit(res.Game);
        }

        return res;
    }

    /// <summary>
    /// 导入压缩包
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="zip"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static Task<GameRes> AddPack(string dir, PackType type, string? name,
        string? group, ColorMCCore.ZipUpdate zip, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte, ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return AddGameHelper.InstallZip(new InstallZipArg
        {
            Dir = dir,
            Type = type,
            Name = name,
            Group = group,
            Zip = zip,
            Request = request,
            Overwirte = overwirte,
            Update = update,
            Update2 = update2
        });
    }

    /// <summary>
    /// 获取游戏分组
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    /// <summary>
    /// 获取CF支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetCurseForgeGameVersions()
    {
        return CurseForgeHelper.GetGameVersions();
    }

    /// <summary>
    /// 获取MO支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetModrinthGameVersions()
    {
        return ModrinthHelper.GetGameVersion();
    }

    /// <summary>
    /// 获取CF分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Task<Dictionary<string, string>?> GetCurseForgeCategories(
        FileType type = FileType.ModPack)
    {
        return CurseForgeHelper.GetCategories(type);
    }

    /// <summary>
    /// 获取MO分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Task<Dictionary<string, string>?> GetModrinthCategories(FileType type = FileType.ModPack)
    {
        return ModrinthHelper.GetModrinthCategories(type);
    }

    /// <summary>
    /// 安装CF整合包
    /// </summary>
    /// <param name="data"></param>
    /// <param name="data1"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="zip"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static async Task<GameRes> InstallCurseForge(CurseForgeModObj.Data data,
        CurseForgeObjList.Data data1, string? name, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return await AddGameHelper.InstallCurseForge(new DownloadCurseForgeArg
        {
            Data = data,
            Data1 = data1,
            Name = name,
            Group = group,
            Zip = zip,
            Request = request,
            Overwirte = overwirte,
            Update = update,
            Update2 = update2
        });
    }

    /// <summary>
    /// 安装MO整合包
    /// </summary>
    /// <param name="data"></param>
    /// <param name="data1"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="zip"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static async Task<GameRes> InstallModrinth(ModrinthVersionObj data,
        ModrinthSearchObj.Hit data1, string? name, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return await AddGameHelper.InstallModrinth(new DownloadModrinthArg
        {
            Data = data,
            Data1 = data1,
            Name = name,
            Group = group,
            Zip = zip,
            Request = request,
            Overwirte = overwirte,
            Update = update,
            Update2 = update2
        });
    }

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="model"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task SetGameIconFromFile(TopLevel top, BaseModel model, GameSettingObj obj)
    {
        try
        {
            var file = await PathBinding.SelectFile(top, FileType.Icon);
            if (file.Item1 != null)
            {
                bool resize = await model.ShowWait(App.Lang("GameBinding.Info14"));

                model.Progress(App.Lang("GameBinding.Info4"));
                using var info = SKBitmap.Decode(PathHelper.OpenRead(file.Item1)!);

                if (resize && (info.Width > 100 || info.Height > 100))
                {
                    using var image = await Task.Run(() =>
                    {
                        return ImageUtils.Resize(info, 100, 100);
                    });
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    obj.SetGameIconFromBytes(data.AsSpan().ToArray());
                }
                else
                {
                    using var data = info.Encode(SKEncodedImageFormat.Png, 100);
                    obj.SetGameIconFromBytes(data.AsSpan().ToArray());
                }

                model.ProgressClose();
                model.Notify(App.Lang("GameBinding.Info3"));
            }
        }
        catch (Exception e)
        {
            var text = App.Lang("GameBinding.Error2");
            Logs.Error(text, e);
            WindowManager.ShowError(text, e);
        }
    }

    /// <summary>
    /// 启动多个实例
    /// </summary>
    /// <param name="model"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<GameLaunchListRes> Launch(BaseModel model, ICollection<GameSettingObj> obj)
    {
        if (SystemInfo.Os == OsType.Android)
        {
            return new();
        }
        var list = obj.Where(item => !GameManager.IsGameRun(item))
            .ToList();

        if (list.Count == 0)
        {
            return new()
            {
                Message = App.Lang("GameBinding.Error6")
            };
        }

        var res = await UserBinding.GetLaunchUser(model);
        if (res.User is not { } user)
        {
            return new() { Message = res.Message };
        }

        s_launchCancel = new();

        foreach (var item in list)
        {
            GameManager.ClearGameLog(item.UUID);
        }

        var port = LaunchSocketUtils.Port;

        //锁定账户
        UserBinding.AddLockUser(user);

        var state1 = LaunchState.End;
        var arg = MakeArg(user, model, port);

        //设置自动加入服务器
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfo(GuiConfigUtils.Config.ServerCustom.IP,
                GuiConfigUtils.Config.ServerCustom.Port);

            arg.Server = new()
            {
                IP = server.ServerAddress,
                Port = server.ServerPort
            };
        }

        var res1 = await Task.Run(async () =>
        {
            return await obj.StartGameAsync(arg, s_launchCancel.Token);
        });

        model.Title1 = "";
        FuntionUtils.RunGC();

        if (s_launchCancel.IsCancellationRequested)
        {
            UserBinding.UnLockUser(user);
            return new() { User = user };
        }

        if (GuiConfigUtils.Config.ServerCustom.RunPause)
        {
            Media.Pause();
        }

        var list1 = new Dictionary<string, LaunchState>();
        var list2 = new List<string>();
        foreach (var item in res1)
        {
            if (item.Value.Handel is { } pr)
            {
                item.Key.LaunchData.LastTime = DateTime.Now;
                item.Key.SaveLaunchData();

                WindowManager.MainWindow?.ShowMessage(App.Lang("Live2dControl.Text2"));

                GameManager.StartGame(item.Key);
                GameCount.LaunchDone(item.Key);
                GameStateUpdate(item.Key);

                if (pr is DesktopGameHandel handel)
                {
                    GameHandel(model, item.Key, handel);
                }

                list2.Add(item.Key.UUID);
            }
            else
            {
                var temp = App.Lang("BaseBinding.Error4");
                if (item.Value.Ex is LaunchException e1)
                {
                    state1 = e1.State;
                    if (!string.IsNullOrWhiteSpace(e1.Message))
                    {
                        temp = e1.Message;
                    }
                    else if (e1.Ex != null)
                    {
                        Logs.Error(temp, e1.Ex);
                        WindowManager.ShowError(temp, e1.Ex);
                    }

                    list1.Add(item.Key.UUID, state1);
                }
                else
                {
                    Logs.Error(temp, item.Value.Ex);
                    WindowManager.ShowError(temp, item.Value.Ex);

                    list1.Add(item.Key.UUID, LaunchState.End);
                }

                GameCount.LaunchError(item.Key);
            }
        }

        if (list2.Count == 0)
        {
            UserBinding.UnLockUser(user);
        }

        return new()
        {
            Done = list2,
            Fail = list1,
            User = user
        };
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="model"></param>
    /// <param name="obj"></param>
    /// <param name="world"></param>
    /// <param name="hide"></param>
    /// <returns></returns>
    public static async Task<GameLaunchOneRes> Launch(BaseModel model, GameSettingObj? obj,
        WorldObj? world = null, bool hide = false)
    {
        if (obj == null)
        {
            return new()
            {
                Message = App.Lang("GameBinding.Error6")
            };
        }

        if (GameManager.IsGameRun(obj))
        {
            return new()
            {
                Message = App.Lang("BaseBinding.Error3")
            };
        }

        var res = await UserBinding.GetLaunchUser(model);
        if (res.User is not { } user)
        {
            return new() { Message = res.Message };
        }

        if (SystemInfo.Os == OsType.Android)
        {
            hide = false;
        }

        s_launchCancel = new();

        GameManager.ClearGameLog(obj.UUID);

        var port = LaunchSocketUtils.Port;

        //锁定账户
        UserBinding.AddLockUser(user);

        var state1 = LaunchState.End;
        string? temp = null;
        var arg = MakeArg(user, model, port);
        arg.World = world;
        //设置自动加入服务器
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfo(GuiConfigUtils.Config.ServerCustom.IP,
                GuiConfigUtils.Config.ServerCustom.Port);

            arg.Server = new()
            {
                IP = server.ServerAddress,
                Port = server.ServerPort
            };
        }
        var res1 = await Task.Run(async () =>
        {
            try
            {
                return await obj.StartGameAsync(arg, s_launchCancel.Token);
            }
            catch (Exception e)
            {
                temp = App.Lang("BaseBinding.Error4");
                if (e is LaunchException e1)
                {
                    state1 = e1.State;
                    if (!string.IsNullOrWhiteSpace(e1.Message))
                    {
                        temp = e1.Message;
                    }
                    else if (e1.Ex != null)
                    {
                        Logs.Error(temp, e1.Ex);
                        WindowManager.ShowError(temp, e1.Ex);
                    }
                }
                else
                {
                    Logs.Error(temp, e);
                    WindowManager.ShowError(temp, e);
                }
            }
            return null;
        });

        model.ProgressClose();
        model.Title1 = "";
        FuntionUtils.RunGC();

        if (s_launchCancel.IsCancellationRequested)
        {
            UserBinding.UnLockUser(user);
            return new() { Res = true };
        }

        if (res1 is { } pr)
        {
            if (GuiConfigUtils.Config.ServerCustom.RunPause)
            {
                Media.Pause();
            }

            obj.LaunchData.LastTime = DateTime.Now;
            obj.SaveLaunchData();

            WindowManager.MainWindow?.ShowMessage(App.Lang("Live2dControl.Text2"));

            GameManager.StartGame(obj);
            GameCount.LaunchDone(obj);
            GameStateUpdate(obj);

            if (pr is DesktopGameHandel handel)
            {
                GameHandel(model, obj, handel);

                if (hide)
                {
                    Dispatcher.UIThread.Post(App.Hide);
                }
            }

            ConfigBinding.SetLastLaunch(obj.UUID);
        }
        else
        {
            GameCount.LaunchError(obj);
            UserBinding.UnLockUser(user);
        }

        return new()
        {
            Res = res1 != null,
            Message = temp,
            LoginFail = state1 == LaunchState.LoginFail,
            User = user
        };
    }

    private static GameLaunchArg MakeArg(LoginObj user, BaseModel model, int port)
    {
        return new GameLaunchArg
        {
            Auth = user,
            Request = (a) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowWait(a);
                });
            },
            Pre = (pre) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                    model.ShowWait(pre ? App.Lang("MainWindow.Info29") : App.Lang("MainWindow.Info30")));
            },
            State = (text) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (text == null)
                    {
                        model.ProgressClose();
                    }
                    else
                    {
                        model.Progress(text);
                    }
                });
            },
            Select = (text) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowTextWait(App.Lang("BaseBinding.Info2"), text ?? "");
                });
            },
            Nojava = (version) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    WindowManager.ShowSetting(SettingType.SetJava, version);
                });
            },
            Loginfail = (login) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowWait(string.Format(
                        App.Lang("MainWindow.Info21"), login.UserName));
                });
            },
            Update2 = (obj, state) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (GuiConfigUtils.Config.CloseBeforeLaunch)
                    {
                        if (state == LaunchState.End)
                        {
                            model.ProgressClose();
                        }
                        model.ProgressUpdate(App.Lang(state switch
                        {
                            LaunchState.Login => "MainWindow.Info8",
                            LaunchState.Check => "MainWindow.Info9",
                            LaunchState.CheckVersion => "MainWindow.Info10",
                            LaunchState.CheckLib => "MainWindow.Info11",
                            LaunchState.CheckAssets => "MainWindow.Info12",
                            LaunchState.CheckLoader => "MainWindow.Info13",
                            LaunchState.CheckLoginCore => "MainWindow.Info14",
                            LaunchState.CheckMods => "MainWindow.Info17",
                            LaunchState.Download => "MainWindow.Info15",
                            LaunchState.JvmPrepare => "MainWindow.Info16",
                            LaunchState.LaunchPre => "MainWindow.Info31",
                            LaunchState.LaunchPost => "MainWindow.Info32",
                            LaunchState.InstallForge => "MainWindow.Info38",
                            _ => ""
                        }));
                    }
                    else
                    {
                        model.Title1 = App.Lang(state switch
                        {
                            LaunchState.Login => "MainWindow.Info8",
                            LaunchState.Check => "MainWindow.Info9",
                            LaunchState.CheckVersion => "MainWindow.Info10",
                            LaunchState.CheckLib => "MainWindow.Info11",
                            LaunchState.CheckAssets => "MainWindow.Info12",
                            LaunchState.CheckLoader => "MainWindow.Info13",
                            LaunchState.CheckLoginCore => "MainWindow.Info14",
                            LaunchState.CheckMods => "MainWindow.Info17",
                            LaunchState.Download => "MainWindow.Info15",
                            LaunchState.JvmPrepare => "MainWindow.Info16",
                            LaunchState.LaunchPre => "MainWindow.Info31",
                            LaunchState.LaunchPost => "MainWindow.Info32",
                            LaunchState.InstallForge => "MainWindow.Info38",
                            _ => ""
                        });
                    }
                });
            },
            Mixinport = port
        };
    }

    /// <summary>
    /// 添加游戏分组
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool AddGameGroup(string name)
    {
        return InstancesPath.AddGroup(name);
    }

    /// <summary>
    /// 移动游戏到分组
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="now"></param>
    public static void MoveGameGroup(GameSettingObj obj, string? now)
    {
        obj.MoveGameGroup(now);
        WindowManager.MainWindow?.LoadMain();
    }

    /// <summary>
    /// 重新获取游戏版本
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> ReloadVersion()
    {
        await VersionPath.GetFromWebAsync();

        return await VersionPath.IsHaveVersionInfoAsync();
    }

    /// <summary>
    /// 设置游戏Jvm参数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public static void SetGameJvmMemArg(GameSettingObj obj, uint? min, uint? max)
    {
        obj.JvmArg ??= new();
        obj.JvmArg.MinMemory = min;
        obj.JvmArg.MaxMemory = max;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏Jvm参数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameJvmArg(GameSettingObj obj, RunArgObj obj1)
    {
        obj.JvmArg = obj1;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏窗口
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameWindow(GameSettingObj obj, WindowSettingObj obj1)
    {
        obj.Window = obj1;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏加入的服务器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameServer(GameSettingObj obj, ServerObj obj1)
    {
        obj.StartServer = obj1;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏端口代理
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameProxy(GameSettingObj obj, ProxyHostObj obj1)
    {
        obj.ProxyHost = obj1;
        obj.Save();
    }

    /// <summary>
    /// 获取游戏的模组列表
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sha256"></param>
    /// <returns></returns>
    public static async Task<List<ModDisplayModel>> GetGameMods(GameSettingObj obj,
        bool sha256 = false)
    {
        var list = new List<ModDisplayModel>();
        var list1 = await obj.GetModsAsync(sha256);
        if (list1 == null)
        {
            return list;
        }

        list1.ForEach(item =>
        {
            var obj1 = obj.Mods.Values.FirstOrDefault(a => a.SHA1 == item.Sha1);
            list.Add(new ModDisplayModel(item, obj1));
        });
        return list;
    }

    /// <summary>
    /// 启用/禁用模组
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static MessageRes ModEnableDisable(ModObj obj)
    {
        try
        {
            if (obj.Disable)
            {
                obj.Enable();
            }
            else
            {
                obj.Disable();
            }

            return new() { State = true };
        }
        catch (Exception e)
        {
            string temp = string.Format(App.Lang("GameBinding.Error14"), obj.Local);
            Logs.Error(temp, e);
            return new() { Message = temp };
        }
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="mod"></param>
    public static void DeleteMod(ModObj mod)
    {
        string name = new FileInfo(mod.Local).Name;
        foreach (var item in mod.Game.Mods)
        {
            if (item.Value.File == name)
            {
                mod.Game.Mods.Remove(item.Key);
                mod.Game.SaveModInfo();
                break;
            }
        }
        mod.Delete();
    }

    /// <summary>
    /// 添加模组
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task<bool> AddMods(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
            {
                list.Add(item1);
            }
        }
        return obj.AddModsAsync(list);
    }

    /// <summary>
    /// 获取所有配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetAllConfig(GameSettingObj obj)
    {
        var list = new List<string>();
        var dir = obj.GetGamePath().Length + 1;

        var file = obj.GetOptionsFile();
        if (!File.Exists(file))
        {
            File.Create(file).Dispose();
        }

        list.Add(obj.GetOptionsFile()[dir..]);
        string con = obj.GetConfigPath();

        var list1 = PathHelper.GetAllFile(con);
        foreach (var item in list1)
        {
            list.Add(item.FullName[dir..].Replace("\\", "/"));
        }

        return list;
    }

    /// <summary>
    /// 获取世界所有配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetAllConfig(WorldObj obj)
    {
        var list = new List<string>();
        var dir = obj.Local.Length + 1;

        var list1 = PathHelper.GetAllFile(obj.Local);
        foreach (var item in list1)
        {
            if (item.Extension is ".png" or ".lock")
                continue;
            list.Add(item.FullName[dir..].Replace("\\", "/"));
        }

        return list;
    }

    /// <summary>
    /// 获取根目录所有配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetAllTopConfig(GameSettingObj obj)
    {
        var list = new List<string>();
        var dir = obj.GetGamePath();

        var dir1 = new DirectoryInfo(dir);
        var list1 = dir1.GetFileSystemInfos();
        foreach (var item in list1)
        {
            string name = item.Name;
            if (item.Attributes == FileAttributes.Directory)
            {
                if (name == "mods" || name == "resourcepacks")
                    continue;
                name += "/";
            }

            list.Add(name);
        }

        return list;
    }

    /// <summary>
    /// 读取区块信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMca(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await ChunkMca.ReadAsync(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取区块信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMca(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await ChunkMca.ReadAsync(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbt(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbt(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ReadConfigFile(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ReadConfigFile(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="text"></param>
    public static void SaveConfigFile(WorldObj obj, string name, string? text)
    {
        var dir = obj.Local;

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="text"></param>
    public static void SaveConfigFile(GameSettingObj obj, string name, string? text)
    {
        var dir = obj.GetGamePath();

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    /// <summary>
    /// 保存Nbt文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="nbt"></param>
    public static void SaveNbtFile(WorldObj obj, string file, NbtBase nbt)
    {
        var dir = obj.Local;

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存Nbt文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="nbt"></param>
    public static void SaveNbtFile(GameSettingObj obj, string file, NbtBase nbt)
    {
        var dir = obj.GetGamePath();

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="data"></param>
    public static void SaveMcaFile(WorldObj obj, string file, ChunkDataObj data)
    {
        var dir = obj.Local;

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="data"></param>
    public static void SaveMcaFile(GameSettingObj obj, string file, ChunkDataObj data)
    {
        var dir = obj.GetGamePath();

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Task<List<WorldObj>> GetWorlds(GameSettingObj obj)
    {
        return obj.GetWorldsAsync();
    }

    /// <summary>
    /// 添加世界
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<bool> AddWorld(GameSettingObj obj, string? file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return false;
        }
        var res = await obj.AddWorldZipAsync(file);
        if (!res)
        {
            PathBinding.OpenFileWithExplorer(file);
        }

        return res;
    }

    /// <summary>
    /// 删除世界
    /// </summary>
    /// <param name="world"></param>
    public static void DeleteWorld(WorldObj world)
    {
        world.Remove();
    }

    /// <summary>
    /// 导出世界
    /// </summary>
    /// <param name="world"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task ExportWorld(WorldObj world, string? file)
    {
        if (file == null)
            return Task.CompletedTask;

        return world.ExportWorldZip(file);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sha256"></param>
    /// <returns></returns>
    public static Task<List<ResourcepackObj>> GetResourcepacks(GameSettingObj obj,
        bool sha256 = false)
    {
        return obj.GetResourcepacksAsync(sha256);
    }

    /// <summary>
    /// 删除资源包
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteResourcepack(ResourcepackObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 导入资源包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task<bool> AddResourcepack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
            {
                list.Add(item1);
            }
        }
        return obj.AddResourcepackAsync(list);
    }

    /// <summary>
    /// 删除截图
    /// </summary>
    /// <param name="file"></param>
    public static void DeleteScreenshot(string file)
    {
        Screenshots.Delete(file);
    }

    /// <summary>
    /// 删除所有截图
    /// </summary>
    /// <param name="obj"></param>
    public static void ClearScreenshots(GameSettingObj obj)
    {
        obj.ClearScreenshots();
    }

    /// <summary>
    /// 获取所有截图
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetScreenshots(GameSettingObj obj)
    {
        return obj.GetScreenshots();
    }

    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    public static GameSettingObj? GetGame(string? uuid)
    {
        return InstancesPath.GetGame(uuid);
    }

    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<ServerInfoObj>> GetServers(GameSettingObj obj)
    {
        return await obj.GetServerInfosAsync();
    }

    /// <summary>
    /// 添加服务器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static Task AddServer(GameSettingObj obj, string name, string ip)
    {
        return obj.AddServerAsync(name, ip);
    }

    /// <summary>
    /// 删除服务器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="server"></param>
    /// <returns></returns>
    public static Task DeleteServer(GameSettingObj obj, ServerInfoObj server)
    {
        return obj.RemoveServerAsync(server.Name, server.IP);
    }

    /// <summary>
    /// 删除配置文件
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteConfig(GameSettingObj obj)
    {
        obj.JvmArg = null;
        obj.JvmName = null;
        obj.JvmLocal = null;
        obj.Window = null;
        obj.StartServer = null;
        obj.ProxyHost = null;
        obj.AdvanceJvm = null;

        obj.Save();
    }

    /// <summary>
    /// 获取光影包列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Task<List<ShaderpackObj>> GetShaderpacks(GameSettingObj obj)
    {
        return obj.GetShaderpacksAsync();
    }

    /// <summary>
    /// 添加光影包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task<bool> AddShaderpack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
            {
                list.Add(item1);
            }
        }

        return obj.AddShaderpackAsync(list);
    }

    /// <summary>
    /// 删除光影包
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteShaderpack(ShaderpackObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 获取结构文件列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<List<SchematicObj>> GetSchematics(GameSettingObj obj)
    {
        var list = await obj.GetSchematicsAsync();
        var list1 = new List<SchematicObj>();
        foreach (var item in list)
        {
            if (item.Broken)
            {
                list1.Add(new()
                {
                    Name = App.Lang("GameBinding.Info17"),
                    Local = item.Local,
                });
            }
            else
            {
                list1.Add(item);
            }
        }

        return list1;
    }

    /// <summary>
    /// 添加结构文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool AddSchematic(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
            {
                list.Add(item1);
            }
        }

        return obj.AddSchematic(list);
    }

    /// <summary>
    /// 删除结构文件
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteSchematic(SchematicObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 设置模组信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    public static void SetModInfo(GameSettingObj obj, CurseForgeModObj.Data? data)
    {
        if (data == null)
            return;

        data.FixDownloadUrl();

        var obj1 = new ModInfoObj()
        {
            FileId = data.id.ToString(),
            ModId = data.modId.ToString(),
            File = data.fileName,
            Name = data.displayName,
            Url = data.downloadUrl,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                .Select(a => a.value).FirstOrDefault()
        };
        if (!obj.Mods.TryAdd(obj1.ModId, obj1))
        {
            obj.Mods[obj1.ModId] = obj1;
        }

        obj.SaveModInfo();
    }

    /// <summary>
    /// 设置模组信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    public static void SetModInfo(GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return;
        }

        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        var obj1 = new ModInfoObj()
        {
            FileId = data.id.ToString(),
            ModId = data.project_id,
            File = file.filename,
            Name = data.name,
            Url = file.url,
            SHA1 = file.hashes.sha1
        };
        if (!obj.Mods.TryAdd(obj1.ModId, obj1))
        {
            obj.Mods[obj1.ModId] = obj1;
        }

        obj.SaveModInfo();
    }

    /// <summary>
    /// 备份世界
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    public static async Task<bool> BackupWorld(WorldObj world)
    {
        try
        {
            await world.BackupAsync();
            return true;
        }
        catch (Exception e)
        {
            string text = App.Lang("GameBinding.Error7");
            Logs.Error(text, e);
            WindowManager.ShowError(text, e);
            return false;
        }
    }

    /// <summary>
    /// 还原世界
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="item1"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Task<bool> BackupWorld(GameSettingObj obj, FileInfo item1, ColorMCCore.Request request)
    {
        return obj.UnzipBackupWorldAsync(new UnzipBackupWorldArg { File = item1.FullName, Request = request });
    }

    /// <summary>
    /// 设置游戏名字
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    public static void SetGameName(GameSettingObj obj, string data)
    {
        obj.Name = data;
        obj.Save();

        WindowManager.MainWindow?.LoadMain();
    }

    /// <summary>
    /// 复制游戏实例
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<bool> CopyGame(GameSettingObj obj, string data,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        if (GameManager.IsGameRun(obj))
        {
            return false;
        }

        if (await obj.Copy(data, request, overwirte) == null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 保存服务器包
    /// </summary>
    /// <param name="obj1"></param>
    public static void SaveServerPack(ServerPackObj obj1)
    {
        obj1.Save();
    }

    /// <summary>
    /// 获取服务器包
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static ServerPackObj? GetServerPack(GameSettingObj obj)
    {
        return obj.GetServerPack().Pack;
    }

    /// <summary>
    /// 生成服务器包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="local"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Task<bool> GenServerPack(ServerPackObj obj, string local,
        ColorMCCore.Request request)
    {
        return obj.GenServerPackAsync(new ServerPackGenArg
        {
            Local = local,
            Request = request
        });
    }

    /// <summary>
    /// 复制服务器地址到剪贴板
    /// </summary>
    /// <param name="obj"></param>
    public static async void CopyServer(TopLevel top, ServerInfoObj obj)
    {
        await BaseBinding.CopyTextClipboard(top, $"{obj.Name}\n{obj.IP}");
    }

    /// <summary>
    /// 模组检测
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static Task<bool> ModCheck(List<ModDisplayModel> list)
    {
        return Task.Run(() =>
        {
            var modid = new List<string>();
            var mod = new List<ModDisplayModel>();
            foreach (var item in list)
            {
                if (item.Obj.ModId == null || item.Obj.Disable || item.Obj.CoreMod)
                    continue;
                modid.Add(item.Obj.ModId);
                if (item.Obj.InJar?.Count > 0)
                {
                    foreach (var item1 in item.Obj.InJar)
                    {
                        modid.Add(item1.ModId);
                    }
                }
                mod.Add(item);
            }
            modid.Add("forge");
            modid.Add("Forge");
            modid.Add("mod_MinecraftForge");

            modid.Add("fabric");
            modid.Add("minecraft");
            modid.Add("java");
            modid.Add("fabricloader");

            modid.Add("neoforge");

            var lost = new ConcurrentBag<(string, List<string>)>();

            Parallel.ForEach(mod, item =>
            {
                if (item == null)
                {
                    return;
                }

                var list1 = new List<string>();
                if (item.Obj.Dependants != null)
                {
                    foreach (var item1 in item.Obj.Dependants)
                    {
                        var item2 = item1;
                        if (item2.Contains("@["))
                        {
                            item2 = StringHelper.RemovePartAfterSymbol(item1, '@');
                        }
                        if (item2.StartsWith("required-after:"))
                        {
                            item2 = StringHelper.ReplaceFirst(item2, "required-after:", "");
                        }

                        if (!modid.Contains(item2))
                        {
                            list1.Add(item2);
                        }
                    }
                }
                if (item.Obj.Dependants != null)
                {
                    foreach (var item1 in item.Obj.Dependants)
                    {
                        var list2 = item1.Split(",");
                        foreach (var item2 in list2)
                        {
                            if (item2 == ")")
                            {
                                continue;
                            }
                            var item3 = item2;
                            if (item2.Contains("@["))
                            {
                                item3 = StringHelper.RemovePartAfterSymbol(item2, '@');
                            }
                            if (item3.StartsWith("required-after:"))
                            {
                                item3 = StringHelper.ReplaceFirst(item3, "required-after:", "");
                            }

                            if (!modid.Contains(item3))
                            {
                                list1.Add(item3);
                            }
                        }
                    }
                }

                if (list1.Count > 0)
                {
                    lost.Add((item.Name, list1));
                }
            });

            if (!lost.IsEmpty)
            {
                static string GetString(List<string> list)
                {
                    var str = new StringBuilder();
                    foreach (var item in list)
                    {
                        str.Append(item).Append(',');
                    }

                    return str.ToString()[..^1];
                }

                var str = new StringBuilder();
                foreach (var item in lost)
                {
                    str.Append(string.Format(App.Lang("GameBinding.Info1"), item.Item1,
                        GetString(item.Item2))).Append(Environment.NewLine);
                }

                WindowManager.ShowError(App.Lang("GameBinding.Info2"), str.ToString());
                return false;
            }

            return true;
        });
    }

    /// <summary>
    /// 获取日志文件列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetLogList(GameSettingObj obj)
    {
        return obj.GetLogFiles();
    }

    /// <summary>
    /// 读取日志文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<string?> ReadLog(GameSettingObj obj, string name)
    {
        if (GameManager.IsGameRun(obj))
        {
            if (name.EndsWith("latest.log") || name.EndsWith("debug.log"))
                return null;
        }

        return await Task.Run(() => obj.ReadLog(name));
    }

    /// <summary>
    /// 升级整合包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fid"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static Task<bool> ModPackUpdate(GameSettingObj obj, CurseForgeModObj.Data fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return ModPackHelper.UpdateModPack(new UpdateCurseForgeModPackArg
        {
            Data = fid,
            Game = obj,
            Update = update,
            Update2 = update2
        });
    }

    /// <summary>
    /// 升级整合包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fid"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static Task<bool> ModPackUpdate(GameSettingObj obj, ModrinthVersionObj fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return ModPackHelper.UpdateModPack(new UpdateModrinthModPackArg
        {
            Game = obj,
            Data = fid,
            Update = update,
            Update2 = update2
        });
    }

    /// <summary>
    /// 启用/禁用模组
    /// </summary>
    /// <param name="item"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<ModDisplayModel> ModDisable(ModDisplayModel item, List<ModDisplayModel> items)
    {
        var list = new List<ModDisplayModel>();
        foreach (var item1 in items)
        {
            if (!item1.Enable || item1.Obj.ModId == item.Obj.ModId)
            {
                continue;
            }
            if (item1.Obj.Dependants?.Contains(item.Obj.ModId) == true)
            {
                list.Add(item1);
                continue;
            }
            if (item.Obj.InJar != null)
            {
                foreach (var item2 in item.Obj.InJar)
                {
                    if (item1.Obj.Dependants?.Contains(item2.ModId) == true)
                    {
                        list.Add(item1);
                        break;
                    }
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 游戏实例状态发生改变
    /// </summary>
    /// <param name="obj"></param>
    public static void GameStateUpdate(GameSettingObj obj)
    {
        if (WindowManager.GameLogWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Update();
        }
    }

    /// <summary>
    /// 拖拽添加文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task<bool> AddFile(GameSettingObj obj, IDataObject data, FileType type)
    {
        if (!data.Contains(DataFormats.Files))
        {
            return false;
        }
        var list = data.GetFiles();
        if (list == null)
        {
            return false;
        }
        switch (type)
        {
            case FileType.Mod:
                var list1 = new List<string>();
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }
                    if (File.Exists(file) && file.ToLower().EndsWith(".jar"))
                    {
                        list1.Add(file);
                    }
                }

                return await obj.AddModsAsync(list1);
            case FileType.World:
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }
                    if (File.Exists(file) && file.ToLower().EndsWith(".zip"))
                    {
                        return await obj.AddWorldZipAsync(file);
                    }
                }
                return false;
            case FileType.Resourcepack:
                list1 = [];
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }
                    if (File.Exists(file) && file.ToLower().EndsWith(".zip"))
                    {
                        list1.Add(file);
                    }
                }
                return await obj.AddResourcepackAsync(list1);
            case FileType.Shaderpack:
                list1 = [];
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    if (File.Exists(file) && file.ToLower().EndsWith(".zip"))
                    {
                        list1.Add(file);
                    }
                }
                return await obj.AddShaderpackAsync(list1);
            case FileType.Schematic:
                list1 = [];
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    var file1 = file.ToLower();
                    if (File.Exists(file) &&
                        (file1.EndsWith(Schematic.Name1) || file1.EndsWith(Schematic.Name2)))
                    {
                        list1.Add(file);
                    }
                }
                return obj.AddSchematic(list1);
        }

        return false;
    }

    /// <summary>
    /// 检测压缩包类型
    /// </summary>
    /// <param name="local"></param>
    /// <returns></returns>
    public static PackType? CheckType(string local)
    {
        Stream? stream = null;
        try
        {
            stream = PathHelper.OpenRead(local);

            if (stream == null)
            {
                return null;
            }

            if (local.EndsWith(".mrpack"))
            {
                return PackType.Modrinth;
            }
            if (local.EndsWith(".zip"))
            {
                using var zFile = new ZipFile(stream);
                foreach (ZipEntry item in zFile)
                {
                    if (item.Name.EndsWith("game.json"))
                    {
                        return PackType.ColorMC;
                    }
                    else if (item.Name.EndsWith("mcbbs.packmeta"))
                    {
                        return PackType.HMCL;
                    }
                    else if (item.Name.EndsWith("instance.cfg"))
                    {
                        return PackType.MMC;
                    }
                    else if (item.Name.EndsWith("manifest.json"))
                    {
                        return PackType.CurseForge;
                    }
                    else if (item.Name.Contains(".minecraft/"))
                    {
                        return PackType.ZipPack;
                    }
                }
            }
        }
        finally
        {
            stream?.Close();
            stream?.Dispose();
        }

        return null;
    }

    /// <summary>
    /// 解压云同步配置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <param name="local"></param>
    /// <returns></returns>
    public static async Task<bool> UnZipCloudConfig(GameSettingObj obj, CloudDataObj data, string local)
    {
        data.Config.Clear();
        return await Task.Run(() =>
        {
            try
            {
                using var s = new ZipInputStream(PathHelper.OpenRead(local));
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string filename = $"{obj.GetBasePath()}/{theEntry.Name}";
                    data.Config.Add(theEntry.Name);
                    var directoryName = Path.GetDirectoryName(filename);
                    string fileName = Path.GetFileName(theEntry.Name);

                    if (directoryName?.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != string.Empty)
                    {
                        using var streamWriter = PathHelper.OpenWrite(filename, true);
                        s.CopyTo(streamWriter);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("GameBinding.Error8"), e);
            }
            return false;
        });
    }

    /// <summary>
    /// 下载云同步实例
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="group"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<MessageRes> DownloadCloud(CloundListObj obj, string? group,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        var game = await InstancesPath.CreateGame(new CreateGameArg
        {
            Game = new()
            {
                Name = obj.Name,
                UUID = obj.UUID,
                GroupName = group
            },
            Request = request,
            Overwirte = overwirte
        });
        if (game == null)
        {
            return new() { Message = App.Lang("GameBinding.Error9") };
        }

        var cloud = new CloudDataObj()
        {
            Config = []
        };

        GameCloudUtils.SetCloudData(game, cloud);
        string local = Path.GetFullPath(game.GetBasePath() + "/config.zip");
        var res = await GameCloudUtils.DownloadConfig(obj, local);
        if (res == 100)
        {
            await UnZipCloudConfig(game, cloud, local);
        }

        var temp = await GameCloudUtils.HaveCloud(game);
        try
        {
            cloud.ConfigTime = DateTime.Parse(temp.Item3!);
        }
        catch
        {

        }
        GameCloudUtils.SetCloudData(game, cloud);

        game = game.Reload();

        if (game.Mods != null)
        {
            var list = new List<DownloadItemObj>();
            foreach (var item in game.Mods.Values)
            {
                list.Add(new()
                {
                    Url = item.Url,
                    Name = item.File,
                    Local = game.GetGamePath() + "/" + item.Path + "/" + item.File,
                    SHA1 = item.SHA1
                });
            }

            if (list.Count > 0)
            {
                var res1 = await DownloadManager.StartAsync(list);
                if (!res1)
                {
                    return new() { Message = App.Lang("GameBinding.Error10") };
                }
            }
        }

        return new() { State = true, Message = game.UUID };
    }

    /// <summary>
    /// 根据名字获取游戏实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameSettingObj? GetGameByName(string name)
    {
        return InstancesPath.GetGameByName(name);
    }

    /// <summary>
    /// 检测是否有云同步并打开窗口
    /// </summary>
    /// <param name="obj"></param>
    public static async void CheckCloudAndOpen(GameSettingObj obj)
    {
        var res = await GameCloudUtils.HaveCloud(obj);
        if (res.Item1 == 100 && res.Item2)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WindowManager.ShowGameCloud(obj, true);
            });
        }
    }

    /// <summary>
    /// 删除游戏实例
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteGame(GameSettingObj obj,
        ColorMCCore.Request request)
    {
        var res = await obj.Remove(request);
        if (res)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WindowManager.CloseGameWindow(obj);
            });
        }

        return res;
    }

    /// <summary>
    /// 下载服务器包
    /// </summary>
    /// <param name="model"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="text"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<MessageRes> DownloadServerPack(BaseModel model,
        string? name, string? group, string text, ColorMCCore.GameOverwirte overwirte)
    {
        try
        {
            var data = await CoreHttpClient.GetStringAsync(text + "server.json");
            if (!data.State)
            {
                return new() { Message = App.Lang("GameBinding.Error11") };
            }
            var obj = JsonConvert.DeserializeObject<ServerPackObj>(data.Message!);
            if (obj == null)
            {
                return new() { Message = App.Lang("GameBinding.Error12") };
            }

            var game = obj.Game;
            if (!string.IsNullOrWhiteSpace(name))
            {
                game.Name = name;
            }
            if (!string.IsNullOrWhiteSpace(group))
            {
                game.GroupName = group;
            }
            game.UUID = null!;
            game.LaunchData = null!;
            game.ServerUrl = text;
            game.ModPackType = SourceType.ColorMC;
            game = await InstancesPath.CreateGame(new CreateGameArg
            {
                Game = game,
                Request = model.ShowWait,
                Overwirte = overwirte
            });

            if (game == null)
            {
                return new() { Message = App.Lang("GameBinding.Error9") };
            }

            model.Progress(App.Lang("GameBinding.Info16"));

            var res1 = await obj.UpdateAsync((text) =>
            {
                if (text == null)
                {
                    model.ProgressClose();
                }
                else
                {
                    model.Progress(text);
                }
            });
            if (!res1)
            {
                model.ProgressClose();
                model.ShowOk(App.Lang("GameBinding.Error10"), async () =>
                {
                    await game.Remove(model.ShowWait);
                });

                return new();
            }

            PathHelper.WriteText(game.GetServerPackFile(), data.Message!);

            return new() { State = true, Message = game.UUID };
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameBinding.Error12");
            Logs.Error(temp, e);
            return new() { Message = temp };
        }
    }

    /// <summary>
    /// 启用/禁用数据包
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool DataPackDisableOrEnable(DataPackObj obj)
    {
        if (GameManager.IsGameRun(obj.World.Game))
        {
            return false;
        }
        return obj.World.DisableOrEnableDataPack([obj]);
    }

    /// <summary>
    /// 启用/禁用数据包
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool DataPackDisE(IEnumerable<DataPackModel> pack)
    {
        var list = new List<DataPackObj>();
        foreach (var item in pack)
        {
            list.Add(item.Pack);
        }
        if (GameManager.IsGameRun(list[0].World.Game))
        {
            return false;
        }
        return list[0].World.DisableOrEnableDataPack(list);
    }

    /// <summary>
    /// 删除数据包
    /// </summary>
    /// <param name="item"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteDataPack(DataPackModel item, ColorMCCore.Request request)
    {
        if (GameManager.IsGameRun(item.Pack.World.Game))
        {
            return false;
        }
        return await item.Pack.World.DeleteDataPackAsync(new DataPackDeleteArg
        {
            List = [item.Pack],
            Request = request
        });
    }

    /// <summary>
    /// 删除数据包
    /// </summary>
    /// <param name="items"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteDataPack(IEnumerable<DataPackModel> items, ColorMCCore.Request request)
    {
        var list = new List<DataPackObj>();
        foreach (var item in items)
        {
            list.Add(item.Pack);
        }
        if (GameManager.IsGameRun(list[0].World.Game))
        {
            return false;
        }
        return await list[0].World.DeleteDataPackAsync(new DataPackDeleteArg
        {
            List = list,
            Request = request
        });
    }

    /// <summary>
    /// 查询支持的模组加载器
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static async Task<List<Loaders>> GetSupportLoader(string version)
    {
        var loaders = new List<Loaders>();
        Task[] list =
        [
            Task.Run(async () =>
            {
                var list = await WebBinding.GetForgeSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.Forge);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetFabricSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.Fabric);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetQuiltSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.Quilt);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetNeoForgeSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.NeoForge);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetOptifineSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.OptiFine);
                }
            })
        ];

        await Task.WhenAll(list);

        loaders.Sort();

        return loaders;
    }

    /// <summary>
    /// 生成实例信息
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task GenGameInfo(GameSettingObj obj)
    {
        var list = await obj.GetModsAsync(false);
        var info = new StringBuilder();
        await Task.Run(() =>
        {
            info.AppendLine($"ColorMC:{ColorMCCore.Version}")
                .AppendLine($"{App.Lang("GameBinding.Info5")}{obj.Name}")
                .AppendLine($"{App.Lang("GameBinding.Info6")}{obj.Version}");
            if (obj.ModPack)
            {
                info.AppendLine(string.Format(App.Lang("GameBinding.Info12"),
                    obj.ModPackType.GetName(), obj.PID, obj.FID));
            }
            if (obj.Loader != Loaders.Normal)
            {
                if (obj.Loader == Loaders.Custom)
                {
                    info.AppendLine(string.Format(App.Lang("GameBinding.Info8"),
                        GetGameLoader(obj), obj.CustomLoader?.OffLib));
                }
                else
                {
                    info.AppendLine(string.Format(App.Lang("GameBinding.Info7"),
                        obj.Loader.GetName(), obj.LoaderVersion));
                }

                if (list.Count != 0)
                {
                    info.AppendLine(App.Lang("GameBinding.Info9"));

                    foreach (var item in list)
                    {
                        info.AppendLine(string.Format(App.Lang("GameBinding.Info10"),
                            item.Name, item.ModId, StringHelper.MakeString(item.Author),
                            Path.GetFileName(item.Local), item.Disable, item.CoreMod,
                            item.Sha1, StringHelper.MakeString(item.Loaders)));
                        if (obj.Mods.Values.FirstOrDefault(item => item.SHA1 == item.SHA1) is { } item1)
                        {
                            info.AppendLine(string.Format(App.Lang("GameBinding.Info11"),
                                item1.Type.GetName(), item1.ModId, item1.FileId));
                        }
                    }
                }
            }
        });
        WindowManager.ShowError(App.Lang("GameBinding.Info13"), info.ToString());
    }

    /// <summary>
    /// 获取自定义加载器数据
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<string> GetGameLoader(GameSettingObj obj)
    {
        var res = await obj.GetGameLoaderInfo();
        if (res != null)
        {
            return res;
        }
        else
        {
            return App.Lang("GameBinding.Error13");
        }
    }

    /// <summary>
    /// 设置自定义加载器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Task<MessageRes> SetGameLoader(GameSettingObj obj, string path)
    {
        return obj.SetGameLoader(path);
    }

    /// <summary>
    /// 获取McMod分类
    /// </summary>
    /// <returns></returns>
    public static Task<McModTypsObj?> GetMcModCategories()
    {
        return ColorMCAPI.GetMcModGroup();
    }

    /// <summary>
    /// 自动标记模组
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="cov"></param>
    /// <returns></returns>
    public static Task<IntRes> AutoMarkMods(GameSettingObj obj, bool cov)
    {
        return ModrinthHelper.AutoMark(obj, cov);
    }

    /// <summary>
    /// 消息解析
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static Chat StringToChat(string description)
    {
        return ServerDescriptionJsonConverter.StringToChar(description);
    }

    /// <summary>
    /// 取消启动
    /// </summary>
    public static void CancelLaunch()
    {
        s_launchCancel.Cancel();
    }

    /// <summary>
    /// 游戏进程启动后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="obj"></param>
    /// <param name="handel"></param>
    private static void GameHandel(BaseModel model, GameSettingObj obj, DesktopGameHandel handel)
    {
        var pr = handel.Process;
        Task.Run(async () =>
        {
            try
            {
                var conf = obj.Window;
                if (SystemInfo.Os == OsType.Windows)
                {
                    Win32Native.Win32.WaitWindowDisplay(pr);
                }
                else if (SystemInfo.Os == OsType.Linux)
                {
                    X11Hook.WaitWindowDisplay(pr);
                }
                else if (SystemInfo.Os == OsType.MacOS)
                {
                    return;
                }

                if (pr.HasExited)
                {
                    return;
                }

                //启用手柄支持
                if (SystemInfo.Os == OsType.Windows && GuiConfigUtils.Config.Input.Enable)
                {
                    var run = true;
                    var uuid = GuiConfigUtils.Config.Input.NowConfig;

                    if (string.IsNullOrWhiteSpace(uuid) || !JoystickConfig.Configs.ContainsKey(uuid))
                    {
                        run = await model.ShowWait(App.Lang("BaseBinding.Error7"));
                    }
                    if (run)
                    {
                        GameJoystick.Start(obj, handel);
                    }
                }

                if (SystemInfo.Os == OsType.MacOS)
                {
                    return;
                }

                //修改窗口标题
                if (!string.IsNullOrWhiteSpace(conf?.GameTitle))
                {
                    var ran = new Random();
                    int i = 0;
                    var list = new List<string>();
                    var list1 = conf.GameTitle.Split('\n');

                    foreach (var item in list1)
                    {
                        var temp = item.Trim();
                        if (string.IsNullOrWhiteSpace(temp))
                        {
                            continue;
                        }

                        list.Add(temp);
                    }
                    if (list.Count == 0)
                    {
                        return;
                    }

                    Thread.Sleep(1000);

                    do
                    {
                        string title1 = "";
                        if (conf.RandomTitle)
                        {
                            title1 = list[ran.Next(list.Count)];
                        }
                        else
                        {
                            i++;
                            if (i >= list.Count)
                            {
                                i = 0;
                            }
                            title1 = list[i];
                        }

                        if (SystemInfo.Os == OsType.Windows)
                        {
                            Win32Native.Win32.SetTitle(pr, title1);
                        }
                        else if (SystemInfo.Os == OsType.Linux)
                        {
                            X11Hook.SetTitle(pr, title1);
                        }

                        if (!conf.CycTitle || conf.TitleDelay <= 0 || pr.HasExited)
                        {
                            break;
                        }

                        Thread.Sleep(conf.TitleDelay);
                    }
                    while (!ColorMCGui.IsClose && !handel.IsExit);
                }
            }
            catch
            {

            }
        });
    }
}