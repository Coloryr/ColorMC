using System.Diagnostics;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏启动类
/// </summary>
public static class Launch
{
    /// <summary>
    /// 实例启动前登录账户
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <exception cref="LaunchException"></exception>
    private static async Task AuthLoginAsync(GameSettingObj obj, GameLaunchArg larg, CancellationToken token)
    {
        larg.Gui?.StateUpdate(obj, LaunchState.Loging);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        //刷新账户token
        try
        {
            var res1 = await larg.Auth.RefreshTokenAsync(token);
            larg.Auth = res1;
            larg.Auth.Save();
        }
        catch (Exception e)
        {
            if (larg.Auth.AuthType != AuthType.Offline
                    && !string.IsNullOrWhiteSpace(larg.Auth.UUID)
                    && larg.Gui != null
                    && await larg.Gui.LoginFail(larg.Auth))
            {
                larg.Auth = new LoginObj
                {
                    UserName = larg.Auth.UserName,
                    UUID = larg.Auth.UUID,
                    AuthType = AuthType.Offline
                };
            }
            else
            {
                throw new LaunchException(LaunchError.AuthLoginFail, e);
            }
        }

        stopwatch.Stop();
        ColorMCCore.OnGameLog(obj, GameSystemLog.LoginTime, stopwatch.Elapsed.ToString(), "");
    }

    /// <summary>
    /// 处理服务器实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token"></param>
    /// <exception cref="LaunchException"></exception>
    private static async Task PackUpdateAsync(GameSettingObj obj, GameLaunchArg larg, CancellationToken token)
    {
        if (obj.ModPackType != SourceType.ColorMC || string.IsNullOrWhiteSpace(obj.ServerUrl))
        {
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await obj.ServerPackCheckAsync(larg.Gui, token);

        stopwatch.Stop();
        ColorMCCore.OnGameLog(obj, GameSystemLog.ServerPackCheckTime, stopwatch.Elapsed.ToString(), "");
    }

    /// <summary>
    /// 检查游戏实例文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token"></param>
    /// <returns>游戏实例启动配置</returns>
    /// <exception cref="LaunchException"></exception>
    private static async Task<GameLaunchObj> CheckGameFileAsync(GameSettingObj obj, GameLaunchArg larg,
        CancellationToken token)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var res3 = await larg.Auth.CheckLoginCoreAsync(token);
        var arg1 = await obj.MakeArgAsync(larg, token);
        var res = await obj.CheckGameFileAsync(arg1, token);
        stopwatch.Stop();
        ColorMCCore.OnGameLog(obj, GameSystemLog.CheckGameFileTime, stopwatch.Elapsed.ToString(), "");

        if (res3 != null)
        {
            res.Add(res3);
        }

        //下载缺失的文件
        if (res.IsEmpty)
        {
            return arg1;
        }

        bool download = true;
        if (ConfigLoad.Config.Http.AutoDownload == false && larg.Gui != null)
        {
            download = await larg.Gui.RequestDownload();
        }

        if (download)
        {
            larg.Gui?.StateUpdate(obj, LaunchState.Downloading);

            stopwatch.Reset();
            stopwatch.Start();

            var ok = await DownloadManager.StartAsync([.. res]);
            if (!ok)
            {
                throw new LaunchException(LaunchError.DownloadFileError);
            }

            stopwatch.Stop();
            ColorMCCore.OnGameLog(obj, GameSystemLog.DownloadFileTime, stopwatch.Elapsed.ToString(), "");
        }

        return arg1;
    }

    /// <summary>
    /// 创建启动运行环境
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>运行环境</returns>
    private static Dictionary<string, string> MakeEnv(GameSettingObj obj)
    {
        var env = new Dictionary<string, string>();
        string envstr;
        if (obj.JvmArg?.JvmEnv is { } str)
        {
            envstr = str;
        }
        else if (ConfigLoad.Config.DefaultJvmArg.JvmEnv is { } str1)
        {
            envstr = str1;
        }
        else
        {
            envstr = "";
        }

        if (!string.IsNullOrWhiteSpace(envstr))
        {
            var list1 = envstr.Split('\n');
            foreach (var item in list1)
            {
                var item1 = item.Trim();
                var index = item1.IndexOf('=');
                string key, value;
                if (index == -1)
                {
                    key = item1;
                    value = "";
                }
                else if (index + 1 == item1.Length)
                {
                    key = item1[..^1];
                    value = "";
                }
                else
                {
                    key = item1[..index];
                    value = item1[(index + 1)..];
                }

                if (!env.TryAdd(key, value))
                {
                    env[key] = value;
                }
            }
        }

        return env;
    }

    /// <summary>
    /// 查找合适的Java
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="arg1"></param>
    /// <returns>Java路径</returns>
    /// <exception cref="LaunchException"></exception>
    private static async Task<string> FindJavaAsync(GameSettingObj obj, GameLaunchArg larg, GameLaunchObj arg1)
    {
        if (!string.IsNullOrWhiteSpace(obj.JvmLocal))
        {
            return obj.JvmLocal;
        }

        if (JvmPath.Jvms.Count == 0)
        {
            var list1 = await JavaHelper.FindJava();
            list1?.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        }

        var jvm = JvmPath.GetInfo(obj.JvmName);
        if (jvm == null)
        {
            foreach (var item in arg1.JavaVersions.OrderDescending())
            {
                jvm = JvmPath.FindJava(item);
                if (jvm != null)
                {
                    break;
                }
            }
        }

        if (jvm == null)
        {
            var jv = arg1.JavaVersions.First();
            larg.Gui?.NoJava(jv);
            throw new LaunchException(LaunchError.JavaNotFound, data: jv.ToString());
        }

        return jvm.GetPath();
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cmd">命令</param>
    /// <param name="env">运行环境</param>
    /// <param name="runsame">是否不等待结束</param>
    /// <param name="admin">管理员启动</param>
    private static void CmdRun(this GameSettingObj obj, string cmd, Dictionary<string, string> env, bool runsame,
        bool admin)
    {
        var args = cmd.Split('\n');
        var name = args[0].Trim();

        //查找文件
        if (!File.Exists(name))
        {
            name = Path.Combine(obj.GetBasePath(), name);
        }

        if (!File.Exists(name))
        {
            name = Path.Combine(obj.GetGamePath(), name);
        }

        if (!File.Exists(name))
        {
            throw new LaunchException(LaunchError.CmdFileNotFound, data: name);
        }

        //准备启动
        var info = new ProcessStartInfo(name)
        {
            WorkingDirectory = obj.GetGamePath(),
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        foreach (var item in env)
        {
            info.Environment.Add(item.Key, item.Value);
        }

        for (int a = 1; a < args.Length; a++)
        {
            info.ArgumentList.Add(args[a].Trim());
        }

        var p = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = info
        };
        p.OutputDataReceived += (_, b) => { ColorMCCore.OnGameLog(obj, b.Data); };
        p.ErrorDataReceived += (_, b) => { ColorMCCore.OnGameLog(obj, b.Data); };
        p.Exited += (_, _) => { p.Dispose(); };
        ProcessUtils.Launch(p, admin);
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        //是否与游戏同时启动
        if (runsame)
        {
            return;
        }

        p.WaitForExit();
    }

    /// <summary>
    /// 同时启动多个实例
    /// </summary>
    /// <param name="objs">启动的游戏列表</param>
    /// <param name="larg">启动参数</param>
    /// <param name="cancels">取消Token</param>
    /// <returns>启动结果</returns>
    public static async Task<Dictionary<GameSettingObj, GameLaunchRes>>
        StartGameAsync(this ICollection<GameSettingObj> objs, GameLaunchArg larg, Dictionary<string, CancellationToken> cancels)
    {
        //清理存在的实例日志
        foreach (var item in objs)
        {
            ColorMCCore.GameLogClear(item);
        }

        var list = new Dictionary<GameSettingObj, GameLaunchRes>();
        var list2 = new List<GameRunObj>();
        //登录账户
        var temp1 = objs.First();
        try
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource([.. cancels.Values]);
            await AuthLoginAsync(temp1, larg, token.Token);
        }
        catch (Exception e)
        {
            list.Add(temp1, new GameLaunchRes { Exception = e });
            return list;
        }

        if (cancels.Values.Any(item => item.IsCancellationRequested))
        {
            return list;
        }

        //逐个实例启动
        foreach (var obj in objs)
        {
            try
            {
                //版本号检测
                if (string.IsNullOrWhiteSpace(obj.Version)
                    || (obj.Loader != Loaders.Normal && string.IsNullOrWhiteSpace(obj.LoaderVersion))
                    || (obj.Loader == Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
                {
                    throw new LaunchException(LaunchError.VersionError);
                }

                var cancel = cancels[obj.UUID];

                //服务器实例更新
                await PackUpdateAsync(obj, larg, cancel);
                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                larg.Gui?.StateUpdate(obj, LaunchState.Checking);

                //检查游戏文件
                var arg1 = await CheckGameFileAsync(obj, larg, cancel);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //查找合适的JAVA
                var path = await FindJavaAsync(obj, larg, arg1);
                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //准备Jvm参数
                larg.Gui?.StateUpdate(obj, LaunchState.JvmPrepare);
                var arg = obj.MakeRunArg(larg, arg1, true);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //显示使用的Java
                ColorMCCore.OnGameLog(obj, GameSystemLog.JavaPath, path, "");

                //打印Jvm参数
                ColorMCCore.OnGameLog(obj, GameSystemLog.LaunchArgs);
                bool hidenext = false;
                foreach (var item in arg)
                {
                    if (hidenext)
                    {
                        hidenext = false;
                        ColorMCCore.OnGameLog(obj, "******");
                    }
                    else
                    {
                        ColorMCCore.OnGameLog(obj, item);
                    }

                    var low = item.ToLower();
                    if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
                    {
                        hidenext = true;
                    }
                }

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //自定义启动参数
                var env = MakeEnv(obj);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                list2.Add(new GameRunObj()
                {
                    Obj = obj,
                    Path = path,
                    Arg = arg,
                    Env = env,
                    Admin = larg.Admin,
                    Auth = larg.Auth
                });
            }
            catch (Exception e)
            {
                list.Add(obj, new GameLaunchRes { Exception = e });
            }
        }

        foreach (var item1 in list2)
        {
            //启动进程
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var handel = new GameHandel(item1);
            stopwatch.Stop();
            ColorMCCore.OnGameLog(item1.Obj, GameSystemLog.LaunchTime, stopwatch.Elapsed.ToString(), "");

            ColorMCCore.AddGameHandel(item1.Obj.UUID, handel);
            list.Add(item1.Obj, new GameLaunchRes { Handel = handel });
        }

        return list;
    }

    /// <summary>
    /// 生成游戏启动参数，不用于启动，用于生成启动脚本
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">请求参数</param>
    /// <returns>启动参数</returns>
    public static async Task<CreateCmdRes> CreateGameCmdAsync(this GameSettingObj obj, GameLaunchArg larg, CancellationToken token)
    {
        //版本号检测
        if (string.IsNullOrWhiteSpace(obj.Version)
            || (obj.Loader is not (Loaders.Normal or Loaders.Custom) && string.IsNullOrWhiteSpace(obj.LoaderVersion))
            || (obj.Loader is Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
        {
            throw new LaunchException(LaunchError.VersionError);
        }

        //登录账户
        await AuthLoginAsync(obj, larg, token);

        //检查游戏文件
        var arg1 = await obj.MakeArgAsync(larg, CancellationToken.None);

        //查找Java
        var path = await FindJavaAsync(obj, larg, arg1);

        if (!File.Exists(path))
        {
            throw new LaunchException(LaunchError.SelectJavaNotFound);
        }

        //准备Jvm参数
        larg.Gui?.StateUpdate(obj, LaunchState.JvmPrepare);
        var arg = obj.MakeRunArg(larg, arg1, false);

        //自定义启动参数
        var env = new Dictionary<string, string>();
        string envstr;
        if (obj.JvmArg?.JvmEnv is { } str)
        {
            envstr = str;
        }
        else if (ConfigLoad.Config.DefaultJvmArg.JvmEnv is { } str1)
        {
            envstr = str1;
        }
        else
        {
            envstr = "";
        }

        //需要处理环境变量
        if (!string.IsNullOrWhiteSpace(envstr))
        {
            var list1 = envstr.Split('\n');
            foreach (var item in list1)
            {
                var item1 = item.Trim();
                var index = item1.IndexOf('=');
                string key, value;
                if (index == -1)
                {
                    key = item1;
                    value = "";
                }
                else if (index + 1 == item1.Length)
                {
                    key = item1[..^1];
                    value = "";
                }
                else
                {
                    key = item1[..index];
                    value = item1[(index + 1)..];
                }

                if (!env.TryAdd(key, value))
                {
                    env[key] = value;
                }
            }
        }

        larg.Gui?.StateUpdate(obj, LaunchState.End);

        return new CreateCmdRes
        {
            Res = true,
            Args = arg,
            Dir = obj.GetGamePath(),
            Java = path,
            Envs = env
        };
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token">取消Token</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns>游戏句柄</returns>
    public static async Task<GameHandel?> StartGameAsync(this GameSettingObj obj, GameLaunchArg larg,
        CancellationToken token)
    {
        ColorMCCore.GameLogClear(obj);

        if (obj.CustomLoader?.CustomJson != true)
        {
            //版本号检测
            if (string.IsNullOrWhiteSpace(obj.Version)
                || (obj.Loader is not (Loaders.Normal or Loaders.Custom) &&
                    string.IsNullOrWhiteSpace(obj.LoaderVersion))
                || (obj.Loader is Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
            {
                throw new LaunchException(LaunchError.VersionError);
            }
        }

        //登录账户
        await AuthLoginAsync(obj, larg, token);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //处理服务器实例
        await PackUpdateAsync(obj, larg, token);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        larg.Gui?.StateUpdate(obj, LaunchState.Checking);

        //检查游戏文件
        var arg1 = await CheckGameFileAsync(obj, larg, token);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //查找合适的JAVA
        var path = await FindJavaAsync(obj, larg, arg1);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        if (!File.Exists(path))
        {
            throw new LaunchException(LaunchError.JavaNotFound);
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //准备Jvm参数
        larg.Gui?.StateUpdate(obj, LaunchState.JvmPrepare);

        var arg = obj.MakeRunArg(larg, arg1, true);

        ColorMCCore.OnGameLog(obj, GameSystemLog.JavaPath, path, "");

        ColorMCCore.OnGameLog(obj, GameSystemLog.LaunchArgs);
        bool hidenext = false;
        //打印参数
        foreach (var item in arg)
        {
            if (hidenext)
            {
                hidenext = false;
                ColorMCCore.OnGameLog(obj, "******");
            }
            else
            {
                ColorMCCore.OnGameLog(obj, item);
            }

            var low = item.ToLower();
            if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
            {
                hidenext = true;
            }
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //自定义启动参数
        var env = MakeEnv(obj);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        var stopwatch = new Stopwatch();

        //启动前运行
        if (obj.JvmArg?.LaunchPre == true || ConfigLoad.Config.DefaultJvmArg.LaunchPre)
        {
            var cmd1 = obj.JvmArg?.LaunchPreData;
            var cmd2 = ConfigLoad.Config.DefaultJvmArg.LaunchPreData;
            var start = string.IsNullOrWhiteSpace(cmd1) ? cmd2 : cmd1;
            var prerun = obj.JvmArg?.PreRunSame ?? ConfigLoad.Config.DefaultJvmArg.PreRunSame;
            if (!string.IsNullOrWhiteSpace(start))
            {
                if (larg.Gui == null || await larg.Gui.LaunchProcess(true))
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    larg.Gui?.StateUpdate(obj, LaunchState.LaunchPre);
                    start = obj.ReplaceArg(path, arg, start);
                    obj.CmdRun(start, env, prerun, larg.Admin);
                    stopwatch.Stop();
                    ColorMCCore.OnGameLog(obj, GameSystemLog.CmdPreTime, stopwatch.Elapsed.ToString(), "");
                }
            }
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //启动进程
        stopwatch.Reset();
        stopwatch.Start();
        var handel = new GameHandel(new GameRunObj
        {
            Obj = obj,
            Arg = arg,
            Env = env,
            Path = path,
            Auth = larg.Auth,
            Admin = larg.Admin
        });
        stopwatch.Stop();
        ColorMCCore.OnGameLog(obj, GameSystemLog.LaunchTime, stopwatch.Elapsed.ToString(), "");

        ColorMCCore.AddGameHandel(obj.UUID, handel);

        //启动后执行
        if (obj.JvmArg?.LaunchPost != true && !ConfigLoad.Config.DefaultJvmArg.LaunchPost)
        {
            return handel;
        }

        var start1 = obj.JvmArg?.LaunchPostData;
        if (string.IsNullOrWhiteSpace(start1))
        {
            start1 = ConfigLoad.Config.DefaultJvmArg?.LaunchPostData;
        }

        if (!string.IsNullOrWhiteSpace(start1))
        {
            if (larg.Gui == null || await larg.Gui.LaunchProcess(false))
            {
                stopwatch.Start();
                larg.Gui?.StateUpdate(obj, LaunchState.LaunchPost);
                start1 = obj.ReplaceArg(path, arg, start1);
                obj.CmdRun(start1, env, false, larg.Admin);
                stopwatch.Stop();
                ColorMCCore.OnGameLog(obj, GameSystemLog.CmdPostTime,
                     stopwatch.Elapsed.ToString(), "");
            }
        }

        return handel;
    }
}
