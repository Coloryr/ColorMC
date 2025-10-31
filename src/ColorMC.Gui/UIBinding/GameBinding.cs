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
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.Utils;
using SkiaSharp;

namespace ColorMC.Gui.UIBinding;

/// <summary>
/// 游戏实例相关操作
/// </summary>
public static class GameBinding
{
    /// <summary>
    /// 添加游戏实例
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <returns></returns>
    public static async Task<bool> AddGameAsync(GameSettingObj game, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte)
    {
        var game1 = await InstancesPath.CreateGameAsync(new CreateGameArg
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
    public static async Task<GameRes> AddGameAsync(string? name, string local, List<string>? unselect,
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
    public static Task<GameRes> AddPackAsync(string dir, PackType type, string? name,
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
    /// 安装CF整合包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <param name="icon">整合包图标</param>
    /// <param name="group">游戏分组</param>
    /// <param name="zip">UI相关</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
    /// <returns></returns>
    public static async Task<GameRes> InstallCurseForgeAsync(CurseForgeModObj.CurseForgeDataObj data,
        string? icon, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return await AddGameHelper.InstallCurseForge(new DownloadCurseForgeArg
        {
            Data = data,
            IconUrl = icon,
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
    /// <param name="icon">整合包图标</param>
    /// <param name="group">游戏分组</param>
    /// <param name="zip">UI相关</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <param name="update">UI相关</param>
    /// <param name="update2">UI相关</param>
    /// <returns></returns>
    public static async Task<GameRes> InstallModrinthAsync(ModrinthVersionObj data,
        string? icon, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return await AddGameHelper.InstallModrinth(new DownloadModrinthArg
        {
            Data = data,
            IconUrl = icon,
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
    public static async Task SetGameIconFromFileAsync(TopLevel top, BaseModel model, GameSettingObj obj)
    {
        try
        {
            var file = await PathBinding.SelectFileAsync(top, FileType.GameIcon);
            if (file.Path != null)
            {
                bool resize = await model.ShowAsync(App.Lang("GameBinding.Info14"));

                model.Progress(App.Lang("GameBinding.Info4"));
                using var info = SKBitmap.Decode(PathHelper.OpenRead(file.Path)!);

                if (resize && (info.Width > 100 || info.Height > 100))
                {
                    using var image = await ImageUtils.ResizeAsync(info, 100, 100);
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
    public static async Task<GameLaunchListRes> LaunchAsync(BaseModel model, ICollection<GameSettingObj> objs)
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

        var port = LaunchSocketUtils.Port;

        //锁定账户
        UserManager.LockUser(user);
        var cancels = new Dictionary<string, CancellationToken>();
        foreach (var item in list)
        {
            GameManager.SetConnect(item.UUID, false);
            GameManager.ClearGameLog(item);
            cancels[item.UUID] = GameManager.StartGame(item);
        }

        var state1 = LaunchState.End;
        var arg = MakeArg(user, model, port);
        arg.Admin = GuiConfigUtils.Config.ServerCustom.GameAdminLaunch;

        //设置自动加入服务器
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfoAsync(GuiConfigUtils.Config.ServerCustom.IP,
                GuiConfigUtils.Config.ServerCustom.Port);

            arg.Server = new()
            {
                IP = server.ServerAddress,
                Port = server.ServerPort
            };
        }

        var res1 = await Task.Run(async () =>
        {
            return await objs.StartGameAsync(arg, cancels);
        });

        model.SubTitle = "";
        FuntionUtils.RunGC();

        //if (s_launchCancel.IsCancellationRequested)
        //{
        //    UserBinding.UnLockUser(user);
        //    foreach (var item in list)
        //    {
        //        GameManager.GameExit(item);
        //    }
        //    return new() { User = user };
        //}

        if (GuiConfigUtils.Config.ServerCustom.RunPause)
        {
            Media.PlayState = PlayState.Pause;
        }

        var list1 = new Dictionary<string, LaunchState>();
        var list2 = new List<string>();
        //逐一启动
        foreach (var item in res1)
        {
            if (item.Value.Handel is { } handel)
            {
                item.Key.LaunchData.LastTime = DateTime.Now;
                item.Key.SaveLaunchData();

                GameCountUtils.LaunchDone(item.Key);
                GameStateUpdate(item.Key);

                GameManager.StartGameHandel(item.Key, handel);

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
            UserManager.UnlockUser(user);
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
    public static async Task<GameLaunchOneRes> LaunchAsync(BaseModel model, GameSettingObj? obj,
        SaveObj? world = null, bool hide = false)
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
                var mem = obj.JvmArg?.MaxMemory ?? ConfigLoad.Config.DefaultJvmArg.MaxMemory;
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

        GameManager.SetConnect(obj.UUID, false);
        GameManager.ClearGameLog(obj);
        var cancel = GameManager.StartGame(obj);

        var port = LaunchSocketUtils.Port;

        //锁定账户
        UserManager.LockUser(user);

        var state1 = LaunchState.End;
        string? temp = null;
        var arg = MakeArg(user, model, port);
        arg.World = world;
        arg.Admin = GuiConfigUtils.Config.ServerCustom.GameAdminLaunch;
        //设置自动加入服务器
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfoAsync(GuiConfigUtils.Config.ServerCustom.IP,
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
                return await obj.StartGameAsync(arg, cancel);
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
        if (cancel.IsCancellationRequested)
        {
            UserManager.UnlockUser(user);
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

            GameCountUtils.LaunchDone(obj);
            GameStateUpdate(obj);

            if (pr is { } handel)
            {
                GameManager.StartGameHandel(obj, handel);

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
            UserManager.UnlockUser(user);
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
                    model.ShowAsync(pre ? App.Lang("App.Text17") : App.Lang("App.Text18")));
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
                        App.Lang("App.Text16"), login.UserName));
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
                            LaunchState.Login => "App.Text6",
                            LaunchState.Check => "App.Text7",
                            LaunchState.CheckVersion => "App.Text8",
                            LaunchState.CheckLib => "App.Text9",
                            LaunchState.CheckAssets => "App.Text10",
                            LaunchState.CheckLoader => "App.Text11",
                            LaunchState.CheckLoginCore => "App.Text12",
                            LaunchState.CheckMods => "App.Text15",
                            LaunchState.Download => "App.Text13",
                            LaunchState.JvmPrepare => "App.Text14",
                            LaunchState.LaunchPre => "App.Text19",
                            LaunchState.LaunchPost => "App.Text20",
                            LaunchState.InstallForge => "App.Text23",
                            _ => ""
                        }));
                    }
                    else
                    {
                        model.SubTitle = App.Lang(state switch
                        {
                            LaunchState.Login => "App.Text6",
                            LaunchState.Check => "App.Text7",
                            LaunchState.CheckVersion => "App.Text8",
                            LaunchState.CheckLib => "App.Text9",
                            LaunchState.CheckAssets => "App.Text10",
                            LaunchState.CheckLoader => "App.Text11",
                            LaunchState.CheckLoginCore => "App.Text12",
                            LaunchState.CheckMods => "App.Text15",
                            LaunchState.Download => "App.Text13",
                            LaunchState.JvmPrepare => "App.Text14",
                            LaunchState.LaunchPre => "App.Text19",
                            LaunchState.LaunchPost => "App.Text20",
                            LaunchState.InstallForge => "App.Text23",
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
    public static async Task<bool> ReloadVersionAsync()
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
    public static Task<List<ModObj>> GetGameModsAsync(GameSettingObj obj, bool sha256 = false)
    {
        return obj.GetModsAsync(sha256);
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
    public static StringRes ModEnableDisable(ModObj obj)
    {
        try
        {
            if (!File.Exists(obj.Local))
            {
                return new() { Data = App.Lang("GameBinding.Error15") };
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
            return new() { Data = temp };
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
        mod.DeleteAsync();
    }

    /// <summary>
    /// 添加模组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns></returns>
    public static Task<bool> AddModsAsync(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
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
    public static List<string> GetAllConfig(SaveObj obj)
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
    public static async Task<ChunkDataObj?> ReadMcaAsync(SaveObj obj, string name)
    {
        var dir = obj.Local;
        var file = Path.GetFullPath(dir + "/" + name);
        if (!File.Exists(file))
        {
            return null;
        }

        return await ChunkMca.ReadAsync(file);
    }

    /// <summary>
    /// 读取区块信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">区块文件名</param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMcaAsync(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();
        var file = Path.GetFullPath(dir + "/" + name);
        if (!File.Exists(file))
        {
            return null;
        }

        return await ChunkMca.ReadAsync(file);
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj">存档</param>
    /// <param name="name">文件名</param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbtAsync(SaveObj obj, string name)
    {
        var dir = obj.Local;
        var file = Path.GetFullPath(dir + "/" + name);
        if (!File.Exists(file))
        {
            return null;
        }

        return await NbtBase.ReadAsync(file);
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">文件名</param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbtAsync(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();
        var file = Path.GetFullPath(dir + "/" + name);
        if (!File.Exists(file))
        {
            return null;
        }

        return await NbtBase.ReadAsync(file);
    }

    /// <summary>
    /// 添加存档
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<bool> AddWorldAsync(GameSettingObj obj, string? file)
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
    /// 导入资源包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns></returns>
    public static Task<bool> AddResourcepackAsync(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
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
    /// 添加光影包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns></returns>
    public static Task<bool> AddShaderpackAsync(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
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
    /// 备份存档
    /// </summary>
    /// <param name="world">存档</param>
    /// <returns></returns>
    public static async Task<bool> BackupWorldAsync(SaveObj world)
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
    public static Task<bool> BackupWorldAsync(GameSettingObj obj, FileInfo item1, ColorMCCore.Request request)
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

        EventManager.OnGameNameChange(obj.UUID);
    }

    /// <summary>
    /// 复制游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">实例名字</param>
    /// <param name="request">UI相关</param>
    /// <param name="overwirte">UI相关</param>
    /// <returns></returns>
    public static async Task<bool> CopyGameAsync(GameSettingObj obj, string data,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        if (GameManager.IsGameRun(obj))
        {
            return false;
        }

        if (await obj.CopyAsync(new()
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
    /// 生成服务器包
    /// </summary>
    /// <param name="obj">服务器包</param>
    /// <param name="local">生成路径</param>
    /// <param name="request">UI相关</param>
    /// <returns></returns>
    public static Task<bool> GenServerPackAsync(ServerPackObj obj, string local,
        ColorMCCore.Request request)
    {
        return obj.GenServerPackAsync(new ServerPackGenArg
        {
            Local = local,
            Request = request
        });
    }

    /// <summary>
    /// 模组检测
    /// </summary>
    /// <param name="list">检测列表</param>
    /// <returns></returns>
    public static Task<bool> ModCheckAsync(List<ModDisplayModel> list)
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
    public static async Task<GameRuntimeLog?> ReadLogAsync(GameSettingObj obj, string name)
    {
        if (GameManager.IsGameRun(obj) && (name.EndsWith(Names.NameLatestLogFile) || name.EndsWith(Names.NameDebugLogFile)))
        {
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
    public static Task<bool> ModPackUpgradeAsync(GameSettingObj obj, CurseForgeModObj.CurseForgeDataObj fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return CurseForgeHelper.UpgradeModPackAsync(new UpdateCurseForgeModPackArg
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
    public static Task<bool> ModPackUpgradeAsync(GameSettingObj obj, ModrinthVersionObj fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return ModrinthHelper.UpgradeModPackAsync(new UpdateModrinthModPackArg
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
    public static async Task<bool> AddFileAsync(GameSettingObj obj, IDataTransfer data, FileType type)
    {
        if (!data.Contains(DataFormat.File))
        {
            return false;
        }

        var list = data.TryGetFiles();
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
                        (file1.EndsWith(Names.NameLitematicExt) || file1.EndsWith(Names.NameSchematicExt) ||
                         file1.EndsWith(Names.NameSchemExt)))
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
    /// <param name="local">文件位置</param>
    /// <returns>压缩包类型</returns>
    public static async Task<PackType?> CheckTypeAsync(string local)
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

            if (local.EndsWith(Names.NameMrpackExt))
            {
                return PackType.Modrinth;
            }
            if (local.EndsWith(Names.NameZipExt))
            {
                using var zFile = new ZipArchive(stream);
                foreach (var item in zFile.Entries)
                {
                    if (item.Name == Names.NameGameFile)
                    {
                        return PackType.ColorMC;
                    }
                    else if (item.Name == Names.NameHMCLFile)
                    {
                        return PackType.HMCL;
                    }
                    else if (item.Name == Names.NameMMCCfgFile)
                    {
                        return PackType.MMC;
                    }
                    else if (item.Name == Names.NameManifestFile)
                    {
                        return PackType.CurseForge;
                    }
                }
                foreach (var item in zFile.Entries)
                {
                    if (item.FullName.StartsWith(".minecraft/"))
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
    public static async Task<bool> UnZipCloudConfigAsync(GameSettingObj obj, CloudDataObj data, string local)
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
    public static async Task<StringRes> DownloadCloudAsync(CloundListObj obj, string? group,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        var game = await InstancesPath.CreateGameAsync(new CreateGameArg
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
            return new() { Data = App.Lang("GameBinding.Error9") };
        }

        var cloud = new CloudDataObj()
        {
            Config = []
        };

        GameCloudUtils.SetCloudData(game, cloud);
        string local = Path.GetFullPath(game.GetBasePath() + "/config.zip");
        var res = await ColorMCCloudAPI.DownloadConfigAsync(obj.UUID, local);
        if (res == 100)
        {
            await UnZipCloudConfigAsync(game, cloud, local);
        }

        var temp = await ColorMCCloudAPI.HaveCloudAsync(game);
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
                    return new() { Data = App.Lang("GameBinding.Error10") };
                }
            }
        }

        return new() { State = true, Data = game.UUID };
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
        var res = await ColorMCCloudAPI.HaveCloudAsync(obj);
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
    /// <returns></returns>
    public static async Task<bool> DeleteGameAsync(GameSettingObj obj)
    {
        var res = await obj.RemoveAsync();
        if (res)
        {
            EventManager.OnGameDelete(obj.UUID);
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
    public static async Task<StringRes> DownloadServerPackAsync(BaseModel model,
        string? name, string? group, string text, ColorMCCore.GameOverwirte overwirte)
    {
        try
        {
            var data = await CoreHttpClient.GetStringAsync(text + "server.json");
            if (!data.State)
            {
                return new() { Data = App.Lang("GameBinding.Error11") };
            }
            var obj = JsonUtils.ToObj(data.Data!, JsonType.ServerPackObj);
            if (obj == null)
            {
                return new() { Data = App.Lang("GameBinding.Error12") };
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
            game = await InstancesPath.CreateGameAsync(new CreateGameArg
            {
                Game = game,
                Request = model.ShowAsync,
                Overwirte = overwirte
            });

            if (game == null)
            {
                return new() { Data = App.Lang("GameBinding.Error9") };
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
            }, CancellationToken.None);
            if (!res1)
            {
                model.ProgressClose();
                model.ShowWithOk(App.Lang("GameBinding.Error10"), async () =>
                {
                    await game.RemoveAsync();
                });

                return new();
            }

            PathHelper.WriteText(game.GetServerPackFile(), data.Data!);

            return new() { State = true, Data = game.UUID };
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameBinding.Error12");
            Logs.Error(temp, e);
            return new() { Data = temp };
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
    /// <param name="pack">数据包列表</param>
    /// <returns></returns>
    public static bool DataPackDisableOrEnable(IEnumerable<DataPackModel> pack)
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
    public static async Task<bool> DeleteDataPackAsync(DataPackModel item)
    {
        if (GameManager.IsGameRun(item.Pack.World.Game))
        {
            return false;
        }
        return await item.Pack.World.DeleteDataPackAsync([item.Pack]);
    }

    /// <summary>
    /// 删除数据包
    /// </summary>
    /// <param name="items"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteDataPackAsync(IEnumerable<DataPackModel> items)
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
        return await list[0].World.DeleteDataPackAsync(list);
    }

    /// <summary>
    /// 生成实例信息
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task GenGameInfoAsync(GameSettingObj obj)
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
                        GetGameLoaderAsync(obj), obj.CustomLoader?.OffLib));
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
    public static async Task<string> GetGameLoaderAsync(GameSettingObj obj)
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
    /// 标记模组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="pid">项目ID</param>
    /// <param name="fid">文件ID</param>
    /// <returns>标记结果</returns>
    public static async Task<bool> MarkModsAsync(GameSettingObj obj, string pid, string fid)
    {
        var source = GameDownloadHelper.TestSourceType(pid, fid);
        if (source == SourceType.CurseForge)
        {
            var res = await CurseForgeAPI.GetModInfoAsync(pid);
            if (res == null)
            {
                return false;
            }

            int page = 0;

            for (; ; )
            {
                var list = await CurseForgeAPI.GetCurseForgeFilesAsync(pid, null, page);
                if (list == null)
                {
                    return false;
                }

                foreach (var item in list.Data)
                {
                    if (item.Id.ToString() != fid)
                    {
                        continue;
                    }

                    obj.AddModInfo(CurseForgeHelper.MakeModInfo(item, obj.GetModsPath()));
                    return true;
                }

                if (list.Pagination.TotalCount < page)
                {
                    page++;
                }
                else
                {
                    return false;
                }
            }
        }
        else if (source == SourceType.Modrinth)
        {
            var res = await ModrinthAPI.GetFileVersionsAsync(pid, null, Loaders.Normal);
            if (res == null)
            {
                return false;
            }

            foreach (var item in res)
            {
                if (item.Id != fid)
                {
                    continue;
                }

                obj.AddModInfo(ModrinthHelper.MakeModInfo(item, obj.GetModsPath()));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 导出启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="model"></param>
    public static async void ExportCmd(GameSettingObj obj, BaseModel model)
    {
        var top = TopLevel.GetTopLevel(WindowManager.MainWindow);
        if (top == null)
        {
            model.Show(App.Lang("App.Error7"));
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
                    LaunchState.Login => "App.Text6",
                    LaunchState.Check => "App.Text7",
                    LaunchState.CheckVersion => "App.Text8",
                    LaunchState.CheckLib => "App.Text9",
                    LaunchState.CheckAssets => "App.Text10",
                    LaunchState.CheckLoader => "App.Text11",
                    LaunchState.CheckLoginCore => "App.Text12",
                    LaunchState.CheckMods => "App.Text15",
                    LaunchState.Download => "App.Text13",
                    LaunchState.JvmPrepare => "App.Text14",
                    LaunchState.LaunchPre => "App.Text19",
                    LaunchState.LaunchPost => "App.Text20",
                    LaunchState.InstallForge => "App.Text23",
                    _ => ""
                }));
            });
        };
        model.Progress(App.Lang("App.Text25"));
        var res1 = await obj.CreateGameCmdAsync(arg);
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

        var res2 = await PathBinding.SaveFileAsync(top, FileType.Cmd, args);
        if (res2 == false)
        {
            model.Show(App.Lang("App.Error7"));
        }
    }

    /// <summary>
    /// 启用云同步
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<StringRes> StartCloudAsync(GameSettingObj obj)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }

        var res = await ColorMCCloudAPI.StartCloudAsync(obj);
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
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }

        return new()
        {
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 关闭云同步
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<StringRes> StopCloudAsync(GameSettingObj obj)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }

        var res = await ColorMCCloudAPI.StopCloudAsync(obj);

        if (res == 100)
        {
            return new() { State = true };
        }
        else if (res == 101)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 102)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error4")
            };
        }

        return new()
        {
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 上传云同步配置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="files">选中的配置</param>
    /// <param name="model">Gui回调</param>
    /// <returns></returns>
    public static async Task<StringRes> UploadConfigAsync(GameSettingObj obj, List<string> files, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
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
        await new ZipProcess().ZipFileAsync(name, files, dir);
        model.Update?.Invoke(App.Lang("GameCloudWindow.Info9"));
        var res = await ColorMCCloudAPI.UploadConfigAsync(obj, name);
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
                Data = App.Lang("GameCloudWindow.Error5")
            };
        }
        else if (res.Data1 == 101)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }

        return new()
        {
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 下载云同步配置压缩包
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="model">UI相关</param>
    /// <returns></returns>
    public static async Task<StringRes> DownloadConfigAsync(GameSettingObj game, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info10"));
        var data = GameCloudUtils.GetCloudData(game);
        string local = Path.Combine(game.GetBasePath(), GuiNames.NameCloudConfigFile);
        var res = await ColorMCCloudAPI.DownloadConfigAsync(game.UUID, local);
        if (res == 101)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res != 100)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error3")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info11"));
        var res1 = await UnZipCloudConfigAsync(game, data, local);
        if (!res1)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error6")
            };
        }

        return new() { State = true };
    }

    /// <summary>
    /// 是否有云同步
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<CloudRes> HaveCloudAsync(GameSettingObj obj)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }

        return await ColorMCCloudAPI.HaveCloudAsync(obj);
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
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }

        return await ColorMCCloudAPI.GetWorldListAsync(game);
    }

    /// <summary>
    /// 上传存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">存档</param>
    /// <param name="model">Gui回调</param>
    /// <returns></returns>
    public static async Task<StringRes> UploadCloudWorldAsync(GameSettingObj game, WorldCloudModel world, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }

        model.Update?.Invoke(App.Lang("GameCloudWindow.Info9"));
        string dir = world.World.Local;
        string local = Path.Combine(world.World.Game.GetSavesPath(), $"{world.World.LevelName}{Names.NameZipExt}");
        if (!world.HaveCloud)
        {
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info8"));
            using var stream = PathHelper.OpenWrite(local);
            using var zip = await new ZipProcess().ZipFileAsync(dir, stream);
        }
        else
        {
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info12"));
            //云端文件
            var list = await ColorMCCloudAPI.GetWorldFilesAsync(game, world.World);
            if (list == null)
            {
                return new()
                {
                    Data = App.Lang("GameCloudWindow.Error7")
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
                    Data = App.Lang("GameCloudWindow.Info13")
                };
            }
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info8"));
            await new ZipProcess().ZipFileAsync(local, pack, dir);
            if (have)
            {
                PathHelper.Delete(delete);
            }
        }

        var res = await ColorMCCloudAPI.UploadWorldAsync(game, world.World, local);
        PathHelper.Delete(local);
        if (res == 100)
        {
            return new() { State = true };
        }
        else if (res == 101)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 104)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error5")
            };
        }

        return new()
        {
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 下载存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">云存档</param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<StringRes> DownloadCloudWorldAsync(GameSettingObj game, WorldCloudModel world, ProcessUpdateArg model)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
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

        var res = await ColorMCCloudAPI.DownloadWorldAsync(game, world.Cloud, local, list);
        if (res == 100)
        {
            model.Update?.Invoke(App.Lang("GameCloudWindow.Info11"));
            try
            {
                using var file = PathHelper.OpenRead(local)!;
                await new ZipProcess().UnzipAsync(dir, local, file);
                return new() { State = true };
            }
            catch (Exception e)
            {
                string temp = App.Lang("GameCloudWindow.Error9");
                Logs.Error(temp, e);
                return new() { Data = temp };
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
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 102)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error8")
            };
        }
        else if (res == 103)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Info13")
            };
        }

        return new()
        {
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 删除云存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">存档名字</param>
    /// <returns></returns>
    public static async Task<StringRes> DeleteCloudWorldAsync(GameSettingObj game, string name)
    {
        if (!ColorMCCloudAPI.Connect)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error11")
            };
        }
        var res = await ColorMCCloudAPI.DeleteWorldAsync(game, name);

        if (res == 100)
        {
            return new() { State = true };
        }
        else if (res == 101)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error2")
            };
        }
        else if (res == 102)
        {
            return new()
            {
                Data = App.Lang("GameCloudWindow.Error8")
            };
        }

        return new()
        {
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 打开种子信息
    /// </summary>
    /// <param name="model"></param>
    public static void OpenWorldSeed(WorldModel model)
    {
        var obj = model.World;
        var game = obj.Game;

        var url = ChunkbaseApi.GenUrl(game.Version, obj.RandomSeed, obj.GeneratorName == "minecraft:large_biomes");
        BaseBinding.OpenUrl(url);
    }

    /// <summary>
    /// 设置游戏实例方块图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="block">方块</param>
    public static void SetGameIconBlock(GameSettingObj obj, string block)
    {
        GameManager.SetGameBlock(obj, block);
        EventManager.OnGameIconChange(obj.UUID);
    }

    /// <summary>
    /// 主页面选择游戏实例，并刷新实例图标
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    public static void SelectAndReloadGame(string? uuid)
    {
        if (uuid == null)
        {
            return;
        }
        if (WindowManager.MainWindow?.DataContext is MainModel model)
        {
            model.Select(uuid);
            if (model.Game != null)
            {
                ImageManager.ReloadImage(model.Game.Obj);
            }
        }
    }
}