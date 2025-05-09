using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Chunk;
using ColorMC.Core.Config;
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
using ColorMC.Core.Objs.ColorMC;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
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
    /// 游戏是否链接到ColorMC
    /// </summary>
    private readonly static Dictionary<string, bool> s_gameConnect = [];

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
    /// <param name="game">游戏实例</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
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
    /// <param name="name">实例名字</param>
    /// <param name="local">文件夹路径</param>
    /// <param name="unselect">不导入的文件列表</param>
    /// <param name="group">游戏分组</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
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
    /// <param name="dir">压缩包路径</param>
    /// <param name="type">压缩包类型</param>
    /// <param name="name">实例名字</param>
    /// <param name="group">游戏分组</param>
    /// <param name="zip">UI相关</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
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
        return CurseForgeHelper.GetGameVersionsAsync();
    }

    /// <summary>
    /// 获取MO支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetModrinthGameVersions()
    {
        return ModrinthHelper.GetGameVersionAsync();
    }

    /// <summary>
    /// 获取CF分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Task<Dictionary<string, string>?> GetCurseForgeCategories(
        FileType type = FileType.ModPack)
    {
        return CurseForgeHelper.GetCategoriesAsync(type);
    }

    /// <summary>
    /// 获取MO分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Task<Dictionary<string, string>?> GetModrinthCategories(FileType type = FileType.ModPack)
    {
        return ModrinthHelper.GetModrinthCategoriesAsync(type);
    }

    /// <summary>
    /// 安装CF整合包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <param name="data1">整合包信息</param>
    /// <param name="group">游戏分组</param>
    /// <param name="zip">UI相关</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
    /// <returns></returns>
    public static async Task<GameRes> InstallCurseForge(CurseForgeModObj.CurseForgeDataObj data,
        CurseForgeListObj.CurseForgeListDataObj data1, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return await AddGameHelper.InstallCurseForge(new DownloadCurseForgeArg
        {
            Data = data,
            Data1 = data1,
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
    /// <param name="data">整合包信息</param>
    /// <param name="data1">整合包信息</param>
    /// <param name="group">游戏分组</param>
    /// <param name="zip">UI相关</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
    /// <returns></returns>
    public static async Task<GameRes> InstallModrinth(ModrinthVersionObj data,
        ModrinthSearchObj.HitObj data1, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return await AddGameHelper.InstallModrinth(new DownloadModrinthArg
        {
            Data = data,
            Data1 = data1,
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
    /// <param name="top">窗口</param>
    /// <param name="model">基础窗口模型</param>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task SetGameIconFromFile(TopLevel top, BaseModel model, GameSettingObj obj)
    {
        try
        {
            var file = await PathBinding.SelectFile(top, FileType.GameIcon);
            if (file.Item1 != null)
            {
                bool resize = await model.ShowAsync(App.Lang("GameBinding.Info14"));

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
    /// <param name="model">基础窗口模型</param>
    /// <param name="objs">游戏实例列表</param>
    /// <returns></returns>
    public static async Task<GameLaunchListRes> Launch(BaseModel model, ICollection<GameSettingObj> objs)
    {
        var list = objs.Where(item => !GameManager.IsGameRun(item))
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

        var port = LaunchSocketUtils.Port;

        //锁定账户
        UserBinding.AddLockUser(user);
        foreach (var item in list)
        {
            s_gameConnect[item.UUID] = false;
            GameManager.ClearGameLog(item);
            GameManager.StartGame(item);
        }

        var state1 = LaunchState.End;
        var arg = MakeArg(user, model, port);
        arg.Admin = GuiConfigUtils.Config.ServerCustom.GameAdminLaunch;

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
            return await objs.StartGameAsync(arg, s_launchCancel.Token);
        });

        model.SubTitle = "";
        FuntionUtils.RunGC();

        if (s_launchCancel.IsCancellationRequested)
        {
            UserBinding.UnLockUser(user);
            foreach (var item in list)
            {
                GameManager.GameExit(item);
            }
            return new() { User = user };
        }

        if (GuiConfigUtils.Config.ServerCustom.RunPause)
        {
            Media.PlayState = PlayState.Pause;
        }

        var list1 = new Dictionary<string, LaunchState>();
        var list2 = new List<string>();
        //逐一启动
        foreach (var item in res1)
        {
            if (item.Value.Handel is { } pr)
            {
                item.Key.LaunchData.LastTime = DateTime.Now;
                item.Key.SaveLaunchData();

                WindowManager.MainWindow?.ShowMessage(App.Lang("Live2dControl.Text2"));

                GameCountUtils.LaunchDone(item.Key);
                GameStateUpdate(item.Key);

                if (pr is DesktopGameHandel handel)
                {
                    GameHandel(item.Key, handel);
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

                GameManager.GameExit(item.Key);
                GameCountUtils.LaunchError(item.Key.UUID);
            }
        }

        //游戏实例只要有一个启动成功了就不解锁账户
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
    /// <param name="model">基础窗口模型</param>
    /// <param name="obj">游戏实例</param>
    /// <param name="world">进入的存档</param>
    /// <param name="hide">是否隐藏启动器</param>
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

        var count = obj.ReadModCountFast();

        if (count > 0)
        {
            //检测加载器有没有启用
            if (GuiConfigUtils.Config.LaunchCheck.CheckLoader && obj.Loader == Loaders.Normal)
            {
                var res2 = await model.ShowAsync(string.Format(App.Lang("GameBinding.Info19"), obj.Name));
                if (!res2)
                {
                    return new() { };
                }

                res2 = await model.ShowAsync(App.Lang("GameBinding.Info23"));
                if (res2 == true)
                {
                    GuiConfigUtils.Config.LaunchCheck.CheckLoader = false;
                    GuiConfigUtils.Save();
                }
            }

            //检测内存分配是否够
            if (GuiConfigUtils.Config.LaunchCheck.CheckMemory && obj.Loader != Loaders.Normal)
            {
                var mem = obj.JvmArg?.MaxMemory ?? ConfigUtils.Config.DefaultJvmArg.MaxMemory;
                if ((mem <= 4096 && count > 150)
                    || (mem <= 8192 && count > 300))
                {
                    bool launch = false;
                    await model.ShowChoiseCancelWait(App.Lang("GameBinding.Info20"), App.Lang("GameBinding.Info21"), () =>
                    {
                        model.ShowClose();
                        if (obj.JvmArg?.MaxMemory != null)
                        {
                            WindowManager.ShowGameEdit(obj, GameEditWindowType.Arg);
                        }
                        else
                        {
                            WindowManager.ShowSetting(SettingType.Arg);
                        }
                    }, (data) =>
                    {
                        launch = data;
                    });
                    if (!launch)
                    {
                        return new();
                    }

                    var res3 = await model.ShowAsync(App.Lang("GameBinding.Info22"));
                    if (res3 == true)
                    {
                        GuiConfigUtils.Config.LaunchCheck.CheckMemory = false;
                        GuiConfigUtils.Save();
                    }
                }
            }
        }

        //获取选中的账户
        var res = await UserBinding.GetLaunchUser(model);
        if (res.User is not { } user)
        {
            return new() { Message = res.Message };
        }
#if Phone
        hide = false;
#endif
        s_launchCancel = new();

        s_gameConnect[obj.UUID] = false;
        GameManager.ClearGameLog(obj);
        GameManager.StartGame(obj);

        var port = LaunchSocketUtils.Port;

        //锁定账户
        UserBinding.AddLockUser(user);

        var state1 = LaunchState.End;
        string? temp = null;
        var arg = MakeArg(user, model, port);
        arg.World = world;
        arg.Admin = GuiConfigUtils.Config.ServerCustom.GameAdminLaunch;
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

        arg.Update2?.Invoke(obj, LaunchState.End);

        model.ProgressClose();
        model.SubTitle = "";
        FuntionUtils.RunGC();

        //是否取消启动
        if (s_launchCancel.IsCancellationRequested)
        {
            UserBinding.UnLockUser(user);
            GameManager.GameExit(obj);
            return new() { Res = true };
        }

        //是否成功启动
        if (res1 is { } pr)
        {
            if (GuiConfigUtils.Config.ServerCustom.RunPause)
            {
                Media.PlayState = PlayState.Pause;
            }

            obj.LaunchData.LastTime = DateTime.Now;
            obj.SaveLaunchData();

            WindowManager.MainWindow?.ShowMessage(App.Lang("Live2dControl.Text2"));

            GameCountUtils.LaunchDone(obj);
            GameStateUpdate(obj);

            if (pr is DesktopGameHandel handel)
            {
                GameHandel(obj, handel);

                if (hide)
                {
                    Dispatcher.UIThread.Post(App.Hide);
                }
            }

            ConfigBinding.SetLastLaunch(obj.UUID);
            if (obj.LogAutoShow)
            {
                WindowManager.ShowGameLog(obj);
            }
        }
        else
        {
            GameCountUtils.LaunchError(obj.UUID);
            GameManager.GameExit(obj);
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

    /// <summary>
    /// 创建启动参数
    /// </summary>
    /// <param name="user">登录的账户</param>
    /// <param name="model">基础窗口模型</param>
    /// <param name="port">ColorMC ASM端口</param>
    /// <returns></returns>
    private static GameLaunchArg MakeArg(LoginObj user, BaseModel model, int port)
    {
        return new GameLaunchArg
        {
            Auth = user,
            Request = (a) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return model.ShowAsync(a);
                });
            },
            Pre = (pre) =>
            {
                return Dispatcher.UIThread.InvokeAsync(() =>
                    model.ShowAsync(pre ? App.Lang("MainWindow.Info29") : App.Lang("MainWindow.Info30")));
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
                    return model.TextAsync(App.Lang("BaseBinding.Info2"), text ?? "");
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
                    return model.ShowAsync(string.Format(
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
                        model.SubTitle = App.Lang(state switch
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
        WindowManager.MainWindow?.LoadGameItem();
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
    public static async Task<List<ModDisplayModel>> GetGameMods(GameSettingObj obj, IModEdit? edit,
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
            var obj1 = obj.Mods.Values.FirstOrDefault(a => a.Sha1 == item.Sha1);
            list.Add(new ModDisplayModel(item, obj1, edit));
        });
        return list;
    }

    /// <summary>
    /// 快速获取游戏实例模组列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static Task<List<ModObj>> GetModFastAsync(GameSettingObj obj)
    {
        return obj.GetModFastAsync();
    }

    /// <summary>
    /// 启用/禁用模组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static MessageRes ModEnableDisable(ModObj obj)
    {
        try
        {
            if (!File.Exists(obj.Local))
            {
                return new() { Message = App.Lang("GameBinding.Error15") };
            }

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
    /// <param name="mod">模组</param>
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
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
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
    /// <param name="obj">游戏实例</param>
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

        var list1 = PathHelper.GetAllFiles(con);
        foreach (var item in list1)
        {
            list.Add(item.FullName[dir..].Replace("\\", "/"));
        }

        return list;
    }

    /// <summary>
    /// 获取存档所有配置文件
    /// </summary>
    /// <param name="obj">存档</param>
    /// <returns></returns>
    public static List<string> GetAllConfig(WorldObj obj)
    {
        var list = new List<string>();
        var dir = obj.Local.Length + 1;

        var list1 = PathHelper.GetAllFiles(obj.Local);
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
    /// <param name="obj">游戏实例</param>
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
    /// <param name="obj">存档</param>
    /// <param name="name">区块文件名</param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMca(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await ChunkMca.ReadAsync(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取区块信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">区块文件名</param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMca(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await ChunkMca.ReadAsync(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj">存档</param>
    /// <param name="name">文件名</param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbt(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">文件名</param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbt(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="obj">存档</param>
    /// <param name="name">文件名</param>
    /// <returns></returns>
    public static string ReadConfigFile(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">文件名</param>
    /// <returns></returns>
    public static string ReadConfigFile(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="obj">存档</param>
    /// <param name="name">文件名</param>
    /// <param name="text">文件内容</param>
    public static void SaveConfigFile(WorldObj obj, string name, string? text)
    {
        var dir = obj.Local;

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">文件名</param>
    /// <param name="text">文件内容</param>
    public static void SaveConfigFile(GameSettingObj obj, string name, string? text)
    {
        var dir = obj.GetGamePath();

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    /// <summary>
    /// 保存Nbt文件
    /// </summary>
    /// <param name="obj">存档</param>
    /// <param name="file">文件名</param>
    /// <param name="nbt">文件内容</param>
    public static void SaveNbtFile(WorldObj obj, string file, NbtBase nbt)
    {
        var dir = obj.Local;

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存Nbt文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件名</param>
    /// <param name="nbt">文件内容</param>
    public static void SaveNbtFile(GameSettingObj obj, string file, NbtBase nbt)
    {
        var dir = obj.GetGamePath();

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="obj">存档</param>
    /// <param name="file">文件名</param>
    /// <param name="data">文件内容</param>
    public static void SaveMcaFile(WorldObj obj, string file, ChunkDataObj data)
    {
        var dir = obj.Local;

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件名</param>
    /// <param name="data">文件内容</param>
    public static void SaveMcaFile(GameSettingObj obj, string file, ChunkDataObj data)
    {
        var dir = obj.GetGamePath();

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 获取存档列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static Task<List<WorldObj>> GetWorldsAsync(GameSettingObj obj)
    {
        return obj.GetWorldsAsync();
    }

    /// <summary>
    /// 添加存档
    /// </summary>
    /// <param name="obj">游戏实例</param>
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
    /// 删除存档
    /// </summary>
    /// <param name="world">存档</param>
    public static void DeleteWorld(WorldObj world)
    {
        world.Remove();
    }

    /// <summary>
    /// 导出存档
    /// </summary>
    /// <param name="world">存档</param>
    /// <param name="file">导出路径</param>
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
    /// <param name="obj">游戏实例</param>
    /// <param name="sha256">是否获取SHA256</param>
    /// <returns></returns>
    public static Task<List<ResourcepackObj>> GetResourcepacks(GameSettingObj obj,
        bool sha256 = false)
    {
        return obj.GetResourcepacksAsync(sha256);
    }

    /// <summary>
    /// 删除资源包
    /// </summary>
    /// <param name="obj">资源包</param>
    public static void DeleteResourcepack(ResourcepackObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 导入资源包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
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
    /// <param name="file">截图</param>
    public static void DeleteScreenshot(ScreenshotObj file)
    {
        Screenshots.Delete(file);
    }

    /// <summary>
    /// 删除所有截图
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ClearScreenshots(GameSettingObj obj)
    {
        obj.ClearScreenshots();
    }

    /// <summary>
    /// 获取所有截图
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static List<ScreenshotObj> GetScreenshots(GameSettingObj obj)
    {
        return obj.GetScreenshots();
    }

    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    /// <returns></returns>
    public static GameSettingObj? GetGame(string? uuid)
    {
        return InstancesPath.GetGame(uuid);
    }

    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<IEnumerable<ServerInfoObj>> GetServers(GameSettingObj obj)
    {
        return await obj.GetServerInfosAsync();
    }

    /// <summary>
    /// 添加服务器
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="ip">地址</param>
    /// <returns></returns>
    public static Task AddServer(GameSettingObj obj, string name, string ip)
    {
        return obj.AddServerAsync(name, ip);
    }

    /// <summary>
    /// 删除服务器
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="server">服务器信息</param>
    /// <returns></returns>
    public static Task DeleteServer(GameSettingObj obj, ServerInfoObj server)
    {
        return obj.RemoveServerAsync(server.Name, server.IP);
    }

    /// <summary>
    /// 删除配置文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
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
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static Task<List<ShaderpackObj>> GetShaderpacks(GameSettingObj obj)
    {
        return obj.GetShaderpacksAsync();
    }

    /// <summary>
    /// 添加光影包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
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
    /// <param name="obj">光影包</param>
    public static void DeleteShaderpack(ShaderpackObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 获取结构文件列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
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
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
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
    /// <param name="obj">结构文件</param>
    public static void DeleteSchematic(SchematicObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 设置模组信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">信息</param>
    public static void SetModInfo(GameSettingObj obj, CurseForgeModObj.CurseForgeDataObj? data)
    {
        if (data == null)
            return;

        data.FixDownloadUrl();

        var obj1 = new Core.Objs.ModInfoObj()
        {
            FileId = data.Id.ToString(),
            ModId = data.ModId.ToString(),
            File = data.FileName,
            Name = data.DisplayName,
            Url = data.DownloadUrl,
            Sha1 = data.Hashes.Where(a => a.Algo == 1)
                .Select(a => a.Value).FirstOrDefault()!
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
    /// <param name="obj">游戏实例</param>
    /// <param name="data">信息</param>
    public static void SetModInfo(GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return;
        }

        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
        var obj1 = new Core.Objs.ModInfoObj()
        {
            FileId = data.Id.ToString(),
            ModId = data.ProjectId,
            File = file.Filename,
            Name = data.Name,
            Url = file.Url,
            Sha1 = file.Hashes.Sha1
        };
        if (!obj.Mods.TryAdd(obj1.ModId, obj1))
        {
            obj.Mods[obj1.ModId] = obj1;
        }

        obj.SaveModInfo();
    }

    /// <summary>
    /// 备份存档
    /// </summary>
    /// <param name="world">存档</param>
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
    /// <param name="obj">游戏实例</param>
    /// <param name="item1">文件</param>
    /// <param name="request">UI相关</param>
    /// <returns></returns>
    public static Task<bool> BackupWorld(GameSettingObj obj, FileInfo item1, ColorMCCore.Request request)
    {
        return obj.UnzipBackupWorldAsync(new UnzipBackupWorldArg { File = item1.FullName, Request = request });
    }

    /// <summary>
    /// 设置游戏名字
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">名字</param>
    public static void SetGameName(GameSettingObj obj, string data)
    {
        obj.Name = data;
        obj.Save();

        WindowManager.MainWindow?.LoadGameItem();
        WindowManager.ReloadTitle(obj);
    }

    /// <summary>
    /// 复制游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">实例名字</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <returns></returns>
    public static async Task<bool> CopyGame(GameSettingObj obj, string data,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        if (GameManager.IsGameRun(obj))
        {
            return false;
        }

        if (await obj.Copy(new()
        {
            Game = data,
            Request = request,
            Overwirte = overwirte
        }) == null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 保存服务器包
    /// </summary>
    /// <param name="obj1">服务器包</param>
    public static void SaveServerPack(ServerPackObj obj1)
    {
        obj1.Save();
    }

    /// <summary>
    /// 获取服务器包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static ServerPackObj? GetServerPack(GameSettingObj obj)
    {
        return obj.GetServerPack().Pack;
    }

    /// <summary>
    /// 生成服务器包
    /// </summary>
    /// <param name="obj">服务器包</param>
    /// <param name="local">生成路径</param>
    /// <param name="request">UI相关</param>
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
    /// <param name="top">窗口</param>
    /// <param name="obj">服务器信息</param>
    public static async void CopyServer(TopLevel top, ServerInfoObj obj)
    {
        await BaseBinding.CopyTextClipboard(top, obj.IP);
    }

    /// <summary>
    /// 模组检测
    /// </summary>
    /// <param name="list">检测列表</param>
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
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static List<string> GetLogList(GameSettingObj obj)
    {
        return obj.GetLogFiles();
    }

    /// <summary>
    /// 读取日志文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">文件名</param>
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
    /// <param name="obj">游戏实例</param>
    /// <param name="fid">整合包信息</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
    /// <returns></returns>
    public static Task<bool> ModPackUpgrade(GameSettingObj obj, CurseForgeModObj.CurseForgeDataObj fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return ModPackHelper.UpgradeModPack(new UpdateCurseForgeModPackArg
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
    /// <param name="obj">游戏实例</param>
    /// <param name="fid">整合包信息</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
    /// <returns></returns>
    public static Task<bool> ModPackUpgrade(GameSettingObj obj, ModrinthVersionObj fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return ModPackHelper.UpgradeModPack(new UpdateModrinthModPackArg
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
    /// <param name="item">模组</param>
    /// <param name="items">依赖的模组</param>
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
    /// <param name="obj">游戏实例</param>
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
    /// <param name="obj">游戏实例</param>
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
                        (file1.EndsWith(Names.NameLitematicExt) || file1.EndsWith(Names.NameSchematicExt) || file1.EndsWith(Names.NameSchemExt)))
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
    public static async Task<PackType?> CheckType(string local)
    {
        Stream? stream = null;
        try
        {
            if (local.StartsWith("http"))
            {
                using var res = await CoreHttpClient.GetAsync(local);
                using var stream1 = await res.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await stream1.CopyToAsync(memoryStream);
                stream = memoryStream;
            }
            else
            {
                stream = PathHelper.OpenRead(local);
            }

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
                using var zFile = new ZipArchive(stream);
                foreach (var item in zFile.Entries)
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
                }
                foreach (var item in zFile.Entries)
                {
                    if (item.Name.StartsWith(".minecraft/"))
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
                using var stream = PathHelper.OpenRead(local);
                if (stream == null)
                {
                    return false;
                }
                using var s = new ZipArchive(stream);
                s.ExtractToDirectory(obj.GetBasePath(), true);
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
        var res = await ColorMCCloudAPI.DownloadConfig(obj.UUID, local);
        if (res == 100)
        {
            await UnZipCloudConfig(game, cloud, local);
        }

        var temp = await ColorMCCloudAPI.HaveCloud(game);
        try
        {
            cloud.ConfigTime = DateTime.Parse(temp.Data2!);
        }
        catch
        {

        }
        GameCloudUtils.SetCloudData(game, cloud);

        game = game.Reload();

        if (game.Mods != null)
        {
            var list = new List<FileItemObj>();
            foreach (var item in game.Mods.Values)
            {
                list.Add(new()
                {
                    Url = item.Url,
                    Name = item.File,
                    Local = game.GetGamePath() + "/" + item.Path + "/" + item.File,
                    Sha1 = item.Sha1
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
        var res = await ColorMCCloudAPI.HaveCloud(obj);
        if (res.State && res.Data1)
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
    public static async Task<bool> DeleteGame(GameSettingObj obj)
    {
        var res = await obj.Remove();
        if (res)
        {
            WindowManager.CloseGameWindow(obj);
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
            var obj = JsonUtils.ToObj(data.Message!, JsonType.ServerPackObj);
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
                Request = model.ShowAsync,
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
                model.ShowWithOk(App.Lang("GameBinding.Error10"), async () =>
                {
                    await game.Remove();
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
                        if (obj.Mods.Values.FirstOrDefault(item => item.Sha1 == item.Sha1) is { } item1)
                        {
                            info.AppendLine(string.Format(App.Lang("GameBinding.Info11"),
                                GameDownloadHelper.TestSourceType(item1.ModId, item1.FileId), item1.ModId, item1.FileId));
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
        var res = await obj.GetGameLoaderInfoAsync();
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
        return ModrinthHelper.AutoMarkAsync(obj, cov);
    }

    /// <summary>
    /// 消息解析
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static ChatObj StringToChat(string description)
    {
        return ChatConverter.StringToChar(description);
    }

    /// <summary>
    /// 取消启动
    /// </summary>
    public static void CancelLaunch()
    {
        s_launchCancel.Cancel();
    }

    /// <summary>
    /// 游戏进程已通过netty连接启动器
    /// </summary>
    /// <param name="uuid"></param>
    public static void GameConnect(string uuid)
    {
        s_gameConnect[uuid] = true;
    }

    /// <summary>
    /// 游戏进程启动后
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="handel"></param>
    private static void GameHandel(GameSettingObj obj, DesktopGameHandel handel)
    {
        var pr = handel.Process;
        Task.Run(() =>
        {
            try
            {
                var conf = obj.Window;

                do
                {
                    if (pr.HasExited ||
                    (s_gameConnect.TryGetValue(obj.UUID, out var temp) && temp))
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
                while (true);

                if (pr.HasExited)
                {
                    return Task.CompletedTask;
                }

                var config = GuiConfigUtils.Config.Input;
                //启用手柄支持
                if (config.Enable && !config.Disable)
                {
                    GameJoystick.Start(obj, handel);
                }

                //修改窗口标题

                if (string.IsNullOrWhiteSpace(conf?.GameTitle))
                {
                    return Task.CompletedTask;
                }

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
                    return Task.CompletedTask;
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

                    LaunchSocketUtils.SetTitle(obj, title1);

                    if (!conf.CycTitle || conf.TitleDelay <= 0 || pr.HasExited)
                    {
                        break;
                    }

                    Thread.Sleep(conf.TitleDelay);
                }
                while (!ColorMCGui.IsClose && !handel.IsExit);
            }
            catch
            {

            }

            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// 获取存档数据包列表
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    public static Task<List<DataPackObj>> GetWorldDataPackAsync(WorldObj world)
    {
        return world.GetDataPacksAsync();
    }

    /// <summary>
    /// 导出启动参数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="model"></param>
    public static async void ExportCmd(GameSettingObj obj, BaseModel model)
    {
        var top = TopLevel.GetTopLevel(WindowManager.MainWindow);
        if (top == null)
        {
            model.Show(App.Lang("MainWindow.Error10"));
            return;
        }
        var res = await UserBinding.GetLaunchUser(model);
        if (res.User is not { } user)
        {
            model.Show(App.Lang("GameBinding.Error3"));
            return;
        }
        var arg = MakeArg(user, model, -1);

        arg.Update2 = (obj, state) =>
        {
            Dispatcher.UIThread.Post(() =>
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
            });
        };
        model.Progress(App.Lang("MainWindow.Info43"));
        var res1 = await obj.CreateGameCmd(arg);
        if (!res1.Res)
        {
            model.Show(res1.Message!);
            return;
        }

        string[] args;

        if (SystemInfo.Os == OsType.Windows)
        {
            var str = new StringBuilder();
            foreach (var item in res1.Envs)
            {
                str.AppendLine($"$env:{item.Key} = '{item.Value}'");
            }

            str.Append("Set-Location -Path ")
                .AppendLine($"'{res1.Dir}'")
                .Append($"& '{res1.Java.Replace("javaw.exe", "java.exe")}'");

            foreach (var item in res1.Args)
            {
                str.Append($" '{item}'");
            }

            args = [".ps1", "game.ps1", str.ToString()];
        }
        else
        {
            var str = new StringBuilder();
            str.AppendLine("#!/bin/sh");
            str.AppendLine($"cd '{res1.Dir}' || exit 1");
            foreach (var item in res1.Envs)
            {
                str.AppendLine($"export {item.Key}='{item.Value}'");
            }

            str.Append($"'{res1.Java}'");

            foreach (var item in res1.Args)
            {
                str.Append($" '{item}'");
            }

            args = [".sh", "game.sh", str.ToString()];
        }

        var res2 = await PathBinding.SaveFile(top, FileType.Cmd, args);
        if (res2 == false)
        {
            model.Show(App.Lang("MainWindow.Error10"));
        }
    }

    /// <summary>
    /// 获取最后崩溃日志
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string? GetLastCrash(GameSettingObj obj)
    {
        return obj.GetLastCrash();
    }

    /// <summary>
    /// 启用云同步
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<MessageRes> StartCloud(GameSettingObj obj)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        var res = await ColorMCCloudAPI.StartCloud(obj);
        if (res == 100)
        {
            return new()
            {
                State = true
            };
        }
        else if (res == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 关闭云同步
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<MessageRes> StopCloud(GameSettingObj obj)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        var res = await ColorMCCloudAPI.StopCloud(obj);

        if (res == 100)
        {
            return new() { State = true };
        }
        else if (res == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 102)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error4")
            };
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 上传云同步配置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="files">选中的配置</param>
    /// <param name="model">Gui回调</param>
    /// <returns></returns>
    public static async Task<MessageRes> UploadConfig(GameSettingObj obj, List<string> files, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info8"));

        var data = GameCloudUtils.GetCloudData(obj);
        string dir = obj.GetBasePath();
        data.Config ??= [];
        data.Config.Clear();
        foreach (var item in files)
        {
            data.Config.Add(item[(dir.Length + 1)..]);
        }
        string name = Path.Combine(dir, GuiNames.NameCloudConfigFile);
        files.Remove(name);
        await new ZipUtils().ZipFileAsync(name, files, dir);
        model.Update?.Invoke(App.Lang("GameCloudWindow.Info9"));
        var res = await ColorMCCloudAPI.UploadConfig(obj, name);
        PathHelper.Delete(name);
        if (res.Data1 == 100)
        {
            if (res.Data2 != null)
            {
                data.ConfigTime = DateTime.Parse(res.Data2);
                GameCloudUtils.Save();
            }
            return new() { State = true };
        }
        if (res.Data1 == 104)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error5")
            };
        }
        else if (res.Data1 == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 下载云同步配置压缩包
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="model">UI相关</param>
    /// <returns></returns>
    public static async Task<MessageRes> DownloadConfig(GameSettingObj game, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info10"));
        var data = GameCloudUtils.GetCloudData(game);
        string local = Path.Combine(game.GetBasePath(), GuiNames.NameCloudConfigFile);
        var res = await ColorMCCloudAPI.DownloadConfig(game.UUID, local);
        if (res == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res != 100)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error3")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info11"));
        var res1 = await UnZipCloudConfig(game, data, local);
        if (!res1)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error6")
            };
        }

        return new() { State = true };
    }

    /// <summary>
    /// 是否有云同步
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<CloudRes> HaveCloud(GameSettingObj obj)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        return await ColorMCCloudAPI.HaveCloud(obj);
    }

    /// <summary>
    /// 获取云同步存档列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns></returns>
    public static async Task<CloudWorldRes> GetCloudWorldListAsync(GameSettingObj game)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        return await ColorMCCloudAPI.GetWorldList(game);
    }

    /// <summary>
    /// 上传存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">存档</param>
    /// <param name="model">Gui回调</param>
    /// <returns></returns>
    public static async Task<MessageRes> UploadCloudWorldAsync(GameSettingObj game, WorldCloudModel world, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info9"));
        string dir = world.World.Local;
        string local = Path.Combine(world.World.Game.GetSavesPath(), $"{world.World.LevelName}{Names.NameZipExt}");
        if (!world.HaveCloud)
        {
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info8"));
            await new ZipUtils().ZipFileAsync(dir, local);
        }
        else
        {
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info12"));
            //云端文件
            var list = await ColorMCCloudAPI.GetWorldFiles(game, world.World);
            if (list == null)
            {
                return new()
                {
                    Message = App.Lang("GameCloudWindow.Error7")
                };
            }
            //本地文件
            var files = PathHelper.GetAllFiles(dir);
            var pack = new List<string>();

            string dir1 = Path.GetFullPath(dir + "/");
            foreach (var item in files)
            {
                var name = item.FullName.Replace(dir1, "").Replace("\\", "/");
                using var file = PathHelper.OpenRead(item.FullName)!;
                var sha1 = await HashHelper.GenSha1Async(file);
                if (list.TryGetValue(name, out var sha11))
                {
                    list.Remove(name);
                    if (sha1 != sha11)
                    {
                        pack.Add(item.FullName);
                    }
                }
                else
                {
                    pack.Add(item.FullName);
                }
            }

            bool have = false;
            var delete = Path.Combine(dir, GuiNames.NameCloudDeleteFile);
            if (list.Count > 0)
            {
                have = true;
                await PathHelper.WriteTextAsync(delete, JsonUtils.ToString([.. list.Keys], JsonType.ListString));
                pack.Add(delete);
            }
            if (pack.Count == 0)
            {
                return new()
                {
                    Message = App.Lang("GameCloudWindow.Info13")
                };
            }
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info8"));
            await new ZipUtils().ZipFileAsync(local, pack, dir);
            if (have)
            {
                PathHelper.Delete(delete);
            }
        }

        var res = await ColorMCCloudAPI.UploadWorld(game, world.World, local);
        PathHelper.Delete(local);
        if (res == 100)
        {
            return new() { State = true };
        }
        else if (res == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 104)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error5")
            };
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 下载存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">云存档</param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<MessageRes> DownloadCloudWorldAsync(GameSettingObj game, WorldCloudModel world, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info10"));
        string dir = Path.Combine(game.GetSavesPath(), world.Cloud.Name);
        string local = Path.Combine(game.GetSavesPath(), $"{world.Cloud.Name}{Names.NameZipExt}");
        var list = new Dictionary<string, string>();
        if (world.HaveLocal)
        {
            var files = PathHelper.GetAllFiles(dir);
            string dir1 = Path.GetFullPath(dir + "/");
            foreach (var item in files)
            {
                var name = item.FullName.Replace(dir1, "").Replace("\\", "/");
                using var file = PathHelper.OpenRead(item.FullName)!;
                var sha1 = await HashHelper.GenSha1Async(file);
                list.Add(name, sha1);
            }
        }

        var res = await ColorMCCloudAPI.DownloadWorld(game, world.Cloud, local, list);
        if (res == 100)
        {
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info11"));
            try
            {
                using var file = PathHelper.OpenRead(local)!;
                await new ZipUtils().UnzipAsync(dir, local, file);
                return new() { State = true };
            }
            catch (Exception e)
            {
                string temp = App.Lang("GameCloudWindow.Error9");
                Logs.Error(temp, e);
                return new() { Message = temp };
            }
            finally
            {
                PathHelper.Delete(local);
            }
        }
        if (res == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 102)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error8")
            };
        }
        else if (res == 103)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Info13")
            };
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 删除云存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">存档名字</param>
    /// <returns></returns>
    public static async Task<MessageRes> DeleteCloudWorld(GameSettingObj game, string name)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error11")
            };
        }
        var res = await ColorMCCloudAPI.DeleteWorld(game, name);

        if (res == 100)
        {
            return new() { State = true };
        }
        else if (res == 101)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 102)
        {
            return new()
            {
                Message = App.Lang("GameCloudWindow.Error8")
            };
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 重读自定义启动配置
    /// </summary>
    /// <param name="obj"></param>
    public static void ReloadJson(GameSettingObj obj)
    {
        obj.ReadCustomJson();
    }
}